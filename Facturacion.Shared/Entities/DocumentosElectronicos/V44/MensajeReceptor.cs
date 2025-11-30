using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44;

/// <summary>
/// Mensaje de aceptación o rechazo de los documentos electrónicos por parte del obligado tributario
/// Documento tipo 05, 06, 07
/// </summary>
[XmlRoot("MensajeReceptor", Namespace = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/mensajeReceptor")]
public class MensajeReceptor
{
    /// <summary>
    /// Clave numérica del comprobante (50 dígitos)
    /// </summary>
    [XmlElement("Clave")]
    [Required]
    [RegularExpression("^\\d{50}$")]
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Número de cédula del emisor (vendedor)
    /// </summary>
    [XmlElement("NumeroCedulaEmisor")]
    [Required]
    [RegularExpression("^\\d{9,12}$")]
    [StringLength(12)]
    public string NumeroCedulaEmisor { get; set; } = null!;

    /// <summary>
    /// Fecha de emisión de la confirmación
    /// </summary>
    [XmlElement("FechaEmisionDoc")]
    [Required]
    public DateTime FechaEmisionDoc { get; set; }

    /// <summary>
    /// Código del mensaje de respuesta: 1 Aceptado, 2 Aceptado Parcialmente, 3 Rechazado
    /// </summary>
    [XmlElement("Mensaje")]
    [Required]
    [Range(1, 3)]
    public int Mensaje { get; set; }

    /// <summary>
    /// Detalle del mensaje (opcional, máximo 160 caracteres)
    /// </summary>
    [XmlElement("DetalleMensaje")]
    [StringLength(160)]
    public string? DetalleMensaje { get; set; }

    /// <summary>
    /// Monto total del impuesto (obligatorio si el comprobante tiene impuesto)
    /// </summary>
    [XmlElement("MontoTotalImpuesto")]
    [Range(typeof(decimal), "0", "999999999999999.99999")]
    public decimal? MontoTotalImpuesto { get; set; }

    /// <summary>
    /// Código de actividad económica (CIIU4, 6 dígitos)
    /// </summary>
    [XmlElement("CodigoActividad")]
    [StringLength(6)]
    [RegularExpression("^\\d{6}$")]
    public string? CodigoActividad { get; set; }

    /// <summary>
    /// Condición del IVA: 01 General Crédito IVA, 02 General Crédito parcial, 03 Bienes de Capital, 04 Gasto corriente no genera crédito, 05 Proporcionalidad
    /// </summary>
    [XmlElement("CondicionImpuesto")]
    [RegularExpression("^(01|02|03|04|05)$")]
    public string? CondicionImpuesto { get; set; }

    /// <summary>
    /// Monto del impuesto a acreditar
    /// </summary>
    [XmlElement("MontoTotalImpuestoAcreditar")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? MontoTotalImpuestoAcreditar { get; set; }

    /// <summary>
    /// Monto total del gasto a aplicar
    /// </summary>
    [XmlElement("MontoTotalDeGastoAplicable")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? MontoTotalDeGastoAplicable { get; set; }

    /// <summary>
    /// Monto total de la factura
    /// </summary>
    [XmlElement("TotalFactura")]
    [Required]
    [Range(typeof(decimal), "0", "999999999999999.99999")]
    public decimal TotalFactura { get; set; }

    /// <summary>
    /// Número de cédula del receptor (comprador)
    /// </summary>
    [XmlElement("NumeroCedulaReceptor")]
    [Required]
    [RegularExpression("^\\d{9,12}$")]
    [StringLength(20)]
    public string NumeroCedulaReceptor { get; set; } = null!;

    /// <summary>
    /// Numeración consecutiva de los mensajes de confirmación (20 dígitos)
    /// </summary>
    [XmlElement("NumeroConsecutivoReceptor")]
    [Required]
    [RegularExpression("^\\d{20}$")]
    public string NumeroConsecutivoReceptor { get; set; } = null!;

    /// <summary>
    /// Firma digital del documento
    /// </summary>
    [XmlElement("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public object? Signature { get; set; }
}
