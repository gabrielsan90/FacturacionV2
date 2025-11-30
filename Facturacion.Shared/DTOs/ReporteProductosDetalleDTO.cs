namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para detalle de productos en el reporte
/// </summary>
public class ReporteProductosDetalleDTO
{
    public Guid ProductoId { get; set; }
    public string CodigoProducto { get; set; } = null!;
    public string NombreProducto { get; set; } = null!;
    public decimal CantidadVendida { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal PromedioVenta { get; set; }
}
