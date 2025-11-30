using Facturacion.Shared.Enums;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO con resumen de gasto incluyendo nombres de relaciones
/// </summary>
public class GastoResumenDTO
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string NumeroDocumento { get; set; } = null!;
    public DateTime FechaGasto { get; set; }
    public string Descripcion { get; set; } = null!;
    public decimal MontoSubtotal { get; set; }
    public decimal MontoImpuesto { get; set; }
    public decimal MontoTotal { get; set; }
    public TipoMoneda Moneda { get; set; }
    public decimal? TipoCambio { get; set; }
    public FormaPago FormaPago { get; set; }
    public EstadoPago EstadoPago { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime? FechaPago { get; set; }
    public bool Aprobado { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? NotasAprobacion { get; set; }

    // Datos de relaciones
    public Guid? ProveedorId { get; set; }
    public string? ProveedorNombre { get; set; }
    public int CategoriaGastoId { get; set; }
    public string CategoriaGastoNombre { get; set; } = null!;
    public string? UsuarioCreacionNombre { get; set; }
    public string? UsuarioAprobacionNombre { get; set; }
    public DateTime FechaCreacion { get; set; }
}
