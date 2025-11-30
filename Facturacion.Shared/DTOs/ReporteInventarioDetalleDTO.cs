namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para detalle de inventario en el reporte
/// </summary>
public class ReporteInventarioDetalleDTO
{
    public string CodigoProducto { get; set; } = null!;
    public string NombreProducto { get; set; } = null!;
    public string Sucursal { get; set; } = null!;
    public decimal StockActual { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public string EstadoStock { get; set; } = null!;
}
