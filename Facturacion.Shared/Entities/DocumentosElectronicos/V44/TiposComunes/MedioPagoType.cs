using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Facturacion.Shared.Entities.DocumentosElectronicos.V44.TiposComunes;

/// <summary>
/// Medio de pago utilizado
/// </summary>
public class MedioPagoType
{
    /// <summary>
    /// Tipo de medio de pago: 01 Efectivo, 02 Tarjeta, 03 Cheque, 04 Transferencia, 05 Recaudado por terceros, 06 SINPE MOVIL, 07 Plataforma Digital, 99 Otros
    /// </summary>
    [XmlElement("TipoMedioPago")]
    [Required]
    [RegularExpression("^(01|02|03|04|05|06|07|99)$")]
    public string TipoMedioPago { get; set; } = null!;

    /// <summary>
    /// Descripción del medio de pago cuando se usa código 99 (Otros)
    /// </summary>
    [XmlElement("MedioPagoOtros")]
    [StringLength(100, MinimumLength = 3)]
    public string? MedioPagoOtros { get; set; }

    /// <summary>
    /// Monto total del medio de pago (obligatorio cuando se usa más de un medio)
    /// </summary>
    [XmlElement("TotalMedioPago")]
    [Range(typeof(decimal), "0", "9999999999999.99999")]
    public decimal? TotalMedioPago { get; set; }
}
