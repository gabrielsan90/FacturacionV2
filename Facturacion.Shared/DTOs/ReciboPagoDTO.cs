using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para crear un Recibo Electrónico de Pago (REP)
/// </summary>
public class ReciboPagoDTO
{
    /// <summary>
    /// ID del documento original que se está pagando
    /// </summary>
    [Required(ErrorMessage = "El documento original es obligatorio.")]
    public Guid DocumentoOriginalId { get; set; }

    /// <summary>
    /// Clave del documento original (50 dígitos)
    /// Opcional: se puede obtener del documento
    /// </summary>
    [MaxLength(50)]
    public string? ClaveDocumentoOriginal { get; set; }

    /// <summary>
    /// Fecha en que se recibió el pago
    /// </summary>
    [Required(ErrorMessage = "La fecha de pago es obligatoria.")]
    public DateTime FechaPago { get; set; }

    /// <summary>
    /// Monto pagado en este recibo
    /// </summary>
    [Required(ErrorMessage = "El monto pagado es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto pagado debe ser mayor a cero.")]
    public decimal MontoPagado { get; set; }

    /// <summary>
    /// Moneda del pago (debe coincidir con documento original)
    /// </summary>
    [Required(ErrorMessage = "La moneda es obligatoria.")]
    public TipoMoneda Moneda { get; set; }

    /// <summary>
    /// Tipo de cambio aplicado (obligatorio si no es CRC)
    /// </summary>
    public decimal? TipoCambio { get; set; }

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    [MaxLength(500)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Medios de pago utilizados (puede ser pago mixto)
    /// La suma debe coincidir con MontoPagado
    /// </summary>
    [Required(ErrorMessage = "Debe especificar al menos un medio de pago.")]
    [MinLength(1, ErrorMessage = "Debe especificar al menos un medio de pago.")]
    public List<MedioPagoDTO> MediosPago { get; set; } = new List<MedioPagoDTO>();
}

/// <summary>
/// DTO para medio de pago en un REP
/// </summary>
public class MedioPagoDTO
{
    /// <summary>
    /// Código del medio de pago según catálogo Hacienda
    /// 01-Efectivo, 02-Tarjeta, 03-Cheque, 04-Transferencia,
    /// 05-Recaudado por terceros, 06-SINPE Móvil, 99-Otros
    /// </summary>
    [Required(ErrorMessage = "El código del medio de pago es obligatorio.")]
    [MaxLength(2)]
    public string CodigoMedioPago { get; set; } = null!;

    /// <summary>
    /// Monto pagado con este medio de pago
    /// </summary>
    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
    public decimal Monto { get; set; }

    /// <summary>
    /// Número de referencia (para tarjetas, transferencias, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? NumeroReferencia { get; set; }

    /// <summary>
    /// Descripción adicional del medio de pago
    /// </summary>
    [MaxLength(200)]
    public string? Descripcion { get; set; }
}

/// <summary>
/// Resultado de la generación de un REP
/// </summary>
public class ResultadoREP
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = null!;
    public Guid? ReciboId { get; set; }
    public Guid? DocumentoREPId { get; set; }
    public string? ClaveREP { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string? Estado { get; set; }
    public List<string> Errores { get; set; } = new List<string>();
}

/// <summary>
/// DTO para documentos pendientes de pago (cuentas por cobrar)
/// </summary>
public class DocumentoPendientePagoDTO
{
    public Guid DocumentoId { get; set; }
    public string Clave { get; set; } = null!;
    public string NumeroConsecutivo { get; set; } = null!;
    public string TipoDocumento { get; set; } = null!;
    public string ClienteNombre { get; set; } = null!;
    public string? ClienteIdentificacion { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal TotalVenta { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public int DiasVencido { get; set; }
    public int PlazoDias { get; set; }
    public TipoMoneda Moneda { get; set; }
    public string Estado { get; set; } = null!;
    public int CantidadPagos { get; set; }  // Cantidad de REPs aplicados
}

/// <summary>
/// DTO para detalle de pagos de un documento
/// </summary>
public class DetallePagosDocumentoDTO
{
    public Guid DocumentoId { get; set; }
    public string NumeroConsecutivo { get; set; } = null!;
    public decimal TotalVenta { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public List<ReciboPagoResumenDTO> Pagos { get; set; } = new List<ReciboPagoResumenDTO>();
}

/// <summary>
/// DTO resumen de un recibo de pago
/// </summary>
public class ReciboPagoResumenDTO
{
    public Guid ReciboId { get; set; }
    public Guid DocumentoREPId { get; set; }
    public string ClaveREP { get; set; } = null!;
    public string NumeroConsecutivoREP { get; set; } = null!;
    public DateTime FechaPago { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoRestante { get; set; }
    public string Estado { get; set; } = null!;
    public List<string> MediosPago { get; set; } = new List<string>();
    public string? Observaciones { get; set; }
}
