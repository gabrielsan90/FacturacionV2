using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controller para envío de correos electrónicos
/// Endpoints protegidos con autenticación JWT
/// Soporta envío de documentos, recibos, notificaciones y validación SMTP
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class EmailsController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailsController> _logger;

    public EmailsController(
        IEmailService emailService,
        ILogger<EmailsController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Envía un correo electrónico genérico
    /// </summary>
    /// <param name="empresaId">ID de la empresa para obtener configuración SMTP</param>
    /// <param name="emailDTO">Datos del correo a enviar</param>
    /// <returns>Resultado del envío</returns>
    [HttpPost("enviar")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> EnviarAsync(
        [FromQuery] Guid empresaId,
        [FromBody] EmailDTO emailDTO)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var action = await _emailService.EnviarEmailAsync(empresaId, emailDTO);

            if (!action.WasSuccess)
            {
                return BadRequest(action.Message);
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email genérico para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Envía un documento electrónico (factura, tiquete, etc.) por correo
    /// Incluye XML y representación HTML adjuntos
    /// </summary>
    /// <param name="model">Datos del documento a enviar</param>
    /// <returns>Resultado del envío</returns>
    [HttpPost("documento")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> EnviarDocumentoAsync([FromBody] EnviarDocumentoEmailDTO model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var action = await _emailService.EnviarDocumentoElectronicoAsync(
                model.DocumentoId,
                model.EmailsAdicionales,
                model.Mensaje);

            if (!action.WasSuccess)
            {
                return BadRequest(action.Message);
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando documento electrónico {DocumentoId}", model.DocumentoId);
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Envía un recibo de pago electrónico por correo
    /// </summary>
    /// <param name="reciboPagoId">ID del recibo de pago</param>
    /// <param name="emailsAdicionales">Emails adicionales opcionales</param>
    /// <returns>Resultado del envío</returns>
    [HttpPost("recibo-pago")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> EnviarReciboPagoAsync(
        [FromQuery] Guid reciboPagoId,
        [FromBody] List<string>? emailsAdicionales = null)
    {
        try
        {
            var action = await _emailService.EnviarReciboPagoAsync(reciboPagoId, emailsAdicionales);

            if (!action.WasSuccess)
            {
                return BadRequest(action.Message);
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando recibo de pago {ReciboPagoId}", reciboPagoId);
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Envía correo de bienvenida a un nuevo usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Resultado del envío</returns>
    [HttpPost("bienvenida")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EnviarBienvenidaAsync(
        [FromQuery] string userId,
        [FromQuery] Guid empresaId)
    {
        try
        {
            var action = await _emailService.EnviarBienvenidaAsync(userId, empresaId);

            if (!action.WasSuccess)
            {
                return BadRequest(action.Message);
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo de bienvenida para usuario {UserId}", userId);
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Envía correo para restablecer contraseña
    /// Este endpoint no requiere autenticación (acceso público para usuarios que olvidaron su contraseña)
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="token">Token de restablecimiento</param>
    /// <returns>Resultado del envío</returns>
    [HttpPost("restablecer-password")]
    [AllowAnonymous]
    public async Task<IActionResult> EnviarRestablecerPasswordAsync(
        [FromQuery] string email,
        [FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Email y token son obligatorios");
            }

            var action = await _emailService.EnviarRestablecerPasswordAsync(email, token);

            if (!action.WasSuccess)
            {
                return BadRequest(action.Message);
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo de restablecimiento de contraseña para {Email}", email);
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Envía una notificación genérica
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="destinatarios">Lista de destinatarios</param>
    /// <param name="asunto">Asunto del correo</param>
    /// <param name="mensaje">Contenido del mensaje</param>
    /// <returns>Resultado del envío</returns>
    [HttpPost("notificacion")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> EnviarNotificacionAsync(
        [FromQuery] Guid empresaId,
        [FromBody] NotificacionEmailRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.Destinatarios == null || !request.Destinatarios.Any())
            {
                return BadRequest("Debe especificar al menos un destinatario");
            }

            var action = await _emailService.EnviarNotificacionAsync(
                empresaId,
                request.Destinatarios,
                request.Asunto,
                request.Mensaje);

            if (!action.WasSuccess)
            {
                return BadRequest(action.Message);
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando notificación para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida la configuración SMTP de una empresa
    /// Intenta conectar y autenticar con el servidor SMTP configurado
    /// </summary>
    /// <param name="empresaId">ID de la empresa a validar</param>
    /// <returns>Resultado de la validación</returns>
    [HttpPost("validar-smtp")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ValidarSMTPAsync([FromQuery] Guid empresaId)
    {
        try
        {
            var action = await _emailService.ValidarConfiguracionSMTPAsync(empresaId);

            if (!action.WasSuccess)
            {
                return BadRequest(action.Message);
            }

            return Ok(action.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando configuración SMTP para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }
}

/// <summary>
/// Request model para envío de notificaciones
/// </summary>
public class NotificacionEmailRequest
{
    public List<string> Destinatarios { get; set; } = new List<string>();
    public string Asunto { get; set; } = null!;
    public string Mensaje { get; set; } = null!;
}
