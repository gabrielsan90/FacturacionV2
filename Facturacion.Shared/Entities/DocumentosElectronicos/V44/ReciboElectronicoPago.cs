using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44;

/// <summary>
/// Recibo Electrónico de Pago (REP) - NUEVO en v4.4
/// Documento tipo 10
/// Requerido para ventas a crédito con IVA (hasta 90 días)
/// </summary>
[XmlRoot("ReciboElectronicoPago", Namespace = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/reciboElectronicoPago")]
public class ReciboElectronicoPago
{
    /// <summary>
    /// Clave numérica del comprobante (50 dígitos)
    /// </summary>
    [XmlElement("Clave")]
    [Required]
    [RegularExpression("^\\d{50}$")]
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Número de cédula del proveedor de sistemas
    /// </summary>
    [XmlElement("ProveedorSistemas")]
    [Required]
    [StringLength(20)]
    public string ProveedorSistemas { get; set; } = null!;

    /// <summary>
    /// Numeración consecutiva del comprobante (20 dígitos)
    /// Formato: XXX-YYYYY-ZZ-AAAAAAAAAA
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
    public EmisorREPType Emisor { get; set; } = null!;

    /// <summary>
    /// Información del receptor
    /// </summary>
    [XmlElement("Receptor")]
    [Required]
    public ReceptorREPType Receptor { get; set; } = null!;

    /// <summary>
    /// Condición de venta: 09 Pago del servicio prestado al Estado, 11 Pago de venta a crédito en IVA hasta 90 días
    /// </summary>
    [XmlElement("CondicionVenta")]
    [Required]
    [RegularExpression("^(09|11)$")]
    public string CondicionVenta { get; set; } = null!;

    /// <summary>
    /// Detalle del servicio, mercancía u otro
    /// </summary>
    [XmlElement("DetalleServicio")]
    [Required]
    public DetalleServicioREPType DetalleServicio { get; set; } = null!;

    /// <summary>
    /// Resumen de totales del recibo
    /// </summary>
    [XmlElement("ResumenFactura")]
    [Required]
    public ResumenFacturaREPType ResumenFactura { get; set; } = null!;

    /// <summary>
    /// Información de referencia a documentos relacionados (hasta 10)
    /// </summary>
    [XmlElement("InformacionReferencia")]
    [Required]
    [MinLength(1)]
    [MaxLength(10)]
    public List<InformacionReferenciaType> InformacionReferencia { get; set; } = new();

    /// <summary>
    /// Firma digital del documento
    /// </summary>
    [XmlElement("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public object? Signature { get; set; }
}

/// <summary>
/// Emisor para REP (versión simplificada)
/// </summary>
public class EmisorREPType
{
    [XmlElement("Nombre")]
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string Nombre { get; set; } = null!;

    [XmlElement("Identificacion")]
    [Required]
    public IdentificacionType Identificacion { get; set; } = null!;

    [XmlElement("CorreoElectronico")]
    [Required]
    [MinLength(1)]
    [MaxLength(4)]
    public List<string> CorreoElectronico { get; set; } = new();
}

/// <summary>
/// Receptor para REP (versión simplificada)
/// </summary>
public class ReceptorREPType
{
    [XmlElement("Nombre")]
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; } = null!;

    [XmlElement("Identificacion")]
    [Required]
    public IdentificacionType Identificacion { get; set; } = null!;

    [XmlElement("CorreoElectronico")]
    [StringLength(160)]
    public string? CorreoElectronico { get; set; }
}

/// <summary>
/// Detalle del servicio para REP
/// </summary>
public class DetalleServicioREPType
{
    [XmlElement("LineaDetalle")]
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public List<LineaDetalleREPType> LineaDetalle { get; set; } = new();
}

/// <summary>
/// Línea de detalle del REP
/// </summary>
public class LineaDetalleREPType
{
    [XmlElement("NumeroLinea")]
    [Required]
    [Range(1, 1000)]
    public int NumeroLinea { get; set; }

    [XmlElement("Detalle")]
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Detalle { get; set; } = null!;

    [XmlElement("MontoTotal")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal MontoTotal { get; set; }

    [XmlElement("SubTotal")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal SubTotal { get; set; }

    [XmlElement("Impuesto")]
    [MaxLength(1000)]
    public List<ImpuestoType>? Impuesto { get; set; }

    [XmlElement("ImpuestoNeto")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal ImpuestoNeto { get; set; }

    [XmlElement("MontoTotalLinea")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal MontoTotalLinea { get; set; }
}

/// <summary>
/// Resumen de totales para REP
/// </summary>
public class ResumenFacturaREPType
{
    [XmlElement("CodigoTipoMoneda")]
    [Required]
    public CodigoMonedaType CodigoTipoMoneda { get; set; } = null!;

    [XmlElement("TotalVenta")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal TotalVenta { get; set; }

    [XmlElement("TotalVentaNeta")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal TotalVentaNeta { get; set; }

    [XmlElement("TotalDesgloseImpuesto")]
    [MaxLength(1000)]
    public List<TotalDesgloseImpuestoType>? TotalDesgloseImpuesto { get; set; }

    [XmlElement("TotalImpuesto")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalImpuesto { get; set; }

    [XmlElement("MedioPago")]
    [Required]
    [MinLength(1)]
    [MaxLength(4)]
    public List<MedioPagoType> MedioPago { get; set; } = new();

    [XmlElement("TotalComprobante")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal TotalComprobante { get; set; }
}

/// <summary>
/// Desglose de impuestos por código
/// </summary>
public class TotalDesgloseImpuestoType
{
    [XmlElement("Codigo")]
    [Required]
    [RegularExpression("^(01|02|03|04|05|06|07|08|12|99)$")]
    public string Codigo { get; set; } = null!;

    [XmlElement("CodigoTarifaIVA")]
    [RegularExpression("^(01|02|03|04|05|06|07|08|09|10|11)$")]
    public string? CodigoTarifaIVA { get; set; }

    [XmlElement("TotalMontoImpuesto")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal TotalMontoImpuesto { get; set; }
}
