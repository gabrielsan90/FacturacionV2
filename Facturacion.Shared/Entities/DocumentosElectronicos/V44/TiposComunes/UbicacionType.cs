using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

/// <summary>
/// Ubicación geográfica
/// </summary>
public class UbicacionType
{
    /// <summary>
    /// Código de provincia según división territorial administrativa de Costa Rica
    /// </summary>
    [XmlElement("Provincia")]
    [Required]
    [RegularExpression("^[1-7]$")]
    public string Provincia { get; set; } = null!;

    /// <summary>
    /// Código de cantón según división territorial administrativa de Costa Rica
    /// </summary>
    [XmlElement("Canton")]
    [Required]
    [RegularExpression("^(0[1-9]|1[0-9]|2[0-2])$")]
    public string Canton { get; set; } = null!;

    /// <summary>
    /// Código de distrito según división territorial administrativa de Costa Rica
    /// </summary>
    [XmlElement("Distrito")]
    [Required]
    [RegularExpression("^(0[1-9]|1[0-5])$")]
    public string Distrito { get; set; } = null!;

    /// <summary>
    /// Código de barrio (opcional)
    /// </summary>
    [XmlElement("Barrio")]
    [RegularExpression("^(0[1-9]|[1-9][0-9])$")]
    public string? Barrio { get; set; }

    /// <summary>
    /// Otras señas de la ubicación exacta (OBLIGATORIO según XSD v4.4)
    /// </summary>
    [XmlElement("OtrasSenas")]
    [Required(ErrorMessage = "Las otras señas son obligatorias según Hacienda v4.4")]
    [StringLength(250, ErrorMessage = "Las otras señas no pueden exceder 250 caracteres")]
    public string OtrasSenas { get; set; } = null!;
}
