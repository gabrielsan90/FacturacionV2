using Facturacion.Shared.Enums;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para detalles de un documento con información de pagos
/// </summary>
public class DocumentoDetalleDTO
{
    public Guid DocumentoId { get; set; }
    public string NumeroConsecutivo { get; set; } = null!;
    public string ClienteNombre { get; set; } = null!;
    public DateTime FechaEmision { get; set; }
    public decimal TotalVenta { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public TipoMoneda Moneda { get; set; }
    public decimal? TipoCambio { get; set; }
}
