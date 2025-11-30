using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

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
    private readonly ILogger<DocumentoHaciendaService> _logger;

    public DocumentoHaciendaService(
        DataContext context,
        IClaveGeneradorService claveGenerador,
        IXmlGeneradorService xmlGenerador,
        IFirmaDigitalService firmaDigital,
        IHaciendaApiService haciendaApi,
        ILogger<DocumentoHaciendaService> logger)
    {
        _context = context;
        _claveGenerador = claveGenerador;
        _xmlGenerador = xmlGenerador;
        _firmaDigital = firmaDigital;
        _haciendaApi = haciendaApi;
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
            documento.Estado = EstadoDocumento.Pendiente;
            await _context.SaveChangesAsync();

            var xmlGenerado = await _xmlGenerador.GenerarXmlAsync(documento);
            documento.XmlGenerado = xmlGenerado;
            resultado.XmlGenerado = xmlGenerado;

            await _context.SaveChangesAsync();

            // 7. Firmar XML
            _logger.LogInformation("Firmando XML para documento {DocumentoId}", documentoId);

            try
            {
                var certificado = await _firmaDigital.ObtenerCertificadoAsync(empresa.Id);
                var xmlFirmado = await _firmaDigital.FirmarXmlAsync(xmlGenerado, certificado, empresa.PinCertificado);
                documento.XmlFirmado = xmlFirmado;
                documento.FechaFirma = DateTime.Now;
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

            // 8. Enviar a Hacienda
            _logger.LogInformation("Enviando documento {DocumentoId} a Hacienda", documentoId);
            documento.Estado = EstadoDocumento.Procesando;
            documento.FechaEnvioHacienda = DateTime.Now;
            await _context.SaveChangesAsync();

            string ambiente = empresa.Ambiente == Ambiente.Produccion ? "prod" : "stag";

            var respuestaHacienda = await _haciendaApi.EnviarDocumentoAsync(
                documento.Clave,
                documento.XmlFirmado!,
                empresa.UsuarioHacienda,
                empresa.ClaveHacienda,
                ambiente
            );

            resultado.RespuestaHacienda = respuestaHacienda;

            // 9. Procesar respuesta según el código HTTP
            documento.FechaRespuestaHacienda = DateTime.Now;
            documento.XmlRespuestaHacienda = respuestaHacienda.RespuestaXml;

            var estadoRespuesta = respuestaHacienda.IndEstado.ToLower();

            if (estadoRespuesta == "enviado")
            {
                // 201/202: Documento recibido por Hacienda y en proceso de validación
                documento.Estado = EstadoDocumento.Procesando;
                documento.MensajeHacienda = "Documento enviado exitosamente a Hacienda y en proceso de validación";
                resultado.Exitoso = true;
                resultado.Mensaje = "Documento enviado exitosamente a Hacienda";
                resultado.Estado = "Enviado";

                _logger.LogInformation("Documento {DocumentoId} enviado exitosamente a Hacienda (HTTP 201/202)", documentoId);
            }
            else if (estadoRespuesta == "aceptado")
            {
                // Respuesta posterior indica que fue aceptado
                documento.Estado = EstadoDocumento.Aceptado;
                documento.MensajeHacienda = "Documento aceptado por Hacienda";
                resultado.Exitoso = true;
                resultado.Mensaje = "Documento aceptado exitosamente por Hacienda";
                resultado.Estado = "Aceptado";

                _logger.LogInformation("Documento {DocumentoId} aceptado por Hacienda", documentoId);
            }
            else if (estadoRespuesta == "rechazado")
            {
                // 400 Bad Request: Error de validación
                documento.Estado = EstadoDocumento.Rechazado;

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
                // Mantener estado anterior y registrar el error
                var mensajes = string.Join("; ", respuestaHacienda.Mensajes.Select(m => m.Mensaje));
                documento.MensajeHacienda = $"Error técnico: {mensajes}";

                resultado.Exitoso = false;
                resultado.Mensaje = "Error técnico al comunicarse con Hacienda";
                resultado.Estado = "Error";
                resultado.Errores = respuestaHacienda.Mensajes.Select(m =>
                    string.IsNullOrWhiteSpace(m.Detalle) ? m.Mensaje : $"{m.Mensaje}: {m.Detalle}"
                ).ToList();

                _logger.LogError("Error técnico al enviar documento {DocumentoId}: {Mensajes}",
                    documentoId, mensajes);
            }
            else // procesando u otro estado
            {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar documento {DocumentoId}", documentoId);
            resultado.Mensaje = "Error al procesar el documento";
            resultado.Errores.Add($"Error interno: {ex.Message}");
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
            FechaConsulta = DateTime.Now
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

            var respuestaHacienda = await _haciendaApi.ConsultarEstadoAsync(
                documento.Clave,
                empresa.UsuarioHacienda,
                empresa.ClaveHacienda!,
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
                documento.FechaRespuestaHacienda = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            else if (respuestaHacienda.IndEstado.ToLower() == "rechazado" &&
                     documento.Estado != EstadoDocumento.Rechazado)
            {
                var mensajes = string.Join("; ", respuestaHacienda.Mensajes.Select(m => m.Mensaje));
                documento.Estado = EstadoDocumento.Rechazado;
                documento.MensajeHacienda = $"Rechazado: {mensajes}";
                documento.FechaRespuestaHacienda = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            resultado.Exitoso = true;
            resultado.Mensaje = $"Estado actual: {respuestaHacienda.IndEstado}";

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar estado del documento {DocumentoId}", documentoId);
            resultado.Mensaje = $"Error al consultar estado: {ex.Message}";
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

        // Validar que el subtotal calculado coincida
        var subtotalCalculado = documento.Detalles?.Sum(d => d.MontoTotalLinea) ?? 0;
        if (Math.Abs(subtotalCalculado - documento.TotalVenta) > 0.01m)
        {
            errores.Add($"El total del documento ({documento.TotalVenta}) no coincide con la suma de las líneas ({subtotalCalculado})");
        }

        // Validar plazo de crédito si la condición de venta es crédito
        if (documento.CondicionVenta == "02" && !documento.PlazoCreditoDias.HasValue)
        {
            errores.Add("Falta el plazo de crédito (condición de venta es crédito)");
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

    #endregion
}
