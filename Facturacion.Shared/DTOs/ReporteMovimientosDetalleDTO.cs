namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para detalle de movimientos de inventario en el reporte
/// </summary>
public class ReporteMovimientosDetalleDTO
{
    public DateTime Fecha { get; set; }
    public string TipoMovimiento { get; set; } = null!;
    public string Producto { get; set; } = null!;
    public decimal Cantidad { get; set; }
    public string Referencia { get; set; } = null!;
    public string Usuario { get; set; } = null!;
}
