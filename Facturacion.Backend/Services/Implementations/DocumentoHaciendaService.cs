using Facturacion.Backend.Helpers;
using System.Text;
using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio orquestador para el proceso completo de Hacienda
/// </summary>
public class DocumentoHaciendaService : IDocumentoHaciendaService
{
    private readonly DataContext _context;
    private readonly IClaveGeneradorService _claveGenerador;
    private readonly IXmlGeneradorService _xmlGenerador;
    private readonly IFirmaDigitalService _firmaDigital;
    private readonly IHaciendaApiService _haciendaApi;
    private readonly IXsdValidacionService _xsdValidacion;
    private readonly IValidacionCalculosService _validacionCalculos;
    private readonly IInventarioRepository _inventarioRepository;
    private readonly IContabilidadIntegracionService _contabilidadIntegracion;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentoHaciendaService> _logger;

    public DocumentoHaciendaService(
        DataContext context,
        IClaveGeneradorService claveGenerador,
        IXmlGeneradorService xmlGenerador,
        IFirmaDigitalService firmaDigital,
        IHaciendaApiService haciendaApi,
        IXsdValidacionService xsdValidacion,
        IValidacionCalculosService validacionCalculos,
        IInventarioRepository inventarioRepository,
        IContabilidadIntegracionService contabilidadIntegracion,
        IServiceScopeFactory scopeFactory,
        ILogger<DocumentoHaciendaService> logger)
    {
        _context = context;
        _claveGenerador = claveGenerador;
        _xmlGenerador = xmlGenerador;
        _firmaDigital = firmaDigital;
        _haciendaApi = haciendaApi;
        _xsdValidacion = xsdValidacion;
        _validacionCalculos = validacionCalculos;
        _inventarioRepository = inventarioRepository;
        _contabilidadIntegracion = contabilidadIntegracion;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Procesa un documento completo
    /// </summary>
    public async Task<ResultadoEnvio> ProcesarYEnviarAsync(Guid documentoId)
    {
        var resultado = new ResultadoEnvio
        {
            Exitoso = false,
            Mensaje = "Procesando documento...",
            Errores = new List<string>()
        };

        try
        {
            _logger.LogInformation("Iniciando procesamiento de documento {DocumentoId}", documentoId);

            // 1. Obtener el documento con todas sus relaciones
            var documento = await ObtenerDocumentoCompletoAsync(documentoId);

            if (documento == null)
            {
                resultado.Mensaje = "No se encontró el documento";
                resultado.Errores.Add("El documento no existe");
                return resultado;
            }

            // 2. Validar que el documento esté en estado correcto
            if (documento.Estado != EstadoDocumento.Borrador && documento.Estado != EstadoDocumento.Pendiente)
            {
                resultado.Mensaje = $"El documento está en estado {documento.Estado} y no puede ser procesado";
                resultado.Errores.Add($"Estado inválido: {documento.Estado}");
                return resultado;
            }

            // 3. Validar el documento
            var erroresValidacion = await ValidarDocumentoAsync(documentoId);
            if (erroresValidacion.Any())
            {
                resultado.Mensaje = "El documento tiene errores de validación";
                resultado.Errores = erroresValidacion;
                return resultado;
            }

            // 4. Obtener empresa con configuración de Hacienda
            var empresa = await _context.Set<Empresa>()
                .Include(e => e.Telefonos)
                .Include(e => e.Emails)
                .Include(e => e.ActividadesEconomicas)
                .FirstOrDefaultAsync(e => e.Id == documento.EmpresaId);

            if (empresa == null)
            {
                resultado.Mensaje = "No se encontró la empresa";
                resultado.Errores.Add("Empresa no existe");
                return resultado;
            }

            // Validar credenciales de Hacienda
            if (string.IsNullOrWhiteSpace(empresa.UsuarioHacienda) ||
                string.IsNullOrWhiteSpace(empresa.ClaveHacienda))
            {
                resultado.Mensaje = "La empresa no tiene configuradas las credenciales de Hacienda";
                resultado.Errores.Add("Falta configurar Usuario y Clave de ATV");
                return resultado;
            }

            // Determinar situación del documento (1=Normal, 2=Contingencia, 3=Sin internet)
            int situacion = documento.EsContingencia ? 2 : 1;

            // 5. Generar o validar la Clave
            if (string.IsNullOrWhiteSpace(documento.Clave) || documento.Clave.Length != 50)
            {
                _logger.LogInformation("Generando clave para documento {DocumentoId}", documentoId);
                documento.Clave = await _claveGenerador.GenerarClaveAsync(documento, situacion);
                resultado.Clave = documento.Clave;
            }
            else
            {
                resultado.Clave = documento.Clave;
            }

            // 6. Generar XML
            _logger.LogInformation("Generando XML para documento {DocumentoId}", documentoId);

            // DEBUG: Log document totals before XML generation
            _logger.LogInformation("=== TOTALES ANTES DE XML === DocId: {DocId}", documentoId);
            _logger.LogInformation("  TotalServiciosGravados: {Total}", documento.TotalServiciosGravados);
            _logger.LogInformation("  TotalServiciosExentos: {Total}", documento.TotalServiciosExentos);
            _logger.LogInformation("  TotalServiciosExonerados: {Total}", documento.TotalServiciosExonerados);
            _logger.LogInformation("  TotalMercanciasGravadas: {Total}", documento.TotalMercanciasGravadas);
            _logger.LogInformation("  TotalMercanciasExentas: {Total}", documento.TotalMercanciasExentas);
            _logger.LogInformation("  TotalMercanciasExoneradas: {Total}", documento.TotalMercanciasExoneradas);
            _logger.LogInformation("  TotalGravado: {Total}", documento.TotalGravado);
            _logger.LogInformation("  TotalVenta: {Total}", documento.TotalVenta);
            _logger.LogInformation("  Detalles count: {Count}", documento.Detalles?.Count ?? 0);
            if (documento.Detalles != null)
            {
                foreach (var det in documento.Detalles)
                {
                    _logger.LogInformation("    Detalle {Num}: Desc={Desc}, ProductoId={PId}, MontoTotal={MT}",
                        det.NumeroLinea, det.Descripcion, det.ProductoId, det.MontoTotal);
                }
            }

            documento.Estado = EstadoDocumento.Pendiente;
            await _context.SaveChangesAsync();

            var xmlGenerado = await _xmlGenerador.GenerarXmlAsync(documento);
            documento.XmlGenerado = xmlGenerado;
            resultado.XmlGenerado = xmlGenerado;

            await _context.SaveChangesAsync();

            // 6.1. Validar XML contra XSD v4.4 (NUEVO)
            _logger.LogInformation("Validando XML contra XSD v4.4 para documento {DocumentoId}", documentoId);

            try
            {
                var resultadoXsd = await _xsdValidacion.ValidarXmlContraXsdAsync(xmlGenerado, documento.TipoDocumento);

                if (!resultadoXsd.EsValido)
                {
                    resultado.Mensaje = "El XML generado no cumple con el esquema XSD v4.4";
                    resultado.Errores = resultadoXsd.Errores;

                    // Registrar errores para análisis
                    _logger.LogError("Validación XSD falló para documento {DocumentoId}. Errores: {Errores}",
                        documentoId, string.Join("; ", resultadoXsd.Errores));

                    // Cambiar estado a Error para que el usuario pueda corregir
                    documento.Estado = EstadoDocumento.Error;
                    documento.MensajeHacienda = $"Error de validación XSD: {string.Join("; ", resultadoXsd.Errores.Take(3))}";
                    await _context.SaveChangesAsync();

                    return resultado;
                }

                // Log de advertencias si las hay (no impiden el envío)
                if (resultadoXsd.Advertencias.Any())
                {
                    _logger.LogWarning("Advertencias XSD para documento {DocumentoId}: {Advertencias}",
                        documentoId, string.Join("; ", resultadoXsd.Advertencias));
                }

                _logger.LogInformation("Validación XSD exitosa para documento {DocumentoId}", documentoId);
            }
            catch (Exception ex)
            {
                // No bloquear el envío si hay error en la validación XSD (modo permisivo)
                // Solo registrar el error
                _logger.LogWarning(ex, "Error durante validación XSD para documento {DocumentoId}. Se continúa con el envío.", documentoId);
            }

            // 7. Firmar XML
            _logger.LogInformation("Firmando XML para documento {DocumentoId}", documentoId);

            try
            {
                var certificado = await _firmaDigital.ObtenerCertificadoAsync(empresa.Id);
                var xmlFirmado = await _firmaDigital.FirmarXmlAsync(xmlGenerado, certificado, empresa.PinCertificado);
                documento.XmlFirmado = xmlFirmado;
                documento.FechaFirma = FechaCostaRicaHelper.Ahora;
                resultado.XmlFirmado = xmlFirmado;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al firmar documento {DocumentoId}", documentoId);
                resultado.Mensaje = "Error al firmar el documento";
                resultado.Errores.Add($"Error de firma: {ex.Message}");
                return resultado;
            }

            // 8. Enviar a Hacienda usando OAuth2 Bearer token (método correcto)
            _logger.LogInformation("Enviando documento {DocumentoId} a Hacienda con OAuth2", documentoId);
            documento.Estado = EstadoDocumento.Procesando;
            documento.FechaEnvioHacienda = FechaCostaRicaHelper.Ahora;
            await _context.SaveChangesAsync();

            string ambiente = empresa.Ambiente == Ambiente.Produccion ? "prod" : "stag";

            // Obtener el cliente si existe para incluirlo en el payload
            Cliente? cliente = documento.ClienteId.HasValue
                ? await _context.Set<Cliente>().FirstOrDefaultAsync(c => c.Id == documento.ClienteId)
                : null;

            // IMPORTANTE: Usar el método con Emisor/Receptor que obtiene el tipo de identificación
            // correctamente de las entidades (la clave NO contiene el tipo de identificación)
            var respuestaHacienda = await _haciendaApi.EnviarDocumentoConEmisorReceptorAsync(
                documento.Clave,
                documento.XmlFirmado!,
                empresa,
                cliente,
                ambiente
            );

            resultado.RespuestaHacienda = respuestaHacienda;

            // 9. Procesar respuesta según el código HTTP
            // IMPORTANTE: Solo establecer FechaRespuestaHacienda cuando hay una respuesta FINAL
            // Si el documento queda en "Procesando", NO establecer la fecha para que el BackgroundService lo consulte
            documento.XmlRespuestaHacienda = respuestaHacienda.RespuestaXml;

            // Guardar los mensajes estructurados como JSON para poder visualizarlos posteriormente
            if (respuestaHacienda.Mensajes != null && respuestaHacienda.Mensajes.Any())
            {
                documento.MensajesHaciendaJson = System.Text.Json.JsonSerializer.Serialize(
                    respuestaHacienda.Mensajes,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = false });
            }

            var estadoRespuesta = respuestaHacienda.IndEstado.ToLower();

            if (estadoRespuesta == "enviado" || estadoRespuesta == "procesando")
            {
                // 201/202: Documento recibido por Hacienda y en proceso de validación
                // NO establecer FechaRespuestaHacienda para que el BackgroundService lo consulte posteriormente
                documento.Estado = EstadoDocumento.Procesando;
                documento.MensajeHacienda = "Documento enviado exitosamente a Hacienda y en proceso de validación";
                resultado.Exitoso = true;
                resultado.Mensaje = "Documento enviado exitosamente a Hacienda";
                resultado.Estado = "Enviado";

                _logger.LogInformation("Documento {DocumentoId} enviado exitosamente a Hacienda (HTTP 201/202) - será consultado automáticamente", documentoId);

                // MEJORA: Iniciar consultas inmediatas en background con reintentos progresivos
                // Esto mejora la experiencia del usuario al obtener la respuesta más rápido
                // Usa un scope independiente para evitar problemas con el scope del request HTTP
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var scopedService = scope.ServiceProvider.GetRequiredService<IDocumentoHaciendaService>();
                        await ((DocumentoHaciendaService)scopedService).ConsultarEstadoConReintentosAsync(documentoId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error en reintentos automáticos para documento {DocumentoId} - el BackgroundService lo procesará", documentoId);
                    }
                });
            }
            else if (estadoRespuesta == "aceptado")
            {
                // Respuesta posterior indica que fue aceptado
                documento.Estado = EstadoDocumento.Aceptado;
                documento.FechaRespuestaHacienda = FechaCostaRicaHelper.Ahora;
                documento.MensajeHacienda = "Documento aceptado por Hacienda";
                resultado.Exitoso = true;
                resultado.Mensaje = "Documento aceptado exitosamente por Hacienda";
                resultado.Estado = "Aceptado";

                // Procesar inventario según tipo de documento
                await ProcesarInventarioDocumentoAsync(documento);

                // Generar asiento contable automático (si está habilitado)
                try
                {
                    await _contabilidadIntegracion.GenerarAsientoVentaAsync(documento, documento.UsuarioCreacionId ?? "sistema");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error generando asiento contable para documento {DocumentoId} - operación continúa", documentoId);
                }

                _logger.LogInformation("Documento {DocumentoId} aceptado por Hacienda", documentoId);
            }
            else if (estadoRespuesta == "rechazado")
            {
                // 400 Bad Request: Error de validación
                documento.Estado = EstadoDocumento.Rechazado;
                documento.FechaRespuestaHacienda = FechaCostaRicaHelper.Ahora;

                var mensajes = string.Join("; ", respuestaHacienda.Mensajes.Select(m => m.Mensaje));
                var detalles = string.Join("; ", respuestaHacienda.Mensajes
                    .Where(m => !string.IsNullOrWhiteSpace(m.Detalle))
                    .Select(m => m.Detalle));

                documento.MensajeHacienda = $"Rechazado: {mensajes}";
                if (!string.IsNullOrWhiteSpace(detalles))
                {
                    documento.MensajeHacienda += $" | Detalles: {detalles}";
                }

                resultado.Exitoso = false;
                resultado.Mensaje = "Documento rechazado por Hacienda";
                resultado.Estado = "Rechazado";
                resultado.Errores = respuestaHacienda.Mensajes.Select(m =>
                    string.IsNullOrWhiteSpace(m.Detalle) ? m.Mensaje : $"{m.Mensaje}: {m.Detalle}"
                ).ToList();

                _logger.LogWarning("Documento {DocumentoId} rechazado por Hacienda: {Mensajes}",
                    documentoId, mensajes);
            }
            else if (estadoRespuesta == "error")
            {
                // 401/403/429/50x: Error técnico (no del documento)
                // Cambiar a estado Error para que sea visible y se pueda reintentar
                var mensajes = string.Join("; ", respuestaHacienda.Mensajes.Select(m => m.Mensaje));
                documento.Estado = EstadoDocumento.Error;
                documento.MensajeHacienda = $"Error técnico: {mensajes}";
                // NO establecer FechaRespuestaHacienda porque no es una respuesta de Hacienda

                resultado.Exitoso = false;
                resultado.Mensaje = "Error técnico al comunicarse con Hacienda";
                resultado.Estado = "Error";
                resultado.Errores = respuestaHacienda.Mensajes.Select(m =>
                    string.IsNullOrWhiteSpace(m.Detalle) ? m.Mensaje : $"{m.Mensaje}: {m.Detalle}"
                ).ToList();

                _logger.LogError("Error técnico al enviar documento {DocumentoId}: {Mensajes}",
                    documentoId, mensajes);
            }
            else // otros estados desconocidos
            {
                // NO establecer FechaRespuestaHacienda para que el BackgroundService lo consulte
                documento.Estado = EstadoDocumento.Procesando;
                documento.MensajeHacienda = "Documento en proceso de validación por Hacienda";
                resultado.Exitoso = true;
                resultado.Mensaje = "Documento enviado y en proceso de validación";
                resultado.Estado = "Procesando";

                _logger.LogInformation("Documento {DocumentoId} en procesamiento por Hacienda", documentoId);
            }

