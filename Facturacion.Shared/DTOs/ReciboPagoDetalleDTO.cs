using Facturacion.Shared.Enums;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para detalles completos de un recibo de pago (para uso en OnGetRecibosGeneradosAsync)
/// </summary>
public class ReciboPagoDetalleDTO
{
    public Guid Id { get; set; }
    public string NumeroConsecutivoOriginal { get; set; } = null!;
    public DateTime FechaPago { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public TipoMoneda Moneda { get; set; }
    public decimal? TipoCambio { get; set; }
    public string? Observaciones { get; set; }
}
