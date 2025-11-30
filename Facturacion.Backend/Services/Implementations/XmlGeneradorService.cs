using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Generador de XML para documentos electrónicos según Hacienda v4.4
/// </summary>
public class XmlGeneradorService : IXmlGeneradorService
{
    private readonly DataContext _context;
    private const string NamespaceFactura = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/facturaElectronica";
    private const string NamespaceTiquete = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/tiqueteElectronico";
    private const string NamespaceNotaCredito = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/notaCreditoElectronica";
    private const string NamespaceNotaDebito = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/notaDebitoElectronica";
    private const string NamespaceFacturaExportacion = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/facturaElectronicaExportacion";

    public XmlGeneradorService(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Genera el XML del documento según su tipo
    /// </summary>
    public async Task<string> GenerarXmlAsync(Documento documento)
    {
        // Cargar todas las relaciones necesarias
        await CargarRelacionesAsync(documento);

        // Generar XML según tipo de documento
        XDocument xmlDoc = documento.TipoDocumento switch
        {
            DocumentoTipo.FacturaElectronica => GenerarFacturaElectronica(documento),
            DocumentoTipo.TiqueteElectronico => GenerarTiqueteElectronico(documento),
            DocumentoTipo.NotaCreditoElectronica => GenerarNotaCredito(documento),
            DocumentoTipo.NotaDebitoElectronica => GenerarNotaDebito(documento),
            DocumentoTipo.FacturaElectronicaExportacion => GenerarFacturaExportacion(documento),
            _ => throw new NotImplementedException($"Tipo de documento {documento.TipoDocumento} no implementado")
        };

        // Convertir a string con formato
        return xmlDoc.Declaration?.ToString() + Environment.NewLine + xmlDoc.ToString();
    }

    /// <summary>
    /// Valida estructura básica del XML
    /// </summary>
    public bool ValidarXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.Root != null;
        }
        catch
        {
            return false;
        }
    }

    #region Cargar Relaciones

    private async Task CargarRelacionesAsync(Documento documento)
    {
        // Cargar Empresa
        if (documento.Empresa == null)
        {
            documento.Empresa = await _context.Set<Empresa>()
                .Include(e => e.Telefonos)
                .Include(e => e.Emails)
                .Include(e => e.ActividadesEconomicas)
                .FirstOrDefaultAsync(e => e.Id == documento.EmpresaId)
                ?? throw new InvalidOperationException("No se encontró la empresa");
        }

        // Cargar Sucursal
        if (documento.Sucursal == null)
        {
            documento.Sucursal = await _context.Set<Sucursal>()
                .FirstOrDefaultAsync(s => s.Id == documento.SucursalId)
                ?? throw new InvalidOperationException("No se encontró la sucursal");
        }

        // Cargar Terminal
        if (documento.Terminal == null)
        {
            documento.Terminal = await _context.Set<Terminal>()
                .FirstOrDefaultAsync(t => t.Id == documento.TerminalId)
                ?? throw new InvalidOperationException("No se encontró la terminal");
        }

        // Cargar Cliente o Proveedor
        if (documento.ClienteId.HasValue && documento.Cliente == null)
        {
            documento.Cliente = await _context.Set<Cliente>()
                .Include(c => c.Telefonos)
                .Include(c => c.Emails)
                .FirstOrDefaultAsync(c => c.Id == documento.ClienteId.Value);
        }

        if (documento.ProveedorId.HasValue && documento.Proveedor == null)
        {
            documento.Proveedor = await _context.Set<Proveedor>()
                .FirstOrDefaultAsync(p => p.Id == documento.ProveedorId.Value);
        }

        // Cargar Detalles con sus impuestos y descuentos
        if (documento.Detalles == null || !documento.Detalles.Any())
        {
            var detalles = await _context.Set<DocumentoDetalle>()
                .Include(d => d.Impuestos)
                .Include(d => d.Descuentos)
                .Include(d => d.Producto)
                .Where(d => d.DocumentoId == documento.Id)
                .OrderBy(d => d.NumeroLinea)
                .ToListAsync();

            documento.Detalles = detalles;
        }

        // Cargar Referencias
        if (documento.Referencias == null || !documento.Referencias.Any())
        {
            documento.Referencias = await _context.Set<DocumentoReferencia>()
                .Where(r => r.DocumentoId == documento.Id)
                .ToListAsync();
        }

        // Cargar Medios de Pago
        if (documento.MediosPago == null || !documento.MediosPago.Any())
        {
            documento.MediosPago = await _context.Set<DocumentoMedioPago>()
                .Where(m => m.DocumentoId == documento.Id)
                .ToListAsync();
        }

        // Cargar Otra Información
        if (documento.OtraInformacion == null || !documento.OtraInformacion.Any())
        {
            documento.OtraInformacion = await _context.Set<DocumentoOtraInformacion>()
                .Where(o => o.DocumentoId == documento.Id)
                .ToListAsync();
        }

        // Cargar Exportación si aplica
        if (documento.TipoDocumento == DocumentoTipo.FacturaElectronicaExportacion && documento.Exportacion == null)
        {
            documento.Exportacion = await _context.Set<DocumentoExportacion>()
                .FirstOrDefaultAsync(e => e.DocumentoId == documento.Id);
        }
    }

    #endregion

    #region Factura Electrónica (FE)

    private XDocument GenerarFacturaElectronica(Documento doc)
    {
        XNamespace ns = NamespaceFactura;
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "FacturaElectronica",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                GenerarClave(doc, ns),
                GenerarCodigoActividad(doc, ns),
                GenerarNumeroConsecutivo(doc, ns),
                GenerarFechaEmision(doc, ns),
                GenerarEmisor(doc, ns),
                GenerarReceptor(doc, ns),
                GenerarCondicionVenta(doc, ns),
                GenerarPlazoCredito(doc, ns),
                GenerarMedioPago(doc, ns),
                GenerarDetalleServicio(doc, ns),
                GenerarResumenFactura(doc, ns),
                GenerarInformacionReferencia(doc, ns),
                GenerarNormativa(doc, ns),
                GenerarOtros(doc, ns)
            )
        );

        return documento;
    }

    #endregion

    #region Tiquete Electrónico (TE)

    private XDocument GenerarTiqueteElectronico(Documento doc)
    {
        XNamespace ns = NamespaceTiquete;
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "TiqueteElectronico",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                GenerarClave(doc, ns),
                GenerarCodigoActividad(doc, ns),
                GenerarNumeroConsecutivo(doc, ns),
                GenerarFechaEmision(doc, ns),
                GenerarEmisor(doc, ns),
                GenerarCondicionVenta(doc, ns),
                GenerarPlazoCredito(doc, ns),
                GenerarMedioPago(doc, ns),
                GenerarDetalleServicio(doc, ns),
                GenerarResumenFactura(doc, ns),
                GenerarInformacionReferencia(doc, ns),
                GenerarNormativa(doc, ns),
                GenerarOtros(doc, ns)
            )
        );

        return documento;
    }

    #endregion

    #region Nota de Crédito (NC)

    private XDocument GenerarNotaCredito(Documento doc)
    {
        XNamespace ns = NamespaceNotaCredito;
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "NotaCreditoElectronica",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                GenerarClave(doc, ns),
                GenerarCodigoActividad(doc, ns),
                GenerarNumeroConsecutivo(doc, ns),
                GenerarFechaEmision(doc, ns),
                GenerarEmisor(doc, ns),
                GenerarReceptor(doc, ns),
                GenerarCondicionVenta(doc, ns),
                GenerarPlazoCredito(doc, ns),
                GenerarMedioPago(doc, ns),
                GenerarDetalleServicio(doc, ns),
                GenerarResumenFactura(doc, ns),
                GenerarInformacionReferencia(doc, ns),
                GenerarNormativa(doc, ns),
                GenerarOtros(doc, ns)
            )
        );

        return documento;
    }

    #endregion

    #region Nota de Débito (ND)

    private XDocument GenerarNotaDebito(Documento doc)
    {
        XNamespace ns = NamespaceNotaDebito;
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "NotaDebitoElectronica",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                GenerarClave(doc, ns),
                GenerarCodigoActividad(doc, ns),
                GenerarNumeroConsecutivo(doc, ns),
                GenerarFechaEmision(doc, ns),
                GenerarEmisor(doc, ns),
                GenerarReceptor(doc, ns),
                GenerarCondicionVenta(doc, ns),
                GenerarPlazoCredito(doc, ns),
                GenerarMedioPago(doc, ns),
                GenerarDetalleServicio(doc, ns),
                GenerarResumenFactura(doc, ns),
                GenerarInformacionReferencia(doc, ns),
                GenerarNormativa(doc, ns),
                GenerarOtros(doc, ns)
            )
        );

        return documento;
    }

    #endregion

    #region Factura de Exportación (FEE)

    private XDocument GenerarFacturaExportacion(Documento doc)
    {
        XNamespace ns = NamespaceFacturaExportacion;
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "FacturaElectronicaExportacion",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                GenerarClave(doc, ns),
                GenerarCodigoActividad(doc, ns),
                GenerarNumeroConsecutivo(doc, ns),
                GenerarFechaEmision(doc, ns),
                GenerarEmisor(doc, ns),
                GenerarReceptor(doc, ns),
                GenerarCondicionVenta(doc, ns),
                GenerarPlazoCredito(doc, ns),
                GenerarMedioPago(doc, ns),
                GenerarDetalleServicio(doc, ns),
                GenerarResumenFactura(doc, ns),
                GenerarInformacionReferencia(doc, ns),
                GenerarNormativa(doc, ns),
                GenerarOtros(doc, ns)
            )
        );

        return documento;
    }

    #endregion

    #region Elementos Comunes

    private XElement GenerarClave(Documento doc, XNamespace ns)
    {
        return new XElement(ns + "Clave", doc.Clave);
    }

    private XElement GenerarCodigoActividad(Documento doc, XNamespace ns)
    {
        return new XElement(ns + "CodigoActividad", doc.ActividadEconomica);
    }

    private XElement GenerarNumeroConsecutivo(Documento doc, XNamespace ns)
    {
        return new XElement(ns + "NumeroConsecutivo", doc.NumeroConsecutivo);
    }

    private XElement GenerarFechaEmision(Documento doc, XNamespace ns)
    {
        // Formato: 2025-01-15T10:30:00-06:00
        string fecha = doc.FechaEmision.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        return new XElement(ns + "FechaEmision", fecha);
    }

    private XElement GenerarEmisor(Documento doc, XNamespace ns)
    {
        var empresa = doc.Empresa!;

        var emisor = new XElement(ns + "Emisor",
            new XElement(ns + "Nombre", empresa.RazonSocial),
            new XElement(ns + "Identificacion",
                new XElement(ns + "Tipo", ObtenerCodigoTipoIdentificacion(empresa.TipoIdentificacion)),
                new XElement(ns + "Numero", empresa.NumeroIdentificacion)
            )
        );

        // Nombre Comercial (opcional)
        if (!string.IsNullOrWhiteSpace(empresa.NombreComercial))
        {
            emisor.Add(new XElement(ns + "NombreComercial", empresa.NombreComercial));
        }

        // Ubicación
        emisor.Add(new XElement(ns + "Ubicacion",
            new XElement(ns + "Provincia", empresa.Provincia.ToString()),
            new XElement(ns + "Canton", empresa.Canton.ToString("D2")),
            new XElement(ns + "Distrito", empresa.Distrito.ToString("D2")),
            new XElement(ns + "OtrasSenas", empresa.OtrasSenas ?? "")
        ));

        // Teléfono (opcional)
        var telefono = empresa.Telefonos?.FirstOrDefault();
        if (telefono != null)
        {
            emisor.Add(new XElement(ns + "Telefono",
                new XElement(ns + "CodigoPais", telefono.CodigoPais ?? "506"),
                new XElement(ns + "NumTelefono", telefono.NumeroTelefono)
            ));
        }

        // Email (opcional)
        var email = empresa.Emails?.FirstOrDefault();
        if (email != null)
        {
            emisor.Add(new XElement(ns + "CorreoElectronico", email.DireccionEmail));
        }

        return emisor;
    }

    private XElement? GenerarReceptor(Documento doc, XNamespace ns)
    {
        // El tiquete electrónico no lleva receptor
        if (doc.TipoDocumento == DocumentoTipo.TiqueteElectronico)
            return null;

        // Si no hay receptor, retornar null
        if (doc.Cliente == null && string.IsNullOrWhiteSpace(doc.ReceptorNombre))
            return null;

        var receptor = new XElement(ns + "Receptor");

        // Nombre
        string nombreReceptor = doc.ReceptorNombre ?? doc.Cliente?.Nombre ?? "Cliente";
        receptor.Add(new XElement(ns + "Nombre", nombreReceptor));

        // Identificación (opcional para extranjeros)
        if (doc.ReceptorTipoIdentificacion.HasValue && !string.IsNullOrWhiteSpace(doc.ReceptorNumeroIdentificacion))
        {
            receptor.Add(new XElement(ns + "Identificacion",
                new XElement(ns + "Tipo", ObtenerCodigoTipoIdentificacion(doc.ReceptorTipoIdentificacion.Value)),
                new XElement(ns + "Numero", doc.ReceptorNumeroIdentificacion)
            ));
        }

        // Nombre Comercial (opcional)
        if (!string.IsNullOrWhiteSpace(doc.ReceptorNombreComercial))
        {
            receptor.Add(new XElement(ns + "NombreComercial", doc.ReceptorNombreComercial));
        }

        // Actividad Económica del Receptor (NUEVO en v4.4 - OBLIGATORIO en facturas)
        if (!string.IsNullOrWhiteSpace(doc.ReceptorActividadEconomica))
        {
            receptor.Add(new XElement(ns + "ActividadEconomica", doc.ReceptorActividadEconomica));
        }

        // Ubicación (opcional)
        if (doc.ReceptorProvincia.HasValue)
        {
            receptor.Add(new XElement(ns + "Ubicacion",
                new XElement(ns + "Provincia", doc.ReceptorProvincia.Value.ToString()),
                new XElement(ns + "Canton", doc.ReceptorCanton?.ToString("D2") ?? "01"),
                new XElement(ns + "Distrito", doc.ReceptorDistrito?.ToString("D2") ?? "01"),
                new XElement(ns + "OtrasSenas", doc.ReceptorOtrasSenas ?? "")
            ));
        }

        // Teléfono (opcional)
        if (!string.IsNullOrWhiteSpace(doc.ReceptorTelefono))
        {
            receptor.Add(new XElement(ns + "Telefono",
                new XElement(ns + "CodigoPais", "506"),
                new XElement(ns + "NumTelefono", doc.ReceptorTelefono)
            ));
        }

        // Email (opcional - hasta 4 emails en v4.4)
        if (!string.IsNullOrWhiteSpace(doc.ReceptorEmails))
        {
            var emails = doc.ReceptorEmails.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (emails.Any())
            {
                receptor.Add(new XElement(ns + "CorreoElectronico", emails[0].Trim()));
            }
        }

        return receptor;
    }

    private XElement GenerarCondicionVenta(Documento doc, XNamespace ns)
    {
        return new XElement(ns + "CondicionVenta", doc.CondicionVenta);
    }

    private XElement? GenerarPlazoCredito(Documento doc, XNamespace ns)
    {
        // Solo si la condición de venta es crédito (02)
        if (doc.CondicionVenta == "02" && doc.PlazoCreditoDias.HasValue)
        {
            return new XElement(ns + "PlazoCredito", doc.PlazoCreditoDias.Value.ToString());
        }
        return null;
    }

    private XElement GenerarMedioPago(Documento doc, XNamespace ns)
    {
        // Si hay medios de pago múltiples, tomar el primero
        // En implementaciones futuras, se puede mejorar para soportar múltiples medios
        return new XElement(ns + "MedioPago", doc.MedioPago);
    }

    private XElement GenerarDetalleServicio(Documento doc, XNamespace ns)
    {
        var detalleServicio = new XElement(ns + "DetalleServicio");

        foreach (var linea in doc.Detalles.OrderBy(d => d.NumeroLinea))
        {
            var lineaDetalle = new XElement(ns + "LineaDetalle",
                new XElement(ns + "NumeroLinea", linea.NumeroLinea),
                GenerarCodigoLinea(linea, ns),
                new XElement(ns + "Cantidad", FormatearDecimal(linea.Cantidad, 3)),
                new XElement(ns + "UnidadMedida", linea.UnidadMedida?.Descripcion ?? "Unid"),
                new XElement(ns + "Detalle", linea.Descripcion),
                new XElement(ns + "PrecioUnitario", FormatearDecimal(linea.PrecioUnitario, 5)),
                new XElement(ns + "MontoTotal", FormatearDecimal(linea.MontoTotal, 5))
            );

            // Descuentos (opcional)
            if (linea.Descuentos != null && linea.Descuentos.Any())
            {
                foreach (var desc in linea.Descuentos)
                {
                    lineaDetalle.Add(new XElement(ns + "Descuento",
                        new XElement(ns + "MontoDescuento", FormatearDecimal(desc.MontoDescuento, 5)),
                        new XElement(ns + "NaturalezaDescuento", desc.NaturalezaDescuento ?? "Descuento comercial")
                    ));
                }
            }

            lineaDetalle.Add(new XElement(ns + "SubTotal", FormatearDecimal(linea.Subtotal, 5)));

            // Impuestos
            if (linea.Impuestos != null && linea.Impuestos.Any())
            {
                foreach (var imp in linea.Impuestos)
                {
                    lineaDetalle.Add(new XElement(ns + "Impuesto",
                        new XElement(ns + "Codigo", imp.CodigoImpuesto),
                        new XElement(ns + "CodigoTarifa", imp.CodigoTarifa),
                        new XElement(ns + "Tarifa", FormatearDecimal(imp.Tarifa, 2)),
                        new XElement(ns + "Monto", FormatearDecimal(imp.MontoImpuesto, 5))
                    ));
                }
            }

            lineaDetalle.Add(new XElement(ns + "ImpuestoNeto", FormatearDecimal(linea.ImpuestoNeto ?? linea.MontoImpuesto, 5)));
            lineaDetalle.Add(new XElement(ns + "MontoTotalLinea", FormatearDecimal(linea.MontoTotalLinea, 5)));

            detalleServicio.Add(lineaDetalle);
        }

        return detalleServicio;
    }

    private XElement? GenerarCodigoLinea(DocumentoDetalle linea, XNamespace ns)
    {
        if (linea.Producto == null || string.IsNullOrWhiteSpace(linea.Producto.Codigo))
            return null;

        var codigo = new XElement(ns + "Codigo",
            new XElement(ns + "Tipo", "01"), // 01 = Código del vendedor
            new XElement(ns + "Codigo", linea.Producto.Codigo)
        );

        return codigo;
    }

    private XElement GenerarResumenFactura(Documento doc, XNamespace ns)
    {
        var resumen = new XElement(ns + "ResumenFactura");

        // Moneda y tipo de cambio
        resumen.Add(new XElement(ns + "CodigoTipoMoneda",
            new XElement(ns + "CodigoMoneda", ObtenerCodigoMoneda(doc.Moneda)),
            new XElement(ns + "TipoCambio", FormatearDecimal(doc.TipoCambio ?? 1.00m, 5))
        ));

        // Totales
        resumen.Add(new XElement(ns + "TotalServGravados", FormatearDecimal(doc.TotalServiciosGravados, 5)));
        resumen.Add(new XElement(ns + "TotalServExentos", FormatearDecimal(doc.TotalServiciosExentos, 5)));
        resumen.Add(new XElement(ns + "TotalServExonerado", FormatearDecimal(doc.TotalServiciosExonerados, 5)));
        resumen.Add(new XElement(ns + "TotalMercanciasGravadas", FormatearDecimal(doc.TotalMercanciasGravadas, 5)));
        resumen.Add(new XElement(ns + "TotalMercanciasExentas", FormatearDecimal(doc.TotalMercanciasExentas, 5)));
        resumen.Add(new XElement(ns + "TotalMercanciasExoneradas", FormatearDecimal(doc.TotalMercanciasExoneradas, 5)));
        resumen.Add(new XElement(ns + "TotalGravado", FormatearDecimal(doc.TotalGravado, 5)));
        resumen.Add(new XElement(ns + "TotalExento", FormatearDecimal(doc.TotalExento, 5)));
        resumen.Add(new XElement(ns + "TotalExonerado", FormatearDecimal(doc.TotalExonerado, 5)));
        resumen.Add(new XElement(ns + "TotalVenta", FormatearDecimal(doc.Subtotal, 5)));
        resumen.Add(new XElement(ns + "TotalDescuentos", FormatearDecimal(doc.TotalDescuentos, 5)));
        resumen.Add(new XElement(ns + "TotalVentaNeta", FormatearDecimal(doc.Subtotal - doc.TotalDescuentos, 5)));
        resumen.Add(new XElement(ns + "TotalImpuesto", FormatearDecimal(doc.TotalImpuestos, 5)));

        // IVA Devuelto (opcional)
        if (doc.IVADevuelto.HasValue && doc.IVADevuelto.Value > 0)
        {
            resumen.Add(new XElement(ns + "TotalIVADevuelto", FormatearDecimal(doc.IVADevuelto.Value, 5)));
        }

        // Otros cargos (opcional)
        if (doc.TotalOtrosCargos > 0)
        {
            resumen.Add(new XElement(ns + "TotalOtrosCargos", FormatearDecimal(doc.TotalOtrosCargos, 5)));
        }

        resumen.Add(new XElement(ns + "TotalComprobante", FormatearDecimal(doc.TotalVenta, 5)));

        return resumen;
    }

    private XElement? GenerarInformacionReferencia(Documento doc, XNamespace ns)
    {
        if (doc.Referencias == null || !doc.Referencias.Any())
            return null;

        var infoReferencia = new XElement(ns + "InformacionReferencia");

        foreach (var referencia in doc.Referencias)
        {
            infoReferencia.Add(new XElement(ns + "Referencia",
                new XElement(ns + "TipoDoc", referencia.TipoDocumentoReferenciado),
                new XElement(ns + "Numero", referencia.NumeroDocumentoReferenciado),
                new XElement(ns + "FechaEmision", referencia.FechaEmisionDocumentoReferenciado.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)),
                new XElement(ns + "Codigo", ((int)referencia.CodigoReferencia).ToString("D2")),
                new XElement(ns + "Razon", referencia.RazonReferencia)
            ));
        }

        return infoReferencia;
    }

    private XElement GenerarNormativa(Documento doc, XNamespace ns)
    {
        return new XElement(ns + "Normativa",
            new XElement(ns + "NumeroResolucion", "DGT-R-48-2016"),
            new XElement(ns + "FechaResolucion", "07-10-2016 08:00:00")
        );
    }

    private XElement? GenerarOtros(Documento doc, XNamespace ns)
    {
        if (doc.OtraInformacion == null || !doc.OtraInformacion.Any())
            return null;

        var otros = new XElement(ns + "Otros");

        foreach (var info in doc.OtraInformacion)
        {
            otros.Add(new XElement(ns + "OtraInformacion",
                new XElement(ns + "Codigo", info.Clave ?? ""),
                new XElement(ns + "Texto", info.Valor ?? "")
            ));
        }

        return otros;
    }

    #endregion

    #region Helpers

    private string ObtenerCodigoTipoIdentificacion(TipoIdentificacion tipo)
    {
        return tipo switch
        {
            TipoIdentificacion.Fisica => "01",
            TipoIdentificacion.Juridica => "02",
            TipoIdentificacion.DIMEX => "03",
            TipoIdentificacion.NITE => "04",
            TipoIdentificacion.Pasaporte => "04",
            TipoIdentificacion.Extranjera => "05",
            _ => "01"
        };
    }

    private string ObtenerCodigoMoneda(TipoMoneda moneda)
    {
        return moneda switch
        {
            TipoMoneda.CRC => "CRC",
            TipoMoneda.USD => "USD",
            TipoMoneda.EUR => "EUR",
            _ => "CRC"
        };
    }

    private string FormatearDecimal(decimal valor, int decimales)
    {
        return valor.ToString($"F{decimales}", CultureInfo.InvariantCulture);
    }

    #endregion

    #region Mensaje Receptor (MR)

    /// <summary>
    /// Genera el XML de un Mensaje Receptor (tipos 05, 06, 07)
    /// </summary>
    public async Task<string> GenerarMensajeReceptorAsync(DocumentoReceptorMensaje mensaje, Documento documentoOriginal)
    {
        // Cargar relaciones necesarias del documento original
        await CargarRelacionesAsync(documentoOriginal);

        // Namespace del Mensaje Receptor
        const string nsMR = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.3/mensajeReceptor";
        XNamespace ns = nsMR;

        // Crear documento XML
        var xmlDoc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "MensajeReceptor",
                // Clave del mensaje (50 dígitos)
                new XElement(ns + "Clave", mensaje.ClaveMensaje),

                // NumeroCedulaEmisor: Cédula del receptor del documento original (nosotros)
                // En el MR, nosotros somos el emisor del mensaje
                new XElement(ns + "NumeroCedulaEmisor", documentoOriginal.ReceptorNumeroIdentificacion),

                // FechaEmisionDoc: Fecha de emisión del mensaje
                new XElement(ns + "FechaEmisionDoc", mensaje.FechaEmision.ToString("yyyy-MM-ddTHH:mm:sszzz")),

                // Mensaje: Tipo de mensaje (1=Aceptación, 2=Aceptación Parcial, 3=Rechazo)
                new XElement(ns + "Mensaje", mensaje.TipoMensaje.ToString()),

                // DetalleMensaje: Detalle opcional (obligatorio para rechazo y aceptación parcial)
                mensaje.DetalleMensaje != null
                    ? new XElement(ns + "DetalleMensaje", mensaje.DetalleMensaje)
                    : null,

                // MontoTotalImpuesto: Solo para aceptación parcial (tipo 2)
                mensaje.TipoMensaje == 2 && mensaje.MontoTotalImpuestoAceptado.HasValue
                    ? new XElement(ns + "MontoTotalImpuesto", FormatearDecimal(mensaje.MontoTotalImpuestoAceptado.Value, 5))
                    : null,

                // TotalFactura: Solo para aceptación parcial (tipo 2)
                mensaje.TipoMensaje == 2 && mensaje.MontoTotalAceptado.HasValue
                    ? new XElement(ns + "TotalFactura", FormatearDecimal(mensaje.MontoTotalAceptado.Value, 5))
                    : null,

                // NumeroCedulaReceptor: Cédula del emisor del documento original (el proveedor)
                // En el MR, el proveedor es el receptor del mensaje
                new XElement(ns + "NumeroCedulaReceptor",
                    ObtenerNumeroIdentificacionEmisor(documentoOriginal)),

                // NumeroConsecutivoReceptor: Consecutivo del mensaje
                new XElement(ns + "NumeroConsecutivoReceptor", mensaje.NumeroConsecutivo)
            )
        );

        // Convertir a string con formato
        return xmlDoc.Declaration?.ToString() + Environment.NewLine + xmlDoc.ToString();
    }

    /// <summary>
    /// Obtiene el número de identificación del emisor del documento
    /// </summary>
    private string ObtenerNumeroIdentificacionEmisor(Documento documento)
    {
        // Si el documento es recibido, el emisor es el proveedor
        if (documento.EsDocumentoRecibido && documento.Proveedor != null)
        {
            return documento.Proveedor.NumeroIdentificacion;
        }

        // Si el documento es emitido, el emisor es la empresa
        if (documento.Empresa != null)
        {
            return documento.Empresa.NumeroIdentificacion;
        }

        throw new InvalidOperationException("No se pudo determinar el número de identificación del emisor");
    }

    #endregion

    #region Generar REP (Recibo Electrónico de Pago)

    /// <summary>
    /// Genera el XML de un Recibo Electrónico de Pago (REP) - NUEVO en v4.4
    /// </summary>
    public async Task<string> GenerarREPAsync(Documento documentoREP, ReciboPago reciboPago)
    {
        // Cargar todas las relaciones necesarias
        await CargarRelacionesAsync(documentoREP);

        // Cargar documento original referenciado
        var documentoOriginal = await _context.Documentos
            .Include(d => d.Empresa)
            .FirstOrDefaultAsync(d => d.Id == reciboPago.DocumentoOriginalId)
            ?? throw new InvalidOperationException("No se encontró el documento original");

        // Cargar medios de pago
        var mediosPago = await _context.DocumentoMediosPago
            .Where(m => m.DocumentoId == documentoREP.Id)
            .ToListAsync();

        // Crear documento XML con namespace v4.4 para REP
        var ns = XNamespace.Get("https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/reciboElectronicoPago");

        var xml = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(ns + "ReciboElectronicoPago",
                // 1. Clave del REP (50 dígitos)
                new XElement(ns + "Clave", documentoREP.Clave),

                // 2. CodigoActividad (CIIU4 - 6 dígitos)
                new XElement(ns + "CodigoActividad", documentoREP.ActividadEconomica),

                // 3. NumeroConsecutivo del REP
                new XElement(ns + "NumeroConsecutivo", documentoREP.NumeroConsecutivo),

                // 4. FechaEmision (ISO 8601 con timezone)
                new XElement(ns + "FechaEmision", documentoREP.FechaEmision.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)),

                // 5. Emisor (la empresa que recibe el pago)
                GenerarEmisor(documentoREP, ns),

                // 6. Receptor (el cliente que paga)
                GenerarReceptor(documentoREP, ns),

                // 7. DocumentoReferencia (enlace al documento original)
                new XElement(ns + "DocumentoReferencia",
                    new XElement(ns + "TipoDoc", reciboPago.TipoDocumentoOriginal),
                    new XElement(ns + "Numero", reciboPago.NumeroConsecutivoOriginal),
                    new XElement(ns + "FechaEmision", documentoOriginal.FechaEmision.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)),
                    new XElement(ns + "Codigo", "01"), // 01 = Anula documento de referencia (para REP siempre es 01)
                    new XElement(ns + "Razon", $"Pago recibido por {FormatearDecimal(reciboPago.MontoPagado, 5)}")
                ),

                // 8. CodigoMoneda (CRC, USD, EUR)
                new XElement(ns + "CodigoMoneda", documentoREP.Moneda.ToString()),

                // 9. TipoCambio (si no es CRC)
                documentoREP.TipoCambio.HasValue
                    ? new XElement(ns + "TipoCambio", FormatearDecimal(documentoREP.TipoCambio.Value, 5))
                    : null,

                // 10. TotalComprobante (monto total pagado en este REP)
                new XElement(ns + "TotalComprobante", FormatearDecimal(reciboPago.MontoPagado, 5)),

                // 11. MedioPago (puede haber varios)
                from medio in mediosPago
                select new XElement(ns + "MedioPago",
                    new XElement(ns + "Medio", medio.CodigoMedioPago),
                    new XElement(ns + "Monto", FormatearDecimal(medio.Monto, 5)),
                    !string.IsNullOrWhiteSpace(medio.NumeroReferencia)
                        ? new XElement(ns + "NumeroReferencia", medio.NumeroReferencia)
                        : null
                ),

                // 12. SaldoPendiente (saldo que queda después de este pago)
                new XElement(ns + "SaldoPendiente", FormatearDecimal(reciboPago.SaldoPendiente, 5)),

                // 13. Otros (información adicional)
                !string.IsNullOrWhiteSpace(documentoREP.Observaciones)
                    ? new XElement(ns + "Otros",
                        new XElement(ns + "OtroTexto", documentoREP.Observaciones)
                    )
                    : null
            )
        );

        return xml.Declaration?.ToString() + Environment.NewLine + xml.ToString();
    }

    #endregion
}
