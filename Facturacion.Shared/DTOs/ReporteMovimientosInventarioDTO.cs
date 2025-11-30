namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para reporte de movimientos de inventario
/// </summary>
public class ReporteMovimientosInventarioDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalMovimientos { get; set; }
    public List<ReporteMovimientosDetalleDTO> Detalles { get; set; } = new List<ReporteMovimientosDetalleDTO>();
}
