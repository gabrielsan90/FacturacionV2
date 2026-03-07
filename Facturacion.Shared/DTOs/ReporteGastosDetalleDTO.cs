namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para detalle de gastos en el reporte
/// </summary>
public class ReporteGastosDetalleDTO
{
    public DateTime FechaGasto { get; set; }
    public string NumeroDocumento { get; set; } = null!;
    public string Proveedor { get; set; } = null!;
    public string Categoria { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "CRC";
}
