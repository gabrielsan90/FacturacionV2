namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para detalle de clientes en el reporte
/// </summary>
public class ReporteClientesDetalleDTO
{
    public Guid ClienteId { get; set; }
    public string NombreCliente { get; set; } = null!;
    public string Identificacion { get; set; } = null!;
    public int CantidadCompras { get; set; }
    public decimal TotalCompras { get; set; }
    public DateTime? UltimaCompra { get; set; }
    public decimal SaldoPendiente { get; set; }
}
