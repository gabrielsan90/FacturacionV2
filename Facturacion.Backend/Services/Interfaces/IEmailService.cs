using Facturacion.Shared.DTOs;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para envío de correos electrónicos usando MailKit
/// Soporta múltiples plantillas HTML y envío con adjuntos
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un correo electrónico genérico
    /// </summary>
    /// <param name="empresaId">ID de la empresa (para obtener configuración SMTP)</param>
    /// <param name="emailDTO">Datos del correo a enviar</param>
    /// <returns>Resultado del envío</returns>
    Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarEmailAsync(Guid empresaId, EmailDTO emailDTO);

    /// <summary>
    /// Envía un documento electrónico (factura, tiquete, etc.) por correo
    /// Incluye XML y PDF adjuntos
    /// </summary>
    /// <param name="documentoId">ID del documento a enviar</param>
    /// <param name="emailsAdicionales">Emails adicionales además del cliente</param>
    /// <param name="mensaje">Mensaje personalizado opcional</param>
    /// <returns>Resultado del envío</returns>
    Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarDocumentoElectronicoAsync(
        Guid documentoId,
        List<string>? emailsAdicionales = null,
        string? mensaje = null);

    /// <summary>
    /// Envía un recibo de pago electrónico por correo
    /// </summary>
    /// <param name="reciboPagoId">ID del recibo de pago</param>
    /// <param name="emailsAdicionales">Emails adicionales</param>
    /// <returns>Resultado del envío</returns>
    Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarReciboPagoAsync(
        Guid reciboPagoId,
        List<string>? emailsAdicionales = null);

    /// <summary>
    /// Envía correo de bienvenida a nuevo usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Resultado del envío</returns>
    Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarBienvenidaAsync(string userId, Guid empresaId);

    /// <summary>
    /// Envía correo para restablecer contraseña
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="token">Token de restablecimiento</param>
    /// <returns>Resultado del envío</returns>
    Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarRestablecerPasswordAsync(string email, string token);

    /// <summary>
    /// Envía una notificación genérica
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="destinatarios">Lista de destinatarios</param>
    /// <param name="asunto">Asunto del correo</param>
    /// <param name="mensaje">Contenido del mensaje</param>
    /// <returns>Resultado del envío</returns>
    Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarNotificacionAsync(
        Guid empresaId,
        List<string> destinatarios,
        string asunto,
        string mensaje);

    /// <summary>
    /// Valida la configuración SMTP de una empresa
    /// Intenta conectar y autenticar con el servidor SMTP
    /// </summary>
    /// <param name="empresaId">ID de la empresa a validar</param>
    /// <returns>Resultado de la validación</returns>
    Task<ActionResponse<ResultadoEnvioEmailDTO>> ValidarConfiguracionSMTPAsync(Guid empresaId);
}
