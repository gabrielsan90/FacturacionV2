using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

/// <summary>
/// Tipo de impuesto aplicado a una línea de detalle
/// </summary>
public class ImpuestoType
{
    /// <summary>
    /// Código del impuesto: 01 IVA, 02 ISC, 03 Combustibles, 04 Bebidas alcohólicas, 05 Bebidas sin alcohol y jabones, 06 Tabaco, 07 IVA cálculo especial, 08 IVA Bienes Usados, 12 Cemento, 99 Otros
    /// </summary>
    [XmlElement("Codigo")]
    [Required]
    [RegularExpression("^(01|02|03|04|05|06|07|08|12|99)$")]
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Descripción del impuesto cuando se usa código 99 (Otros)
    /// </summary>
    [XmlElement("CodigoImpuestoOTRO")]
    [StringLength(100, MinimumLength = 5)]
    public string? CodigoImpuestoOTRO { get; set; }

    /// <summary>
    /// Código de tarifa de IVA cuando aplica
    /// </summary>
    [XmlElement("CodigoTarifaIVA")]
    [RegularExpression("^(01|02|03|04|05|06|07|08|09|10|11)$")]
    public string? CodigoTarifaIVA { get; set; }

    /// <summary>
    /// Tarifa del impuesto en porcentaje
    /// </summary>
    [XmlElement("Tarifa")]
    [Range(typeof(decimal), "0", "99.99")]
    public decimal? Tarifa { get; set; }

    /// <summary>
    /// Factor de cálculo de IVA para régimen de bienes usados
    /// </summary>
    [XmlElement("FactorCalculoIVA")]
    [Range(typeof(decimal), "0", "9.9999")]
    public decimal? FactorCalculoIVA { get; set; }

    /// <summary>
    /// Monto del impuesto
    /// </summary>
    [XmlElement("Monto")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal Monto { get; set; }

    /// <summary>
    /// Monto de exoneración del impuesto
    /// </summary>
    [XmlElement("Exoneracion")]
    public ExoneracionType? Exoneracion { get; set; }

    /// <summary>
    /// Monto del impuesto asumido por el emisor o cobrado a nivel de fábrica
    /// </summary>
    [XmlElement("MontoImpuestoCobradoFabricaOEmisor")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? MontoImpuestoCobradoFabricaOEmisor { get; set; }
}

/// <summary>
/// Información de exoneración de impuestos
/// </summary>
public class ExoneracionType
{
    /// <summary>
    /// Tipo de documento de exoneración: 01 Compras Autorizadas, 02 Ventas exentas a diplomáticos, 03 Orden de compra, 04 Exenciones Dirección General de Hacienda, 05 Zonas Francas, 99 Otros
    /// </summary>
    [XmlElement("TipoDocumento")]
    [Required]
    [RegularExpression("^(01|02|03|04|05|99)$")]
    public string TipoDocumento { get; set; } = null!;

    /// <summary>
    /// Número del documento de exoneración
    /// </summary>
    [XmlElement("NumeroDocumento")]
    [Required]
    [StringLength(40)]
    public string NumeroDocumento { get; set; } = null!;

    /// <summary>
    /// Nombre de la institución que emite la exoneración
    /// </summary>
    [XmlElement("NombreInstitucion")]
    [Required]
    [StringLength(100)]
    public string NombreInstitucion { get; set; } = null!;

    /// <summary>
    /// Fecha de emisión del documento de exoneración
    /// </summary>
    [XmlElement("FechaEmision")]
    [Required]
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Porcentaje de exoneración del impuesto
    /// </summary>
    [XmlElement("PorcentajeExoneracion")]
    [Required]
    [Range(0, 100)]
    public int PorcentajeExoneracion { get; set; }

    /// <summary>
    /// Monto total exonerado
    /// </summary>
    [XmlElement("MontoExoneracion")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal MontoExoneracion { get; set; }
}
