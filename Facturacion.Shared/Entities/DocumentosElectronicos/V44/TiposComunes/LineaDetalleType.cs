using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

/// <summary>
/// Línea de detalle para facturas, notas de crédito y débito
/// </summary>
public class LineaDetalleType
{
    /// <summary>
    /// Número de línea del detalle (1 a 1000)
    /// </summary>
    [XmlElement("NumeroLinea")]
    [Required]
    [Range(1, 1000)]
    public int NumeroLinea { get; set; }

    /// <summary>
    /// Partido arancelario (opcional, para importaciones)
    /// </summary>
    [XmlElement("PartidaArancelaria")]
    [StringLength(12)]
    public string? PartidaArancelaria { get; set; }

    /// <summary>
    /// Código comercial del artículo (opcional)
    /// </summary>
    [XmlElement("Codigo")]
    [StringLength(20)]
    public string? Codigo { get; set; }

    /// <summary>
    /// Tipo de código: 01 Código del producto del vendedor, 02 Código del producto del comprador, 03 Código del producto asignado por la industria, 04 Código uso interno, 99 Otros
    /// </summary>
    [XmlElement("TipoCodigo")]
    [RegularExpression("^(01|02|03|04|99)$")]
    public string? TipoCodigo { get; set; }

    /// <summary>
    /// Descripción del tipo de código cuando es 99 (Otros)
    /// </summary>
    [XmlElement("TipoCodigoOTRO")]
    [StringLength(100, MinimumLength = 3)]
    public string? TipoCodigoOTRO { get; set; }

    /// <summary>
    /// Código CAByS (13 dígitos) - Obligatorio desde 01/06/2025 versión CAByS 2025
    /// </summary>
    [XmlElement("CodigoCaByS")]
    [RegularExpression("^\\d{13}$")]
    public string? CodigoCaByS { get; set; }

    /// <summary>
    /// Detalle de la mercancía o servicio
    /// </summary>
    [XmlElement("Detalle")]
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Detalle { get; set; } = null!;

    /// <summary>
    /// Cantidad de la mercancía o servicio
    /// </summary>
    [XmlElement("Cantidad")]
    [Required]
    [Range(typeof(decimal), "0.00001", "9999999999999.99999")]
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Unidad de medida: Sp Servicios Profesionales, m Metro, kg Kilogramo, s Segundo, A Amperio, etc.
    /// </summary>
    [XmlElement("UnidadMedida")]
    [Required]
    [StringLength(20)]
    public string UnidadMedida { get; set; } = null!;

    /// <summary>
    /// Descripción de la unidad de medida cuando no se encuentra en el catálogo
    /// </summary>
    [XmlElement("UnidadMedidaComercial")]
    [StringLength(20)]
    public string? UnidadMedidaComercial { get; set; }

    /// <summary>
    /// Precio unitario de la mercancía o servicio
    /// </summary>
    [XmlElement("PrecioUnitario")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Monto total de la línea (Cantidad × PrecioUnitario)
    /// </summary>
    [XmlElement("MontoTotal")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal MontoTotal { get; set; }

    /// <summary>
    /// Descuento aplicado a la línea (opcional)
    /// </summary>
    [XmlElement("Descuento")]
    public DescuentoType? Descuento { get; set; }

    /// <summary>
    /// Subtotal de la línea (MontoTotal - Descuento)
    /// </summary>
    [XmlElement("SubTotal")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Impuestos aplicados a la línea (hasta 1000)
    /// </summary>
    [XmlElement("Impuesto")]
    [MaxLength(1000)]
    public List<ImpuestoType>? Impuesto { get; set; }

    /// <summary>
    /// Impuesto neto de la línea (suma de impuestos - exoneraciones)
    /// </summary>
    [XmlElement("ImpuestoNeto")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? ImpuestoNeto { get; set; }

    /// <summary>
    /// Monto total de la línea (SubTotal + ImpuestoNeto)
    /// </summary>
    [XmlElement("MontoTotalLinea")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal MontoTotalLinea { get; set; }

    /// <summary>
    /// Número de VIN para vehículos
    /// </summary>
    [XmlElement("NumeroVin")]
    [StringLength(17, MinimumLength = 17)]
    public string? NumeroVin { get; set; }

    /// <summary>
    /// Número de registro de producto farmacéutico (obligatorio desde 01/12/2024)
    /// </summary>
    [XmlElement("NumeroRegistro")]
    [StringLength(50)]
    public string? NumeroRegistro { get; set; }

    /// <summary>
    /// Forma farmacéutica del producto (obligatorio desde 01/12/2024 para farmacéuticos)
    /// </summary>
    [XmlElement("FormaFarmaceutica")]
    [StringLength(100)]
    public string? FormaFarmaceutica { get; set; }
}

/// <summary>
/// Descuento aplicado a una línea
/// </summary>
public class DescuentoType
{
    /// <summary>
    /// Monto del descuento
    /// </summary>
    [XmlElement("MontoDescuento")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal MontoDescuento { get; set; }

    /// <summary>
    /// Naturaleza del descuento (descripción)
    /// </summary>
    [XmlElement("NaturalezaDescuento")]
    [StringLength(80)]
    public string? NaturalezaDescuento { get; set; }
}
