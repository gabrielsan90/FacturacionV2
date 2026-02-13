namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para reporte de antigüedad de saldos (aging report).
/// Agrupa las cuentas por pagar según su antigüedad.
/// </summary>
public class AntiguedadSaldosDTO
{
    public Guid ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = null!;
    public string ProveedorIdentificacion { get; set; } = null!;

    // Saldos por rango de días
    public decimal Corriente { get; set; }           // 0-30 días
    public decimal Dias31a60 { get; set; }          // 31-60 días
    public decimal Dias61a90 { get; set; }          // 61-90 días
    public decimal Dias91a120 { get; set; }         // 91-120 días
    public decimal MasDe120Dias { get; set; }       // Más de 120 días

    // Totales
    public decimal TotalSaldo { get; set; }
    public int CantidadFacturas { get; set; }

    // Detalle de facturas
    public List<CuentaPorPagarResumenDTO> Facturas { get; set; } = new();
}

/// <summary>
/// DTO para resumen de una cuenta por pagar.
/// </summary>
public class CuentaPorPagarResumenDTO
{
    public Guid Id { get; set; }
    public string NumeroFactura { get; set; } = null!;
    public DateTime FechaFactura { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal MontoSaldo { get; set; }
    public string Estado { get; set; } = null!;
    public string EstadoDescripcion { get; set; } = null!;
    public int DiasVencido { get; set; }
    public decimal MontoAbonado { get; set; }
}

/// <summary>
/// DTO para estado de cuenta de un proveedor.
/// </summary>
public class EstadoCuentaProveedorDTO
{
    public Guid ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = null!;
    public string ProveedorIdentificacion { get; set; } = null!;
    public string? EmailPrincipal { get; set; }
    public string? TelefonoPrincipal { get; set; }

    // Resumen
    public decimal TotalFacturas { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal TotalPendiente { get; set; }
    public int CantidadFacturasTotal { get; set; }
    public int CantidadFacturasPendientes { get; set; }
    public int CantidadFacturasVencidas { get; set; }

    // Detalle de cuentas por pagar
    public List<CuentaPorPagarDetalleDTO> CuentasPorPagar { get; set; } = new();
}

/// <summary>
/// DTO para detalle completo de una cuenta por pagar con sus abonos.
/// </summary>
public class CuentaPorPagarDetalleDTO
{
    public Guid Id { get; set; }
    public string NumeroFactura { get; set; } = null!;
    public DateTime FechaFactura { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal MontoSaldo { get; set; }
    public decimal MontoAbonado { get; set; }
    public string Estado { get; set; } = null!;
    public string EstadoDescripcion { get; set; } = null!;
    public int DiasVencido { get; set; }
    public bool EstaVencida { get; set; }
    public string? Observaciones { get; set; }
    public string Moneda { get; set; } = null!;
    public decimal TipoCambio { get; set; }

    // Relaciones
    public string? OrdenCompraNumero { get; set; }
    public Guid? OrdenCompraId { get; set; }

    // Abonos realizados
    public List<AbonoPagoDTO> Abonos { get; set; } = new();
}

/// <summary>
/// DTO para abono de pago.
/// </summary>
public class AbonoPagoDTO
{
    public Guid Id { get; set; }
    public DateTime FechaPago { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = null!;
    public string MetodoPagoDescripcion { get; set; } = null!;
    public string? NumeroReferencia { get; set; }
    public string? CuentaBancariaNombre { get; set; }
    public string? Notas { get; set; }
    public string? RegistradoPorNombre { get; set; }
}

/// <summary>
/// DTO para proyección de flujo de caja de cuentas por pagar.
/// </summary>
public class FlujoCajaCuentasPorPagarDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    // Resumen general
    public decimal TotalPorPagar { get; set; }
    public decimal TotalVencido { get; set; }
    public decimal TotalProximoVencer { get; set; }

    // Proyección por períodos
    public List<PeriodoFlujoCajaDTO> Periodos { get; set; } = new();

    // Detalle por proveedor
    public List<ProveedorFlujoCajaDTO> Proveedores { get; set; } = new();
}

/// <summary>
/// DTO para flujo de caja por período.
/// </summary>
public class PeriodoFlujoCajaDTO
{
    public string Periodo { get; set; } = null!;        // "Vencido", "Esta Semana", "Próximos 30 días", etc.
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Monto { get; set; }
    public int CantidadFacturas { get; set; }
}

/// <summary>
/// DTO para flujo de caja por proveedor.
/// </summary>
public class ProveedorFlujoCajaDTO
{
    public Guid ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = null!;
    public decimal TotalPendiente { get; set; }
    public decimal TotalVencido { get; set; }
    public DateTime? ProximoVencimiento { get; set; }
    public decimal MontoProximoVencimiento { get; set; }
}

/// <summary>
/// DTO para registrar un pago a cuenta por pagar.
/// </summary>
public class RegistrarPagoCuentaDTO
{
    public Guid CuentaPorPagarId { get; set; }
    public DateTime FechaPago { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = null!;
    public string? NumeroReferencia { get; set; }
    public Guid? CuentaBancariaId { get; set; }
    public string? Notas { get; set; }
}
