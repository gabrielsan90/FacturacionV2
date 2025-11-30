using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

/// <summary>
/// Tipo de identificación usado para emisor y receptor
/// </summary>
public class IdentificacionType
{
    /// <summary>
    /// Tipo de identificación: 01 Cédula Física, 02 Cédula Jurídica, 03 DIMEX, 04 NITE, 05 Extranjero no domiciliado, 06 No Contribuyente
    /// </summary>
    [XmlElement("Tipo")]
    [Required]
    [RegularExpression("^(01|02|03|04|05|06)$")]
    public string Tipo { get; set; } = null!;

    /// <summary>
    /// Número de identificación, el contribuyente debe estar inscrito ante la Administración Tributaria
    /// </summary>
    [XmlElement("Numero")]
    [Required]
    [StringLength(20)]
    public string Numero { get; set; } = null!;
}
