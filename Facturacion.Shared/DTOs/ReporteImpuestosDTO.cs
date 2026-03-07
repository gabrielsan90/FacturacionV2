namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para reporte de impuestos (declaraciones D-104 a Hacienda)
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
    public int CantidadDocumentosVenta { get; set; }
    public int CantidadDocumentosCompra { get; set; }
    public List<ReporteImpuestosTarifaDTO> DetallesPorTarifa { get; set; } = new List<ReporteImpuestosTarifaDTO>();
}
