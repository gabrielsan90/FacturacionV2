using Facturacion.Shared.Enums;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para listar gastos con pagos pendientes
/// </summary>
public class GastoPendienteDTO
{
    public Guid Id { get; set; }
    public string NumeroDocumento { get; set; } = null!;
    public DateTime FechaGasto { get; set; }
    public string Descripcion { get; set; } = null!;
    public decimal MontoTotal { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public TipoMoneda Moneda { get; set; }
    public EstadoPago EstadoPago { get; set; }
    public string? ProveedorNombre { get; set; }
    public string CategoriaGastoNombre { get; set; } = null!;
    public int DiasVencido { get; set; }
}
