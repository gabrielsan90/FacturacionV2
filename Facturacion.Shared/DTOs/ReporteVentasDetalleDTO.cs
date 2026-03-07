namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para detalle de ventas en el reporte
/// </summary>
public class ReporteVentasDetalleDTO
{
    public DateTime FechaEmision { get; set; }
    public string NumeroConsecutivo { get; set; } = null!;
    public string TipoDocumento { get; set; } = null!;
    public string Cliente { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Descuentos { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "CRC";
}
