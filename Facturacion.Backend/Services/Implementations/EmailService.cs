using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Text;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de envío de correos electrónicos usando MailKit
/// Soporta múltiples plantillas HTML, adjuntos y configuración SMTP multi-tenant
/// </summary>
public class EmailService : IEmailService
{
    private readonly DataContext _context;
    private readonly IXmlGeneradorService _xmlGeneradorService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        DataContext context,
        IXmlGeneradorService xmlGeneradorService,
        ILogger<EmailService> logger)
    {
        _context = context;
        _xmlGeneradorService = xmlGeneradorService;
        _logger = logger;
    }

    #region Métodos Públicos

    public async Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarEmailAsync(Guid empresaId, EmailDTO emailDTO)
    {
        try
        {
            var empresa = await ObtenerEmpresaConSMTPAsync(empresaId);
            if (empresa == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Empresa no encontrada"
                };
            }

            var validacionSMTP = ValidarConfiguracionSMTP(empresa);
            if (!validacionSMTP.WasSuccess)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = validacionSMTP.Message
                };
            }

            var resultado = await EnviarEmailInternoAsync(empresa, emailDTO);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = resultado.Exitoso,
                Message = resultado.Mensaje,
                Result = resultado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email genérico para empresa {EmpresaId}", empresaId);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = false,
                Message = $"Error enviando correo: {ex.Message}"
            };
        }
    }

    public async Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarDocumentoElectronicoAsync(
        Guid documentoId,
        List<string>? emailsAdicionales = null,
        string? mensaje = null)
    {
        try
        {
            // Cargar documento con todas las relaciones necesarias
            var documento = await _context.Set<Documento>()
                .Include(d => d.Empresa)
                .Include(d => d.Cliente)
                    .ThenInclude(c => c!.Emails)
                .Include(d => d.Detalles)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == documentoId);

            if (documento == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Documento no encontrado"
                };
            }

            if (documento.Empresa == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Empresa del documento no encontrada"
                };
            }

            var validacionSMTP = ValidarConfiguracionSMTP(documento.Empresa);
            if (!validacionSMTP.WasSuccess)
            {
                return validacionSMTP;
            }

            // Obtener emails del cliente
            var destinatarios = new List<string>();
            if (documento.Cliente?.Emails != null && documento.Cliente.Emails.Any())
            {
                destinatarios.AddRange(documento.Cliente.Emails.Select(e => e.DireccionEmail));
            }

            // Agregar emails adicionales
            if (emailsAdicionales != null && emailsAdicionales.Any())
            {
                destinatarios.AddRange(emailsAdicionales);
            }

            if (!destinatarios.Any())
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "No hay destinatarios de correo. El cliente no tiene emails registrados y no se proporcionaron emails adicionales."
                };
            }

            // Generar XML del documento
            var xmlContent = await _xmlGeneradorService.GenerarXmlAsync(documento);
            if (string.IsNullOrEmpty(xmlContent))
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Error generando XML del documento"
                };
            }

            // Preparar adjuntos
            var adjuntos = new List<AdjuntoEmailDTO>();

            // XML del documento
            var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlContent));
            adjuntos.Add(new AdjuntoEmailDTO
            {
                NombreArchivo = $"{documento.Clave}.xml",
                ContenidoBase64 = xmlBase64,
                TipoContenido = "application/xml"
            });

            // Generar representación PDF simple (texto HTML convertido)
            var pdfHtml = GenerarPDFSimpleDocumento(documento);
            var pdfBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(pdfHtml));
            adjuntos.Add(new AdjuntoEmailDTO
            {
                NombreArchivo = $"{documento.Clave}.html",
                ContenidoBase64 = pdfBase64,
                TipoContenido = "text/html"
            });

            // Crear cuerpo del email con plantilla
            var cuerpoHtml = GetPlantillaDocumentoElectronico(documento, mensaje);

            var emailDTO = new EmailDTO
            {
                Para = destinatarios,
                Asunto = $"Documento Electrónico #{documento.NumeroConsecutivo} - {documento.Empresa.NombreComercial}",
                Cuerpo = cuerpoHtml,
                Adjuntos = adjuntos
            };

            var resultado = await EnviarEmailInternoAsync(documento.Empresa, emailDTO);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = resultado.Exitoso,
                Message = resultado.Mensaje,
                Result = resultado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando documento electrónico {DocumentoId}", documentoId);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = false,
                Message = $"Error enviando documento: {ex.Message}"
            };
        }
    }

    public async Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarReciboPagoAsync(
        Guid reciboPagoId,
        List<string>? emailsAdicionales = null)
    {
        try
        {
            var reciboPago = await _context.Set<ReciboPago>()
                .Include(r => r.Documento)
                    .ThenInclude(d => d!.Empresa)
                .Include(r => r.DocumentoOriginal)
                    .ThenInclude(d => d!.Cliente)
                        .ThenInclude(c => c!.Emails)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reciboPagoId);

            if (reciboPago == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Recibo de pago no encontrado"
                };
            }

            if (reciboPago.Documento?.Empresa == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Empresa del recibo no encontrada"
                };
            }

            var empresa = reciboPago.Documento.Empresa;
            var validacionSMTP = ValidarConfiguracionSMTP(empresa);
            if (!validacionSMTP.WasSuccess)
            {
                return validacionSMTP;
            }

            // Obtener emails del cliente del documento original
            var destinatarios = new List<string>();
            if (reciboPago.DocumentoOriginal?.Cliente?.Emails != null && reciboPago.DocumentoOriginal.Cliente.Emails.Any())
            {
                destinatarios.AddRange(reciboPago.DocumentoOriginal.Cliente.Emails.Select(e => e.DireccionEmail));
            }

            if (emailsAdicionales != null && emailsAdicionales.Any())
            {
                destinatarios.AddRange(emailsAdicionales);
            }

            if (!destinatarios.Any())
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "No hay destinatarios de correo"
                };
            }

            var cuerpoHtml = GetPlantillaReciboPago(reciboPago);

            var emailDTO = new EmailDTO
            {
                Para = destinatarios,
                Asunto = $"Recibo de Pago - {empresa.NombreComercial}",
                Cuerpo = cuerpoHtml
            };

            var resultado = await EnviarEmailInternoAsync(empresa, emailDTO);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = resultado.Exitoso,
                Message = resultado.Mensaje,
                Result = resultado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando recibo de pago {ReciboPagoId}", reciboPagoId);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = false,
                Message = $"Error enviando recibo: {ex.Message}"
            };
        }
    }

    public async Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarBienvenidaAsync(string userId, Guid empresaId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Usuario no encontrado"
                };
            }

            var empresa = await ObtenerEmpresaConSMTPAsync(empresaId);
            if (empresa == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Empresa no encontrada"
                };
            }

            var validacionSMTP = ValidarConfiguracionSMTP(empresa);
            if (!validacionSMTP.WasSuccess)
            {
                return validacionSMTP;
            }

            var cuerpoHtml = GetPlantillaBienvenida(user, empresa);

            var emailDTO = new EmailDTO
            {
                Para = new List<string> { user.Email! },
                Asunto = $"Bienvenido a {empresa.NombreComercial}",
                Cuerpo = cuerpoHtml
            };

            var resultado = await EnviarEmailInternoAsync(empresa, emailDTO);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = resultado.Exitoso,
                Message = resultado.Mensaje,
                Result = resultado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo de bienvenida para usuario {UserId}", userId);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = false,
                Message = $"Error enviando correo de bienvenida: {ex.Message}"
            };
        }
    }

    public async Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarRestablecerPasswordAsync(string email, string token)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Usuario no encontrado"
                };
            }

            // Obtener empresa por defecto del usuario (primera empresa asociada)
            var empresaUsuario = await _context.Set<UsuarioEmpresa>()
                .Include(ue => ue.Empresa)
                .FirstOrDefaultAsync(ue => ue.UserId == user.Id);

            Empresa? empresa = null;
            if (empresaUsuario?.Empresa != null)
            {
                empresa = empresaUsuario.Empresa;
            }
            else
            {
                // Si el usuario no tiene empresa, usar la primera empresa activa del sistema
                empresa = await _context.Set<Empresa>()
                    .Where(e => e.Activa && !e.IsDeleted)
                    .FirstOrDefaultAsync();
            }

            if (empresa == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "No se encontró una empresa válida para enviar el correo"
                };
            }

            var validacionSMTP = ValidarConfiguracionSMTP(empresa);
            if (!validacionSMTP.WasSuccess)
            {
                return validacionSMTP;
            }

            var cuerpoHtml = GetPlantillaRestablecerPassword(user, token);

            var emailDTO = new EmailDTO
            {
                Para = new List<string> { email },
                Asunto = "Restablecer Contraseña",
                Cuerpo = cuerpoHtml
            };

            var resultado = await EnviarEmailInternoAsync(empresa, emailDTO);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = resultado.Exitoso,
                Message = resultado.Mensaje,
                Result = resultado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo de restablecimiento de contraseña para {Email}", email);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = false,
                Message = $"Error enviando correo: {ex.Message}"
            };
        }
    }

    public async Task<ActionResponse<ResultadoEnvioEmailDTO>> EnviarNotificacionAsync(
        Guid empresaId,
        List<string> destinatarios,
        string asunto,
        string mensaje)
    {
        try
        {
            var empresa = await ObtenerEmpresaConSMTPAsync(empresaId);
            if (empresa == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Empresa no encontrada"
                };
            }

            var validacionSMTP = ValidarConfiguracionSMTP(empresa);
            if (!validacionSMTP.WasSuccess)
            {
                return validacionSMTP;
            }

            var cuerpoHtml = GetPlantillaNotificacion(empresa, asunto, mensaje);

            var emailDTO = new EmailDTO
            {
                Para = destinatarios,
                Asunto = asunto,
                Cuerpo = cuerpoHtml
            };

            var resultado = await EnviarEmailInternoAsync(empresa, emailDTO);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = resultado.Exitoso,
                Message = resultado.Mensaje,
                Result = resultado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando notificación para empresa {EmpresaId}", empresaId);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = false,
                Message = $"Error enviando notificación: {ex.Message}"
            };
        }
    }

    public async Task<ActionResponse<ResultadoEnvioEmailDTO>> ValidarConfiguracionSMTPAsync(Guid empresaId)
    {
        try
        {
            var empresa = await ObtenerEmpresaConSMTPAsync(empresaId);
            if (empresa == null)
            {
                return new ActionResponse<ResultadoEnvioEmailDTO>
                {
                    WasSuccess = false,
                    Message = "Empresa no encontrada"
                };
            }

            var validacion = ValidarConfiguracionSMTP(empresa);
            if (!validacion.WasSuccess)
            {
                return validacion;
            }

            // Intentar conectar y autenticar
            using var client = new SmtpClient();

            await client.ConnectAsync(
                empresa.ServidorSMTP!,
                empresa.PuertoSMTP ?? 587,
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(empresa.UsuarioSMTP!, empresa.ClaveSMTP!);

            await client.DisconnectAsync(true);

            var resultado = new ResultadoEnvioEmailDTO
            {
                Exitoso = true,
                Mensaje = "Configuración SMTP válida. Conexión exitosa.",
                FechaEnvio = FechaCostaRicaHelper.Ahora,
                EmailsEnviados = new List<string>(),
                Errores = new List<string>()
            };

            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = true,
                Message = "Configuración SMTP válida",
                Result = resultado
            };
        }
        catch (MailKit.Security.AuthenticationException ex)
        {
            _logger.LogWarning(ex, "Error de autenticación SMTP para empresa {EmpresaId}", empresaId);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = false,
                Message = $"Error de autenticación: {ex.Message}. Verifique usuario y contraseña SMTP."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando configuración SMTP para empresa {EmpresaId}", empresaId);
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = false,
                Message = $"Error validando SMTP: {ex.Message}"
            };
        }
    }

    #endregion

    #region Métodos Privados

    private async Task<Empresa?> ObtenerEmpresaConSMTPAsync(Guid empresaId)
    {
        return await _context.Set<Empresa>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empresaId && e.Activa && !e.IsDeleted);
    }

    private ActionResponse<ResultadoEnvioEmailDTO> ValidarConfiguracionSMTP(Empresa empresa)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(empresa.ServidorSMTP))
            errores.Add("Servidor SMTP no configurado");

        if (!empresa.PuertoSMTP.HasValue || empresa.PuertoSMTP.Value <= 0)
            errores.Add("Puerto SMTP no configurado");

        if (string.IsNullOrWhiteSpace(empresa.UsuarioSMTP))
            errores.Add("Usuario SMTP no configurado");

        if (string.IsNullOrWhiteSpace(empresa.ClaveSMTP))
            errores.Add("Contraseña SMTP no configurada");

        if (errores.Any())
        {
            return new ActionResponse<ResultadoEnvioEmailDTO>
            {
                WasSuccess = false,
                Message = $"Configuración SMTP incompleta: {string.Join(", ", errores)}"
            };
        }

        return new ActionResponse<ResultadoEnvioEmailDTO>
        {
            WasSuccess = true,
            Message = "Configuración SMTP válida"
        };
    }

    private async Task<ResultadoEnvioEmailDTO> EnviarEmailInternoAsync(Empresa empresa, EmailDTO emailDTO)
    {
        var resultado = new ResultadoEnvioEmailDTO
        {
            Exitoso = false,
            Mensaje = "",
            FechaEnvio = null,
            EmailsEnviados = new List<string>(),
            Errores = new List<string>()
        };

        try
        {
            // Crear mensaje
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(empresa.NombreComercial, empresa.UsuarioSMTP));

            // Agregar destinatarios
            foreach (var destinatario in emailDTO.Para)
            {
                try
                {
                    message.To.Add(MailboxAddress.Parse(destinatario));
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add($"Email inválido '{destinatario}': {ex.Message}");
                }
            }

            // Agregar CC
            if (emailDTO.CC != null)
            {
                foreach (var cc in emailDTO.CC)
                {
                    try
                    {
                        message.Cc.Add(MailboxAddress.Parse(cc));
                    }
                    catch (Exception ex)
                    {
                        resultado.Errores.Add($"Email CC inválido '{cc}': {ex.Message}");
                    }
                }
            }

            // Agregar CCO
            if (emailDTO.CCO != null)
            {
                foreach (var cco in emailDTO.CCO)
                {
                    try
                    {
                        message.Bcc.Add(MailboxAddress.Parse(cco));
                    }
                    catch (Exception ex)
                    {
                        resultado.Errores.Add($"Email CCO inválido '{cco}': {ex.Message}");
                    }
                }
            }

            if (!message.To.Any() && !message.Cc.Any() && !message.Bcc.Any())
            {
                resultado.Mensaje = "No hay destinatarios válidos";
                return resultado;
            }

            message.Subject = emailDTO.Asunto;

            // Construir cuerpo con adjuntos
            var builder = new BodyBuilder
            {
                HtmlBody = emailDTO.Cuerpo
            };

            // Agregar adjuntos
            if (emailDTO.Adjuntos != null && emailDTO.Adjuntos.Any())
            {
                foreach (var adjunto in emailDTO.Adjuntos)
                {
                    try
                    {
                        var bytes = Convert.FromBase64String(adjunto.ContenidoBase64);
                        builder.Attachments.Add(
                            adjunto.NombreArchivo,
                            bytes,
                            ContentType.Parse(adjunto.TipoContenido));
                    }
                    catch (Exception ex)
                    {
                        resultado.Errores.Add($"Error agregando adjunto '{adjunto.NombreArchivo}': {ex.Message}");
                    }
                }
            }

            message.Body = builder.ToMessageBody();

            // Enviar
            using var client = new SmtpClient();

            await client.ConnectAsync(
                empresa.ServidorSMTP!,
                empresa.PuertoSMTP ?? 587,
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(empresa.UsuarioSMTP!, empresa.ClaveSMTP!);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);

            // Registrar éxito
            resultado.Exitoso = true;
            resultado.FechaEnvio = FechaCostaRicaHelper.Ahora;
            resultado.Mensaje = "Correo enviado exitosamente";
            resultado.EmailsEnviados = emailDTO.Para.ToList();

            if (emailDTO.CC != null)
                resultado.EmailsEnviados.AddRange(emailDTO.CC);

            if (emailDTO.CCO != null)
                resultado.EmailsEnviados.AddRange(emailDTO.CCO);

            _logger.LogInformation("Email enviado exitosamente a {Count} destinatarios desde {Empresa}",
                resultado.EmailsEnviados.Count, empresa.NombreComercial);

            return resultado;
        }
        catch (MailKit.Net.Smtp.SmtpCommandException ex)
        {
            resultado.Errores.Add($"Error SMTP: {ex.Message}");
            resultado.Mensaje = $"Error de comando SMTP: {ex.Message}";
            _logger.LogError(ex, "Error SMTP enviando email desde {Empresa}", empresa.NombreComercial);
            return resultado;
        }
        catch (MailKit.Net.Smtp.SmtpProtocolException ex)
        {
            resultado.Errores.Add($"Error de protocolo SMTP: {ex.Message}");
            resultado.Mensaje = $"Error de protocolo SMTP: {ex.Message}";
            _logger.LogError(ex, "Error de protocolo SMTP desde {Empresa}", empresa.NombreComercial);
            return resultado;
        }
        catch (MailKit.Security.AuthenticationException ex)
        {
            resultado.Errores.Add($"Error de autenticación: {ex.Message}");
            resultado.Mensaje = $"Error de autenticación SMTP: Verifique usuario y contraseña";
            _logger.LogError(ex, "Error de autenticación SMTP desde {Empresa}", empresa.NombreComercial);
            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Errores.Add($"Error general: {ex.Message}");
            resultado.Mensaje = $"Error enviando correo: {ex.Message}";
            _logger.LogError(ex, "Error general enviando email desde {Empresa}", empresa.NombreComercial);
            return resultado;
        }
    }

    #endregion

    #region Plantillas HTML

    private string GetPlantillaDocumentoElectronico(Documento documento, string? mensajePersonalizado)
    {
        var tipoDoc = documento.TipoDocumento.ToString();
        var total = documento.Detalles?.Sum(d => d.MontoTotalLinea) ?? 0;

        return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Documento Electrónico</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 30px;
        }}
        .info-box {{
            background-color: #f8f9fa;
            border-left: 4px solid #667eea;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .info-row {{
            display: flex;
            justify-content: space-between;
            margin: 10px 0;
            padding: 8px 0;
            border-bottom: 1px solid #e9ecef;
        }}
        .info-row:last-child {{
            border-bottom: none;
        }}
        .label {{
            font-weight: bold;
            color: #495057;
        }}
        .value {{
            color: #212529;
        }}
        .message-box {{
            background-color: #e3f2fd;
            border-left: 4px solid #2196F3;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .btn {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
            text-align: center;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #6c757d;
            font-size: 12px;
        }}
        .attachments {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .attachment-icon {{
            display: inline-block;
            margin-right: 5px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📄 Documento Electrónico</h1>
            <p>{documento.Empresa?.NombreComercial}</p>
        </div>

        <div class='content'>
            <p>Estimado/a <strong>{documento.Cliente?.NombreComercial ?? "Cliente"}</strong>,</p>

            <p>Se adjunta el siguiente documento electrónico:</p>

            <div class='info-box'>
                <div class='info-row'>
                    <span class='label'>Tipo:</span>
                    <span class='value'>{tipoDoc}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Número:</span>
                    <span class='value'>{documento.NumeroConsecutivo}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Fecha:</span>
                    <span class='value'>{documento.FechaEmision:dd/MM/yyyy HH:mm}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Monto Total:</span>
                    <span class='value'>₡{total:N2}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Clave:</span>
                    <span class='value' style='font-size: 11px; word-break: break-all;'>{documento.Clave}</span>
                </div>
            </div>

            {(string.IsNullOrWhiteSpace(mensajePersonalizado) ? "" : $@"
            <div class='message-box'>
                <p style='margin: 0;'><strong>Mensaje:</strong></p>
                <p style='margin: 10px 0 0 0;'>{mensajePersonalizado}</p>
            </div>
            ")}

            <div class='attachments'>
                <p style='margin: 0 0 10px 0;'><strong>📎 Archivos adjuntos:</strong></p>
                <p style='margin: 5px 0;'><span class='attachment-icon'>📄</span> {documento.Clave}.xml (Documento XML)</p>
                <p style='margin: 5px 0;'><span class='attachment-icon'>📋</span> {documento.Clave}.html (Representación HTML)</p>
            </div>

            <p style='margin-top: 30px;'>Si tiene alguna consulta sobre este documento, no dude en contactarnos.</p>
        </div>

        <div class='footer'>
            <p><strong>{documento.Empresa?.NombreComercial}</strong></p>
            <p>{documento.Empresa?.RazonSocial}</p>
            <p>Identificación: {documento.Empresa?.NumeroIdentificacion}</p>
            <p style='margin-top: 15px; font-size: 10px;'>
                Este es un correo electrónico automático, por favor no responder.<br>
                Documento generado por Sistema de Facturación Electrónica Costa Rica
            </p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPlantillaReciboPago(ReciboPago reciboPago)
    {
        var empresa = reciboPago.Documento?.Empresa;
        var cliente = reciboPago.DocumentoOriginal?.Cliente;
        var monedaSimbolo = reciboPago.Moneda == Shared.Enums.TipoMoneda.CRC ? "₡" : "$";

        return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Recibo de Pago</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 30px;
        }}
        .info-box {{
            background-color: #f8f9fa;
            border-left: 4px solid #11998e;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .info-row {{
            display: flex;
            justify-content: space-between;
            margin: 10px 0;
            padding: 8px 0;
            border-bottom: 1px solid #e9ecef;
        }}
        .info-row:last-child {{
            border-bottom: none;
        }}
        .label {{
            font-weight: bold;
            color: #495057;
        }}
        .value {{
            color: #212529;
        }}
        .total-box {{
            background-color: #d4edda;
            border: 2px solid #28a745;
            padding: 20px;
            margin: 20px 0;
            border-radius: 5px;
            text-align: center;
        }}
        .total-amount {{
            font-size: 32px;
            font-weight: bold;
            color: #155724;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #6c757d;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>💰 Recibo de Pago</h1>
            <p>{empresa?.NombreComercial}</p>
        </div>

        <div class='content'>
            <p>Estimado/a <strong>{cliente?.NombreComercial ?? "Cliente"}</strong>,</p>

            <p>Hemos recibido su pago. Gracias por su confianza.</p>

            <div class='info-box'>
                <div class='info-row'>
                    <span class='label'>Documento Original:</span>
                    <span class='value'>{reciboPago.NumeroConsecutivoOriginal}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Fecha de Pago:</span>
                    <span class='value'>{reciboPago.FechaPago:dd/MM/yyyy HH:mm}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Moneda:</span>
                    <span class='value'>{reciboPago.Moneda}</span>
                </div>
            </div>

            <div class='total-box'>
                <p style='margin: 0 0 10px 0; color: #155724;'>Monto Pagado</p>
                <p class='total-amount'>{monedaSimbolo}{reciboPago.MontoPagado:N2}</p>
            </div>

            <div class='info-box'>
                <div class='info-row'>
                    <span class='label'>Saldo Pendiente:</span>
                    <span class='value'>{monedaSimbolo}{reciboPago.SaldoPendiente:N2}</span>
                </div>
            </div>

            <p style='margin-top: 30px;'>Gracias por su pago. Si tiene alguna consulta, no dude en contactarnos.</p>
        </div>

        <div class='footer'>
            <p><strong>{empresa?.NombreComercial}</strong></p>
            <p>Identificación: {empresa?.NumeroIdentificacion}</p>
            <p style='margin-top: 15px; font-size: 10px;'>
                Este es un correo electrónico automático, por favor no responder.
            </p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPlantillaBienvenida(User user, Empresa empresa)
    {
        return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bienvenido</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #FA8BFF 0%, #2BD2FF 52%, #2BFF88 90%);
            color: white;
            padding: 40px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
        }}
        .content {{
            padding: 30px;
        }}
        .welcome-box {{
            background-color: #f8f9fa;
            border-left: 4px solid #2BD2FF;
            padding: 20px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .btn {{
            display: inline-block;
            background: linear-gradient(135deg, #FA8BFF 0%, #2BD2FF 100%);
            color: white;
            padding: 15px 40px;
            text-decoration: none;
            border-radius: 25px;
            font-weight: bold;
            margin: 20px 0;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #6c757d;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 ¡Bienvenido!</h1>
            <p style='font-size: 18px; margin-top: 10px;'>Nos alegra tenerte con nosotros</p>
        </div>

        <div class='content'>
            <p>Hola <strong>{user.FullName}</strong>,</p>

            <p>¡Bienvenido a <strong>{empresa.NombreComercial}</strong>!</p>

            <div class='welcome-box'>
                <p style='margin: 0;'>Tu cuenta ha sido creada exitosamente.</p>
                <p style='margin: 10px 0 0 0;'><strong>Usuario:</strong> {user.Email}</p>
            </div>

            <p>Ya puedes acceder al sistema de facturación electrónica y comenzar a utilizar todas las funcionalidades disponibles.</p>

            <div style='text-align: center; margin: 30px 0;'>
                <a href='#' class='btn'>Acceder al Sistema</a>
            </div>

            <p>Si tienes alguna pregunta o necesitas ayuda, no dudes en contactarnos.</p>
        </div>

        <div class='footer'>
            <p><strong>{empresa.NombreComercial}</strong></p>
            <p style='margin-top: 15px; font-size: 10px;'>
                Este es un correo electrónico automático, por favor no responder.
            </p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPlantillaRestablecerPassword(User user, string token)
    {
        // En producción, esto debería ser la URL del frontend
        var resetUrl = $"https://localhost:7031/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email!)}";

        return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Restablecer Contraseña</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 30px;
        }}
        .warning-box {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .btn {{
            display: inline-block;
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
            padding: 15px 40px;
            text-decoration: none;
            border-radius: 25px;
            font-weight: bold;
            margin: 20px 0;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #6c757d;
            font-size: 12px;
        }}
        .token-box {{
            background-color: #f8f9fa;
            border: 1px dashed #6c757d;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
            font-family: 'Courier New', monospace;
            word-break: break-all;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Restablecer Contraseña</h1>
        </div>

        <div class='content'>
            <p>Hola <strong>{user.FullName}</strong>,</p>

            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>

            <div style='text-align: center; margin: 30px 0;'>
                <a href='{resetUrl}' class='btn'>Restablecer Contraseña</a>
            </div>

            <div class='warning-box'>
                <p style='margin: 0;'><strong>⚠️ Importante:</strong></p>
                <p style='margin: 10px 0 0 0;'>
                    Este enlace expirará en 24 horas por razones de seguridad.
                    Si no solicitaste este cambio, puedes ignorar este correo.
                </p>
            </div>

            <p style='font-size: 12px; color: #6c757d; margin-top: 30px;'>
                Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:
            </p>
            <div class='token-box'>
                {resetUrl}
            </div>
        </div>

        <div class='footer'>
            <p>Sistema de Facturación Electrónica Costa Rica</p>
            <p style='margin-top: 15px; font-size: 10px;'>
                Este es un correo electrónico automático, por favor no responder.
            </p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPlantillaNotificacion(Empresa empresa, string asunto, string mensaje)
    {
        return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Notificación</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 30px;
        }}
        .notification-box {{
            background-color: #e3f2fd;
            border-left: 4px solid #2196F3;
            padding: 20px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #6c757d;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔔 {asunto}</h1>
            <p>{empresa.NombreComercial}</p>
        </div>

        <div class='content'>
            <div class='notification-box'>
                {mensaje}
            </div>

            <p style='margin-top: 30px;'>Si tiene alguna consulta, no dude en contactarnos.</p>
        </div>

        <div class='footer'>
            <p><strong>{empresa.NombreComercial}</strong></p>
            <p>Identificación: {empresa.NumeroIdentificacion}</p>
            <p style='margin-top: 15px; font-size: 10px;'>
                Este es un correo electrónico automático, por favor no responder.
            </p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerarPDFSimpleDocumento(Documento documento)
    {
        var tipoDoc = documento.TipoDocumento.ToString();
        var total = documento.Detalles?.Sum(d => d.MontoTotalLinea) ?? 0;
        var detallesHtml = new StringBuilder();

        if (documento.Detalles != null && documento.Detalles.Any())
        {
            foreach (var detalle in documento.Detalles)
            {
                detallesHtml.AppendLine($@"
                <tr>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{detalle.Cantidad}</td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{detalle.Descripcion}</td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>₡{detalle.PrecioUnitario:N2}</td>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>₡{detalle.MontoTotalLinea:N2}</td>
                </tr>");
            }
        }

        return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <title>{tipoDoc} - {documento.NumeroConsecutivo}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .info {{ margin-bottom: 20px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ background-color: #667eea; color: white; padding: 10px; text-align: left; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>{documento.Empresa?.NombreComercial}</h2>
        <p>{documento.Empresa?.RazonSocial}</p>
        <p>Identificación: {documento.Empresa?.NumeroIdentificacion}</p>
        <h3>{tipoDoc}</h3>
        <p>No. {documento.NumeroConsecutivo}</p>
    </div>

    <div class='info'>
        <p><strong>Cliente:</strong> {documento.Cliente?.NombreComercial}</p>
        <p><strong>Identificación:</strong> {documento.Cliente?.NumeroIdentificacion}</p>
        <p><strong>Fecha:</strong> {documento.FechaEmision:dd/MM/yyyy HH:mm}</p>
        <p><strong>Clave:</strong> {documento.Clave}</p>
    </div>

    <table>
        <thead>
            <tr>
                <th style='border: 1px solid #ddd;'>Cantidad</th>
                <th style='border: 1px solid #ddd;'>Descripción</th>
                <th style='border: 1px solid #ddd;'>Precio Unit.</th>
                <th style='border: 1px solid #ddd;'>Total</th>
            </tr>
        </thead>
        <tbody>
            {detallesHtml}
        </tbody>
        <tfoot>
            <tr>
                <td colspan='3' style='border: 1px solid #ddd; padding: 8px; text-align: right;'><strong>Total:</strong></td>
                <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'><strong>₡{total:N2}</strong></td>
            </tr>
        </tfoot>
    </table>
</body>
</html>";
    }

    #endregion
}
