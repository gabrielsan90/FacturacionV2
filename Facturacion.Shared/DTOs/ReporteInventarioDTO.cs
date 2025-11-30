namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para reporte de inventario consolidado
/// </summary>
public class ReporteInventarioDTO
{
    public DateTime FechaReporte { get; set; }
    public int TotalProductos { get; set; }
    public decimal ValorTotalInventario { get; set; }
    public List<ReporteInventarioDetalleDTO> Detalles { get; set; } = new List<ReporteInventarioDetalleDTO>();
}
