using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class AjustesInventarioController : ControllerBase
{
    private readonly IAjusteInventarioRepository _repository;
    private readonly DataContext _context;
    private readonly ILogger<AjustesInventarioController> _logger;

    public AjustesInventarioController(
        IAjusteInventarioRepository repository,
        DataContext context,
        ILogger<AjustesInventarioController> logger)
    {
        _repository = repository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los ajustes de inventario de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            _logger.LogInformation("Getting inventory adjustments for company {EmpresaId}", empresaId);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("User {UserId} attempted to access company {EmpresaId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var action = await _repository.GetByEmpresaAsync(empresaId);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to get inventory adjustments for company {EmpresaId}: {Message}",
                    empresaId, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Found {Count} inventory adjustments for company {EmpresaId}",
                action.Result?.Count() ?? 0, empresaId);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory adjustments for company {EmpresaId}", empresaId);
            return StatusCode(500, new ActionResponse<IEnumerable<AjusteInventario>>
            {
                WasSuccess = false,
                Message = "Error al obtener los ajustes de inventario"
            });
        }
    }

    /// <summary>
    /// Obtiene un ajuste de inventario por ID con todos sus detalles
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting inventory adjustment {AjusteId}", id);

            var action = await _repository.GetAsync(id);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Inventory adjustment {AjusteId} not found", id);
                return NotFound(action);
            }

            if (!await TieneAccesoEmpresaAsync(action.Result!.EmpresaId))
            {
                _logger.LogWarning("User {UserId} attempted to access inventory adjustment {AjusteId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            _logger.LogInformation("Retrieved inventory adjustment {AjusteId} with {DetalleCount} details",
                id, action.Result.Detalles?.Count ?? 0);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory adjustment {AjusteId}", id);
            return StatusCode(500, new ActionResponse<AjusteInventario>
            {
                WasSuccess = false,
                Message = "Error al obtener el ajuste de inventario"
            });
        }
    }

    /// <summary>
    /// Crea un nuevo ajuste de inventario con sus detalles
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] AjusteInventario ajuste)
    {
        try
        {
            _logger.LogInformation("Creating new inventory adjustment for company {EmpresaId}", ajuste.EmpresaId);

            if (!await TieneAccesoEmpresaAsync(ajuste.EmpresaId))
            {
                _logger.LogWarning("User {UserId} attempted to create inventory adjustment for company {EmpresaId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), ajuste.EmpresaId);
                return Forbid();
            }

            // Generate Numero before validation ([ApiController] auto-validates [Required] fields)
            if (string.IsNullOrWhiteSpace(ajuste.Numero) || ajuste.Numero == "AUTO")
            {
                ajuste.Numero = await GenerarNumeroAjusteAsync(ajuste.EmpresaId);
            }

            ModelState.Remove("Numero");
            if (!TryValidateModel(ajuste))
            {
                _logger.LogWarning("Invalid model state for inventory adjustment creation");
                return BadRequest(ModelState);
            }

            var action = await _repository.AddAsync(ajuste);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to create inventory adjustment: {Message}", action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Created inventory adjustment {AjusteId} with number {Numero} for company {EmpresaId}",
                action.Result!.Id, action.Result.Numero, action.Result.EmpresaId);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory adjustment for company {EmpresaId}", ajuste.EmpresaId);
            return StatusCode(500, new ActionResponse<AjusteInventario>
            {
                WasSuccess = false,
                Message = "Error al crear el ajuste de inventario"
            });
        }
    }

    /// <summary>
    /// Actualiza un ajuste de inventario existente (solo en estado Pendiente)
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] AjusteInventario ajuste)
    {
        try
        {
            if (id != ajuste.Id)
                return BadRequest("El ID de la URL no coincide con el ID del ajuste.");

            _logger.LogInformation("Updating inventory adjustment {AjusteId}", id);

            var existente = await _context.AjustesInventario
                .Include(a => a.Detalles)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (existente == null)
            {
                return NotFound(new ActionResponse<AjusteInventario> { WasSuccess = false, Message = "Ajuste no encontrado." });
            }

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                return Forbid();
            }

            if (existente.Estado != "PEN")
            {
                return BadRequest("Solo se pueden modificar ajustes en estado Pendiente.");
            }

            // Update scalar fields
            existente.Fecha = ajuste.Fecha;
            existente.BodegaId = ajuste.BodegaId;
            existente.TipoAjuste = ajuste.TipoAjuste;
            existente.Motivo = ajuste.Motivo;
            existente.Observaciones = ajuste.Observaciones;
            existente.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            existente.FechaModificacion = DateTime.UtcNow;

            // Update details: remove old, add new
            if (existente.Detalles != null)
            {
                _context.Set<AjusteInventarioDetalle>().RemoveRange(existente.Detalles);
            }
            if (ajuste.Detalles != null)
            {
                foreach (var detalle in ajuste.Detalles)
                {
                    detalle.AjusteInventarioId = id;
                    if (detalle.Id == Guid.Empty) detalle.Id = Guid.NewGuid();
                }
                _context.Set<AjusteInventarioDetalle>().AddRange(ajuste.Detalles);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated inventory adjustment {AjusteId}", id);
            return Ok(new ActionResponse<AjusteInventario> { WasSuccess = true, Result = existente });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory adjustment {AjusteId}", id);
            return StatusCode(500, new ActionResponse<AjusteInventario>
            {
                WasSuccess = false,
                Message = "Error al actualizar el ajuste de inventario"
            });
        }
    }

    /// <summary>
    /// Aplica el ajuste de inventario, actualizando las existencias
    /// Solo se puede aplicar si está en estado Pendiente (PEN)
    /// </summary>
    [HttpPost("{id:guid}/aplicar")]
    public async Task<IActionResult> AplicarAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Applying inventory adjustment {AjusteId}", id);

            // First get the adjustment to check access
            var ajusteAction = await _repository.GetAsync(id);
            if (!ajusteAction.WasSuccess)
            {
                _logger.LogWarning("Inventory adjustment {AjusteId} not found for application", id);
                return NotFound(ajusteAction);
            }

            if (!await TieneAccesoEmpresaAsync(ajusteAction.Result!.EmpresaId))
            {
                _logger.LogWarning("User {UserId} attempted to apply inventory adjustment {AjusteId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var action = await _repository.AprobarAsync(id, userId!);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to apply inventory adjustment {AjusteId}: {Message}",
                    id, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Successfully applied inventory adjustment {AjusteId}", id);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying inventory adjustment {AjusteId}", id);
            return StatusCode(500, new ActionResponse<AjusteInventario>
            {
                WasSuccess = false,
                Message = "Error al aplicar el ajuste de inventario"
            });
        }
    }

    /// <summary>
    /// Elimina lógicamente un ajuste de inventario
    /// Solo se puede eliminar si está en estado Pendiente (no aplicado)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting inventory adjustment {AjusteId}", id);

            // First get the adjustment to check access
            var ajusteAction = await _repository.GetAsync(id);
            if (!ajusteAction.WasSuccess)
            {
                _logger.LogWarning("Inventory adjustment {AjusteId} not found for deletion", id);
                return NotFound(ajusteAction);
            }

            if (!await TieneAccesoEmpresaAsync(ajusteAction.Result!.EmpresaId))
            {
                _logger.LogWarning("User {UserId} attempted to delete inventory adjustment {AjusteId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var action = await _repository.AnularAsync(id, userId!);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to delete inventory adjustment {AjusteId}: {Message}",
                    id, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Successfully deleted inventory adjustment {AjusteId}", id);
            return Ok(new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = "Ajuste de inventario eliminado exitosamente."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory adjustment {AjusteId}", id);
            return StatusCode(500, new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = "Error al eliminar el ajuste de inventario"
            });
        }
    }

    /// <summary>
    /// Verifica si el usuario actual tiene acceso a la empresa especificada (multi-tenant security)
    /// </summary>
    private async Task<string> GenerarNumeroAjusteAsync(Guid empresaId)
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        var prefix = $"AI-{year:D4}-{month:D2}-";

        var lastNumber = await _context.AjustesInventario
            .IgnoreQueryFilters()
            .Where(a => a.EmpresaId == empresaId && a.Numero != null && a.Numero.StartsWith(prefix))
            .OrderByDescending(a => a.Numero)
            .Select(a => a.Numero)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastNumber) && lastNumber.Length > prefix.Length)
        {
            if (int.TryParse(lastNumber[prefix.Length..], out var parsed))
                nextNumber = parsed + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims for multi-tenant validation");
                return false;
            }

            // SuperUser tiene acceso a todas las empresas
            if (User.IsInRole("SuperUser"))
            {
                return true;
            }

            // Verificar si el usuario tiene acceso a la empresa
            return await _context.UsuariosEmpresas
                .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating company access for user {UserId}, company {EmpresaId}",
                User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
            return false;
        }
    }
}
