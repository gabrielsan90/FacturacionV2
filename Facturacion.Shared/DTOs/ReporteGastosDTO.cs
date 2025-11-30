namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para reporte de gastos consolidado
/// </summary>
public class ReporteGastosDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal TotalImpuestos { get; set; }
    public int CantidadGastos { get; set; }
    public List<ReporteGastosDetalleDTO> Detalles { get; set; } = new List<ReporteGastosDetalleDTO>();
}
