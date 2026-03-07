using Facturacion.Backend.Data;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class TrasladosInventarioController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<TrasladosInventarioController> _logger;

    public TrasladosInventarioController(DataContext context, ILogger<TrasladosInventarioController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los traslados de inventario de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var traslados = await _context.TrasladosInventario
                .Include(t => t.BodegaOrigen)
                .Include(t => t.BodegaDestino)
                .Include(t => t.Detalles!)
                    .ThenInclude(d => d.Producto)
                .Where(t => t.EmpresaId == empresaId && !t.IsDeleted)
                .OrderByDescending(t => t.Fecha)
                .AsNoTracking()
                .Select(t => new
                {
                    t.Id,
                    t.Numero,
                    t.Fecha,
                    t.Estado,
                    t.EstadoDescripcion,
                    t.FechaEnvio,
                    t.FechaRecepcion,
                    t.Observaciones,
                    t.PuedeEnviar,
                    t.PuedeRecibir,
                    t.PuedeAnular,
                    bodegaOrigenNombre = t.BodegaOrigen != null ? t.BodegaOrigen.Nombre : "",
                    bodegaDestinoNombre = t.BodegaDestino != null ? t.BodegaDestino.Nombre : "",
                    cantidadProductos = t.Detalles != null ? t.Detalles.Count : 0
                })
                .ToListAsync();

            return Ok(traslados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo traslados para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener traslados: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene un traslado de inventario por ID con todos sus detalles
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var traslado = await _context.TrasladosInventario
                .Include(t => t.BodegaOrigen)
                .Include(t => t.BodegaDestino)
                .Include(t => t.Detalles!)
                    .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (traslado == null)
                return NotFound("Traslado no encontrado.");

            if (!await TieneAccesoEmpresaAsync(traslado.EmpresaId))
                return Forbid();

            return Ok(traslado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo traslado {Id}", id);
            return StatusCode(500, $"Error al obtener traslado: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea un nuevo traslado de inventario
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] TrasladoInventario traslado)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(traslado.EmpresaId))
                return Forbid();

            if (traslado.BodegaOrigenId == traslado.BodegaDestinoId)
                return BadRequest("La bodega origen y destino no pueden ser la misma.");

            // Generate number before validation (required by [ApiController] auto-validation)
            if (string.IsNullOrWhiteSpace(traslado.Numero) || traslado.Numero == "AUTO")
            {
                var count = await _context.TrasladosInventario
                    .CountAsync(t => t.EmpresaId == traslado.EmpresaId);
                traslado.Numero = $"TI-{(count + 1):D6}";
            }

            traslado.Id = Guid.NewGuid();
            traslado.Estado = "PEN";
            traslado.FechaCreacion = DateTime.UtcNow;
            traslado.UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ModelState.Remove("Numero");
            if (!TryValidateModel(traslado))
                return BadRequest(ModelState);

            // Process details
            if (traslado.Detalles != null)
            {
                foreach (var detalle in traslado.Detalles)
                {
                    detalle.Id = Guid.NewGuid();
                    detalle.TrasladoInventarioId = traslado.Id;
                    detalle.CantidadEnviada = 0;
                    detalle.CantidadRecibida = 0;
                }
            }

            _context.TrasladosInventario.Add(traslado);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Traslado creado: {Id}, Número: {Numero}", traslado.Id, traslado.Numero);
            return Ok(traslado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando traslado");
            return StatusCode(500, $"Error al crear traslado: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza un traslado de inventario (solo en estado Pendiente)
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] TrasladoInventario traslado)
    {
        try
        {
            if (id != traslado.Id)
                return BadRequest("El ID de la URL no coincide.");

            var existente = await _context.TrasladosInventario
                .Include(t => t.Detalles)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (existente == null)
                return NotFound("Traslado no encontrado.");

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
                return Forbid();

            if (existente.Estado != "PEN")
                return BadRequest("Solo se pueden editar traslados en estado Pendiente.");

            if (traslado.BodegaOrigenId == traslado.BodegaDestinoId)
                return BadRequest("La bodega origen y destino no pueden ser la misma.");

            existente.Fecha = traslado.Fecha;
            existente.BodegaOrigenId = traslado.BodegaOrigenId;
            existente.BodegaDestinoId = traslado.BodegaDestinoId;
            existente.Observaciones = traslado.Observaciones;
            existente.FechaModificacion = DateTime.UtcNow;
            existente.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Replace details
            if (existente.Detalles != null)
            {
                _context.TrasladosInventarioDetalle.RemoveRange(existente.Detalles);
            }

            if (traslado.Detalles != null)
            {
                foreach (var detalle in traslado.Detalles)
                {
                    detalle.Id = Guid.NewGuid();
                    detalle.TrasladoInventarioId = id;
                    detalle.CantidadEnviada = 0;
                    detalle.CantidadRecibida = 0;
                    _context.TrasladosInventarioDetalle.Add(detalle);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Traslado actualizado: {Id}", id);
            return Ok(existente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando traslado {Id}", id);
            return StatusCode(500, $"Error al actualizar traslado: {ex.Message}");
        }
    }

    /// <summary>
    /// Envía un traslado (PEN → TRA)
    /// </summary>
    [HttpPost("{id:guid}/enviar")]
    public async Task<IActionResult> EnviarAsync(Guid id)
    {
        try
        {
            var traslado = await _context.TrasladosInventario
                .Include(t => t.Detalles)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (traslado == null)
                return NotFound("Traslado no encontrado.");

            if (!await TieneAccesoEmpresaAsync(traslado.EmpresaId))
                return Forbid();

            if (traslado.Estado != "PEN")
                return BadRequest("Solo se pueden enviar traslados en estado Pendiente.");

            if (traslado.Detalles == null || !traslado.Detalles.Any())
                return BadRequest("El traslado debe tener al menos un producto.");

            traslado.Estado = "TRA";
            traslado.FechaEnvio = DateTime.UtcNow;
            traslado.EnviadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            traslado.FechaModificacion = DateTime.UtcNow;
            traslado.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Update sent quantities
            foreach (var detalle in traslado.Detalles)
            {
                detalle.CantidadEnviada = detalle.CantidadSolicitada;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Traslado enviado: {Id}", id);
            return Ok(new { message = "Traslado enviado correctamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando traslado {Id}", id);
            return StatusCode(500, $"Error al enviar traslado: {ex.Message}");
        }
    }

    /// <summary>
    /// Recibe un traslado (TRA → REC)
    /// </summary>
    [HttpPost("{id:guid}/recibir")]
    public async Task<IActionResult> RecibirAsync(Guid id)
    {
        try
        {
            var traslado = await _context.TrasladosInventario
                .Include(t => t.Detalles)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (traslado == null)
                return NotFound("Traslado no encontrado.");

            if (!await TieneAccesoEmpresaAsync(traslado.EmpresaId))
                return Forbid();

            if (traslado.Estado != "TRA")
                return BadRequest("Solo se pueden recibir traslados en estado En Tránsito.");

            traslado.Estado = "REC";
            traslado.FechaRecepcion = DateTime.UtcNow;
            traslado.RecibidoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            traslado.FechaModificacion = DateTime.UtcNow;
            traslado.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Update received quantities
            if (traslado.Detalles != null)
            {
                foreach (var detalle in traslado.Detalles)
                {
                    detalle.CantidadRecibida = detalle.CantidadEnviada;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Traslado recibido: {Id}", id);
            return Ok(new { message = "Traslado recibido correctamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recibiendo traslado {Id}", id);
            return StatusCode(500, $"Error al recibir traslado: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina un traslado (soft delete, solo en estado Pendiente)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var traslado = await _context.TrasladosInventario
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (traslado == null)
                return NotFound("Traslado no encontrado.");

            if (!await TieneAccesoEmpresaAsync(traslado.EmpresaId))
                return Forbid();

            if (traslado.Estado != "PEN")
                return BadRequest("Solo se pueden eliminar traslados en estado Pendiente.");

            traslado.IsDeleted = true;
            traslado.FechaEliminacion = DateTime.UtcNow;
            traslado.UsuarioEliminacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Traslado eliminado: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando traslado {Id}", id);
            return StatusCode(500, $"Error al eliminar traslado: {ex.Message}");
        }
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        if (User.IsInRole("SuperUser"))
            return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
