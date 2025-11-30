namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para reporte de ventas consolidado
/// </summary>
public class ReporteVentasDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalImpuestos { get; set; }
    public decimal TotalDescuentos { get; set; }
    public int CantidadDocumentos { get; set; }
    public List<ReporteVentasDetalleDTO> Detalles { get; set; } = new List<ReporteVentasDetalleDTO>();
}
