using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

/// <summary>
/// Información de referencia a otros documentos
/// </summary>
public class InformacionReferenciaType
{
    /// <summary>
    /// Tipo de documento de referencia
    /// </summary>
    [XmlElement("TipoDocIR")]
    [Required]
    [RegularExpression("^(01|02|03|04|05|06|07|08|09|10|11|12|13|14|15|16|17|18|99)$")]
    public string TipoDocIR { get; set; } = null!;

    /// <summary>
    /// Descripción cuando se usa tipo 99 (Otros)
    /// </summary>
    [XmlElement("TipoDocRefOTRO")]
    [StringLength(100, MinimumLength = 5)]
    public string? TipoDocRefOTRO { get; set; }

    /// <summary>
    /// Clave numérica del comprobante electrónico o consecutivo del documento de referencia
    /// </summary>
    [XmlElement("Numero")]
    [Required]
    [StringLength(50)]
    public string Numero { get; set; } = null!;

    /// <summary>
    /// Fecha de emisión del documento de referencia
    /// </summary>
    [XmlElement("FechaEmisionIR")]
    [Required]
    public DateTime FechaEmisionIR { get; set; }

    /// <summary>
    /// Código de referencia: 01 Anula, 02 Corrige texto, 04 Referencia, 05 Sustituye provisional, 06 Devolución, 07 Sustituye, 08 Endosada, 09 NC financiera, 10 ND financiera, 11 No domiciliado, 12 Exoneración posterior, 99 Otros
    /// </summary>
    [XmlElement("Codigo")]
    [RegularExpression("^(01|02|04|05|06|07|08|09|10|11|12|99)$")]
    public string? Codigo { get; set; }

    /// <summary>
    /// Descripción cuando se usa código 99 (Otros)
    /// </summary>
    [XmlElement("CodigoReferenciaOTRO")]
    [StringLength(100, MinimumLength = 5)]
    public string? CodigoReferenciaOTRO { get; set; }

    /// <summary>
    /// Razón de la referencia
    /// </summary>
    [XmlElement("Razon")]
    [StringLength(180)]
    public string? Razon { get; set; }
}
