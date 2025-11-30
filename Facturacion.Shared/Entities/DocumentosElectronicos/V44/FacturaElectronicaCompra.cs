using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44;

/// <summary>
/// Factura Electrónica de Compra (FEC) - Documento tipo 08
/// Usada para auto-facturación de compras a proveedores no obligados a facturar electrónicamente
/// </summary>
[XmlRoot("FacturaElectronicaCompra", Namespace = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronicaCompra")]
public class FacturaElectronicaCompra
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
    /// Información del emisor (comprador que emite la FEC)
    /// </summary>
    [XmlElement("Emisor")]
    [Required]
    public EmisorType Emisor { get; set; } = null!;

    /// <summary>
    /// Información del receptor (proveedor)
    /// </summary>
    [XmlElement("Receptor")]
    [Required]
    public ReceptorType Receptor { get; set; } = null!;

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
    /// Resumen de totales de la factura de compra
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
