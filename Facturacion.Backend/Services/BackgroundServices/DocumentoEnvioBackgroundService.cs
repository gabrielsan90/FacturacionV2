using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Services.BackgroundServices;

/// <summary>
/// BackgroundService para procesar la cola de documentos pendientes de envío a Hacienda
/// Recomendación 10 de Hacienda v4.4
/// </summary>
public class DocumentoEnvioBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentoEnvioBackgroundService> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(30); // Cada 30 segundos
    private readonly int _maxReintentos = 3;

    public DocumentoEnvioBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<DocumentoEnvioBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DocumentoEnvioBackgroundService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarDocumentosPendientesAsync(stoppingToken);
                await VerificarDocumentosEnProcesoAsync(stoppingToken);
                await VerificarCertificadosProximosAVencerAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo del DocumentoEnvioBackgroundService");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }

        _logger.LogInformation("DocumentoEnvioBackgroundService detenido");
    }

    /// <summary>
    /// Procesa documentos pendientes de envío
    /// </summary>
    private async Task ProcesarDocumentosPendientesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var documentoService = scope.ServiceProvider.GetRequiredService<IDocumentoHaciendaService>();
        var notificacionService = scope.ServiceProvider.GetRequiredService<INotificacionService>();

        // Tiempo mínimo para reintentar documentos con error (esperar 2 minutos antes de reintentar)
        var tiempoMinimoReintento = FechaCostaRicaHelper.Ahora.AddMinutes(-2);

        // Obtener documentos pendientes (estado Pendiente) o con error técnico (estado Error)
        var documentosPendientes = await context.Documentos
            .Include(d => d.Empresa)
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Detalles)
                .ThenInclude(det => det.Impuestos)
            .Where(d => !d.IsDeleted &&
                       d.Empresa!.RequiereFacturacionElectronica &&
                       ((d.Estado == EstadoDocumento.Pendiente && d.FechaEnvioHacienda == null) ||
                        (d.Estado == EstadoDocumento.Error && d.FechaEnvioHacienda < tiempoMinimoReintento)))
            .OrderBy(d => d.FechaEmision) // Más antiguos primero
            .Take(10) // Procesar máximo 10 a la vez
            .ToListAsync(stoppingToken);

        if (!documentosPendientes.Any())
            return;

        var pendientes = documentosPendientes.Count(d => d.Estado == EstadoDocumento.Pendiente);
        var conError = documentosPendientes.Count(d => d.Estado == EstadoDocumento.Error);
        _logger.LogInformation("Procesando {Pendientes} documentos pendientes y {ConError} documentos con error", pendientes, conError);

        foreach (var documento in documentosPendientes)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                // Verificar que la empresa tenga credenciales configuradas
                if (!TieneCredencialesHacienda(documento.Empresa!))
                {
                    _logger.LogWarning("La empresa {EmpresaId} no tiene credenciales de Hacienda configuradas",
                        documento.EmpresaId);
                    continue;
                }

                // Enviar documento a Hacienda
                var resultado = await documentoService.ProcesarYEnviarAsync(documento.Id);

                if (resultado.Exitoso)
                {
                    documento.Estado = EstadoDocumento.Procesando;
                    documento.FechaEnvioHacienda = FechaCostaRicaHelper.Ahora;
                    documento.MensajeHacienda = "Documento enviado exitosamente";

                    _logger.LogInformation("Documento {0} enviado a Hacienda", documento.Clave);
                }
                else
                {
                    // Mantener en pendiente para reintentar después
                    documento.Estado = EstadoDocumento.Pendiente;
                    documento.MensajeHacienda = resultado.Mensaje;

                    _logger.LogWarning("Error al enviar documento {Clave}: {Mensaje}",
                        documento.Clave, resultado.Mensaje);

                    // Notificar error
                    await notificacionService.CrearNotificacionAsync(new Notificacion
                    {
                        EmpresaId = documento.EmpresaId,
                        UsuarioId = documento.UsuarioCreacionId!,
                        TipoNotificacion = TipoNotificacion.ErrorEnvioHacienda,
                        Titulo = "Error al enviar documento",
                        Mensaje = $"Error al enviar el documento {documento.NumeroConsecutivo}: {resultado.Mensaje}",
                        Icono = "fa-exclamation-triangle",
                        Color = "danger",
                        EntidadRelacionadaId = documento.Id,
                        TipoEntidad = "Documento",
                        Importante = true,
                        FechaCreacion = FechaCostaRicaHelper.Ahora
                    });
                }

                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando documento {0}", documento.Id);

                documento.Estado = EstadoDocumento.Pendiente;
                documento.MensajeHacienda = $"Error interno: {ex.Message}";
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }

    /// <summary>
    /// Verifica el estado de documentos que ya fueron enviados
    /// Consulta documentos en estado Procesando que:
    /// 1. No tienen FechaRespuestaHacienda (respuesta pendiente)
    /// 2. O tienen más de 5 minutos en estado Procesando (por seguridad)
    /// </summary>
    private async Task VerificarDocumentosEnProcesoAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var documentoService = scope.ServiceProvider.GetRequiredService<IDocumentoHaciendaService>();
        var notificacionService = scope.ServiceProvider.GetRequiredService<INotificacionService>();

        // Tiempo límite para considerar un documento "atascado" en Procesando
        var tiempoLimite = FechaCostaRicaHelper.Ahora.AddMinutes(-5);

        // Obtener documentos en proceso:
        // - Sin respuesta final (FechaRespuestaHacienda == null)
        // - O que llevan más de 5 minutos en Procesando (por si acaso)
        var documentosEnProceso = await context.Documentos
            .Include(d => d.Empresa)
            .Where(d => !d.IsDeleted &&
                       d.Estado == EstadoDocumento.Procesando &&
                       d.FechaEnvioHacienda != null &&
                       (d.FechaRespuestaHacienda == null || d.FechaEnvioHacienda < tiempoLimite))
            .OrderBy(d => d.FechaEnvioHacienda) // Más antiguos primero
            .Take(10)
            .ToListAsync(stoppingToken);

        if (documentosEnProceso.Any())
        {
            _logger.LogInformation("Verificando estado de {Count} documentos en proceso", documentosEnProceso.Count);
        }

        foreach (var documento in documentosEnProceso)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                // Calcular tiempo en proceso
                var tiempoEnProceso = documento.FechaEnvioHacienda.HasValue
                    ? FechaCostaRicaHelper.Ahora - documento.FechaEnvioHacienda.Value
                    : TimeSpan.Zero;

                // Consultar estado en Hacienda
                _logger.LogInformation(
                    "Consultando estado en Hacienda para documento {Clave} (ID: {DocumentoId}). " +
                    "FechaEnvio: {FechaEnvio}, TiempoEnProceso: {TiempoMinutos} minutos, " +
                    "FechaRespuesta: {FechaRespuesta}",
                    documento.Clave,
                    documento.Id,
                    documento.FechaEnvioHacienda,
                    tiempoEnProceso.TotalMinutes,
                    documento.FechaRespuestaHacienda);

                var estado = await documentoService.ConsultarEstadoAsync(documento.Id);

                if (!string.IsNullOrEmpty(estado.Estado))
                {
                    var estadoAnterior = documento.Estado;
                    var respHacienda = estado.RespuestaHacienda;

                    switch (estado.Estado.ToLowerInvariant())
                    {
                        case "aceptado":
                            // Solo actualizar si cambió el estado (ConsultarEstadoAsync ya pudo haberlo actualizado)
                            if (documento.Estado != EstadoDocumento.Aceptado)
                            {
                                documento.Estado = EstadoDocumento.Aceptado;
                                documento.FechaRespuestaHacienda = FechaCostaRicaHelper.Ahora;

                                // Guardar mensaje detallado
                                if (respHacienda?.Mensajes != null && respHacienda.Mensajes.Any())
                                {
                                    documento.MensajeHacienda = "Aceptado: " + string.Join("; ", respHacienda.Mensajes.Select(m => m.Mensaje));
                                    documento.MensajesHaciendaJson = System.Text.Json.JsonSerializer.Serialize(respHacienda.Mensajes);
                                }
                                else
                                {
                                    documento.MensajeHacienda = estado.Mensaje ?? "Documento aceptado por Hacienda";
                                }

                                // Guardar XML de respuesta
                                if (!string.IsNullOrWhiteSpace(respHacienda?.RespuestaXml))
                                {
                                    try
                                    {
                                        documento.XmlRespuestaHacienda = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(respHacienda.RespuestaXml));
                                    }
                                    catch
                                    {
                                        documento.XmlRespuestaHacienda = respHacienda.RespuestaXml;
                                    }
                                }

                                _logger.LogInformation("Documento {Clave} cambió de {EstadoAnterior} a Aceptado",
                                    documento.Clave, estadoAnterior);

                                // Procesar integración post-aceptación (inventario + contabilidad + CxC)
                                try
                                {
                                    await documentoService.ProcesarPostAceptacionAsync(documento.Id);
                                    _logger.LogInformation("Integración post-aceptación completada para documento {Clave}", documento.Clave);
                                }
                                catch (Exception exIntegracion)
                                {
                                    _logger.LogError(exIntegracion, "Error en integración post-aceptación para documento {Clave}. El documento ya fue aceptado.", documento.Clave);
                                }

                                await CrearNotificacionEstadoAsync(notificacionService, documento,
                                    TipoNotificacion.DocumentoAceptado, "success", "fa-check-circle");

                                // ENVÍO AUTOMÁTICO DE CORREO AL RECEPTOR (SOLO EN PRODUCCIÓN)
                                if (documento.Ambiente == Shared.Enums.Ambiente.Produccion && !documento.CorreoEnviado)
                                {
                                    _logger.LogInformation("Iniciando envío automático de correo para documento {Clave} en producción",
                                        documento.Clave);
                                    await EnviarCorreoAutomaticoAsync(scope, documento);
                                }
                            }
                            break;

                        case "rechazado":
                            // Solo actualizar si cambió el estado
                            if (documento.Estado != EstadoDocumento.Rechazado)
                            {
                                documento.Estado = EstadoDocumento.Rechazado;
                                documento.FechaRespuestaHacienda = FechaCostaRicaHelper.Ahora;

                                // Guardar mensaje detallado con errores
                                if (respHacienda?.Mensajes != null && respHacienda.Mensajes.Any())
                                {
                                    documento.MensajeHacienda = "Rechazado: " + string.Join("; ", respHacienda.Mensajes.Select(m => m.Mensaje));
                                    documento.MensajesHaciendaJson = System.Text.Json.JsonSerializer.Serialize(respHacienda.Mensajes);
                                }
                                else
                                {
                                    documento.MensajeHacienda = estado.Mensaje ?? "Documento rechazado por Hacienda";
                                }

                                // Guardar XML de respuesta
                                if (!string.IsNullOrWhiteSpace(respHacienda?.RespuestaXml))
                                {
                                    try
                                    {
                                        documento.XmlRespuestaHacienda = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(respHacienda.RespuestaXml));
                                    }
                                    catch
                                    {
                                        documento.XmlRespuestaHacienda = respHacienda.RespuestaXml;
                                    }
                                }

                                _logger.LogWarning("Documento {Clave} cambió de {EstadoAnterior} a Rechazado: {Mensaje}",
                                    documento.Clave, estadoAnterior, documento.MensajeHacienda);

                                await CrearNotificacionEstadoAsync(notificacionService, documento,
                                    TipoNotificacion.DocumentoRechazado, "danger", "fa-times-circle");
                            }
                            break;

                        case "procesando":
                        case "enviado":
                            // Todavía en proceso, no hacer nada
                            // NO establecer FechaRespuestaHacienda para que se vuelva a consultar
                            documento.MensajeHacienda = "Documento aún en proceso de validación por Hacienda";
                            _logger.LogDebug("Documento {Clave} aún en proceso de validación", documento.Clave);
                            break;

                        default:
                            _logger.LogWarning("Documento {Clave} tiene estado desconocido: {Estado}",
                                documento.Clave, estado.Estado);
                            break;
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }
                else
                {
                    _logger.LogWarning("Consulta de estado para documento {Clave} no retornó estado válido",
                        documento.Clave);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "Error HTTP al consultar documento {Clave} (ID: {DocumentoId}): {Message}. " +
                    "Posible problema de conectividad con Hacienda",
                    documento.Clave, documento.Id, ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex,
                    "Timeout al consultar documento {Clave} (ID: {DocumentoId}). " +
                    "Hacienda no respondió a tiempo",
                    documento.Clave, documento.Id);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("token") || ex.Message.Contains("credentials"))
            {
                _logger.LogError(ex,
                    "Error de autenticación al consultar documento {Clave} (ID: {DocumentoId}): {Message}. " +
                    "Verifique las credenciales de Hacienda de la empresa",
                    documento.Clave, documento.Id, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error inesperado al consultar documento {Clave} (ID: {DocumentoId}): {Message}",
                    documento.Clave, documento.Id, ex.Message);
            }
        }
    }

    /// <summary>
    /// Verifica certificados digitales próximos a vencer
    /// </summary>
    private async Task VerificarCertificadosProximosAVencerAsync(CancellationToken stoppingToken)
    {
        // Solo verificar una vez al día
        var ahora = FechaCostaRicaHelper.Ahora;
        if (ahora.Hour != 9 || ahora.Minute > 5)
            return;

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var notificacionService = scope.ServiceProvider.GetRequiredService<INotificacionService>();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

        var empresas = await context.Empresas
            .Where(e => !e.IsDeleted && e.Activa && e.CertificadoDigital != null)
            .ToListAsync(stoppingToken);

        foreach (var empresa in empresas)
        {
            try
            {
                if (empresa.CertificadoDigital == null || string.IsNullOrEmpty(empresa.PinCertificado))
                    continue;

                // Obtener fecha de vencimiento del certificado
                var pinDesencriptado = encryptionService.Decrypt(empresa.PinCertificado);
                var fechaVencimiento = ObtenerFechaVencimientoCertificado(empresa.CertificadoDigital, pinDesencriptado);

                if (!fechaVencimiento.HasValue)
                    continue;

                var diasParaVencer = (fechaVencimiento.Value - FechaCostaRicaHelper.Ahora).Days;

                // Alertar si vence en menos de 30 días
                if (diasParaVencer <= 30 && diasParaVencer > 0)
                {
                    // Verificar si ya se envió notificación hoy
                    var yaNotificado = await context.Set<Notificacion>()
                        .AnyAsync(n => n.EmpresaId == empresa.Id &&
                                      n.TipoNotificacion == TipoNotificacion.CertificadoPorVencer &&
                                      n.FechaCreacion.Date == ahora.Date,
                                  stoppingToken);

                    if (!yaNotificado)
                    {
                        // Obtener administradores de la empresa
                        var admins = await context.UsuariosEmpresas
                            .Where(ue => ue.EmpresaId == empresa.Id)
                            .Select(ue => ue.UserId)
                            .ToListAsync(stoppingToken);

                        foreach (var adminId in admins)
                        {
                            await notificacionService.CrearNotificacionAsync(new Notificacion
                            {
                                EmpresaId = empresa.Id,
                                UsuarioId = adminId,
                                TipoNotificacion = TipoNotificacion.CertificadoPorVencer,
                                Titulo = "Certificado Digital por vencer",
                                Mensaje = $"El certificado digital de {empresa.NombreComercial} vence en {diasParaVencer} días ({fechaVencimiento:dd/MM/yyyy}). Renuévelo pronto.",
                                Icono = "fa-certificate",
                                Color = diasParaVencer <= 7 ? "danger" : "warning",
                                Importante = diasParaVencer <= 7,
                                FechaCreacion = FechaCostaRicaHelper.Ahora
                            });
                        }

                        _logger.LogWarning("Certificado de empresa {EmpresaId} vence en {Dias} días",
                            empresa.Id, diasParaVencer);
                    }
                }
                else if (diasParaVencer <= 0)
                {
                    // Certificado vencido
                    var admins = await context.UsuariosEmpresas
                        .Where(ue => ue.EmpresaId == empresa.Id)
                        .Select(ue => ue.UserId)
                        .ToListAsync(stoppingToken);

                    foreach (var adminId in admins)
                    {
                        await notificacionService.CrearNotificacionAsync(new Notificacion
                        {
                            EmpresaId = empresa.Id,
                            UsuarioId = adminId,
                            TipoNotificacion = TipoNotificacion.CertificadoVencido,
                            Titulo = "¡Certificado Digital VENCIDO!",
                            Mensaje = $"El certificado digital de {empresa.NombreComercial} está VENCIDO desde el {fechaVencimiento:dd/MM/yyyy}. No podrá emitir documentos electrónicos.",
                            Icono = "fa-certificate",
                            Color = "danger",
                            Importante = true,
                            FechaCreacion = FechaCostaRicaHelper.Ahora
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando certificado de empresa {EmpresaId}", empresa.Id);
            }
        }
    }

    private bool TieneCredencialesHacienda(Empresa empresa)
    {
        return !string.IsNullOrEmpty(empresa.UsuarioHacienda) &&
               !string.IsNullOrEmpty(empresa.ClaveHacienda) &&
               empresa.CertificadoDigital != null &&
               !string.IsNullOrEmpty(empresa.PinCertificado);
    }

    private async Task CrearNotificacionEstadoAsync(
        INotificacionService notificacionService,
        Documento documento,
        TipoNotificacion tipo,
        string color,
        string icono)
    {
        var titulo = tipo == TipoNotificacion.DocumentoAceptado
            ? "Documento aceptado"
            : "Documento rechazado";

        var mensaje = tipo == TipoNotificacion.DocumentoAceptado
            ? $"El documento {documento.NumeroConsecutivo} fue aceptado por Hacienda"
            : $"El documento {documento.NumeroConsecutivo} fue rechazado: {documento.MensajeHacienda}";

        await notificacionService.CrearNotificacionAsync(new Notificacion
        {
            EmpresaId = documento.EmpresaId,
            UsuarioId = documento.UsuarioCreacionId!,
            TipoNotificacion = tipo,
            Titulo = titulo,
            Mensaje = mensaje,
            Icono = icono,
            Color = color,
            EntidadRelacionadaId = documento.Id,
            TipoEntidad = "Documento",
            Importante = tipo == TipoNotificacion.DocumentoRechazado,
            FechaCreacion = FechaCostaRicaHelper.Ahora
        });
    }

    private DateTime? ObtenerFechaVencimientoCertificado(byte[] certificado, string pin)
    {
        try
        {
            using var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificado, pin);
            return cert.NotAfter;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Envía automáticamente el correo electrónico al receptor cuando un documento es aceptado en producción
    /// </summary>
    private async Task EnviarCorreoAutomaticoAsync(IServiceScope scope, Documento documento)
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            _logger.LogInformation("Enviando correo automático para documento {Clave} (ID: {DocumentoId})",
                documento.Clave, documento.Id);

            // Enviar el documento electrónico por correo
            var resultado = await emailService.EnviarDocumentoElectronicoAsync(
                documento.Id,
                emailsAdicionales: null,
                mensaje: "Este documento ha sido aceptado por el Ministerio de Hacienda de Costa Rica.");

            if (resultado.WasSuccess && resultado.Result != null)
            {
                // Actualizar documento con información del envío exitoso
                documento.CorreoEnviado = true;
                documento.FechaEnvioCorreo = FechaCostaRicaHelper.Ahora;
                documento.MensajeEnvioCorreo = resultado.Result.Mensaje;

                _logger.LogInformation("Correo enviado exitosamente para documento {Clave} a {Count} destinatarios",
                    documento.Clave, resultado.Result.EmailsEnviados?.Count ?? 0);
            }
            else
            {
                // Registrar el intento fallido
                documento.CorreoEnviado = false;
                documento.FechaEnvioCorreo = FechaCostaRicaHelper.Ahora;
                documento.MensajeEnvioCorreo = $"Error: {resultado.Message}";

                _logger.LogWarning("Error al enviar correo automático para documento {Clave}: {Mensaje}",
                    documento.Clave, resultado.Message);
            }

            // Guardar cambios en la base de datos
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo automático para documento {Clave} (ID: {DocumentoId})",
                documento.Clave, documento.Id);

            // Intentar registrar el error en la base de datos
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                documento.CorreoEnviado = false;
                documento.FechaEnvioCorreo = FechaCostaRicaHelper.Ahora;
                documento.MensajeEnvioCorreo = $"Excepción: {ex.Message}";
                await context.SaveChangesAsync();
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx, "Error guardando estado de envío de correo para documento {DocumentoId}", documento.Id);
            }
        }
    }
}
