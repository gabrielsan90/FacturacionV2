namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para información de saldo de un documento
/// </summary>
public class SaldoDocumentoDTO
{
    public Guid DocumentoId { get; set; }
    public decimal TotalVenta { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
}
