using System.Globalization;
using System.Xml.Linq;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Enums;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio para parsear XML de documentos electrónicos recibidos según Hacienda v4.4
/// Resolución MH-DGT-RES-0027-2024
/// </summary>
public class XmlParserService : IXmlParserService
{
    // Namespaces oficiales de Hacienda v4.3
    private static readonly XNamespace NsFE_43 = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/facturaElectronica";
    private static readonly XNamespace NsTE_43 = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/tiqueteElectronico";
    private static readonly XNamespace NsNC_43 = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/notaCreditoElectronica";
    private static readonly XNamespace NsND_43 = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/notaDebitoElectronica";
    private static readonly XNamespace NsFEE_43 = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/facturaElectronicaExportacion";

    // Namespaces oficiales de Hacienda v4.4
    private static readonly XNamespace NsFE_44 = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica";
    private static readonly XNamespace NsTE_44 = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/tiqueteElectronico";
    private static readonly XNamespace NsNC_44 = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaCreditoElectronica";
    private static readonly XNamespace NsND_44 = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/notaDebitoElectronica";
    private static readonly XNamespace NsFEE_44 = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaExportacion";

    /// <summary>
    /// Determina el tipo de documento y retorna (tipo, namespace) para el namespace del XML
    /// </summary>
    private static (DocumentoTipo tipo, string metodo)? ResolverTipoDocumento(XNamespace ns)
    {
        var nsStr = ns.NamespaceName;

        if (ns == NsFE_43 || ns == NsFE_44) return (DocumentoTipo.FacturaElectronica, "FE");
        if (ns == NsTE_43 || ns == NsTE_44) return (DocumentoTipo.TiqueteElectronico, "TE");
        if (ns == NsNC_43 || ns == NsNC_44) return (DocumentoTipo.NotaCreditoElectronica, "NC");
        if (ns == NsND_43 || ns == NsND_44) return (DocumentoTipo.NotaDebitoElectronica, "ND");
        if (ns == NsFEE_43 || ns == NsFEE_44) return (DocumentoTipo.FacturaElectronicaExportacion, "FEE");

        return null;
    }

