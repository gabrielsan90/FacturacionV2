namespace Facturacion.Shared.DTOs;

// ========================================================================
// FILTROS COMUNES
// ========================================================================

/// <summary>
/// Filtro común para reportes contables por rango de fechas.
/// </summary>
public class FiltroReporteContableDTO
{
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public int? Nivel { get; set; }
    public bool IncluirCuentasSinMovimiento { get; set; }
}

// ========================================================================
// 1. BALANZA DE COMPROBACIÓN (Trial Balance)
// ========================================================================

public class BalanzaComprobacionDTO
{
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public string Empresa { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;

    // Totales generales
    public decimal TotalSaldoAnteriorDebe { get; set; }
    public decimal TotalSaldoAnteriorHaber { get; set; }
    public decimal TotalMovimientosDebe { get; set; }
    public decimal TotalMovimientosHaber { get; set; }
    public decimal TotalSaldoFinalDebe { get; set; }
    public decimal TotalSaldoFinalHaber { get; set; }

    public List<BalanzaCuentaDTO> Cuentas { get; set; } = new();
}

public class BalanzaCuentaDTO
{
    public Guid CuentaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public string Naturaleza { get; set; } = string.Empty;
    public int Nivel { get; set; }

    // Saldos anteriores (antes de FechaDesde)
    public decimal SaldoAnteriorDebe { get; set; }
    public decimal SaldoAnteriorHaber { get; set; }

    // Movimientos del período
    public decimal MovimientosDebe { get; set; }
    public decimal MovimientosHaber { get; set; }

    // Saldo final = SaldoAnterior + Movimientos
    public decimal SaldoFinalDebe { get; set; }
    public decimal SaldoFinalHaber { get; set; }
}

// ========================================================================
// 2. BALANCE GENERAL (Balance Sheet / Estado de Situación Financiera)
// ========================================================================

public class BalanceGeneralDTO
{
    public DateTime FechaCorte { get; set; }
    public string Empresa { get; set; } = string.Empty;

    // Secciones principales
    public BalanceSeccionDTO Activos { get; set; } = new();
    public BalanceSeccionDTO Pasivos { get; set; } = new();
    public BalanceSeccionDTO Capital { get; set; } = new();

    // Resultado del ejercicio (Ingresos - Gastos - Costos del período)
    public decimal ResultadoEjercicio { get; set; }

    // Ecuación contable: Activos = Pasivos + Capital + ResultadoEjercicio
    public decimal TotalPasivoMasCapital { get; set; }
    public decimal Diferencia { get; set; }
    public bool EstaBalanceado { get; set; }
}

public class BalanceSeccionDTO
{
    public string TipoCuenta { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<BalanceCuentaDTO> Cuentas { get; set; } = new();
}

public class BalanceCuentaDTO
{
    public Guid CuentaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public decimal Saldo { get; set; }
    public bool EsCuentaPadre { get; set; }
}

// ========================================================================
// 3. ESTADO DE RESULTADOS (Income Statement / P&L)
// ========================================================================

public class EstadoResultadosDTO
{
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public string Empresa { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;

    public BalanceSeccionDTO Ingresos { get; set; } = new();
    public BalanceSeccionDTO CostosVentas { get; set; } = new();
    public BalanceSeccionDTO Gastos { get; set; } = new();

    // Cálculos
    public decimal TotalIngresos { get; set; }
    public decimal TotalCostosVentas { get; set; }
    public decimal UtilidadBruta { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal UtilidadOperativa { get; set; }
    public decimal UtilidadNeta { get; set; }
}

// ========================================================================
// 4. LIBRO DIARIO (Journal Book)
// ========================================================================

public class LibroDiarioDTO
{
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public string Empresa { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;

    public int TotalAsientos { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }

    public List<AsientoDiarioDTO> Asientos { get; set; } = new();
}

public class AsientoDiarioDTO
{
    public Guid AsientoId { get; set; }
    public int Numero { get; set; }
    public DateTime Fecha { get; set; }
    public string TipoAsiento { get; set; } = string.Empty;
    public string TipoAsientoDescripcion { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public string? ModuloOrigen { get; set; }
    public string? ModuloOrigenDescripcion { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }

    public List<MovimientoDiarioDTO> Movimientos { get; set; } = new();
}

public class MovimientoDiarioDTO
{
    public string CuentaCodigo { get; set; } = string.Empty;
    public string CuentaNombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}

// ========================================================================
// 5. FLUJO DE EFECTIVO (Cash Flow Statement)
// ========================================================================

public class FlujoEfectivoDTO
{
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public string Empresa { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;

    public FlujoSeccionDTO ActividadesOperacion { get; set; } = new();
    public FlujoSeccionDTO ActividadesInversion { get; set; } = new();
    public FlujoSeccionDTO ActividadesFinanciamiento { get; set; } = new();

    public decimal SaldoInicialEfectivo { get; set; }
    public decimal VariacionNeta { get; set; }
    public decimal SaldoFinalEfectivo { get; set; }
}

public class FlujoSeccionDTO
{
    public string Titulo { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<FlujoLineaDTO> Lineas { get; set; } = new();
}

public class FlujoLineaDTO
{
    public string Concepto { get; set; } = string.Empty;
    public string? CuentaCodigo { get; set; }
    public decimal Monto { get; set; }
}

// ========================================================================
// 6. BALANCE DE SUMAS Y SALDOS
// ========================================================================

public class BalanceSumasySaldosDTO
{
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public string Empresa { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;

    // Totales
    public decimal TotalSumasDebe { get; set; }
    public decimal TotalSumasHaber { get; set; }
    public decimal TotalSaldosDebe { get; set; }
    public decimal TotalSaldosHaber { get; set; }

    public List<SumasySaldosCuentaDTO> Cuentas { get; set; } = new();
}

public class SumasySaldosCuentaDTO
{
    public Guid CuentaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoCuenta { get; set; } = string.Empty;
    public string Naturaleza { get; set; } = string.Empty;
    public int Nivel { get; set; }

    // Total de movimientos (sumas)
    public decimal SumasDebe { get; set; }
    public decimal SumasHaber { get; set; }

    // Saldo resultante
    public decimal SaldoDebe { get; set; }
    public decimal SaldoHaber { get; set; }
}
