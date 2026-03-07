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
public class BodegasController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<BodegasController> _logger;

    public BodegasController(DataContext context, ILogger<BodegasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var bodegas = await _context.Bodegas
                .Include(b => b.Sucursal)
                .Where(b => b.EmpresaId == empresaId)
                .OrderBy(b => b.Codigo)
                .ToListAsync();

            return Ok(bodegas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bodegas for EmpresaId: {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener bodegas: {ex.Message}");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var bodega = await _context.Bodegas
                .Include(b => b.Sucursal)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bodega == null)
                return NotFound();

            if (!await TieneAccesoEmpresaAsync(bodega.EmpresaId))
                return Forbid();

            return Ok(bodega);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bodega: {Id}", id);
            return StatusCode(500, $"Error al obtener bodega: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Bodega bodega)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await TieneAccesoEmpresaAsync(bodega.EmpresaId))
                return Forbid();

            // Verificar código único por empresa+sucursal
            var existente = await _context.Bodegas
                .AnyAsync(b => b.EmpresaId == bodega.EmpresaId
                    && b.SucursalId == bodega.SucursalId
                    && b.Codigo == bodega.Codigo);

            if (existente)
                return BadRequest("Ya existe una bodega con este código en la sucursal.");

            // Si se marca como principal, desmarcar la anterior en la misma sucursal
            if (bodega.EsPrincipal)
            {
                var principal = await _context.Bodegas
                    .FirstOrDefaultAsync(b => b.EmpresaId == bodega.EmpresaId
                        && b.SucursalId == bodega.SucursalId
                        && b.EsPrincipal);
                if (principal != null)
                {
                    principal.EsPrincipal = false;
                }
            }

            bodega.Id = Guid.NewGuid();
            bodega.FechaCreacion = DateTime.UtcNow;
            bodega.UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Bodegas.Add(bodega);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created bodega: {Id}, codigo: {Codigo}", bodega.Id, bodega.Codigo);
            return Ok(bodega);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bodega with codigo: {Codigo}", bodega.Codigo);
            return StatusCode(500, $"Error al crear bodega: {ex.Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Bodega bodega)
    {
        try
        {
            if (id != bodega.Id)
                return BadRequest("El ID de la URL no coincide con el ID de la bodega.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existente = await _context.Bodegas.FirstOrDefaultAsync(b => b.Id == id);
            if (existente == null)
                return NotFound();

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
                return Forbid();

            // Verificar código único (excluyendo la actual)
            var duplicado = await _context.Bodegas
                .AnyAsync(b => b.EmpresaId == bodega.EmpresaId
                    && b.SucursalId == bodega.SucursalId
                    && b.Codigo == bodega.Codigo
                    && b.Id != id);

            if (duplicado)
                return BadRequest("Ya existe otra bodega con este código en la sucursal.");

            // Si se marca como principal, desmarcar la anterior
            if (bodega.EsPrincipal && !existente.EsPrincipal)
            {
                var principal = await _context.Bodegas
                    .FirstOrDefaultAsync(b => b.EmpresaId == bodega.EmpresaId
                        && b.SucursalId == bodega.SucursalId
                        && b.EsPrincipal
                        && b.Id != id);
                if (principal != null)
                {
                    principal.EsPrincipal = false;
                }
            }

            // Update fields
            existente.Codigo = bodega.Codigo;
            existente.Nombre = bodega.Nombre;
            existente.Descripcion = bodega.Descripcion;
            existente.Ubicacion = bodega.Ubicacion;
            existente.SucursalId = bodega.SucursalId;
            existente.Responsable = bodega.Responsable;
            existente.Telefono = bodega.Telefono;
            existente.Email = bodega.Email;
            existente.EsPrincipal = bodega.EsPrincipal;
            existente.PermiteNegativos = bodega.PermiteNegativos;
            existente.Activo = bodega.Activo;
            existente.FechaModificacion = DateTime.UtcNow;
            existente.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated bodega: {Id}, codigo: {Codigo}", id, bodega.Codigo);
            return Ok(existente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bodega: {Id}", id);
            return StatusCode(500, $"Error al actualizar bodega: {ex.Message}");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var bodega = await _context.Bodegas.FirstOrDefaultAsync(b => b.Id == id);
            if (bodega == null)
                return NotFound();

            if (!await TieneAccesoEmpresaAsync(bodega.EmpresaId))
                return Forbid();

            // Soft delete
            bodega.IsDeleted = true;
            bodega.FechaEliminacion = DateTime.UtcNow;
            bodega.UsuarioEliminacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted bodega: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bodega: {Id}", id);
            return StatusCode(500, $"Error al eliminar bodega: {ex.Message}");
        }
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (userRoles.Contains("SuperUser"))
            return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
