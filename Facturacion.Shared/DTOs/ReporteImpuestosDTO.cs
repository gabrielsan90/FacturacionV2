namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para reporte de impuestos (declaraciones a Hacienda)
/// </summary>
public class ReporteImpuestosDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalVentasGravadas { get; set; }
    public decimal TotalVentasExentas { get; set; }
    public decimal IVAVentas { get; set; }
    public decimal IVACompras { get; set; }
    public decimal IVAPorPagar { get; set; }
    public List<ReporteImpuestosTarifaDTO> DetallesPorTarifa { get; set; } = new List<ReporteImpuestosTarifaDTO>();
}