            await _context.SaveChangesAsync();

            return resultado;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("token") || ex.Message.Contains("credentials") || ex.Message.Contains("Unauthorized"))
        {
            // Error de autenticación con Hacienda - credenciales inválidas
            _logger.LogError(ex, "Error de autenticación con Hacienda para documento {DocumentoId}", documentoId);

            // Actualizar estado del documento para indicar el error
            var doc = await _context.Set<Documento>().FindAsync(documentoId);
            if (doc != null)
            {
                doc.Estado = EstadoDocumento.Error;
                doc.MensajeHacienda = "Error de autenticación: Las credenciales de Hacienda (ATV) son inválidas. Verifique el usuario y contraseña en la configuración de la empresa.";
                await _context.SaveChangesAsync();
            }

            resultado.Mensaje = "Error de autenticación con Hacienda";
            resultado.Errores.Add("Las credenciales de acceso a Hacienda (ATV) son inválidas.");
            resultado.Errores.Add("Por favor verifique el Usuario y Contraseña de Hacienda en la configuración de la Empresa.");
            resultado.Errores.Add("Puede obtener sus credenciales en: https://www.hacienda.go.cr/ATV/Login.aspx");
            return resultado;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
        {
            // Error HTTP 401 de Hacienda
            _logger.LogError(ex, "Error HTTP 401 con Hacienda para documento {DocumentoId}", documentoId);

            var doc = await _context.Set<Documento>().FindAsync(documentoId);
            if (doc != null)
            {
                doc.Estado = EstadoDocumento.Error;
                doc.MensajeHacienda = "Error de autenticación: Credenciales de Hacienda inválidas o expiradas.";
                await _context.SaveChangesAsync();
            }

            resultado.Mensaje = "Error de autenticación con Hacienda";
            resultado.Errores.Add("Las credenciales de acceso a Hacienda son inválidas o han expirado.");
            resultado.Errores.Add("Verifique el Usuario y Contraseña de Hacienda en la configuración de la Empresa.");
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar documento {DocumentoId}", documentoId);

            // Detectar errores de credenciales en el mensaje de la excepción
            string mensajeError = ex.Message;
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                mensajeError += " | " + innerEx.Message;
                innerEx = innerEx.InnerException;
            }

            if (mensajeError.Contains("invalid_grant") || mensajeError.Contains("Invalid user credentials") ||
                mensajeError.Contains("Unauthorized") || mensajeError.Contains("token"))
            {
                var doc = await _context.Set<Documento>().FindAsync(documentoId);
                if (doc != null)
                {
                    doc.Estado = EstadoDocumento.Error;
                    doc.MensajeHacienda = "Error de autenticación: Credenciales de Hacienda inválidas.";
                    await _context.SaveChangesAsync();
                }

                resultado.Mensaje = "Error de autenticación con Hacienda";
                resultado.Errores.Add("Las credenciales de acceso a Hacienda (ATV) son inválidas.");
                resultado.Errores.Add("Verifique el Usuario y Contraseña en la configuración de la Empresa.");
                resultado.Errores.Add("Si el problema persiste, regenere su contraseña en el portal ATV de Hacienda.");
            }
            else
            {
                resultado.Mensaje = "Error al procesar el documento";
                resultado.Errores.Add($"Error: {ex.Message}");
            }

            return resultado;
        }
    }

    /// <summary>
    /// Consulta el estado de un documento en Hacienda
    /// </summary>
    public async Task<ResultadoConsulta> ConsultarEstadoAsync(Guid documentoId)
    {
        var resultado = new ResultadoConsulta
        {
            Exitoso = false,
            Mensaje = "Consultando estado...",
            FechaConsulta = FechaCostaRicaHelper.Ahora
        };

        try
        {
            var documento = await ObtenerDocumentoCompletoAsync(documentoId);

            if (documento == null)
            {
                resultado.Mensaje = "No se encontró el documento";
                return resultado;
            }

            if (string.IsNullOrWhiteSpace(documento.Clave))
            {
                resultado.Mensaje = "El documento no tiene clave generada";
                return resultado;
            }

            var empresa = await _context.Set<Empresa>()
                .FirstOrDefaultAsync(e => e.Id == documento.EmpresaId);

            if (empresa == null || string.IsNullOrWhiteSpace(empresa.UsuarioHacienda))
            {
                resultado.Mensaje = "No se encontró la configuración de Hacienda";
                return resultado;
            }

            string ambiente = empresa.Ambiente == Ambiente.Produccion ? "prod" : "stag";

            // IMPORTANTE: Usar el método con OAuth2 que obtiene automáticamente un token válido
            var respuestaHacienda = await _haciendaApi.ConsultarEstadoConTokenAsync(
                documento.Clave,
                empresa.Id, // EmpresaId para obtener el token válido
                ambiente
            );

            resultado.Clave = documento.Clave;
            resultado.RespuestaHacienda = respuestaHacienda;
            resultado.Estado = respuestaHacienda.IndEstado;

            // Actualizar estado en la base de datos
            if (respuestaHacienda.IndEstado.ToLower() == "aceptado" &&
                documento.Estado != EstadoDocumento.Aceptado)
            {
                documento.Estado = EstadoDocumento.Aceptado;
                documento.MensajeHacienda = "Documento aceptado por Hacienda";
                documento.FechaRespuestaHacienda = FechaCostaRicaHelper.Ahora;

                // Guardar XML de respuesta si existe
                if (!string.IsNullOrWhiteSpace(respuestaHacienda.RespuestaXml))
                {
                    try
                    {
                        documento.XmlRespuestaHacienda = Encoding.UTF8.GetString(Convert.FromBase64String(respuestaHacienda.RespuestaXml));
                    }
                    catch
                    {
                        documento.XmlRespuestaHacienda = respuestaHacienda.RespuestaXml;
                    }
                }

                // Guardar mensajes estructurados
                if (respuestaHacienda.Mensajes != null && respuestaHacienda.Mensajes.Any())
                {
                    documento.MensajesHaciendaJson = System.Text.Json.JsonSerializer.Serialize(
                        respuestaHacienda.Mensajes,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = false });
                }

                // Procesar inventario según tipo de documento
                await ProcesarInventarioDocumentoAsync(documento);

                await _context.SaveChangesAsync();
            }
            else if (respuestaHacienda.IndEstado.ToLower() == "rechazado" &&
                     documento.Estado != EstadoDocumento.Rechazado)
            {
                var mensajes = respuestaHacienda.Mensajes != null && respuestaHacienda.Mensajes.Any()
                    ? string.Join("; ", respuestaHacienda.Mensajes.Select(m => m.Mensaje))
                    : "Sin detalle de rechazo";
                documento.Estado = EstadoDocumento.Rechazado;
                documento.MensajeHacienda = $"Rechazado: {mensajes}";
                documento.FechaRespuestaHacienda = FechaCostaRicaHelper.Ahora;

                // Guardar XML de respuesta si existe
                if (!string.IsNullOrWhiteSpace(respuestaHacienda.RespuestaXml))
                {
                    try
                    {
                        documento.XmlRespuestaHacienda = Encoding.UTF8.GetString(Convert.FromBase64String(respuestaHacienda.RespuestaXml));
                    }
                    catch
                    {
                        documento.XmlRespuestaHacienda = respuestaHacienda.RespuestaXml;
                    }
                }

                // Guardar mensajes estructurados (IMPORTANTE para ver errores de rechazo)
                if (respuestaHacienda.Mensajes != null && respuestaHacienda.Mensajes.Any())
                {
                    documento.MensajesHaciendaJson = System.Text.Json.JsonSerializer.Serialize(
                        respuestaHacienda.Mensajes,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = false });
                }

                await _context.SaveChangesAsync();
            }

            resultado.Exitoso = true;
            resultado.Mensaje = $"Estado actual: {respuestaHacienda.IndEstado}";

            return resultado;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("token") || ex.Message.Contains("credentials") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogError(ex, "Error de autenticación al consultar documento {DocumentoId}", documentoId);
            resultado.Mensaje = "Error de autenticación con Hacienda: Las credenciales (Usuario/Contraseña ATV) son inválidas. Por favor verifique la configuración de la empresa.";
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar estado del documento {DocumentoId}", documentoId);

            // Detectar errores de credenciales
            string mensajeCompleto = ex.Message;
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                mensajeCompleto += " | " + innerEx.Message;
                innerEx = innerEx.InnerException;
            }

            if (mensajeCompleto.Contains("invalid_grant") || mensajeCompleto.Contains("Invalid user credentials") ||
                mensajeCompleto.Contains("Unauthorized"))
            {
                resultado.Mensaje = "Error de autenticación con Hacienda: Las credenciales (Usuario/Contraseña ATV) son inválidas. Verifique la configuración de la empresa.";
            }
            else
            {
                resultado.Mensaje = $"Error al consultar estado: {ex.Message}";
            }

            return resultado;
        }
    }

    /// <summary>
    /// Reenvía un documento rechazado
    /// </summary>
    public async Task<bool> ReenviarAsync(Guid documentoId)
    {
        try
        {
            var documento = await ObtenerDocumentoCompletoAsync(documentoId);

            if (documento == null)
                return false;

            // Solo se pueden reenviar documentos rechazados o en contingencia
            if (documento.Estado != EstadoDocumento.Rechazado &&
                documento.Estado != EstadoDocumento.Contingencia)
            {
                return false;
            }

            // Cambiar estado a pendiente para permitir reenvío
            documento.Estado = EstadoDocumento.Pendiente;
            await _context.SaveChangesAsync();

            // Procesar y enviar nuevamente
            var resultado = await ProcesarYEnviarAsync(documentoId);

            return resultado.Exitoso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reenviar documento {DocumentoId}", documentoId);
            return false;
        }
    }

    /// <summary>
    /// Genera solo el XML sin enviarlo
    /// </summary>
    public async Task<string> GenerarXmlAsync(Guid documentoId)
    {
        var documento = await ObtenerDocumentoCompletoAsync(documentoId);

        if (documento == null)
            throw new InvalidOperationException("No se encontró el documento");

        return await _xmlGenerador.GenerarXmlAsync(documento);
    }

    /// <summary>
    /// Valida un documento antes de enviarlo
    /// </summary>
    public async Task<List<string>> ValidarDocumentoAsync(Guid documentoId)
    {
        var errores = new List<string>();

        var documento = await ObtenerDocumentoCompletoAsync(documentoId);

        if (documento == null)
        {
            errores.Add("Documento no encontrado");
            return errores;
        }

        // Validar que tenga detalles
        if (documento.Detalles == null || !documento.Detalles.Any())
        {
            errores.Add("El documento no tiene líneas de detalle");
        }

        // Validar actividad económica
        if (string.IsNullOrWhiteSpace(documento.ActividadEconomica))
        {
            errores.Add("Falta la actividad económica del emisor");
        }

        // Validar receptor para FE, NC, ND (no para tiquete)
        if (documento.TipoDocumento != DocumentoTipo.TiqueteElectronico)
        {
            if (string.IsNullOrWhiteSpace(documento.ReceptorNombre) && documento.Cliente == null)
            {
                errores.Add("Falta información del receptor");
            }

            // NUEVO en v4.4: Actividad económica del receptor es obligatoria en facturas
            if (documento.TipoDocumento == DocumentoTipo.FacturaElectronica &&
                string.IsNullOrWhiteSpace(documento.ReceptorActividadEconomica))
            {
                errores.Add("Falta la actividad económica del receptor (obligatorio en v4.4)");
            }
        }

        // Validar referencias para NC y ND
        if ((documento.TipoDocumento == DocumentoTipo.NotaCreditoElectronica ||
             documento.TipoDocumento == DocumentoTipo.NotaDebitoElectronica) &&
            (documento.Referencias == null || !documento.Referencias.Any()))
        {
            errores.Add("Las notas de crédito/débito requieren al menos una referencia");
        }

        // Validar totales
        if (documento.TotalVenta <= 0)
        {
            errores.Add("El total del documento debe ser mayor a cero");
        }

        // Validar que TotalVenta coincida con los totales por categoría según Hacienda v4.4
        // TotalVenta = TotalGravado + TotalExento + TotalExonerado (usando MontoTotal, antes de descuentos)
        var totalVentaEsperado = documento.TotalGravado + documento.TotalExento + documento.TotalExonerado;
        if (Math.Abs(totalVentaEsperado - documento.TotalVenta) > 0.01m)
        {
            errores.Add($"TotalVenta ({documento.TotalVenta}) no coincide con TotalGravado + TotalExento + TotalExonerado ({totalVentaEsperado})");
        }

        // Validar plazo de crédito si la condición de venta es crédito
        if (documento.CondicionVenta == "02" && !documento.PlazoCreditoDias.HasValue)
        {
            errores.Add("Falta el plazo de crédito (condición de venta es crédito)");
        }

        // NUEVO v4.4 - M8: Validar cálculos del documento
        var resultadoCalculos = await _validacionCalculos.ValidarDocumentoAsync(documento);
        if (!resultadoCalculos.EsValido)
        {
            errores.AddRange(resultadoCalculos.Errores);
        }
        // Registrar advertencias en el log
        if (resultadoCalculos.Advertencias.Any())
        {
            foreach (var advertencia in resultadoCalculos.Advertencias)
            {
                _logger.LogWarning("Validación de cálculos - Advertencia: {Advertencia}", advertencia);
            }
        }

        return errores;
    }

    #region Helpers

    private async Task<Documento?> ObtenerDocumentoCompletoAsync(Guid documentoId)
    {
        return await _context.Set<Documento>()
            .Include(d => d.Empresa)
                .ThenInclude(e => e!.Telefonos)
            .Include(d => d.Empresa)
                .ThenInclude(e => e!.Emails)
            .Include(d => d.Empresa)
                .ThenInclude(e => e!.ActividadesEconomicas)
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
                .ThenInclude(c => c!.Telefonos)
            .Include(d => d.Cliente)
                .ThenInclude(c => c!.Emails)
            .Include(d => d.Proveedor)
            .Include(d => d.Detalles)
                .ThenInclude(det => det.Impuestos)
            .Include(d => d.Detalles)
                .ThenInclude(det => det.Descuentos)
            .Include(d => d.Detalles)
                .ThenInclude(det => det.Producto)
            .Include(d => d.Referencias)
            .Include(d => d.MediosPago)
            .Include(d => d.OtraInformacion)
            .Include(d => d.Exportacion)
            .FirstOrDefaultAsync(d => d.Id == documentoId);
    }

    /// <summary>
    /// Procesa el inventario según el tipo de documento aceptado
    /// - Factura/Tiquete: Reduce inventario (venta)
    /// - Nota de Crédito: Aumenta inventario (devolución)
    /// </summary>
    private async Task ProcesarInventarioDocumentoAsync(Documento documento)
    {
        try
        {
            _logger.LogInformation(
                "=== INICIO ProcesarInventarioDocumentoAsync === DocId: {DocId}, Tipo: {Tipo}, Consecutivo: {Consecutivo}, EsRecibido: {EsRecibido}",
                documento.Id, documento.TipoDocumento, documento.NumeroConsecutivo, documento.EsDocumentoRecibido);

            // Solo procesar documentos emitidos (no recibidos)
            if (documento.EsDocumentoRecibido)
            {
                _logger.LogInformation("Documento es recibido, no se procesa inventario");
                return;
            }

            // Obtener el userId del creador del documento o usar "system"
            var userId = documento.UsuarioCreacionId ?? "system";
            _logger.LogInformation("UserId para movimiento: {UserId}, SucursalId: {SucursalId}", userId, documento.SucursalId);

            switch (documento.TipoDocumento)
            {
                case DocumentoTipo.FacturaElectronica:
                case DocumentoTipo.TiqueteElectronico:
                case DocumentoTipo.FacturaElectronicaExportacion:
                    // Ventas: Reducir inventario
                    _logger.LogInformation("Procesando como VENTA - Tipo: {Tipo}", documento.TipoDocumento);
                    var resultadoVenta = await _inventarioRepository.ProcesarVentaDocumentoAsync(
                        documento.Id,
                        documento.SucursalId,
                        userId);
                    _logger.LogInformation(
                        "Inventario procesado para venta - Documento: {NumeroConsecutivo}, Resultado: {Resultado}",
                        documento.NumeroConsecutivo, resultadoVenta);
                    break;

                case DocumentoTipo.NotaCreditoElectronica:
                    // Devolución: Aumentar inventario
                    _logger.LogInformation("Procesando como DEVOLUCIÓN - Tipo: {Tipo}", documento.TipoDocumento);
                    var resultadoDevolucion = await _inventarioRepository.ProcesarDevolucionDocumentoAsync(
                        documento.Id,
                        documento.SucursalId,
                        userId);
                    _logger.LogInformation(
                        "Inventario procesado para devolución - NC: {NumeroConsecutivo}, Resultado: {Resultado}",
                        documento.NumeroConsecutivo, resultadoDevolucion);
                    break;

                // Otros tipos de documento no afectan inventario
                default:
                    _logger.LogInformation("Tipo de documento {Tipo} no afecta inventario", documento.TipoDocumento);
                    break;
            }

            _logger.LogInformation("=== FIN ProcesarInventarioDocumentoAsync ===");
        }
        catch (Exception ex)
        {
            // Log el error pero no fallar el proceso de aceptación
            _logger.LogError(ex,
                "Error al procesar inventario para documento {DocumentoId}: {Error}",
                documento.Id, ex.Message);
        }
    }

    /// <summary>
    /// Consulta el estado de un documento con reintentos progresivos
    /// Estrategia: 5s, 10s, 20s, 40s, 60s (total ~2.5 minutos de reintentos)
    /// </summary>
    private async Task ConsultarEstadoConReintentosAsync(Guid documentoId)
    {
        try
        {
            // Configuración de reintentos progresivos
            var intervalos = new[] { 5, 10, 20, 40, 60 }; // segundos
            var maxIntentos = intervalos.Length;

            _logger.LogInformation(
                "Iniciando consultas automáticas con reintentos progresivos para documento {DocumentoId}",
                documentoId);

            for (int intento = 1; intento <= maxIntentos; intento++)
            {
                try
                {
                    // Esperar antes de consultar (excepto en el primer intento que ya esperó 5s)
                    var delay = intervalos[intento - 1];
                    _logger.LogDebug(
                        "Esperando {Delay}s antes del intento {Intento}/{Max} para documento {DocumentoId}",
                        delay, intento, maxIntentos, documentoId);

                    await Task.Delay(TimeSpan.FromSeconds(delay));

                    // Consultar estado
                    _logger.LogInformation(
                        "Intento {Intento}/{Max}: Consultando estado de documento {DocumentoId}",
                        intento, maxIntentos, documentoId);

                    var resultado = await ConsultarEstadoAsync(documentoId);

                    // Si se obtuvo un estado final, detener los reintentos
                    if (!string.IsNullOrEmpty(resultado.Estado))
                    {
                        var estadoFinal = resultado.Estado.ToLowerInvariant();

                        if (estadoFinal == "aceptado" || estadoFinal == "rechazado")
                        {
                            _logger.LogInformation(
                                "Documento {DocumentoId} obtuvo estado final '{Estado}' en el intento {Intento}. " +
                                "Finalizando reintentos automáticos",
                                documentoId, resultado.Estado, intento);
                            return;
                        }
                        else if (estadoFinal == "procesando" || estadoFinal == "enviado")
                        {
                            _logger.LogDebug(
                                "Documento {DocumentoId} aún en estado '{Estado}'. Continuando reintentos...",
                                documentoId, resultado.Estado);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Documento {DocumentoId} tiene estado desconocido '{Estado}'. Continuando reintentos...",
                                documentoId, resultado.Estado);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex,
                        "Error HTTP en intento {Intento}/{Max} para documento {DocumentoId}: {Message}. " +
                        "Continuando con siguientes reintentos...",
                        intento, maxIntentos, documentoId, ex.Message);
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning(ex,
                        "Timeout en intento {Intento}/{Max} para documento {DocumentoId}. " +
                        "Continuando con siguientes reintentos...",
                        intento, maxIntentos, documentoId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Error en intento {Intento}/{Max} para documento {DocumentoId}: {Message}. " +
                        "Continuando con siguientes reintentos...",
                        intento, maxIntentos, documentoId, ex.Message);
                }
            }

            _logger.LogInformation(
                "Finalizados los {MaxIntentos} reintentos automáticos para documento {DocumentoId}. " +
                "El BackgroundService continuará consultando cada 30 segundos",
                maxIntentos, documentoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error crítico en ConsultarEstadoConReintentosAsync para documento {DocumentoId}",
                documentoId);
        }
    }

    #endregion
}
