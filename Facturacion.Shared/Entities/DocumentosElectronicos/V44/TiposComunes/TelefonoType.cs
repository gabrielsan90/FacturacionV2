using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

/// <summary>
/// Tipo de dato para representar teléfonos
/// </summary>
public class TelefonoType
{
    /// <summary>
    /// Código de país según ITU-T E.164
    /// </summary>
    [XmlElement("CodigoPais")]
    [Required]
    [StringLength(3)]
    public string CodigoPais { get; set; } = null!;

    /// <summary>
    /// Número de teléfono
    /// </summary>
    [XmlElement("NumTelefono")]
    [Required]
    [StringLength(20)]
    public string NumTelefono { get; set; } = null!;
}
