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

    public NotificacionesController(INotificacionUnitOfWork unitOfWork, DataContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    /// <summary>
    /// Obtiene todas las notificaciones del usuario actual
    /// </summary>
    [HttpGet("usuario/{empresaId:guid}")]
    public async Task<IActionResult> GetByUsuarioAsync(Guid empresaId)
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

        var action = await _unitOfWork.GetByUsuarioAsync(userId, empresaId);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(dto.EmpresaId))
        {
            return Forbid();
        }

        var action = await _unitOfWork.CreateAsync(dto);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
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

        var action = await _unitOfWork.MarcarTodasComoLeidasAsync(userId, empresaId);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
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
