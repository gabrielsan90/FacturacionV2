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
public class PresupuestosController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<PresupuestosController> _logger;

    public PresupuestosController(DataContext context, ILogger<PresupuestosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================================================
    // Presupuesto CRUD
    // =========================================================

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var presupuestos = await _context.Presupuestos
                .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
                .OrderByDescending(p => p.AnioFiscal)
                .ThenBy(p => p.Codigo)
                .Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    p.Nombre,
                    p.Descripcion,
                    p.AnioFiscal,
                    p.TipoPresupuesto,
                    p.Estado,
                    p.MontoTotal,
                    p.MontoEjecutado,
                    PorcentajeEjecucion = p.MontoTotal > 0 ? Math.Round((p.MontoEjecutado / p.MontoTotal) * 100, 2) : 0,
                    MontoDisponible = p.MontoTotal - p.MontoEjecutado,
                    p.Version,
                    p.Observaciones,
                    p.FechaCreacion
                })
                .ToListAsync();

            return Ok(presupuestos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving presupuestos for empresa {EmpresaId}", empresaId);
            return BadRequest($"Error al obtener los presupuestos: {ex.Message}");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var presupuesto = await _context.Presupuestos
                .Include(p => p.Lineas!)
                    .ThenInclude(l => l.CuentaContable)
                .Include(p => p.Lineas!)
                    .ThenInclude(l => l.CentroCosto)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (presupuesto == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(presupuesto.EmpresaId))
            {
                return Forbid();
            }

            var result = new
            {
                presupuesto.Id,
                presupuesto.EmpresaId,
                presupuesto.Codigo,
                presupuesto.Nombre,
                presupuesto.Descripcion,
                presupuesto.AnioFiscal,
                presupuesto.TipoPresupuesto,
                presupuesto.Estado,
                presupuesto.MontoTotal,
                presupuesto.MontoEjecutado,
                PorcentajeEjecucion = presupuesto.PorcentajeEjecucion,
                MontoDisponible = presupuesto.MontoDisponible,
                presupuesto.Version,
                presupuesto.Observaciones,
                presupuesto.FechaCreacion,
                presupuesto.FechaAprobacion,
                Lineas = presupuesto.Lineas?.OrderBy(l => l.CuentaContable?.Codigo).Select(l => new
                {
                    l.Id,
                    l.PresupuestoId,
                    l.CuentaContableId,
                    CuentaContableCodigo = l.CuentaContable?.Codigo ?? "",
                    CuentaContableNombre = l.CuentaContable?.Nombre ?? "",
                    l.CentroCostoId,
                    CentroCostoCodigo = l.CentroCosto?.Codigo ?? "",
                    CentroCostoNombre = l.CentroCosto?.Nombre ?? "",
                    l.MontoEnero,
                    l.MontoFebrero,
                    l.MontoMarzo,
                    l.MontoAbril,
                    l.MontoMayo,
                    l.MontoJunio,
                    l.MontoJulio,
                    l.MontoAgosto,
                    l.MontoSeptiembre,
                    l.MontoOctubre,
                    l.MontoNoviembre,
                    l.MontoDiciembre,
                    l.EjecutadoEnero,
                    l.EjecutadoFebrero,
                    l.EjecutadoMarzo,
                    l.EjecutadoAbril,
                    l.EjecutadoMayo,
                    l.EjecutadoJunio,
                    l.EjecutadoJulio,
                    l.EjecutadoAgosto,
                    l.EjecutadoSeptiembre,
                    l.EjecutadoOctubre,
                    l.EjecutadoNoviembre,
                    l.EjecutadoDiciembre,
                    MontoAnual = l.MontoAnual,
                    EjecutadoAnual = l.EjecutadoAnual,
                    DisponibleAnual = l.DisponibleAnual,
                    PorcentajeEjecucion = l.PorcentajeEjecucion,
                    l.Notas
                }).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving presupuesto {Id}", id);
            return BadRequest($"Error al obtener el presupuesto: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Presupuesto presupuesto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(presupuesto.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único por empresa
            var existente = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.EmpresaId == presupuesto.EmpresaId &&
                                         p.Codigo == presupuesto.Codigo &&
                                         !p.IsDeleted);

            if (existente != null)
            {
                return BadRequest("Ya existe un presupuesto con este código.");
            }

            presupuesto.Id = Guid.NewGuid();
            presupuesto.Estado = EstadosPresupuesto.Borrador;
            presupuesto.FechaCreacion = DateTime.Now;
            presupuesto.CreadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Presupuesto {Codigo} created by user {UserId}", presupuesto.Codigo, presupuesto.CreadoPorId);

            return Ok(presupuesto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating presupuesto");
            return BadRequest($"Error al crear el presupuesto: {ex.Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Presupuesto presupuesto)
    {
        try
        {
            if (id != presupuesto.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (existente == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                return Forbid();
            }

            // Solo se puede editar en estado Borrador
            if (existente.Estado != EstadosPresupuesto.Borrador)
            {
                return BadRequest("Solo se puede editar un presupuesto en estado Borrador.");
            }

            // Verificar código único
            var duplicado = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.EmpresaId == existente.EmpresaId &&
                                         p.Codigo == presupuesto.Codigo &&
                                         p.Id != id &&
                                         !p.IsDeleted);

            if (duplicado != null)
            {
                return BadRequest("Ya existe otro presupuesto con este código.");
            }

            // Actualizar campos editables
            existente.Codigo = presupuesto.Codigo;
            existente.Nombre = presupuesto.Nombre;
            existente.Descripcion = presupuesto.Descripcion;
            existente.AnioFiscal = presupuesto.AnioFiscal;
            existente.TipoPresupuesto = presupuesto.TipoPresupuesto;
            existente.Observaciones = presupuesto.Observaciones;
            existente.FechaModificacion = DateTime.Now;
            existente.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Presupuestos.Update(existente);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Presupuesto {Codigo} updated by user {UserId}", existente.Codigo, existente.ModificadoPorId);

            return Ok(existente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating presupuesto {Id}", id);
            return BadRequest($"Error al actualizar el presupuesto: {ex.Message}");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (presupuesto == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(presupuesto.EmpresaId))
            {
                return Forbid();
            }

            if (presupuesto.Estado != EstadosPresupuesto.Borrador)
            {
                return BadRequest("Solo se puede eliminar un presupuesto en estado Borrador.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            presupuesto.IsDeleted = true;
            presupuesto.FechaEliminacion = DateTime.Now;
            presupuesto.UsuarioEliminacionId = userId;

            _context.Presupuestos.Update(presupuesto);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Presupuesto {Id} deleted by user {UserId}", id, userId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting presupuesto {Id}", id);
            return BadRequest($"Error al eliminar el presupuesto: {ex.Message}");
        }
    }

    // =========================================================
    // Lineas de Presupuesto
    // =========================================================

    [HttpPost("{id:guid}/lineas")]
    public async Task<IActionResult> PostLineaAsync(Guid id, [FromBody] LineaPresupuesto linea)
    {
        try
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (presupuesto == null)
            {
                return NotFound("Presupuesto no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(presupuesto.EmpresaId))
            {
                return Forbid();
            }

            if (presupuesto.Estado != EstadosPresupuesto.Borrador)
            {
                return BadRequest("Solo se pueden agregar líneas a un presupuesto en estado Borrador.");
            }

            // Verificar que no exista una línea con la misma cuenta y centro de costo
            var existeLinea = await _context.LineasPresupuesto
                .AnyAsync(l => l.PresupuestoId == id &&
                              l.CuentaContableId == linea.CuentaContableId &&
                              l.CentroCostoId == linea.CentroCostoId);

            if (existeLinea)
            {
                return BadRequest("Ya existe una línea con la misma cuenta contable y centro de costo.");
            }

            linea.Id = Guid.NewGuid();
            linea.EmpresaId = presupuesto.EmpresaId;
            linea.PresupuestoId = id;

            _context.LineasPresupuesto.Add(linea);
            await _context.SaveChangesAsync();

            // Recalcular totales del presupuesto
            await RecalcularTotalesAsync(id);

            _logger.LogInformation("Línea added to presupuesto {PresupuestoId}", id);

            return Ok(linea);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding línea to presupuesto {Id}", id);
            return BadRequest($"Error al agregar la línea: {ex.Message}");
        }
    }

    [HttpPut("{id:guid}/lineas/{lineaId:guid}")]
    public async Task<IActionResult> PutLineaAsync(Guid id, Guid lineaId, [FromBody] LineaPresupuesto linea)
    {
        try
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (presupuesto == null)
            {
                return NotFound("Presupuesto no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(presupuesto.EmpresaId))
            {
                return Forbid();
            }

            if (presupuesto.Estado != EstadosPresupuesto.Borrador)
            {
                return BadRequest("Solo se pueden modificar líneas de un presupuesto en estado Borrador.");
            }

            var existente = await _context.LineasPresupuesto
                .FirstOrDefaultAsync(l => l.Id == lineaId && l.PresupuestoId == id);

            if (existente == null)
            {
                return NotFound("Línea no encontrada.");
            }

            // Verificar duplicado (otra línea con misma cuenta+centro)
            var duplicado = await _context.LineasPresupuesto
                .AnyAsync(l => l.PresupuestoId == id &&
                              l.CuentaContableId == linea.CuentaContableId &&
                              l.CentroCostoId == linea.CentroCostoId &&
                              l.Id != lineaId);

            if (duplicado)
            {
                return BadRequest("Ya existe otra línea con la misma cuenta contable y centro de costo.");
            }

            // Actualizar campos
            existente.CuentaContableId = linea.CuentaContableId;
            existente.CentroCostoId = linea.CentroCostoId;
            existente.MontoEnero = linea.MontoEnero;
            existente.MontoFebrero = linea.MontoFebrero;
            existente.MontoMarzo = linea.MontoMarzo;
            existente.MontoAbril = linea.MontoAbril;
            existente.MontoMayo = linea.MontoMayo;
            existente.MontoJunio = linea.MontoJunio;
            existente.MontoJulio = linea.MontoJulio;
            existente.MontoAgosto = linea.MontoAgosto;
            existente.MontoSeptiembre = linea.MontoSeptiembre;
            existente.MontoOctubre = linea.MontoOctubre;
            existente.MontoNoviembre = linea.MontoNoviembre;
            existente.MontoDiciembre = linea.MontoDiciembre;
            existente.Notas = linea.Notas;

            _context.LineasPresupuesto.Update(existente);
            await _context.SaveChangesAsync();

            // Recalcular totales del presupuesto
            await RecalcularTotalesAsync(id);

            _logger.LogInformation("Línea {LineaId} updated in presupuesto {PresupuestoId}", lineaId, id);

            return Ok(existente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating línea {LineaId} in presupuesto {Id}", lineaId, id);
            return BadRequest($"Error al actualizar la línea: {ex.Message}");
        }
    }

    [HttpDelete("{id:guid}/lineas/{lineaId:guid}")]
    public async Task<IActionResult> DeleteLineaAsync(Guid id, Guid lineaId)
    {
        try
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (presupuesto == null)
            {
                return NotFound("Presupuesto no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(presupuesto.EmpresaId))
            {
                return Forbid();
            }

            if (presupuesto.Estado != EstadosPresupuesto.Borrador)
            {
                return BadRequest("Solo se pueden eliminar líneas de un presupuesto en estado Borrador.");
            }

            var linea = await _context.LineasPresupuesto
                .FirstOrDefaultAsync(l => l.Id == lineaId && l.PresupuestoId == id);

            if (linea == null)
            {
                return NotFound("Línea no encontrada.");
            }

            _context.LineasPresupuesto.Remove(linea);
            await _context.SaveChangesAsync();

            // Recalcular totales del presupuesto
            await RecalcularTotalesAsync(id);

            _logger.LogInformation("Línea {LineaId} deleted from presupuesto {PresupuestoId}", lineaId, id);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting línea {LineaId} from presupuesto {Id}", lineaId, id);
            return BadRequest($"Error al eliminar la línea: {ex.Message}");
        }
    }

    // =========================================================
    // Workflow y Totales
    // =========================================================

    [HttpPost("{id:guid}/cambiar-estado")]
    public async Task<IActionResult> CambiarEstadoAsync(Guid id, [FromBody] CambiarEstadoRequest request)
    {
        try
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (presupuesto == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(presupuesto.EmpresaId))
            {
                return Forbid();
            }

            var nuevoEstado = request.NuevoEstado;
            var estadoActual = presupuesto.Estado;

            // Validar transiciones permitidas
            var transicionesPermitidas = new Dictionary<string, string[]>
            {
                { EstadosPresupuesto.Borrador, new[] { EstadosPresupuesto.EnRevision } },
                { EstadosPresupuesto.EnRevision, new[] { EstadosPresupuesto.Aprobado, EstadosPresupuesto.Borrador } },
                { EstadosPresupuesto.Aprobado, new[] { EstadosPresupuesto.Vigente, EstadosPresupuesto.EnRevision } },
                { EstadosPresupuesto.Vigente, new[] { EstadosPresupuesto.Cerrado } },
                { EstadosPresupuesto.Cerrado, Array.Empty<string>() }
            };

            if (!transicionesPermitidas.ContainsKey(estadoActual) ||
                !transicionesPermitidas[estadoActual].Contains(nuevoEstado))
            {
                return BadRequest($"No se puede cambiar del estado '{estadoActual}' a '{nuevoEstado}'.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            presupuesto.Estado = nuevoEstado;
            presupuesto.FechaModificacion = DateTime.Now;
            presupuesto.ModificadoPorId = userId;

            // Si se aprueba, registrar aprobación
            if (nuevoEstado == EstadosPresupuesto.Aprobado)
            {
                presupuesto.FechaAprobacion = DateTime.Now;
                presupuesto.AprobadoPorId = userId;
            }

            _context.Presupuestos.Update(presupuesto);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Presupuesto {Id} estado changed from {OldEstado} to {NewEstado} by {UserId}",
                id, estadoActual, nuevoEstado, userId);

            return Ok(new { estado = presupuesto.Estado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing estado for presupuesto {Id}", id);
            return BadRequest($"Error al cambiar el estado: {ex.Message}");
        }
    }

    [HttpPost("{id:guid}/recalcular-totales")]
    public async Task<IActionResult> RecalcularTotalesEndpointAsync(Guid id)
    {
        try
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (presupuesto == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(presupuesto.EmpresaId))
            {
                return Forbid();
            }

            await RecalcularTotalesAsync(id);

            var updated = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id);

            return Ok(new { updated!.MontoTotal, updated.MontoEjecutado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating totals for presupuesto {Id}", id);
            return BadRequest($"Error al recalcular totales: {ex.Message}");
        }
    }

    // =========================================================
    // Helpers
    // =========================================================

    private async Task RecalcularTotalesAsync(Guid presupuestoId)
    {
        var lineas = await _context.LineasPresupuesto
            .Where(l => l.PresupuestoId == presupuestoId)
            .ToListAsync();

        var montoTotal = lineas.Sum(l => l.MontoAnual);
        var montoEjecutado = lineas.Sum(l => l.EjecutadoAnual);

        await _context.Presupuestos
            .Where(p => p.Id == presupuestoId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.MontoTotal, montoTotal)
                .SetProperty(p => p.MontoEjecutado, montoEjecutado));
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}

public class CambiarEstadoRequest
{
    public string NuevoEstado { get; set; } = string.Empty;
}
