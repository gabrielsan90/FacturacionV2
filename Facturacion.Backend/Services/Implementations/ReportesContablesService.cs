using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

public class ReportesContablesService : IReportesContablesService
{
    private readonly DataContext _context;

    public ReportesContablesService(DataContext context)
    {
        _context = context;
    }

    // ========================================================================
    // 1. BALANZA DE COMPROBACIÓN
    // ========================================================================

    public async Task<BalanzaComprobacionDTO> GetBalanzaComprobacionAsync(
        Guid empresaId, DateTime fechaDesde, DateTime fechaHasta, int? nivel, bool incluirSinMovimiento)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);
        var cuentas = await _context.CuentasContables
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.Activo)
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        if (nivel.HasValue)
            cuentas = cuentas.Where(c => c.Nivel <= nivel.Value).ToList();

        // Movimientos ANTES del período (saldo anterior)
        var movimientosAnteriores = await _context.MovimientosContables
            .Include(m => m.AsientoContable)
            .Where(m => m.AsientoContable!.EmpresaId == empresaId
                     && m.AsientoContable.Estado == "APR"
                     && m.AsientoContable.Fecha < fechaDesde)
            .GroupBy(m => m.CuentaContableId)
            .Select(g => new
            {
                CuentaId = g.Key,
                TotalDebe = g.Sum(m => m.Debe),
                TotalHaber = g.Sum(m => m.Haber)
            })
            .ToDictionaryAsync(x => x.CuentaId);

        // Movimientos DEL período
        var movimientosPeriodo = await _context.MovimientosContables
            .Include(m => m.AsientoContable)
            .Where(m => m.AsientoContable!.EmpresaId == empresaId
                     && m.AsientoContable.Estado == "APR"
                     && m.AsientoContable.Fecha >= fechaDesde
                     && m.AsientoContable.Fecha <= fechaHasta)
            .GroupBy(m => m.CuentaContableId)
            .Select(g => new
            {
                CuentaId = g.Key,
                TotalDebe = g.Sum(m => m.Debe),
                TotalHaber = g.Sum(m => m.Haber)
            })
            .ToDictionaryAsync(x => x.CuentaId);

        var balanza = new BalanzaComprobacionDTO
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Empresa = empresa?.NombreComercial ?? empresa?.RazonSocial ?? "",
            Periodo = $"{fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}"
        };

        foreach (var cuenta in cuentas)
        {
            movimientosAnteriores.TryGetValue(cuenta.Id, out var anterior);
            movimientosPeriodo.TryGetValue(cuenta.Id, out var periodo);

            var debeAnterior = (anterior?.TotalDebe ?? 0) + cuenta.SaldoInicial;
            var haberAnterior = anterior?.TotalHaber ?? 0;
            var debePeriodo = periodo?.TotalDebe ?? 0;
            var haberPeriodo = periodo?.TotalHaber ?? 0;

            if (!incluirSinMovimiento && anterior == null && periodo == null && cuenta.SaldoInicial == 0)
                continue;

            // Calcular saldos según naturaleza
            decimal saldoAnteriorDebe = 0, saldoAnteriorHaber = 0;
            decimal saldoFinalDebe = 0, saldoFinalHaber = 0;

            var saldoAnterior = cuenta.Naturaleza == "D"
                ? debeAnterior - haberAnterior
                : haberAnterior - debeAnterior;

            if (saldoAnterior >= 0)
            {
                if (cuenta.Naturaleza == "D") saldoAnteriorDebe = saldoAnterior;
                else saldoAnteriorHaber = saldoAnterior;
            }
            else
            {
                if (cuenta.Naturaleza == "D") saldoAnteriorHaber = Math.Abs(saldoAnterior);
                else saldoAnteriorDebe = Math.Abs(saldoAnterior);
            }

            var saldoFinal = saldoAnterior + (cuenta.Naturaleza == "D"
                ? debePeriodo - haberPeriodo
                : haberPeriodo - debePeriodo);

            if (saldoFinal >= 0)
            {
                if (cuenta.Naturaleza == "D") saldoFinalDebe = saldoFinal;
                else saldoFinalHaber = saldoFinal;
            }
            else
            {
                if (cuenta.Naturaleza == "D") saldoFinalHaber = Math.Abs(saldoFinal);
                else saldoFinalDebe = Math.Abs(saldoFinal);
            }

            balanza.Cuentas.Add(new BalanzaCuentaDTO
            {
                CuentaId = cuenta.Id,
                Codigo = cuenta.Codigo,
                Nombre = cuenta.Nombre,
                TipoCuenta = cuenta.TipoCuenta,
                Naturaleza = cuenta.Naturaleza,
                Nivel = cuenta.Nivel,
                SaldoAnteriorDebe = saldoAnteriorDebe,
                SaldoAnteriorHaber = saldoAnteriorHaber,
                MovimientosDebe = debePeriodo,
                MovimientosHaber = haberPeriodo,
                SaldoFinalDebe = saldoFinalDebe,
                SaldoFinalHaber = saldoFinalHaber
            });
        }

        balanza.TotalSaldoAnteriorDebe = balanza.Cuentas.Sum(c => c.SaldoAnteriorDebe);
        balanza.TotalSaldoAnteriorHaber = balanza.Cuentas.Sum(c => c.SaldoAnteriorHaber);
        balanza.TotalMovimientosDebe = balanza.Cuentas.Sum(c => c.MovimientosDebe);
        balanza.TotalMovimientosHaber = balanza.Cuentas.Sum(c => c.MovimientosHaber);
        balanza.TotalSaldoFinalDebe = balanza.Cuentas.Sum(c => c.SaldoFinalDebe);
        balanza.TotalSaldoFinalHaber = balanza.Cuentas.Sum(c => c.SaldoFinalHaber);

        return balanza;
    }

    // ========================================================================
    // 2. BALANCE GENERAL (Estado de Situación Financiera)
    // ========================================================================

    public async Task<BalanceGeneralDTO> GetBalanceGeneralAsync(Guid empresaId, DateTime fechaCorte)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);

        // Obtener saldos acumulados de TODAS las cuentas hasta la fecha de corte
        var saldosPorCuenta = await ObtenerSaldosAcumuladosAsync(empresaId, fechaCorte);

        var cuentas = await _context.CuentasContables
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.Activo)
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        var balance = new BalanceGeneralDTO
        {
            FechaCorte = fechaCorte,
            Empresa = empresa?.NombreComercial ?? empresa?.RazonSocial ?? "",
            Activos = new BalanceSeccionDTO { TipoCuenta = "ACT", Titulo = "ACTIVOS" },
            Pasivos = new BalanceSeccionDTO { TipoCuenta = "PAS", Titulo = "PASIVOS" },
            Capital = new BalanceSeccionDTO { TipoCuenta = "CAP", Titulo = "CAPITAL CONTABLE" }
        };

        // Calcular resultado del ejercicio (ING - COS - GAS)
        decimal totalIngresos = 0, totalCostos = 0, totalGastos = 0;

        foreach (var cuenta in cuentas)
        {
            saldosPorCuenta.TryGetValue(cuenta.Id, out var saldoData);
            var saldo = CalcularSaldoCuenta(cuenta.Naturaleza, cuenta.SaldoInicial,
                saldoData.Debe, saldoData.Haber);

            if (saldo == 0 && !cuenta.AceptaMovimientos) continue;

            var cuentaDto = new BalanceCuentaDTO
            {
                CuentaId = cuenta.Id,
                Codigo = cuenta.Codigo,
                Nombre = cuenta.Nombre,
                Nivel = cuenta.Nivel,
                Saldo = Math.Abs(saldo),
                EsCuentaPadre = !cuenta.AceptaMovimientos
            };

            // Contra-accounts (naturaleza opuesta al tipo) deben restar, no sumar
            // ACT default=D, PAS default=A, CAP default=A, ING default=A, COS default=D, GAS default=D
            switch (cuenta.TipoCuenta)
            {
                case "ACT":
                    balance.Activos.Cuentas.Add(cuentaDto);
                    if (cuenta.AceptaMovimientos)
                        balance.Activos.Total += cuenta.Naturaleza == "D" ? saldo : -saldo;
                    break;
                case "PAS":
                    balance.Pasivos.Cuentas.Add(cuentaDto);
                    if (cuenta.AceptaMovimientos)
                        balance.Pasivos.Total += cuenta.Naturaleza == "A" ? saldo : -saldo;
                    break;
                case "CAP":
                    balance.Capital.Cuentas.Add(cuentaDto);
                    if (cuenta.AceptaMovimientos)
                        balance.Capital.Total += cuenta.Naturaleza == "A" ? saldo : -saldo;
                    break;
                case "ING":
                    if (cuenta.AceptaMovimientos)
                        totalIngresos += cuenta.Naturaleza == "A" ? saldo : -saldo;
                    break;
                case "COS":
                    if (cuenta.AceptaMovimientos)
                        totalCostos += cuenta.Naturaleza == "D" ? saldo : -saldo;
                    break;
                case "GAS":
                    if (cuenta.AceptaMovimientos)
                        totalGastos += cuenta.Naturaleza == "D" ? saldo : -saldo;
                    break;
            }
        }

        balance.ResultadoEjercicio = totalIngresos - totalCostos - totalGastos;
        balance.Activos.Total = Math.Abs(balance.Activos.Total);
        balance.Pasivos.Total = Math.Abs(balance.Pasivos.Total);
        balance.Capital.Total = Math.Abs(balance.Capital.Total);
        balance.TotalPasivoMasCapital = balance.Pasivos.Total + balance.Capital.Total + balance.ResultadoEjercicio;
        balance.Diferencia = balance.Activos.Total - balance.TotalPasivoMasCapital;
        balance.EstaBalanceado = Math.Abs(balance.Diferencia) < 0.01m;

        return balance;
    }

    // ========================================================================
    // 3. ESTADO DE RESULTADOS
    // ========================================================================

    public async Task<EstadoResultadosDTO> GetEstadoResultadosAsync(
        Guid empresaId, DateTime fechaDesde, DateTime fechaHasta)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);

        var cuentas = await _context.CuentasContables
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.Activo
                     && (c.TipoCuenta == "ING" || c.TipoCuenta == "COS" || c.TipoCuenta == "GAS"))
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        // Solo movimientos del período
        var movimientos = await _context.MovimientosContables
            .Include(m => m.AsientoContable)
            .Where(m => m.AsientoContable!.EmpresaId == empresaId
                     && m.AsientoContable.Estado == "APR"
                     && m.AsientoContable.Fecha >= fechaDesde
                     && m.AsientoContable.Fecha <= fechaHasta)
            .GroupBy(m => m.CuentaContableId)
            .Select(g => new
            {
                CuentaId = g.Key,
                Debe = g.Sum(m => m.Debe),
                Haber = g.Sum(m => m.Haber)
            })
            .ToDictionaryAsync(x => x.CuentaId);

        var resultado = new EstadoResultadosDTO
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Empresa = empresa?.NombreComercial ?? empresa?.RazonSocial ?? "",
            Periodo = $"{fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}",
            Ingresos = new BalanceSeccionDTO { TipoCuenta = "ING", Titulo = "INGRESOS" },
            CostosVentas = new BalanceSeccionDTO { TipoCuenta = "COS", Titulo = "COSTO DE VENTAS" },
            Gastos = new BalanceSeccionDTO { TipoCuenta = "GAS", Titulo = "GASTOS DE OPERACIÓN" }
        };

        foreach (var cuenta in cuentas)
        {
            movimientos.TryGetValue(cuenta.Id, out var mov);
            if (mov == null) continue;

            var saldo = CalcularSaldoCuenta(cuenta.Naturaleza, 0, mov.Debe, mov.Haber);
            if (saldo == 0) continue;

            var cuentaDto = new BalanceCuentaDTO
            {
                CuentaId = cuenta.Id,
                Codigo = cuenta.Codigo,
                Nombre = cuenta.Nombre,
                Nivel = cuenta.Nivel,
                Saldo = Math.Abs(saldo),
                EsCuentaPadre = !cuenta.AceptaMovimientos
            };

            // Contra-accounts (naturaleza opuesta) deben restar del total de la sección
            switch (cuenta.TipoCuenta)
            {
                case "ING":
                    resultado.Ingresos.Cuentas.Add(cuentaDto);
                    if (cuenta.AceptaMovimientos)
                        resultado.Ingresos.Total += cuenta.Naturaleza == "A" ? Math.Abs(saldo) : -Math.Abs(saldo);
                    break;
                case "COS":
                    resultado.CostosVentas.Cuentas.Add(cuentaDto);
                    if (cuenta.AceptaMovimientos)
                        resultado.CostosVentas.Total += cuenta.Naturaleza == "D" ? Math.Abs(saldo) : -Math.Abs(saldo);
                    break;
                case "GAS":
                    resultado.Gastos.Cuentas.Add(cuentaDto);
                    if (cuenta.AceptaMovimientos)
                        resultado.Gastos.Total += cuenta.Naturaleza == "D" ? Math.Abs(saldo) : -Math.Abs(saldo);
                    break;
            }
        }

        resultado.TotalIngresos = resultado.Ingresos.Total;
        resultado.TotalCostosVentas = resultado.CostosVentas.Total;
        resultado.UtilidadBruta = resultado.TotalIngresos - resultado.TotalCostosVentas;
        resultado.TotalGastos = resultado.Gastos.Total;
        resultado.UtilidadOperativa = resultado.UtilidadBruta - resultado.TotalGastos;
        resultado.UtilidadNeta = resultado.UtilidadOperativa;

        return resultado;
    }

    // ========================================================================
    // 4. LIBRO DIARIO
    // ========================================================================

    public async Task<LibroDiarioDTO> GetLibroDiarioAsync(
        Guid empresaId, DateTime fechaDesde, DateTime fechaHasta)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);

        var asientos = await _context.AsientosContables
            .Include(a => a.Movimientos!)
                .ThenInclude(m => m.CuentaContable)
            .Where(a => a.EmpresaId == empresaId
                     && a.Estado == "APR"
                     && !a.IsDeleted
                     && a.Fecha >= fechaDesde
                     && a.Fecha <= fechaHasta)
            .OrderBy(a => a.Fecha)
            .ThenBy(a => a.Numero)
            .ToListAsync();

        var libro = new LibroDiarioDTO
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Empresa = empresa?.NombreComercial ?? empresa?.RazonSocial ?? "",
            Periodo = $"{fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}",
            TotalAsientos = asientos.Count
        };

        foreach (var asiento in asientos)
        {
            var asientoDto = new AsientoDiarioDTO
            {
                AsientoId = asiento.Id,
                Numero = asiento.Numero,
                Fecha = asiento.Fecha,
                TipoAsiento = asiento.TipoAsiento,
                TipoAsientoDescripcion = asiento.TipoAsientoDescripcion,
                Concepto = asiento.Concepto,
                Referencia = asiento.Referencia,
                ModuloOrigen = asiento.ModuloOrigen,
                ModuloOrigenDescripcion = asiento.ModuloOrigenDescripcion,
                TotalDebe = asiento.TotalDebe,
                TotalHaber = asiento.TotalHaber,
                Movimientos = asiento.Movimientos?
                    .OrderBy(m => m.NumeroLinea)
                    .Select(m => new MovimientoDiarioDTO
                    {
                        CuentaCodigo = m.CuentaContable?.Codigo ?? "",
                        CuentaNombre = m.CuentaContable?.Nombre ?? "",
                        Descripcion = m.Descripcion,
                        Debe = m.Debe,
                        Haber = m.Haber
                    }).ToList() ?? new()
            };

            libro.Asientos.Add(asientoDto);
        }

        libro.TotalDebe = libro.Asientos.Sum(a => a.TotalDebe);
        libro.TotalHaber = libro.Asientos.Sum(a => a.TotalHaber);

        return libro;
    }

    // ========================================================================
    // 5. FLUJO DE EFECTIVO (Método Directo simplificado)
    // ========================================================================

    public async Task<FlujoEfectivoDTO> GetFlujoEfectivoAsync(
        Guid empresaId, DateTime fechaDesde, DateTime fechaHasta)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);

        // Identificar cuentas de efectivo (Caja y Bancos — típicamente código empieza con 1.1.01 o 1.1.02)
        var cuentasEfectivo = await _context.CuentasContables
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.Activo && c.AceptaMovimientos
                     && c.TipoCuenta == "ACT"
                     && (c.Codigo.StartsWith("1.1.01") || c.Codigo.StartsWith("1.1.02")
                         || c.Codigo.StartsWith("1.01.01") || c.Codigo.StartsWith("1.01.02")
                         || c.Nombre.ToLower().Contains("caja") || c.Nombre.ToLower().Contains("banco")))
            .Select(c => c.Id)
            .ToListAsync();

        // Saldo inicial de efectivo (acumulado antes del período)
        var saldoInicialData = await _context.MovimientosContables
            .Include(m => m.AsientoContable)
            .Where(m => cuentasEfectivo.Contains(m.CuentaContableId)
                     && m.AsientoContable!.EmpresaId == empresaId
                     && m.AsientoContable.Estado == "APR"
                     && m.AsientoContable.Fecha < fechaDesde)
            .GroupBy(m => 1)
            .Select(g => new { Debe = g.Sum(m => m.Debe), Haber = g.Sum(m => m.Haber) })
            .FirstOrDefaultAsync();

        var saldoInicialCuentas = await _context.CuentasContables
            .Where(c => cuentasEfectivo.Contains(c.Id))
            .SumAsync(c => c.SaldoInicial);

        var saldoInicial = saldoInicialCuentas + (saldoInicialData?.Debe ?? 0) - (saldoInicialData?.Haber ?? 0);

        // Movimientos del período en cuentas de efectivo con su contrapartida
        var movimientosEfectivo = await _context.MovimientosContables
            .Include(m => m.AsientoContable)
            .Include(m => m.CuentaContable)
            .Where(m => m.AsientoContable!.EmpresaId == empresaId
                     && m.AsientoContable.Estado == "APR"
                     && m.AsientoContable.Fecha >= fechaDesde
                     && m.AsientoContable.Fecha <= fechaHasta
                     && !cuentasEfectivo.Contains(m.CuentaContableId))
            .Select(m => new
            {
                AsientoId = m.AsientoContableId,
                m.CuentaContable!.TipoCuenta,
                m.CuentaContable.Codigo,
                m.CuentaContable.Nombre,
                m.Debe,
                m.Haber,
                TieneContrapartidaEfectivo = m.AsientoContable!.Movimientos!
                    .Any(m2 => cuentasEfectivo.Contains(m2.CuentaContableId))
            })
            .Where(m => m.TieneContrapartidaEfectivo)
            .ToListAsync();

        // Clasificar movimientos por actividad
        var operacion = new FlujoSeccionDTO { Titulo = "Actividades de Operación" };
        var inversion = new FlujoSeccionDTO { Titulo = "Actividades de Inversión" };
        var financiamiento = new FlujoSeccionDTO { Titulo = "Actividades de Financiamiento" };

        // Agrupar por cuenta contrapartida
        var agrupados = movimientosEfectivo
            .GroupBy(m => new { m.Codigo, m.Nombre, m.TipoCuenta })
            .Select(g => new
            {
                g.Key.Codigo,
                g.Key.Nombre,
                g.Key.TipoCuenta,
                // Contrapartida: si la cuenta acreedora tiene Haber, es un ingreso de efectivo
                // si tiene Debe, es un egreso de efectivo
                Monto = g.Sum(m => m.Haber) - g.Sum(m => m.Debe)
            })
            .Where(x => x.Monto != 0)
            .OrderBy(x => x.Codigo);

        foreach (var item in agrupados)
        {
            var linea = new FlujoLineaDTO
            {
                Concepto = $"{item.Codigo} - {item.Nombre}",
                CuentaCodigo = item.Codigo,
                Monto = item.Monto
            };

            switch (item.TipoCuenta)
            {
                // Operación: ING, GAS, COS, CxC (1.1.03+), CxP (2.1), Impuestos
                case "ING":
                case "GAS":
                case "COS":
                    operacion.Lineas.Add(linea);
                    break;
                // Inversión: Activos fijos (1.2+), Activos no corrientes
                case "ACT":
                    if (item.Codigo.StartsWith("1.2") || item.Codigo.StartsWith("1.02"))
                        inversion.Lineas.Add(linea);
                    else
                        operacion.Lineas.Add(linea);
                    break;
                // Financiamiento: Pasivos a largo plazo (2.2+), Capital
                case "PAS":
                    if (item.Codigo.StartsWith("2.2") || item.Codigo.StartsWith("2.02"))
                        financiamiento.Lineas.Add(linea);
                    else
                        operacion.Lineas.Add(linea);
                    break;
                case "CAP":
                    financiamiento.Lineas.Add(linea);
                    break;
                default:
                    operacion.Lineas.Add(linea);
                    break;
            }
        }

        operacion.Total = operacion.Lineas.Sum(l => l.Monto);
        inversion.Total = inversion.Lineas.Sum(l => l.Monto);
        financiamiento.Total = financiamiento.Lineas.Sum(l => l.Monto);

        return new FlujoEfectivoDTO
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Empresa = empresa?.NombreComercial ?? empresa?.RazonSocial ?? "",
            Periodo = $"{fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}",
            ActividadesOperacion = operacion,
            ActividadesInversion = inversion,
            ActividadesFinanciamiento = financiamiento,
            SaldoInicialEfectivo = saldoInicial,
            VariacionNeta = operacion.Total + inversion.Total + financiamiento.Total,
            SaldoFinalEfectivo = saldoInicial + operacion.Total + inversion.Total + financiamiento.Total
        };
    }

    // ========================================================================
    // 6. BALANCE DE SUMAS Y SALDOS
    // ========================================================================

    public async Task<BalanceSumasySaldosDTO> GetBalanceSumasySaldosAsync(
        Guid empresaId, DateTime fechaDesde, DateTime fechaHasta, int? nivel, bool incluirSinMovimiento)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);

        var cuentas = await _context.CuentasContables
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.Activo)
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        if (nivel.HasValue)
            cuentas = cuentas.Where(c => c.Nivel <= nivel.Value).ToList();

        // Total de movimientos acumulados hasta fechaHasta
        var movimientos = await _context.MovimientosContables
            .Include(m => m.AsientoContable)
            .Where(m => m.AsientoContable!.EmpresaId == empresaId
                     && m.AsientoContable.Estado == "APR"
                     && m.AsientoContable.Fecha >= fechaDesde
                     && m.AsientoContable.Fecha <= fechaHasta)
            .GroupBy(m => m.CuentaContableId)
            .Select(g => new
            {
                CuentaId = g.Key,
                TotalDebe = g.Sum(m => m.Debe),
                TotalHaber = g.Sum(m => m.Haber)
            })
            .ToDictionaryAsync(x => x.CuentaId);

        var reporte = new BalanceSumasySaldosDTO
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            Empresa = empresa?.NombreComercial ?? empresa?.RazonSocial ?? "",
            Periodo = $"{fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}"
        };

        foreach (var cuenta in cuentas)
        {
            movimientos.TryGetValue(cuenta.Id, out var mov);

            var sumasDebe = mov?.TotalDebe ?? 0;
            var sumasHaber = mov?.TotalHaber ?? 0;

            if (!incluirSinMovimiento && sumasDebe == 0 && sumasHaber == 0)
                continue;

            // Calcular saldo resultante
            decimal saldoDebe = 0, saldoHaber = 0;
            var diferencia = sumasDebe - sumasHaber;

            if (diferencia > 0) saldoDebe = diferencia;
            else if (diferencia < 0) saldoHaber = Math.Abs(diferencia);

            reporte.Cuentas.Add(new SumasySaldosCuentaDTO
            {
                CuentaId = cuenta.Id,
                Codigo = cuenta.Codigo,
                Nombre = cuenta.Nombre,
                TipoCuenta = cuenta.TipoCuenta,
                Naturaleza = cuenta.Naturaleza,
                Nivel = cuenta.Nivel,
                SumasDebe = sumasDebe,
                SumasHaber = sumasHaber,
                SaldoDebe = saldoDebe,
                SaldoHaber = saldoHaber
            });
        }

        reporte.TotalSumasDebe = reporte.Cuentas.Sum(c => c.SumasDebe);
        reporte.TotalSumasHaber = reporte.Cuentas.Sum(c => c.SumasHaber);
        reporte.TotalSaldosDebe = reporte.Cuentas.Sum(c => c.SaldoDebe);
        reporte.TotalSaldosHaber = reporte.Cuentas.Sum(c => c.SaldoHaber);

        return reporte;
    }

    // ========================================================================
    // HELPERS
    // ========================================================================

    private async Task<Dictionary<Guid, (decimal Debe, decimal Haber)>> ObtenerSaldosAcumuladosAsync(
        Guid empresaId, DateTime hastaFecha)
    {
        var result = await _context.MovimientosContables
            .Include(m => m.AsientoContable)
            .Where(m => m.AsientoContable!.EmpresaId == empresaId
                     && m.AsientoContable.Estado == "APR"
                     && m.AsientoContable.Fecha <= hastaFecha)
            .GroupBy(m => m.CuentaContableId)
            .Select(g => new
            {
                CuentaId = g.Key,
                Debe = g.Sum(m => m.Debe),
                Haber = g.Sum(m => m.Haber)
            })
            .ToDictionaryAsync(x => x.CuentaId, x => (x.Debe, x.Haber));

        return result;
    }

    private static decimal CalcularSaldoCuenta(string naturaleza, decimal saldoInicial, decimal debe, decimal haber)
    {
        return naturaleza == "D"
            ? saldoInicial + debe - haber
            : saldoInicial + haber - debe;
    }
}
