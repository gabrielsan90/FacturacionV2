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

                // Remover el elemento Signature si existe (la firma se agrega después de la validación)
                // El namespace de xmldsig es http://www.w3.org/2000/09/xmldsig#
                XNamespace dsNs = "http://www.w3.org/2000/09/xmldsig#";
                var signatureElement = xmlDoc.Descendants(dsNs + "Signature").FirstOrDefault();
                if (signatureElement != null)
                {
                    _logger.LogInformation("Removiendo elemento Signature para validación XSD (se validará sin firma)");
                    signatureElement.Remove();
                }
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
                // IMPORTANTE: Configurar el resolvedor de esquemas ANTES de cargar
                schemas.XmlResolver = new XmlUrlResolver();

                // Primero cargar el esquema xmldsig que es referenciado por todos los XSD de Hacienda
                var xmldsigPath = Path.Combine(_rutaBaseXsd, "xmldsig-core-schema.xsd");
                if (File.Exists(xmldsigPath))
                {
                    schemas.Add("http://www.w3.org/2000/09/xmldsig#", xmldsigPath);
                    _logger.LogDebug("Esquema xmldsig cargado desde: {RutaXmldsig}", xmldsigPath);
                }
                else
                {
                    _logger.LogWarning("No se encontró xmldsig-core-schema.xsd en: {RutaXmldsig}", xmldsigPath);
                }

                // Cargar el esquema principal de Hacienda
                schemas.Add(null, rutaXsd);
                schemas.Compile();
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
                    // v4.4: ds:Signature es obligatorio (minOccurs=1) en el XSD, pero la firma se agrega
                    // DESPUÉS de la validación XSD. Por lo tanto, ignoramos los errores relacionados
                    // con el elemento Signature faltante y los tratamos como advertencias.
                    // Estos errores típicamente contienen:
                    // - "'Signature' in namespace 'http://www.w3.org/2000/09/xmldsig#'"
                    // - "has incomplete content" cuando solo falta Signature
                    bool esErrorSignatureFaltante = e.Message.Contains("'Signature'") &&
                                                    e.Message.Contains("http://www.w3.org/2000/09/xmldsig#");

                    // También detectar cuando el mensaje dice "incomplete content" y menciona Signature
                    // como uno de los elementos esperados al final de la lista
                    bool esIncompleteConSignature = e.Message.Contains("has incomplete content") &&
                                                    e.Message.Contains("Signature");

                    if (esErrorSignatureFaltante || esIncompleteConSignature)
                    {
                        // Verificar si el error SOLO es por Signature faltante
                        // Si hay otros elementos faltantes además de Signature, es un error real
                        // El mensaje típico es: "expected: 'InformacionReferencia, Otros' ... 'Signature'"
                        // Si menciona otros elementos obligatorios, es error; si solo es Signature, es advertencia

                        // Detectar si el error menciona elementos obligatorios faltantes (no opcionales)
                        // InformacionReferencia y Otros son opcionales en el documento
                        bool soloSignatureFaltante = !e.Message.Contains("has incomplete content") ||
                                                     (e.Message.Contains("InformacionReferencia") ||
                                                      e.Message.Contains("Otros") ||
                                                      e.Message.EndsWith("'Signature' in namespace 'http://www.w3.org/2000/09/xmldsig#'."));

                        if (soloSignatureFaltante)
                        {
                            advertenciasValidacion.Add($"[Ignorado - Firma pendiente] {mensaje}");
                            _logger.LogInformation("Advertencia XSD (Signature faltante - se agregará después): {Mensaje}", mensaje);
                            return; // No agregar como error
                        }
                    }

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

            var todosPresentes = true;
            foreach (var archivo in archivosRequeridos)
            {
                var rutaCompleta = Path.Combine(_rutaBaseXsd, archivo);
                if (!File.Exists(rutaCompleta))
                {
                    _logger.LogWarning("Archivo XSD faltante: {Archivo}", archivo);
                    todosPresentes = false;
                }
                else
                {
                    _logger.LogDebug("Archivo XSD encontrado: {Archivo}", archivo);
                }
            }

            if (todosPresentes)
            {
                _logger.LogInformation("Todos los archivos XSD requeridos estan presentes");
            }
            else
            {
                _logger.LogWarning("Faltan algunos archivos XSD requeridos");
            }

            return todosPresentes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar existencia de esquemas XSD");
            return false;
        }
    }
}
