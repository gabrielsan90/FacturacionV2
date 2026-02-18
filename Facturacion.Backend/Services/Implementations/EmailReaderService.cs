using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Responses;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio que lee correos vía IMAP y procesa XMLs de facturas adjuntos
/// </summary>
public class EmailReaderService : IEmailReaderService
{
    private readonly DataContext _context;
    private readonly IDocumentoRecepcionService _recepcionService;
    private readonly ILogger<EmailReaderService> _logger;

    private const int MaxEmailsPorEjecucion = 100;

    public EmailReaderService(
        DataContext context,
        IDocumentoRecepcionService recepcionService,
        ILogger<EmailReaderService> logger)
    {
        _context = context;
        _recepcionService = recepcionService;
        _logger = logger;
    }

    public async Task<ResultadoLecturaCorreo> LeerCorreosAsync(Guid empresaId)
    {
        var resultado = new ResultadoLecturaCorreo();

        try
        {
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId && !e.IsDeleted);
            if (empresa == null)
            {
                resultado.Mensaje = "Empresa no encontrada";
                return resultado;
            }

            if (string.IsNullOrEmpty(empresa.ServidorIMAP) ||
                string.IsNullOrEmpty(empresa.UsuarioIMAP) ||
                string.IsNullOrEmpty(empresa.ClaveIMAP))
            {
                resultado.Mensaje = "Configuración IMAP incompleta. Configure el servidor, usuario y contraseña.";
                return resultado;
            }

            using var client = new ImapClient();

            var secureSocketOptions = empresa.ImapEnableSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.None;

            await client.ConnectAsync(
                empresa.ServidorIMAP,
                empresa.PuertoIMAP ?? 993,
                secureSocketOptions);

            await client.AuthenticateAsync(empresa.UsuarioIMAP, empresa.ClaveIMAP);

            var carpeta = empresa.CarpetaIMAP ?? "INBOX";
            var folder = carpeta.Equals("INBOX", StringComparison.OrdinalIgnoreCase)
                ? client.Inbox
                : await client.GetFolderAsync(carpeta);

            await folder.OpenAsync(FolderAccess.ReadWrite);

            var uids = await folder.SearchAsync(SearchQuery.NotSeen);

            _logger.LogInformation(
                "IMAP: {Count} correos no leídos encontrados en {Carpeta} para empresa {EmpresaId}",
                uids.Count, carpeta, empresaId);

            var emailsAProcesar = uids.Take(MaxEmailsPorEjecucion).ToList();
            resultado.TotalEmailsLeidos = emailsAProcesar.Count;

            foreach (var uid in emailsAProcesar)
            {
                try
                {
                    var message = await folder.GetMessageAsync(uid);
                    var xmlsEncontrados = false;

                    foreach (var attachment in message.Attachments)
                    {
                        if (attachment is not MimePart mimePart)
                            continue;

                        var fileName = mimePart.FileName;
                        if (string.IsNullOrEmpty(fileName) ||
                            !fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                            continue;

                        xmlsEncontrados = true;
                        resultado.TotalXmlEncontrados++;

                        try
                        {
                            using var stream = new MemoryStream();
                            await mimePart.Content.DecodeToAsync(stream);
                            stream.Position = 0;

                            using var reader = new StreamReader(stream);
                            var xmlContent = await reader.ReadToEndAsync();

                            var recepcion = await _recepcionService.RecibirDocumentoAsync(xmlContent, empresaId);
                            resultado.Resultados.Add(recepcion);

                            if (recepcion.Exitoso)
                            {
                                resultado.TotalProcesados++;
                                _logger.LogInformation(
                                    "IMAP: XML {FileName} procesado exitosamente. Clave: {Clave}",
                                    fileName, recepcion.Clave);
                            }
                            else
                            {
                                resultado.TotalErrores++;
                                _logger.LogWarning(
                                    "IMAP: Error al procesar XML {FileName}: {Mensaje}",
                                    fileName, recepcion.Mensaje);
                            }
                        }
                        catch (Exception ex)
                        {
                            resultado.TotalErrores++;
                            resultado.Resultados.Add(new ResultadoRecepcion
                            {
                                Exitoso = false,
                                Mensaje = $"Error al leer adjunto {fileName}: {ex.Message}",
                                Errores = new List<string> { ex.Message }
                            });
                            _logger.LogError(ex, "IMAP: Error procesando adjunto {FileName}", fileName);
                        }
                    }

                    // Marcar como leído si se encontraron XMLs o no (para no reprocesar)
                    await folder.AddFlagsAsync(uid, MessageFlags.Seen, true);

                    if (!xmlsEncontrados)
                    {
                        _logger.LogDebug(
                            "IMAP: Email de {From} sin adjuntos XML, marcado como leído",
                            message.From);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "IMAP: Error procesando email UID {Uid}", uid);
                }
            }

            await client.DisconnectAsync(true);

            resultado.Exitoso = true;
            resultado.Mensaje = resultado.TotalXmlEncontrados == 0
                ? $"Se revisaron {resultado.TotalEmailsLeidos} correos pero no se encontraron XMLs adjuntos."
                : $"Se procesaron {resultado.TotalProcesados} XMLs de {resultado.TotalXmlEncontrados} encontrados en {resultado.TotalEmailsLeidos} correos.";

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer correos IMAP para empresa {EmpresaId}", empresaId);
            resultado.Mensaje = $"Error al conectar con el servidor IMAP: {ex.Message}";
            return resultado;
        }
    }

    public async Task<ActionResponse<string>> ProbarConexionImapAsync(Guid empresaId)
    {
        try
        {
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId && !e.IsDeleted);
            if (empresa == null)
            {
                return new ActionResponse<string> { WasSuccess = false, Message = "Empresa no encontrada" };
            }

            if (string.IsNullOrEmpty(empresa.ServidorIMAP) ||
                string.IsNullOrEmpty(empresa.UsuarioIMAP) ||
                string.IsNullOrEmpty(empresa.ClaveIMAP))
            {
                return new ActionResponse<string>
                {
                    WasSuccess = false,
                    Message = "Configuración IMAP incompleta. Guarde la configuración primero."
                };
            }

            using var client = new ImapClient();

            var secureSocketOptions = empresa.ImapEnableSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.None;

            await client.ConnectAsync(
                empresa.ServidorIMAP,
                empresa.PuertoIMAP ?? 993,
                secureSocketOptions);

            await client.AuthenticateAsync(empresa.UsuarioIMAP, empresa.ClaveIMAP);

            var carpeta = empresa.CarpetaIMAP ?? "INBOX";
            var folder = carpeta.Equals("INBOX", StringComparison.OrdinalIgnoreCase)
                ? client.Inbox
                : await client.GetFolderAsync(carpeta);

            await folder.OpenAsync(FolderAccess.ReadOnly);

            var totalMessages = folder.Count;
            var unreadUids = await folder.SearchAsync(SearchQuery.NotSeen);

            await client.DisconnectAsync(true);

            return new ActionResponse<string>
            {
                WasSuccess = true,
                Message = $"Conexión IMAP exitosa. Carpeta: {carpeta}, Total mensajes: {totalMessages}, No leídos: {unreadUids.Count}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al probar conexión IMAP para empresa {EmpresaId}", empresaId);
            return new ActionResponse<string>
            {
                WasSuccess = false,
                Message = $"Error de conexión: {ex.Message}"
            };
        }
    }
}
