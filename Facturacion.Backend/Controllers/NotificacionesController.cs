using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controller para gestionar notificaciones in-app
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<NotificacionesController> _logger;

    public NotificacionesController(
        INotificacionUnitOfWork unitOfWork,
        DataContext context,
        ILogger<NotificacionesController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las notificaciones del usuario actual
    /// </summary>
    [HttpGet("usuario/{empresaId:guid}")]
    public async Task<IActionResult> GetByUsuarioAsync(Guid empresaId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("GetByUsuarioAsync called for User: {UserId}, EmpresaId: {EmpresaId}",
                userId, empresaId);

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Access denied to EmpresaId: {EmpresaId} for user: {UserId}", empresaId, userId);
                return Forbid();
            }

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt - no user ID");
                return Unauthorized();
            }

            var action = await _unitOfWork.GetByUsuarioAsync(userId, empresaId);
            if (action.WasSuccess)
            {
                _logger.LogInformation("Retrieved {Count} notifications for user: {UserId}",
                    (action.Result as IEnumerable<object>)?.Count() ?? 0, userId);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve notifications for user: {UserId}, error: {Error}",
                    userId, action.Message);
            }
            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for EmpresaId: {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener notificaciones: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene las notificaciones no leídas del usuario actual
    /// </summary>
    [HttpGet("no-leidas/{empresaId:guid}")]
    public async Task<IActionResult> GetNoLeidasAsync(Guid empresaId)
    {
        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var action = await _unitOfWork.GetNoLeidasAsync(userId, empresaId);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas
    /// </summary>
    [HttpGet("count-no-leidas/{empresaId:guid}")]
    public async Task<IActionResult> GetCountNoLeidasAsync(Guid empresaId)
    {
        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var action = await _unitOfWork.GetCountNoLeidasAsync(userId, empresaId);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Obtiene el resumen de notificaciones del usuario
    /// </summary>
    [HttpGet("resumen/{empresaId:guid}")]
    public async Task<IActionResult> GetResumenAsync(Guid empresaId)
    {
        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var action = await _unitOfWork.GetResumenAsync(userId, empresaId);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Obtiene una notificación por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var action = await _unitOfWork.GetByIdAsync(id);

        if (!action.WasSuccess)
        {
            return NotFound(action.Message);
        }

        // Verificar que el usuario tiene acceso a la notificación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (action.Result!.UsuarioId != userId)
        {
            return Forbid();
        }

        return Ok(action.Result);
    }

    /// <summary>
    /// Crea una nueva notificación
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CrearNotificacionDTO dto)
    {
        try
        {
            _logger.LogInformation("CreateAsync called to create notification for User: {UserId}, EmpresaId: {EmpresaId}",
                dto.UsuarioId, dto.EmpresaId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for notification creation");
                return BadRequest(ModelState);
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(dto.EmpresaId))
            {
                _logger.LogWarning("Access denied to create notification for EmpresaId: {EmpresaId}", dto.EmpresaId);
                return Forbid();
            }

            var action = await _unitOfWork.CreateAsync(dto);
            if (action.WasSuccess)
            {
                _logger.LogInformation("Created notification successfully for user: {UserId}", dto.UsuarioId);
            }
            else
            {
                _logger.LogWarning("Failed to create notification for user: {UserId}, error: {Error}",
                    dto.UsuarioId, action.Message);
            }
            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification for user: {UserId}", dto.UsuarioId);
            return StatusCode(500, $"Error al crear notificación: {ex.Message}");
        }
    }

    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    [HttpPut("marcar-leida/{id:guid}")]
    public async Task<IActionResult> MarcarComoLeidaAsync(Guid id)
    {
        // Verificar que la notificación pertenece al usuario
        var notificacion = await _unitOfWork.GetByIdAsync(id);
        if (!notificacion.WasSuccess)
        {
            return NotFound(notificacion.Message);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (notificacion.Result!.UsuarioId != userId)
        {
            return Forbid();
        }

        var action = await _unitOfWork.MarcarComoLeidaAsync(id);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Marca todas las notificaciones como leídas
    /// </summary>
    [HttpPut("marcar-todas-leidas/{empresaId:guid}")]
    public async Task<IActionResult> MarcarTodasComoLeidasAsync(Guid empresaId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("MarcarTodasComoLeidasAsync called for User: {UserId}, EmpresaId: {EmpresaId}",
                userId, empresaId);

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Access denied to mark all notifications as read for EmpresaId: {EmpresaId}, user: {UserId}",
                    empresaId, userId);
                return Forbid();
            }

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt - no user ID");
                return Unauthorized();
            }

            var action = await _unitOfWork.MarcarTodasComoLeidasAsync(userId, empresaId);
            if (action.WasSuccess)
            {
                _logger.LogInformation("Marked all notifications as read for user: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Failed to mark all notifications as read for user: {UserId}, error: {Error}",
                    userId, action.Message);
            }
            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for EmpresaId: {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al marcar notificaciones como leídas: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina una notificación
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        // Verificar que la notificación pertenece al usuario
        var notificacion = await _unitOfWork.GetByIdAsync(id);
        if (!notificacion.WasSuccess)
        {
            return NotFound(notificacion.Message);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (notificacion.Result!.UsuarioId != userId)
        {
            return Forbid();
        }

        var action = await _unitOfWork.DeleteAsync(id);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Elimina las notificaciones expiradas de una empresa
    /// </summary>
    [HttpDelete("expiradas/{empresaId:guid}")]
    public async Task<IActionResult> DeleteExpiradasAsync(Guid empresaId)
    {
        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _unitOfWork.DeleteExpiradasAsync(empresaId);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    // Métodos auxiliares

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
