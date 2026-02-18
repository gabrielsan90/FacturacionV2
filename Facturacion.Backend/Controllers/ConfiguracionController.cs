using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracionController : ControllerBase
{
    private readonly IEmpresaUnitOfWork _empresaUnitOfWork;
    private readonly IUserHelper _userHelper;
    private readonly IEmailReaderService _emailReaderService;
    private readonly ILogger<ConfiguracionController> _logger;

    public ConfiguracionController(
        IEmpresaUnitOfWork empresaUnitOfWork,
        IUserHelper userHelper,
        IEmailReaderService emailReaderService,
        ILogger<ConfiguracionController> logger)
    {
        _empresaUnitOfWork = empresaUnitOfWork;
        _userHelper = userHelper;
        _emailReaderService = emailReaderService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la configuración de correo de una empresa
    /// </summary>
    [HttpGet("correo/{empresaId:guid}")]
    public async Task<IActionResult> GetConfiguracionCorreoAsync(Guid empresaId)
    {
        try
        {
            var empresa = await _empresaUnitOfWork.EmpresaRepository.GetAsync(empresaId);
            if (empresa == null || !empresa.WasSuccess || empresa.Result == null)
            {
                return NotFound("Empresa no encontrada");
            }

            var emp = empresa.Result;
            return Ok(new
            {
                smtpServer = emp.ServidorSMTP,
                smtpPort = emp.PuertoSMTP ?? 587,
                emailFrom = emp.UsuarioSMTP,
                emailPassword = string.IsNullOrEmpty(emp.ClaveSMTP) ? "" : "********", // No enviar la contraseña real
                displayName = emp.SmtpDisplayName,
                enableSsl = emp.SmtpEnableSsl,
                useDefaultCredentials = false,
                copiaFacturasEmail = emp.SmtpCopiaEmail,
                enviarPDFAdjunto = emp.SmtpEnviarPDF,
                enviarXMLAdjunto = emp.SmtpEnviarXML
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración de correo");
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Guarda la configuración de correo de una empresa
    /// </summary>
    [HttpPut("correo/{empresaId:guid}")]
    public async Task<IActionResult> GuardarConfiguracionCorreoAsync(Guid empresaId, [FromBody] ConfiguracionCorreoDTO config)
    {
        try
        {
            var empresaResult = await _empresaUnitOfWork.EmpresaRepository.GetAsync(empresaId);
            if (empresaResult == null || !empresaResult.WasSuccess || empresaResult.Result == null)
            {
                return NotFound("Empresa no encontrada");
            }

            var empresa = empresaResult.Result;

            // Actualizar campos SMTP
            empresa.ServidorSMTP = config.SmtpServer;
            empresa.PuertoSMTP = config.SmtpPort;
            empresa.UsuarioSMTP = config.EmailFrom;

            // Solo actualizar la contraseña si no es el placeholder
            if (!string.IsNullOrEmpty(config.EmailPassword) && config.EmailPassword != "********")
            {
                empresa.ClaveSMTP = config.EmailPassword;
            }

            empresa.SmtpDisplayName = config.DisplayName;
            empresa.SmtpEnableSsl = config.EnableSsl;
            empresa.SmtpCopiaEmail = config.CopiaFacturasEmail;
            empresa.SmtpEnviarPDF = config.EnviarPDFAdjunto;
            empresa.SmtpEnviarXML = config.EnviarXMLAdjunto;

            var updateResult = await _empresaUnitOfWork.EmpresaRepository.UpdateAsync(empresa);
            if (!updateResult.WasSuccess)
            {
                return BadRequest(updateResult.Message);
            }

            return Ok(new { success = true, message = "Configuración guardada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar configuración de correo");
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Prueba la conexión SMTP
    /// </summary>
    [HttpPost("correo/{empresaId:guid}/probar")]
    public async Task<IActionResult> ProbarConexionAsync(Guid empresaId)
    {
        try
        {
            var empresaResult = await _empresaUnitOfWork.EmpresaRepository.GetAsync(empresaId);
            if (empresaResult == null || !empresaResult.WasSuccess || empresaResult.Result == null)
            {
                return NotFound("Empresa no encontrada");
            }

            var empresa = empresaResult.Result;

            if (string.IsNullOrEmpty(empresa.ServidorSMTP))
            {
                return BadRequest(new { success = false, message = "No hay configuración SMTP. Guarde la configuración primero." });
            }

            using var client = new SmtpClient();

            var secureSocketOptions = empresa.SmtpEnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(
                empresa.ServidorSMTP,
                empresa.PuertoSMTP ?? 587,
                secureSocketOptions);

            if (!string.IsNullOrEmpty(empresa.UsuarioSMTP) && !string.IsNullOrEmpty(empresa.ClaveSMTP))
            {
                await client.AuthenticateAsync(empresa.UsuarioSMTP, empresa.ClaveSMTP);
            }

            await client.DisconnectAsync(true);

            return Ok(new { success = true, message = "Conexión exitosa al servidor SMTP" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al probar conexión SMTP");
            return Ok(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Envía un correo de prueba
    /// </summary>
    [HttpPost("correo/{empresaId:guid}/enviar-prueba")]
    public async Task<IActionResult> EnviarCorreoPruebaAsync(Guid empresaId, [FromBody] CorreoPruebaDTO testData)
    {
        try
        {
            var empresaResult = await _empresaUnitOfWork.EmpresaRepository.GetAsync(empresaId);
            if (empresaResult == null || !empresaResult.WasSuccess || empresaResult.Result == null)
            {
                return NotFound("Empresa no encontrada");
            }

            var empresa = empresaResult.Result;

            if (string.IsNullOrEmpty(empresa.ServidorSMTP) || string.IsNullOrEmpty(empresa.UsuarioSMTP))
            {
                return BadRequest(new { success = false, message = "Configuración SMTP incompleta. Guarde la configuración primero." });
            }

            // Crear mensaje
            var message = new MimeMessage();

            var fromName = !string.IsNullOrEmpty(empresa.SmtpDisplayName)
                ? empresa.SmtpDisplayName
                : empresa.NombreComercial;

            message.From.Add(new MailboxAddress(fromName, empresa.UsuarioSMTP));
            message.To.Add(new MailboxAddress(testData.Destinatario, testData.Destinatario));
            message.Subject = testData.Asunto ?? "Correo de Prueba - Sistema Facturación";

            message.Body = new TextPart("plain")
            {
                Text = testData.Mensaje ?? "Este es un correo de prueba del sistema de facturación electrónica."
            };

            // Enviar
            using var client = new SmtpClient();

            var secureSocketOptions = empresa.SmtpEnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(
                empresa.ServidorSMTP,
                empresa.PuertoSMTP ?? 587,
                secureSocketOptions);

            if (!string.IsNullOrEmpty(empresa.UsuarioSMTP) && !string.IsNullOrEmpty(empresa.ClaveSMTP))
            {
                await client.AuthenticateAsync(empresa.UsuarioSMTP, empresa.ClaveSMTP);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Correo de prueba enviado exitosamente a {Destinatario}", testData.Destinatario);

            return Ok(new { success = true, message = "Correo de prueba enviado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo de prueba");
            return Ok(new { success = false, message = ex.Message });
        }
    }

    // ========================================
    // IMAP CONFIGURATION ENDPOINTS
    // ========================================

    /// <summary>
    /// Obtiene la configuración IMAP de una empresa
    /// </summary>
    [HttpGet("imap/{empresaId:guid}")]
    public async Task<IActionResult> GetConfiguracionImapAsync(Guid empresaId)
    {
        try
        {
            var empresa = await _empresaUnitOfWork.EmpresaRepository.GetAsync(empresaId);
            if (empresa == null || !empresa.WasSuccess || empresa.Result == null)
            {
                return NotFound("Empresa no encontrada");
            }

            var emp = empresa.Result;
            return Ok(new
            {
                servidorIMAP = emp.ServidorIMAP,
                puertoIMAP = emp.PuertoIMAP ?? 993,
                usuarioIMAP = emp.UsuarioIMAP,
                claveIMAP = string.IsNullOrEmpty(emp.ClaveIMAP) ? "" : "********",
                imapEnableSsl = emp.ImapEnableSsl,
                carpetaIMAP = emp.CarpetaIMAP ?? "INBOX"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración IMAP");
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Guarda la configuración IMAP de una empresa
    /// </summary>
    [HttpPut("imap/{empresaId:guid}")]
    public async Task<IActionResult> GuardarConfiguracionImapAsync(Guid empresaId, [FromBody] ConfiguracionImapDTO config)
    {
        try
        {
            var empresaResult = await _empresaUnitOfWork.EmpresaRepository.GetAsync(empresaId);
            if (empresaResult == null || !empresaResult.WasSuccess || empresaResult.Result == null)
            {
                return NotFound("Empresa no encontrada");
            }

            var empresa = empresaResult.Result;

            empresa.ServidorIMAP = config.ServidorIMAP;
            empresa.PuertoIMAP = config.PuertoIMAP;
            empresa.UsuarioIMAP = config.UsuarioIMAP;

            if (!string.IsNullOrEmpty(config.ClaveIMAP) && config.ClaveIMAP != "********")
            {
                empresa.ClaveIMAP = config.ClaveIMAP;
            }

            empresa.ImapEnableSsl = config.ImapEnableSsl;
            empresa.CarpetaIMAP = config.CarpetaIMAP;

            var updateResult = await _empresaUnitOfWork.EmpresaRepository.UpdateAsync(empresa);
            if (!updateResult.WasSuccess)
            {
                return BadRequest(updateResult.Message);
            }

            return Ok(new { success = true, message = "Configuración IMAP guardada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar configuración IMAP");
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Prueba la conexión IMAP
    /// </summary>
    [HttpPost("imap/{empresaId:guid}/probar")]
    public async Task<IActionResult> ProbarConexionImapAsync(Guid empresaId)
    {
        try
        {
            var resultado = await _emailReaderService.ProbarConexionImapAsync(empresaId);
            return Ok(new { success = resultado.WasSuccess, message = resultado.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al probar conexión IMAP");
            return Ok(new { success = false, message = ex.Message });
        }
    }
}

/// <summary>
/// DTO para configuración IMAP
/// </summary>
public class ConfiguracionImapDTO
{
    public string? ServidorIMAP { get; set; }
    public int PuertoIMAP { get; set; } = 993;
    public string? UsuarioIMAP { get; set; }
    public string? ClaveIMAP { get; set; }
    public bool ImapEnableSsl { get; set; } = true;
    public string? CarpetaIMAP { get; set; } = "INBOX";
}

/// <summary>
/// DTO para configuración de correo
/// </summary>
public class ConfiguracionCorreoDTO
{
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? EmailFrom { get; set; }
    public string? EmailPassword { get; set; }
    public string? DisplayName { get; set; }
    public bool EnableSsl { get; set; } = true;
    public bool UseDefaultCredentials { get; set; }
    public string? CopiaFacturasEmail { get; set; }
    public bool EnviarPDFAdjunto { get; set; } = true;
    public bool EnviarXMLAdjunto { get; set; } = true;
}

/// <summary>
/// DTO para enviar correo de prueba
/// </summary>
public class CorreoPruebaDTO
{
    public string Destinatario { get; set; } = null!;
    public string? Asunto { get; set; }
    public string? Mensaje { get; set; }
}
