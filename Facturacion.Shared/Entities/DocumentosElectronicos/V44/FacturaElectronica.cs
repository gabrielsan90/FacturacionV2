using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44;

/// <summary>
/// Factura Electrónica (FE) - Documento tipo 01
/// </summary>
[XmlRoot("FacturaElectronica", Namespace = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica")]
public class FacturaElectronica
{
    /// <summary>
    /// Clave numérica del comprobante (50 dígitos)
    /// </summary>
    [XmlElement("Clave")]
    [Required]
    [RegularExpression("^\\d{50}$")]
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Código de actividad económica del emisor (CIIU4, 6 dígitos desde 01/09/2025)
    /// </summary>
    [XmlElement("CodigoActividad")]
    [Required]
    [RegularExpression("^\\d{6}$")]
    public string CodigoActividad { get; set; } = null!;

    /// <summary>
    /// Numeración consecutiva del comprobante (20 dígitos)
    /// </summary>
    [XmlElement("NumeroConsecutivo")]
    [Required]
    [RegularExpression("^\\d{20}$")]
    public string NumeroConsecutivo { get; set; } = null!;

    /// <summary>
    /// Fecha de emisión del documento
    /// </summary>
    [XmlElement("FechaEmision")]
    [Required]
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Información del emisor
    /// </summary>
    [XmlElement("Emisor")]
    [Required]
    public EmisorType Emisor { get; set; } = null!;

    /// <summary>
    /// Información del receptor
    /// </summary>
    [XmlElement("Receptor")]
    public ReceptorType? Receptor { get; set; }

    /// <summary>
    /// Condición de venta: 01 Contado, 02 Crédito, 03 Consignación, 04 Apartado, 05 Arrendamiento con opción de compra, 06 Arrendamiento en función financiera, 07 Cobro a favor de un tercero, 08 Servicios prestados al Estado a crédito, 10 Venta a crédito hasta 90 días, 99 Otros
    /// </summary>
    [XmlElement("CondicionVenta")]
    [Required]
    [RegularExpression("^(01|02|03|04|05|06|07|08|10|99)$")]
    public string CondicionVenta { get; set; } = null!;

    /// <summary>
    /// Plazo del crédito en días (obligatorio si CondicionVenta = 02)
    /// </summary>
    [XmlElement("PlazoCredito")]
    [StringLength(10)]
    public string? PlazoCredito { get; set; }

    /// <summary>
    /// Medio de pago empleado (hasta 4)
    /// </summary>
    [XmlElement("MedioPago")]
    [Required]
    [MinLength(1)]
    [MaxLength(4)]
    public List<MedioPagoType> MedioPago { get; set; } = new();

    /// <summary>
    /// Detalle del servicio o mercancía
    /// </summary>
    [XmlElement("DetalleServicio")]
    [Required]
    public DetalleServicioType DetalleServicio { get; set; } = null!;

    /// <summary>
    /// Otros cargos aplicados (opcional)
    /// </summary>
    [XmlElement("OtrosCargos")]
    public OtrosCargosType? OtrosCargos { get; set; }

    /// <summary>
    /// Resumen de totales de la factura
    /// </summary>
    [XmlElement("ResumenFactura")]
    [Required]
    public ResumenFacturaType ResumenFactura { get; set; } = null!;

    /// <summary>
    /// Información de referencia a otros documentos (hasta 10)
    /// </summary>
    [XmlElement("InformacionReferencia")]
    [MaxLength(10)]
    public List<InformacionReferenciaType>? InformacionReferencia { get; set; }

    /// <summary>
    /// Observaciones o información adicional (opcional)
    /// </summary>
    [XmlElement("Otros")]
    public OtrosType? Otros { get; set; }

    /// <summary>
    /// Firma digital del documento
    /// </summary>
    [XmlElement("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public object? Signature { get; set; }
}

/// <summary>
/// Detalle del servicio o mercancía
/// </summary>
public class DetalleServicioType
{
    /// <summary>
    /// Líneas de detalle (hasta 1000)
    /// </summary>
    [XmlElement("LineaDetalle")]
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public List<LineaDetalleType> LineaDetalle { get; set; } = new();
}

/// <summary>
/// Otros cargos aplicados a la factura
/// </summary>
public class OtrosCargosType
{
    /// <summary>
    /// Tipo de documento: 01 Gastos de flete, 02 Gastos de seguro, 03 Gastos de despacho, 04 Gastos de envío, 99 Otros
    /// </summary>
    [XmlElement("TipoDocumento")]
    [Required]
    [RegularExpression("^(01|02|03|04|99)$")]
    public string TipoDocumento { get; set; } = null!;

    /// <summary>
    /// Detalle del cargo cuando es tipo 99
    /// </summary>
    [XmlElement("DetalleCargo")]
    [StringLength(100)]
    public string? DetalleCargo { get; set; }

    /// <summary>
    /// Número del documento asociado al cargo
    /// </summary>
    [XmlElement("NumeroIdentidadTercero")]
    [StringLength(20)]
    public string? NumeroIdentidadTercero { get; set; }

    /// <summary>
    /// Nombre del tercero que genera el cargo
    /// </summary>
    [XmlElement("NombreTercero")]
    [StringLength(100)]
    public string? NombreTercero { get; set; }

    /// <summary>
    /// Porcentaje del cargo
    /// </summary>
    [XmlElement("Porcentaje")]
    [Range(0, 100)]
    public decimal? Porcentaje { get; set; }

    /// <summary>
    /// Monto total del cargo
    /// </summary>
    [XmlElement("MontoCargo")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal MontoCargo { get; set; }
}

/// <summary>
/// Resumen de totales de la factura
/// </summary>
public class ResumenFacturaType
{
    /// <summary>
    /// Código de moneda y tipo de cambio
    /// </summary>
    [XmlElement("CodigoTipoMoneda")]
    [Required]
    public CodigoMonedaType CodigoTipoMoneda { get; set; } = null!;

    /// <summary>
    /// Total de mercancías gravadas
    /// </summary>
    [XmlElement("TotalServGravados")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalServGravados { get; set; }

    /// <summary>
    /// Total de mercancías exentas
    /// </summary>
    [XmlElement("TotalServExentos")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalServExentos { get; set; }

    /// <summary>
    /// Total de mercancías exoneradas
    /// </summary>
    [XmlElement("TotalServExonerado")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalServExonerado { get; set; }

    /// <summary>
    /// Total de mercancías gravadas con tarifa 0% sin derecho a crédito
    /// </summary>
    [XmlElement("TotalMercanciasGravadas0")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalMercanciasGravadas0 { get; set; }

    /// <summary>
    /// Total de mercancías gravadas con tarifa reducida 1%
    /// </summary>
    [XmlElement("TotalMercanciasGravadas1")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalMercanciasGravadas1 { get; set; }

    /// <summary>
    /// Total de mercancías gravadas con tarifa reducida 2%
    /// </summary>
    [XmlElement("TotalMercanciasGravadas2")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalMercanciasGravadas2 { get; set; }

    /// <summary>
    /// Total de mercancías gravadas con tarifa reducida 4%
    /// </summary>
    [XmlElement("TotalMercanciasGravadas4")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalMercanciasGravadas4 { get; set; }

    /// <summary>
    /// Total de mercancías gravadas con tarifa general 13%
    /// </summary>
    [XmlElement("TotalMercanciasGravadas13")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalMercanciasGravadas13 { get; set; }

    /// <summary>
    /// Total de todas las mercancías y servicios
    /// </summary>
    [XmlElement("TotalVenta")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal TotalVenta { get; set; }

    /// <summary>
    /// Total de descuentos aplicados
    /// </summary>
    [XmlElement("TotalDescuentos")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalDescuentos { get; set; }

    /// <summary>
    /// Total neto de la venta (TotalVenta - TotalDescuentos)
    /// </summary>
    [XmlElement("TotalVentaNeta")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal TotalVentaNeta { get; set; }

    /// <summary>
    /// Desglose de impuestos por código (hasta 1000)
    /// </summary>
    [XmlElement("TotalDesgloseImpuesto")]
    [MaxLength(1000)]
    public List<TotalDesgloseImpuestoType>? TotalDesgloseImpuesto { get; set; }

    /// <summary>
    /// Total de todos los impuestos
    /// </summary>
    [XmlElement("TotalImpuesto")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalImpuesto { get; set; }

    /// <summary>
    /// Total de IVA devuelto
    /// </summary>
    [XmlElement("TotalIVADevuelto")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalIVADevuelto { get; set; }

    /// <summary>
    /// Total de otros cargos
    /// </summary>
    [XmlElement("TotalOtrosCargos")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalOtrosCargos { get; set; }

    /// <summary>
    /// Total del comprobante (TotalVentaNeta + TotalImpuesto + TotalOtrosCargos - TotalIVADevuelto)
    /// </summary>
    [XmlElement("TotalComprobante")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal TotalComprobante { get; set; }
}

/// <summary>
/// Información adicional o campo de uso libre
/// </summary>
public class OtrosType
{
    /// <summary>
    /// Texto de uso libre para información adicional (hasta 3 entradas)
    /// </summary>
    [XmlElement("OtroTexto")]
    [MaxLength(3)]
    public List<OtroTextoType>? OtroTexto { get; set; }

    /// <summary>
    /// Contenido adicional de cualquier tipo
    /// </summary>
    [XmlElement("OtroContenido")]
    [MaxLength(3)]
    public List<OtroContenidoType>? OtroContenido { get; set; }
}

/// <summary>
/// Texto adicional de uso libre
/// </summary>
public class OtroTextoType
{
    /// <summary>
    /// Código del tipo de texto
    /// </summary>
    [XmlAttribute("codigo")]
    [StringLength(10)]
    public string? Codigo { get; set; }

    /// <summary>
    /// Contenido del texto
    /// </summary>
    [XmlText]
    [StringLength(1000)]
    public string? Value { get; set; }
}

/// <summary>
/// Contenido adicional de cualquier tipo
/// </summary>
public class OtroContenidoType
{
    /// <summary>
    /// Código del tipo de contenido
    /// </summary>
    [XmlAttribute("codigo")]
    [StringLength(10)]
    public string? Codigo { get; set; }

    /// <summary>
    /// Contenido en formato XML
    /// </summary>
    [XmlAnyElement]
    public System.Xml.XmlElement? Value { get; set; }
}
