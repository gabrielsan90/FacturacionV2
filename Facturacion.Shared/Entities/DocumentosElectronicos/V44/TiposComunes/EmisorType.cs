using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

/// <summary>
/// Información del emisor del comprobante electrónico
/// </summary>
public class EmisorType
{
    /// <summary>
    /// Nombre o razón social del emisor
    /// </summary>
    [XmlElement("Nombre")]
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Identificación del emisor
    /// </summary>
    [XmlElement("Identificacion")]
    [Required]
    public IdentificacionType Identificacion { get; set; } = null!;

    /// <summary>
    /// Nombre comercial del emisor
    /// </summary>
    [XmlElement("NombreComercial")]
    [StringLength(80)]
    public string? NombreComercial { get; set; }

    /// <summary>
    /// Ubicación del emisor
    /// </summary>
    [XmlElement("Ubicacion")]
    [Required]
    public UbicacionType Ubicacion { get; set; } = null!;

    /// <summary>
    /// Teléfono del emisor (opcional)
    /// </summary>
    [XmlElement("Telefono")]
    public TelefonoType? Telefono { get; set; }

    /// <summary>
    /// Fax del emisor (opcional)
    /// </summary>
    [XmlElement("Fax")]
    public TelefonoType? Fax { get; set; }

    /// <summary>
    /// Correos electrónicos del emisor (hasta 4)
    /// </summary>
    [XmlElement("CorreoElectronico")]
    [Required]
    [MinLength(1)]
    [MaxLength(4)]
    public List<string> CorreoElectronico { get; set; } = new();
}
