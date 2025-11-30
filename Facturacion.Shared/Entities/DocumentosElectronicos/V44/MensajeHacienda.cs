using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44;

/// <summary>
/// Mensaje de uso exclusivo por parte de la Dirección General de Tributación
/// Documento tipo 05, 06, 07
/// </summary>
[XmlRoot("MensajeHacienda", Namespace = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeHacienda")]
public class MensajeHacienda
{
    /// <summary>
    /// Clave numérica del comprobante (50 dígitos)
    /// </summary>
    [XmlElement("Clave")]
    [Required]
    [RegularExpression("^\\d{50}$")]
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Nombre o razón social del emisor
    /// </summary>
    [XmlElement("NombreEmisor")]
    [Required]
    [StringLength(100)]
    public string NombreEmisor { get; set; } = null!;

    /// <summary>
    /// Tipo de identificación del emisor: 01 Cédula Física, 02 Cédula Jurídica, 03 DIMEX, 04 NITE, 05 Extranjero No Domiciliado, 06 No Contribuyente
    /// </summary>
    [XmlElement("TipoIdentificacionEmisor")]
    [Required]
    [RegularExpression("^(01|02|03|04|05|06)$")]
    public string TipoIdentificacionEmisor { get; set; } = null!;

    /// <summary>
    /// Número de cédula del emisor
    /// </summary>
    [XmlElement("NumeroCedulaEmisor")]
    [Required]
    [StringLength(20)]
    public string NumeroCedulaEmisor { get; set; } = null!;

    /// <summary>
    /// Nombre o razón social del receptor
    /// </summary>
    [XmlElement("NombreReceptor")]
    [StringLength(100)]
    public string? NombreReceptor { get; set; }

    /// <summary>
    /// Tipo de identificación del receptor
    /// </summary>
    [XmlElement("TipoIdentificacionReceptor")]
    [RegularExpression("^(01|02|03|04|05|06)$")]
    public string? TipoIdentificacionReceptor { get; set; }

    /// <summary>
    /// Número de cédula del receptor
    /// </summary>
    [XmlElement("NumeroCedulaReceptor")]
    [StringLength(20)]
    public string? NumeroCedulaReceptor { get; set; }

    /// <summary>
    /// Código del mensaje de respuesta: 1 Aceptado, 3 Rechazado
    /// </summary>
    [XmlElement("Mensaje")]
    [Required]
    [Range(1, 3)]
    public int Mensaje { get; set; }

    /// <summary>
    /// Estado del mensaje
    /// </summary>
    [XmlElement("EstadoMensaje")]
    [Required]
    [StringLength(9)]
    public string EstadoMensaje { get; set; } = null!;

    /// <summary>
    /// Detalle del mensaje
    /// </summary>
    [XmlElement("DetalleMensaje")]
    [Required]
    public string DetalleMensaje { get; set; } = null!;

    /// <summary>
    /// Monto total del impuesto (obligatorio si el comprobante tiene impuesto)
    /// </summary>
    [XmlElement("MontoTotalImpuesto")]
    [Range(typeof(decimal), "0", "999999999999999.99999")]
    public decimal? MontoTotalImpuesto { get; set; }

    /// <summary>
    /// Monto total de la factura
    /// </summary>
    [XmlElement("TotalFactura")]
    [Required]
    [Range(typeof(decimal), "0", "999999999999999.99999")]
    public decimal TotalFactura { get; set; }

    /// <summary>
    /// Firma digital del documento
    /// </summary>
    [XmlElement("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public object? Signature { get; set; }
}
