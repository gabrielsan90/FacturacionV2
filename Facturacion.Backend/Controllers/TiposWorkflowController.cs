using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
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
public class TiposWorkflowController : ControllerBase
{
    private readonly ITipoWorkflowRepository _repository;
    private readonly DataContext _context;
    private readonly ILogger<TiposWorkflowController> _logger;

    public TiposWorkflowController(
        ITipoWorkflowRepository repository,
        DataContext context,
        ILogger<TiposWorkflowController> logger)
    {
        _repository = repository;
        _context = context;
        _logger = logger;
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            _logger.LogInformation("GetByEmpresaAsync called for EmpresaId: {EmpresaId}", empresaId);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Access denied to EmpresaId: {EmpresaId} for user: {UserId}",
                    empresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var action = await _repository.GetAllAsync(empresaId);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to get workflow types for EmpresaId: {EmpresaId}: {Message}",
                    empresaId, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Retrieved {Count} workflow types for EmpresaId: {EmpresaId}",
                action.Result?.Count() ?? 0, empresaId);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow types for EmpresaId: {EmpresaId}", empresaId);
            return StatusCode(500, new ActionResponse<IEnumerable<TipoWorkflow>>
            {
                WasSuccess = false,
                Message = $"Error al obtener tipos de workflow: {ex.Message}"
            });
        }
    }

    [HttpGet("empresa/{empresaId:guid}/activos")]
    public async Task<IActionResult> GetActivosByEmpresaAsync(Guid empresaId)
    {
        try
        {
            _logger.LogInformation("GetActivosByEmpresaAsync called for EmpresaId: {EmpresaId}", empresaId);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Access denied to EmpresaId: {EmpresaId} for user: {UserId}",
                    empresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var action = await _repository.GetActivosAsync(empresaId);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to get active workflow types for EmpresaId: {EmpresaId}: {Message}",
                    empresaId, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Retrieved {Count} active workflow types for EmpresaId: {EmpresaId}",
                action.Result?.Count() ?? 0, empresaId);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active workflow types for EmpresaId: {EmpresaId}", empresaId);
            return StatusCode(500, new ActionResponse<IEnumerable<TipoWorkflow>>
            {
                WasSuccess = false,
                Message = $"Error al obtener tipos de workflow activos: {ex.Message}"
            });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("GetAsync called for TipoWorkflowId: {TipoWorkflowId}", id);

            var action = await _repository.GetAsync(id);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Workflow type not found: {TipoWorkflowId}", id);
                return NotFound(action);
            }

            if (!await TieneAccesoEmpresaAsync(action.Result!.EmpresaId))
            {
                _logger.LogWarning("Access denied to TipoWorkflow: {TipoWorkflowId} for user: {UserId}",
                    id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            _logger.LogInformation("Retrieved workflow type: {TipoWorkflowId}, codigo: {Codigo}",
                id, action.Result.Codigo);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow type: {TipoWorkflowId}", id);
            return StatusCode(500, new ActionResponse<TipoWorkflow>
            {
                WasSuccess = false,
                Message = $"Error al obtener tipo de workflow: {ex.Message}"
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] TipoWorkflow tipo)
    {
        try
        {
            _logger.LogInformation("PostAsync called to create workflow type with codigo: {Codigo} for EmpresaId: {EmpresaId}",
                tipo.Codigo, tipo.EmpresaId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for workflow type creation");
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(tipo.EmpresaId))
            {
                _logger.LogWarning("Access denied to create workflow type for EmpresaId: {EmpresaId}, user: {UserId}",
                    tipo.EmpresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var action = await _repository.AddAsync(tipo);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to create workflow type: {Message}", action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Created workflow type: {TipoWorkflowId}, codigo: {Codigo}",
                action.Result!.Id, action.Result.Codigo);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow type with codigo: {Codigo}", tipo.Codigo);
            return StatusCode(500, new ActionResponse<TipoWorkflow>
            {
                WasSuccess = false,
                Message = $"Error al crear tipo de workflow: {ex.Message}"
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] TipoWorkflow tipo)
    {
        try
        {
            _logger.LogInformation("PutAsync called for TipoWorkflowId: {TipoWorkflowId}", id);

            if (id != tipo.Id)
            {
                _logger.LogWarning("ID mismatch in workflow type update: URL {UrlId} vs Body {BodyId}", id, tipo.Id);
                return BadRequest(new ActionResponse<TipoWorkflow>
                {
                    WasSuccess = false,
                    Message = "El ID no coincide."
                });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for workflow type update");
                return BadRequest(ModelState);
            }

            // Get existing to check access
            var existingAction = await _repository.GetAsync(id);
            if (!existingAction.WasSuccess)
            {
                _logger.LogWarning("Workflow type not found for update: {TipoWorkflowId}", id);
                return NotFound(existingAction);
            }

            if (!await TieneAccesoEmpresaAsync(existingAction.Result!.EmpresaId))
            {
                _logger.LogWarning("Access denied to update TipoWorkflow: {TipoWorkflowId} for user: {UserId}",
                    id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var action = await _repository.UpdateAsync(tipo);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to update workflow type {Id}: {Message}", id, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Updated workflow type: {TipoWorkflowId}, codigo: {Codigo}",
                id, action.Result!.Codigo);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow type: {TipoWorkflowId}", id);
            return StatusCode(500, new ActionResponse<TipoWorkflow>
            {
                WasSuccess = false,
                Message = $"Error al actualizar tipo de workflow: {ex.Message}"
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("DeleteAsync called for TipoWorkflowId: {TipoWorkflowId}", id);

            // Get existing to check access
            var existingAction = await _repository.GetAsync(id);
            if (!existingAction.WasSuccess)
            {
                _logger.LogWarning("Workflow type not found for deletion: {TipoWorkflowId}", id);
                return NotFound(existingAction);
            }

            if (!await TieneAccesoEmpresaAsync(existingAction.Result!.EmpresaId))
            {
                _logger.LogWarning("Access denied to delete TipoWorkflow: {TipoWorkflowId} for user: {UserId}",
                    id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var action = await _repository.DeleteAsync(id, userId!);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to delete workflow type {Id}: {Message}", id, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Successfully deleted workflow type: {TipoWorkflowId}", id);
            return Ok(new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = "Tipo de workflow eliminado exitosamente."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow type: {TipoWorkflowId}", id);
            return StatusCode(500, new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = $"Error al eliminar tipo de workflow: {ex.Message}"
            });
        }
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // Verificar si es SuperUser
        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a la empresa
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
