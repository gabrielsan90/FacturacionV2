using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

/// <summary>
/// Información del receptor del comprobante electrónico
/// </summary>
public class ReceptorType
{
    /// <summary>
    /// Nombre o razón social del receptor
    /// </summary>
    [XmlElement("Nombre")]
    [StringLength(100, MinimumLength = 3)]
    public string? Nombre { get; set; }

    /// <summary>
    /// Identificación del receptor
    /// </summary>
    [XmlElement("Identificacion")]
    public IdentificacionType? Identificacion { get; set; }

    /// <summary>
    /// Código de actividad económica del receptor (CIIU4, 6 dígitos) - Obligatorio en v4.4
    /// </summary>
    [XmlElement("ActividadEconomica")]
    [RegularExpression("^\\d{6}$")]
    public string? ActividadEconomica { get; set; }

    /// <summary>
    /// Nombre comercial del receptor
    /// </summary>
    [XmlElement("NombreComercial")]
    [StringLength(80)]
    public string? NombreComercial { get; set; }

    /// <summary>
    /// Ubicación del receptor
    /// </summary>
    [XmlElement("Ubicacion")]
    public UbicacionType? Ubicacion { get; set; }

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