    /// <inheritdoc/>
    public async Task<DocumentoRecibido> ParseXmlAsync(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
            throw new ArgumentException("El contenido XML no puede estar vacío", nameof(xmlContent));

        return await Task.Run(() =>
        {
            try
            {
                var doc = XDocument.Parse(xmlContent);
                var root = doc.Root ?? throw new InvalidOperationException("El XML no tiene un elemento raíz válido");

                // Determinar el tipo de documento por el namespace
                var ns = root.Name.Namespace;
                var tipoInfo = ResolverTipoDocumento(ns)
                    ?? throw new NotSupportedException($"Tipo de documento no soportado. Namespace: {ns}");

                return tipoInfo.metodo switch
                {
                    "FE" => ParseFacturaElectronica(doc, xmlContent, ns),
                    "TE" => ParseTiqueteElectronico(doc, xmlContent, ns),
                    "NC" => ParseNotaCredito(doc, xmlContent, ns),
                    "ND" => ParseNotaDebito(doc, xmlContent, ns),
                    "FEE" => ParseFacturaExportacion(doc, xmlContent, ns),
                    _ => throw new NotSupportedException($"Tipo de documento no soportado. Namespace: {ns}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al parsear el XML: {ex.Message}", ex);
            }
        });
    }

    /// <inheritdoc/>
    public async Task<bool> ValidarEstructuraXmlAsync(string xmlContent)
    {
        return await Task.Run(() =>
        {
            try
            {
                var doc = XDocument.Parse(xmlContent);
                var root = doc.Root;

                if (root == null)
                    return false;

                // Validar que tenga elementos básicos requeridos
                var ns = root.Name.Namespace;
                var clave = root.Element(ns + "Clave")?.Value;
                var fecha = root.Element(ns + "FechaEmision")?.Value;

                return !string.IsNullOrWhiteSpace(clave) &&
                       clave.Length == 50 &&
                       !string.IsNullOrWhiteSpace(fecha);
            }
            catch
            {
                return false;
            }
        });
    }

    /// <inheritdoc/>
    public string ExtraerClave(string xmlContent)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            var root = doc.Root ?? throw new InvalidOperationException("XML sin elemento raíz");
            var ns = root.Name.Namespace;

            return root.Element(ns + "Clave")?.Value
                ?? throw new InvalidOperationException("No se encontró el elemento Clave en el XML");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al extraer la clave: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public string ExtraerTipoDocumento(string xmlContent)
    {
        try
        {
            var clave = ExtraerClave(xmlContent);
            // Posiciones 29-30 contienen el tipo de documento
            return clave.Substring(29, 2);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al extraer el tipo de documento: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public string ExtraerConsecutivo(string xmlContent)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            var root = doc.Root ?? throw new InvalidOperationException("XML sin elemento raíz");
            var ns = root.Name.Namespace;

            return root.Element(ns + "NumeroConsecutivo")?.Value
                ?? throw new InvalidOperationException("No se encontró el elemento NumeroConsecutivo en el XML");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al extraer el consecutivo: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public bool EsDocumentoRecibible(string xmlContent)
    {
        try
        {
            var tipoDoc = ExtraerTipoDocumento(xmlContent);
            // 01=FE, 02=ND, 03=NC, 04=TE, 09=FEE son documentos que se pueden recibir
            return tipoDoc == "01" || tipoDoc == "02" || tipoDoc == "03" || tipoDoc == "04" || tipoDoc == "09";
        }
        catch
        {
            return false;
        }
    }

    #region Métodos de Parseo por Tipo de Documento

    private DocumentoRecibido ParseFacturaElectronica(XDocument doc, string xmlOriginal, XNamespace ns)
    {
        var root = doc.Root!;

        var documento = new DocumentoRecibido
        {
            Clave = root.Element(ns + "Clave")!.Value,
            NumeroConsecutivo = root.Element(ns + "NumeroConsecutivo")!.Value,
            TipoDocumento = DocumentoTipo.FacturaElectronica,
            FechaEmision = DateTime.Parse(root.Element(ns + "FechaEmision")!.Value),
            XmlOriginal = xmlOriginal
        };

        // Parsear Emisor
        ParseEmisor(root.Element(ns + "Emisor")!, documento, ns);

        // Parsear Receptor
        ParseReceptor(root.Element(ns + "Receptor"), documento, ns);

        // Parsear CondicionVenta
        documento.CondicionVenta = root.Element(ns + "CondicionVenta")!.Value;
        var plazoCredito = root.Element(ns + "PlazoCredito")?.Value;
        if (!string.IsNullOrWhiteSpace(plazoCredito))
            documento.PlazoCreditoDias = int.Parse(plazoCredito);

        // Parsear MedioPago
        documento.MedioPago = root.Element(ns + "MedioPago")?.Value ?? "01";

        // Parsear ResumenFactura
        var resumen = root.Element(ns + "ResumenFactura")!;
        ParseResumen(resumen, documento, ns);

        // Parsear Detalles
        var detalles = root.Element(ns + "DetalleServicio")?.Elements(ns + "LineaDetalle");
        if (detalles != null)
        {
            foreach (var linea in detalles)
            {
                documento.Detalles.Add(ParseLineaDetalle(linea, ns));
            }
        }

        // Observaciones
        documento.Observaciones = root.Element(ns + "Otros")?.Element(ns + "OtroTexto")?.Value;

        return documento;
    }

    private DocumentoRecibido ParseTiqueteElectronico(XDocument doc, string xmlOriginal, XNamespace ns)
    {
        var root = doc.Root!;

        var documento = new DocumentoRecibido
        {
            Clave = root.Element(ns + "Clave")!.Value,
            NumeroConsecutivo = root.Element(ns + "NumeroConsecutivo")!.Value,
            TipoDocumento = DocumentoTipo.TiqueteElectronico,
            FechaEmision = DateTime.Parse(root.Element(ns + "FechaEmision")!.Value),
            XmlOriginal = xmlOriginal
        };

        // Parsear Emisor
        ParseEmisor(root.Element(ns + "Emisor")!, documento, ns);

        // El tiquete no tiene receptor obligatorio
        ParseReceptor(root.Element(ns + "Receptor"), documento, ns);

        // Parsear CondicionVenta
        documento.CondicionVenta = root.Element(ns + "CondicionVenta")!.Value;

        // Parsear MedioPago
        documento.MedioPago = root.Element(ns + "MedioPago")?.Value ?? "01";

        // Parsear ResumenFactura
        var resumen = root.Element(ns + "ResumenFactura")!;
        ParseResumen(resumen, documento, ns);

        // Parsear Detalles
        var detalles = root.Element(ns + "DetalleServicio")?.Elements(ns + "LineaDetalle");
        if (detalles != null)
        {
            foreach (var linea in detalles)
            {
                documento.Detalles.Add(ParseLineaDetalle(linea, ns));
            }
        }

        return documento;
    }

    private DocumentoRecibido ParseNotaCredito(XDocument doc, string xmlOriginal, XNamespace ns)
    {
        var root = doc.Root!;

        var documento = new DocumentoRecibido
        {
            Clave = root.Element(ns + "Clave")!.Value,
            NumeroConsecutivo = root.Element(ns + "NumeroConsecutivo")!.Value,
            TipoDocumento = DocumentoTipo.NotaCreditoElectronica,
            FechaEmision = DateTime.Parse(root.Element(ns + "FechaEmision")!.Value),
            XmlOriginal = xmlOriginal
        };

        // Parsear Emisor
        ParseEmisor(root.Element(ns + "Emisor")!, documento, ns);

        // Parsear Receptor
        ParseReceptor(root.Element(ns + "Receptor"), documento, ns);

        // Parsear CondicionVenta
        documento.CondicionVenta = root.Element(ns + "CondicionVenta")!.Value;

        // Parsear MedioPago
        documento.MedioPago = root.Element(ns + "MedioPago")?.Value ?? "01";

        // Parsear ResumenFactura
        var resumen = root.Element(ns + "ResumenFactura")!;
        ParseResumen(resumen, documento, ns);

        // Parsear Detalles
        var detalles = root.Element(ns + "DetalleServicio")?.Elements(ns + "LineaDetalle");
        if (detalles != null)
        {
            foreach (var linea in detalles)
            {
                documento.Detalles.Add(ParseLineaDetalle(linea, ns));
            }
        }

        return documento;
    }

    private DocumentoRecibido ParseNotaDebito(XDocument doc, string xmlOriginal, XNamespace ns)
    {
        var root = doc.Root!;

        var documento = new DocumentoRecibido
        {
            Clave = root.Element(ns + "Clave")!.Value,
            NumeroConsecutivo = root.Element(ns + "NumeroConsecutivo")!.Value,
            TipoDocumento = DocumentoTipo.NotaDebitoElectronica,
            FechaEmision = DateTime.Parse(root.Element(ns + "FechaEmision")!.Value),
            XmlOriginal = xmlOriginal
        };

        // Parsear Emisor
        ParseEmisor(root.Element(ns + "Emisor")!, documento, ns);

        // Parsear Receptor
        ParseReceptor(root.Element(ns + "Receptor"), documento, ns);

        // Parsear CondicionVenta
        documento.CondicionVenta = root.Element(ns + "CondicionVenta")!.Value;

        // Parsear MedioPago
        documento.MedioPago = root.Element(ns + "MedioPago")?.Value ?? "01";

        // Parsear ResumenFactura
        var resumen = root.Element(ns + "ResumenFactura")!;
        ParseResumen(resumen, documento, ns);

        // Parsear Detalles
        var detalles = root.Element(ns + "DetalleServicio")?.Elements(ns + "LineaDetalle");
        if (detalles != null)
        {
            foreach (var linea in detalles)
            {
                documento.Detalles.Add(ParseLineaDetalle(linea, ns));
            }
        }

        return documento;
    }

    private DocumentoRecibido ParseFacturaExportacion(XDocument doc, string xmlOriginal, XNamespace ns)
    {
        var root = doc.Root!;

        var documento = new DocumentoRecibido
        {
            Clave = root.Element(ns + "Clave")!.Value,
            NumeroConsecutivo = root.Element(ns + "NumeroConsecutivo")!.Value,
            TipoDocumento = DocumentoTipo.FacturaElectronicaExportacion,
            FechaEmision = DateTime.Parse(root.Element(ns + "FechaEmision")!.Value),
            XmlOriginal = xmlOriginal
        };

        // Parsear Emisor
        ParseEmisor(root.Element(ns + "Emisor")!, documento, ns);

        // Parsear Receptor
        ParseReceptor(root.Element(ns + "Receptor"), documento, ns);

        // Parsear CondicionVenta
        documento.CondicionVenta = root.Element(ns + "CondicionVenta")!.Value;

        // Parsear MedioPago
        documento.MedioPago = root.Element(ns + "MedioPago")?.Value ?? "01";

        // Parsear ResumenFactura
        var resumen = root.Element(ns + "ResumenFactura")!;
        ParseResumen(resumen, documento, ns);

        // Parsear Detalles
        var detalles = root.Element(ns + "DetalleServicio")?.Elements(ns + "LineaDetalle");
        if (detalles != null)
        {
            foreach (var linea in detalles)
            {
                documento.Detalles.Add(ParseLineaDetalle(linea, ns));
            }
        }

        return documento;
    }

    #endregion

    #region Métodos Auxiliares de Parseo

    private void ParseEmisor(XElement emisor, DocumentoRecibido documento, XNamespace ns)
    {
        var identificacion = emisor.Element(ns + "Identificacion")!;
        var tipoId = identificacion.Element(ns + "Tipo")!.Value;
        documento.EmisorTipoIdentificacion = ParseTipoIdentificacion(tipoId);
        documento.EmisorNumeroIdentificacion = identificacion.Element(ns + "Numero")!.Value;

        documento.EmisorNombre = emisor.Element(ns + "Nombre")!.Value;
        documento.EmisorNombreComercial = emisor.Element(ns + "NombreComercial")?.Value;

        // NUEVO v4.4: ActividadEconomica es obligatorio
        documento.EmisorActividadEconomica = emisor.Element(ns + "ActividadEconomica")?.Value;
    }

    private void ParseReceptor(XElement? receptor, DocumentoRecibido documento, XNamespace ns)
    {
        if (receptor == null)
            return;

        var identificacion = receptor.Element(ns + "Identificacion");
        if (identificacion != null)
        {
            var tipoId = identificacion.Element(ns + "Tipo")?.Value;
            if (!string.IsNullOrWhiteSpace(tipoId))
                documento.ReceptorTipoIdentificacion = ParseTipoIdentificacion(tipoId);

            documento.ReceptorNumeroIdentificacion = identificacion.Element(ns + "Numero")?.Value;
        }

        documento.ReceptorNombre = receptor.Element(ns + "Nombre")?.Value;

        // NUEVO v4.4: ActividadEconomicaReceptor
        documento.ReceptorActividadEconomica = receptor.Element(ns + "ActividadEconomica")?.Value;
    }

    private void ParseResumen(XElement resumen, DocumentoRecibido documento, XNamespace ns)
    {
        // Moneda
        var codigoMoneda = resumen.Element(ns + "CodigoTipoMoneda")?.Element(ns + "CodigoMoneda")!.Value ?? "CRC";
        documento.Moneda = ParseMoneda(codigoMoneda);

        var tipoCambio = resumen.Element(ns + "CodigoTipoMoneda")?.Element(ns + "TipoCambio")?.Value;
        documento.TipoCambio = string.IsNullOrWhiteSpace(tipoCambio) ? 1m : ParseDecimal(tipoCambio);

        // Totales
        documento.TotalGravado = ParseDecimal(resumen.Element(ns + "TotalVentaNeta")?.Value ?? "0");
        documento.TotalExento = ParseDecimal(resumen.Element(ns + "TotalExento")?.Value ?? "0");
        documento.TotalExonerado = ParseDecimal(resumen.Element(ns + "TotalExonerado")?.Value ?? "0");
        documento.Subtotal = ParseDecimal(resumen.Element(ns + "TotalVenta")?.Value ?? "0");
        documento.TotalDescuentos = ParseDecimal(resumen.Element(ns + "TotalDescuentos")?.Value ?? "0");
        documento.TotalImpuestos = ParseDecimal(resumen.Element(ns + "TotalImpuesto")?.Value ?? "0");
        documento.TotalVenta = ParseDecimal(resumen.Element(ns + "TotalComprobante")?.Value ?? "0");
    }

    private DetalleRecibido ParseLineaDetalle(XElement linea, XNamespace ns)
    {
        var detalle = new DetalleRecibido
        {
            NumeroLinea = int.Parse(linea.Element(ns + "NumeroLinea")!.Value),
            Codigo = linea.Element(ns + "Codigo")?.Element(ns + "Codigo")?.Value,
            CodigoCabys = linea.Element(ns + "CodigoComercial")?.Element(ns + "Codigo")?.Value,
            Descripcion = linea.Element(ns + "Detalle")!.Value,
            UnidadMedida = linea.Element(ns + "UnidadMedida")?.Value,
            Cantidad = ParseDecimal(linea.Element(ns + "Cantidad")!.Value),
            PrecioUnitario = ParseDecimal(linea.Element(ns + "PrecioUnitario")!.Value),
            MontoTotal = ParseDecimal(linea.Element(ns + "MontoTotal")!.Value),
            MontoDescuento = ParseDecimal(linea.Element(ns + "MontoDescuento")?.Value ?? "0"),
            Subtotal = ParseDecimal(linea.Element(ns + "SubTotal")!.Value),
            MontoTotalLinea = ParseDecimal(linea.Element(ns + "MontoTotalLinea")!.Value)
        };

        // Impuestos
        var impuesto = linea.Element(ns + "Impuesto")?.Element(ns + "Monto")?.Value;
        detalle.MontoImpuesto = ParseDecimal(impuesto ?? "0");

        return detalle;
    }

    private TipoIdentificacion ParseTipoIdentificacion(string codigo)
    {
        return codigo switch
        {
            "01" => TipoIdentificacion.Fisica,
            "02" => TipoIdentificacion.Juridica,
            "03" => TipoIdentificacion.DIMEX,
            "04" => TipoIdentificacion.NITE,
            _ => TipoIdentificacion.Fisica
        };
    }

    private TipoMoneda ParseMoneda(string codigo)
    {
        return codigo switch
        {
            "CRC" => TipoMoneda.CRC,
            "USD" => TipoMoneda.USD,
            "EUR" => TipoMoneda.EUR,
            _ => TipoMoneda.CRC
        };
    }

    private decimal ParseDecimal(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return 0m;

        return decimal.Parse(valor, CultureInfo.InvariantCulture);
    }

    #endregion
}
