namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para detalle de impuestos por tarifa
/// </summary>
public class ReporteImpuestosTarifaDTO
{
    public string TipoImpuesto { get; set; } = null!;
    public decimal BaseImponible { get; set; }
    public decimal MontoImpuesto { get; set; }
}
