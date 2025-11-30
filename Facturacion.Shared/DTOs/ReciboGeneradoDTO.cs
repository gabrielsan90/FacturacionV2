using Facturacion.Shared.Enums;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para lista de recibos de pago generados (REP)
/// </summary>
public class ReciboGeneradoDTO
{
    public Guid DocumentoREPId { get; set; }
    public string NumeroConsecutivoREP { get; set; } = null!;
    public string ClaveREP { get; set; } = null!;
    public string NumeroConsecutivoOriginal { get; set; } = null!;
    public string ClienteNombre { get; set; } = null!;
    public DateTime FechaPago { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public TipoMoneda Moneda { get; set; }
    public string Estado { get; set; } = null!;
}
