namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para reporte de ventas por producto
/// </summary>
public class ReporteProductosDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalProductos { get; set; }
    public List<ReporteProductosDetalleDTO> Detalles { get; set; } = new List<ReporteProductosDetalleDTO>();
}
