using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44;

/// <summary>
/// Factura Electrónica de Exportación (FEE) - Documento tipo 09
/// Usada para ventas de exportación fuera de Costa Rica
/// </summary>
[XmlRoot("FacturaElectronicaExportacion", Namespace = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaExportacion")]
public class FacturaElectronicaExportacion
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
    /// Información del emisor (exportador)
    /// </summary>
    [XmlElement("Emisor")]
    [Required]
    public EmisorType Emisor { get; set; } = null!;

    /// <summary>
    /// Información del receptor (comprador extranjero)
    /// </summary>
    [XmlElement("Receptor")]
    [Required]
    public ReceptorExportacionType Receptor { get; set; } = null!;

    /// <summary>
    /// Condición de venta: 01 Contado, 02 Crédito, 03 Consignación, 04 Apartado, 05 Arrendamiento con opción de compra, 06 Arrendamiento en función financiera, 07 Cobro a favor de un tercero, 99 Otros
    /// </summary>
    [XmlElement("CondicionVenta")]
    [Required]
    [RegularExpression("^(01|02|03|04|05|06|07|99)$")]
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
    /// Detalle del servicio o mercancía exportada
    /// </summary>
    [XmlElement("DetalleServicio")]
    [Required]
    public DetalleServicioType DetalleServicio { get; set; } = null!;

    /// <summary>
    /// Resumen de totales de la factura de exportación
    /// </summary>
    [XmlElement("ResumenFactura")]
    [Required]
    public ResumenFacturaExportacionType ResumenFactura { get; set; } = null!;

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
/// Receptor para factura de exportación (cliente extranjero)
/// </summary>
public class ReceptorExportacionType
{
    /// <summary>
    /// Nombre o razón social del receptor extranjero
    /// </summary>
    [XmlElement("Nombre")]
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Tipo de identificación del receptor extranjero
    /// </summary>
    [XmlElement("TipoIdentificacion")]
    [Required]
    [RegularExpression("^(01|02|03|04|05|06)$")]
    public string TipoIdentificacion { get; set; } = null!;

    /// <summary>
    /// Número de identificación del receptor extranjero
    /// </summary>
    [XmlElement("NumeroIdentificacion")]
    [StringLength(20)]
    public string? NumeroIdentificacion { get; set; }

    /// <summary>
    /// Nombre comercial del receptor
    /// </summary>
    [XmlElement("NombreComercial")]
    [StringLength(80)]
    public string? NombreComercial { get; set; }

    /// <summary>
    /// Teléfono del receptor
    /// </summary>
    [XmlElement("Telefono")]
    public TelefonoType? Telefono { get; set; }

    /// <summary>
    /// Fax del receptor
    /// </summary>
    [XmlElement("Fax")]
    public TelefonoType? Fax { get; set; }

    /// <summary>
    /// Correos electrónicos del receptor (hasta 4)
    /// </summary>
    [XmlElement("CorreoElectronico")]
    [MaxLength(4)]
    public List<string>? CorreoElectronico { get; set; }
}

/// <summary>
/// Resumen de totales para factura de exportación
/// </summary>
public class ResumenFacturaExportacionType
{
    /// <summary>
    /// Código de moneda y tipo de cambio
    /// </summary>
    [XmlElement("CodigoTipoMoneda")]
    [Required]
    public CodigoMonedaType CodigoTipoMoneda { get; set; } = null!;

    /// <summary>
    /// Total de servicios gravados (generalmente 0 en exportaciones)
    /// </summary>
    [XmlElement("TotalServGravados")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalServGravados { get; set; }

    /// <summary>
    /// Total de servicios exentos
    /// </summary>
    [XmlElement("TotalServExentos")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalServExentos { get; set; }

    /// <summary>
    /// Total de mercancías gravadas (generalmente 0 en exportaciones)
    /// </summary>
    [XmlElement("TotalMercanciasGravadas")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalMercanciasGravadas { get; set; }

    /// <summary>
    /// Total de mercancías exentas
    /// </summary>
    [XmlElement("TotalMercanciasExentas")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalMercanciasExentas { get; set; }

    /// <summary>
    /// Total gravado (generalmente 0 en exportaciones)
    /// </summary>
    [XmlElement("TotalGravado")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalGravado { get; set; }

    /// <summary>
    /// Total exento
    /// </summary>
    [XmlElement("TotalExento")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalExento { get; set; }

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
    /// Total de impuestos (generalmente 0 en exportaciones)
    /// </summary>
    [XmlElement("TotalImpuesto")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalImpuesto { get; set; }

    /// <summary>
    /// Total del comprobante
    /// </summary>
    [XmlElement("TotalComprobante")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal TotalComprobante { get; set; }
}
