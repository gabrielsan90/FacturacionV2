using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

/// <summary>
/// Código de moneda según ISO 4217
/// </summary>
public class CodigoMonedaType
{
    /// <summary>
    /// Código de la moneda según ISO 4217
    /// </summary>
    [XmlElement("CodigoMoneda")]
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string CodigoMoneda { get; set; } = "CRC";

    /// <summary>
    /// Tipo de cambio de la moneda con respecto al colón costarricense
    /// </summary>
    [XmlElement("TipoCambio")]
    [Required]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal TipoCambio { get; set; } = 1.00000m;
}
