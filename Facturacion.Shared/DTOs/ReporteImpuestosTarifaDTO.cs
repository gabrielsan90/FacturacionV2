namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para detalle de impuestos por tarifa
/// </summary>
public class ReporteImpuestosTarifaDTO
{
    public string Tipo { get; set; } = null!; // "Ventas" o "Compras"
    public string TipoImpuesto { get; set; } = null!; // Ej: "IVA 13%", "IVA 4%", "Exento"
    public decimal Tarifa { get; set; } // Porcentaje: 13, 4, 2, 1, 0
    public decimal BaseImponible { get; set; }
    public decimal MontoImpuesto { get; set; }
    public int CantidadDocumentos { get; set; }
}
