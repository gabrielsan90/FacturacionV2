using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Enums;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio para validar documentos XML contra esquemas XSD v4.4 de Hacienda
/// Valida que los XML generados cumplan con los esquemas oficiales antes de enviarlos
/// </summary>
public class XsdValidacionService : IXsdValidacionService
{
    private readonly ILogger<XsdValidacionService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _rutaBaseXsd;

    public XsdValidacionService(
        ILogger<XsdValidacionService> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;

        // La ruta base a los esquemas XSD es /4.4/ desde la raíz del proyecto
        // En producción los archivos XSD deben estar en la misma ubicación
        _rutaBaseXsd = Path.Combine(Directory.GetParent(_environment.ContentRootPath)!.FullName, "4.4");

        _logger.LogInformation("XsdValidacionService inicializado. Ruta base XSD: {RutaBase}", _rutaBaseXsd);
    }

    /// <summary>
    /// Valida un XML generado contra el esquema XSD correspondiente
    /// </summary>
    public async Task<ResultadoValidacionXsd> ValidarXmlContraXsdAsync(string xml, DocumentoTipo tipoDocumento)
    {
        var resultado = new ResultadoValidacionXsd
        {
            EsValido = false,
            TipoDocumento = tipoDocumento.ToString()
        };

        try
        {
            // Validar que el XML no esté vacío
            if (string.IsNullOrWhiteSpace(xml))
            {
                resultado.Errores.Add("El XML está vacío o nulo");
                return resultado;
            }

            // Obtener la ruta del XSD correspondiente
            var rutaXsd = ObtenerRutaXsd(tipoDocumento);
            resultado.RutaXsd = rutaXsd;

            // Validar que el archivo XSD exista
            if (!File.Exists(rutaXsd))
            {
                resultado.Errores.Add($"No se encontró el archivo XSD en la ruta: {rutaXsd}");
                _logger.LogError("Archivo XSD no encontrado: {RutaXsd}", rutaXsd);
                return resultado;
            }

            _logger.LogInformation("Validando XML contra XSD: {RutaXsd}", rutaXsd);

            // Cargar el XML
            XDocument xmlDoc;
            try
            {
                xmlDoc = XDocument.Parse(xml);
            }
            catch (Exception ex)
            {
                resultado.Errores.Add($"Error al parsear el XML: {ex.Message}");
                _logger.LogError(ex, "Error al parsear XML para validación XSD");
                return resultado;
            }

            // Configurar el esquema XSD
            var schemas = new XmlSchemaSet();

            try
            {
                // Cargar el esquema principal
                schemas.Add(null, rutaXsd);

                // IMPORTANTE: Configurar el resolvedor de esquemas para dependencias
                // Los XSD de Hacienda referencian xmldsig-core-schema.xsd
                schemas.XmlResolver = new XmlUrlResolver();
            }
            catch (Exception ex)
            {
                resultado.Errores.Add($"Error al cargar el esquema XSD: {ex.Message}");
                _logger.LogError(ex, "Error al cargar esquema XSD: {RutaXsd}", rutaXsd);
                return resultado;
            }

            // Lista para recolectar errores de validación
            var erroresValidacion = new List<string>();
            var advertenciasValidacion = new List<string>();

            // Configurar el manejador de eventos de validación
            void ValidationEventHandler(object? sender, ValidationEventArgs e)
            {
                var mensaje = $"Línea {e.Exception?.LineNumber}, Posición {e.Exception?.LinePosition}: {e.Message}";

                if (e.Severity == XmlSeverityType.Error)
                {
                    erroresValidacion.Add(mensaje);
                    _logger.LogWarning("Error de validación XSD: {Mensaje}", mensaje);
                }
                else if (e.Severity == XmlSeverityType.Warning)
                {
                    advertenciasValidacion.Add(mensaje);
                    _logger.LogInformation("Advertencia de validación XSD: {Mensaje}", mensaje);
                }
            }

            // Validar el documento
            try
            {
                await Task.Run(() =>
                {
                    xmlDoc.Validate(schemas, ValidationEventHandler);
                });

                // Si no hay errores, la validación fue exitosa
                if (erroresValidacion.Count == 0)
                {
                    resultado.EsValido = true;
                    resultado.Advertencias = advertenciasValidacion;
                    _logger.LogInformation("Validación XSD exitosa para documento tipo {TipoDocumento}", tipoDocumento);
                }
                else
                {
                    resultado.EsValido = false;
                    resultado.Errores = erroresValidacion;
                    resultado.Advertencias = advertenciasValidacion;
                    _logger.LogWarning("Validación XSD falló para documento tipo {TipoDocumento}. Errores: {CantidadErrores}",
                        tipoDocumento, erroresValidacion.Count);
                }
            }
            catch (Exception ex)
            {
                resultado.Errores.Add($"Error durante la validación: {ex.Message}");
                _logger.LogError(ex, "Error durante la validación XSD");
                return resultado;
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en ValidarXmlContraXsdAsync para tipo {TipoDocumento}", tipoDocumento);
            resultado.Errores.Add($"Error inesperado: {ex.Message}");
            return resultado;
        }
    }

    /// <summary>
    /// Obtiene la ruta del archivo XSD correspondiente al tipo de documento
    /// </summary>
    public string ObtenerRutaXsd(DocumentoTipo tipoDocumento)
    {
        var nombreArchivo = tipoDocumento switch
        {
            DocumentoTipo.FacturaElectronica => "FacturaElectronica_V4.4.xsd",
            DocumentoTipo.TiqueteElectronico => "TiqueteElectronico_V4.4.xsd",
            DocumentoTipo.NotaCreditoElectronica => "NotaCreditoElectronica_V4.4.xsd",
            DocumentoTipo.NotaDebitoElectronica => "NotaDebitoElectronica_V4.4.xsd",
            DocumentoTipo.FacturaElectronicaExportacion => "FacturaElectronicaExportacion_V4.4.xsd",
            DocumentoTipo.FacturaElectronicaCompra => "FacturaElectronicaCompra_V4.4.xsd",
            DocumentoTipo.NotaCreditoElectronicaCompra => "NotaCreditoElectronica_V4.4.xsd", // Usa el mismo XSD que NC normal
            DocumentoTipo.NotaDebitoElectronicaCompra => "NotaDebitoElectronica_V4.4.xsd", // Usa el mismo XSD que ND normal
            DocumentoTipo.ReciboElectronicoPago => "ReciboElectronicoPago_V4.4.xsd",
            _ => throw new NotSupportedException($"No hay esquema XSD definido para el tipo de documento: {tipoDocumento}")
        };

        return Path.Combine(_rutaBaseXsd, nombreArchivo);
    }

    /// <summary>
    /// Valida que todos los archivos XSD necesarios existan
    /// Útil para diagnóstico y verificación de instalación
    /// </summary>
    public bool ValidarExistenciaEsquemasXsd()
    {
        try
        {
            _logger.LogInformation("Validando existencia de esquemas XSD en: {RutaBase}", _rutaBaseXsd);

            if (!Directory.Exists(_rutaBaseXsd))
            {
                _logger.LogError("El directorio de esquemas XSD no existe: {RutaBase}", _rutaBaseXsd);
                return false;
            }

            // Lista de archivos XSD requeridos
            var archivosRequeridos = new[]
            {
                "FacturaElectronica_V4.4.xsd",
                "TiqueteElectronico_V4.4.xsd",
                "NotaCreditoElectronica_V4.4.xsd",
                "NotaDebitoElectronica_V4.4.xsd",
                "FacturaElectronicaExportacion_V4.4.xsd",
                "FacturaElectronicaCompra_V4.4.xsd",
                "ReciboElectronicoPago_V4.4.xsd",
                "MensajeHacienda_V4.4.xsd",
                "MensajeReceptor_V4.4.xsd"
            };

            var todosPresentses = true;
            foreach (var archivo in archivosRequeridos)
            {
                var rutaCompleta = Path.Combine(_rutaBaseXsd, archivo);
                if (!File.Exists(rutaCompleta))
                {
                    _logger.LogWarning("Archivo XSD faltante: {Archivo}", archivo);
                    todosPresentses = false;
                }
                else
                {
                    _logger.LogDebug("Archivo XSD encontrado: {Archivo}", archivo);
                }
            }

            if (todosPresentses)
            {
                _logger.LogInformation("Todos los archivos XSD requeridos están presentes");
            }
            else
            {
                _logger.LogWarning("Faltan algunos archivos XSD requeridos");
            }

            return todosPresentses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar existencia de esquemas XSD");
            return false;
        }
    }
}
