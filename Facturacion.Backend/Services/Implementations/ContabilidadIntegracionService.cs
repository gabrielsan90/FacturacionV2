using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio de integración contable para generación automática de asientos.
/// Genera asientos contables cuando ocurren operaciones transaccionales.
/// </summary>
public class ContabilidadIntegracionService : IContabilidadIntegracionService
{
    private readonly IContabilidadUnitOfWork _contabilidadUoW;
    private readonly DataContext _context;
    private readonly ILogger<ContabilidadIntegracionService> _logger;

    public ContabilidadIntegracionService(
        IContabilidadUnitOfWork contabilidadUoW,
        DataContext context,
        ILogger<ContabilidadIntegracionService> logger)
    {
        _contabilidadUoW = contabilidadUoW;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> EstaHabilitadoAsync(Guid empresaId)
    {
        return await _contabilidadUoW.ConfiguracionContableRepository
            .EstaHabilitadaGeneracionAutomaticaAsync(empresaId);
    }

    public async Task<bool> ExisteAsientoParaDocumentoAsync(Guid empresaId, string moduloOrigen, Guid documentoOrigenId)
    {
        return await _context.AsientosContables
            .AnyAsync(a => a.EmpresaId == empresaId &&
                          a.ModuloOrigen == moduloOrigen &&
                          a.DocumentoOrigenId == documentoOrigenId &&
                          !a.IsDeleted);
    }

    public async Task<AsientoContable?> GenerarAsientoVentaAsync(Documento documento, string userId)
    {
        try
        {
            // 1. Verificar que está habilitado
            if (!await EstaHabilitadoAsync(documento.EmpresaId))
            {
                _logger.LogDebug("Generación automática de asientos no habilitada para empresa {EmpresaId}", documento.EmpresaId);
                return null;
            }

            // 2. Verificar idempotencia
            if (await ExisteAsientoParaDocumentoAsync(documento.EmpresaId, ModulosContables.Ventas, documento.Id))
            {
                _logger.LogDebug("Ya existe asiento para documento {DocumentoId}", documento.Id);
                return null;
            }

            // 3. Determinar tipo de operación según condición de venta
            string tipoOperacion = documento.CondicionVenta == "01"
                ? TiposOperacionContable.VentaContado
                : TiposOperacionContable.VentaCredito;

            // 4. Obtener mappings de cuentas
            var cuentasIntegracion = await _contabilidadUoW.CuentaIntegracionRepository
                .GetByModuloYTipoOperacionAsync(documento.EmpresaId, ModulosContables.Ventas, tipoOperacion);

            if (!cuentasIntegracion.Any())
            {
                _logger.LogWarning("No hay cuentas de integración configuradas para {Modulo}/{TipoOperacion} en empresa {EmpresaId}",
                    ModulosContables.Ventas, tipoOperacion, documento.EmpresaId);
                return null;
            }

            // 5. Obtener período contable abierto
            var periodo = await _contabilidadUoW.PeriodoContableRepository
                .GetAbiertoAsync(documento.EmpresaId);

            if (periodo == null)
            {
                _logger.LogWarning("No hay período contable abierto para empresa {EmpresaId}", documento.EmpresaId);
                return null;
            }

            // 6. Crear asiento contable
            var asiento = await CrearAsientoBaseAsync(
                documento.EmpresaId,
                periodo,
                $"Venta - {documento.NumeroConsecutivo}",
                documento.NumeroConsecutivo,
                ModulosContables.Ventas,
                documento.Id,
                documento.FechaEmision,
                userId);

            // 7. Crear movimientos contables según mappings
            var movimientos = new List<MovimientoContable>();
            int numeroLinea = 1;

            foreach (var cuentaInt in cuentasIntegracion.OrderBy(c => c.Orden))
            {
                decimal monto = CalcularMontoVenta(documento, cuentaInt.ConceptoContable);
                if (monto == 0) continue;

                var movimiento = new MovimientoContable
                {
                    Id = Guid.NewGuid(),
                    AsientoContableId = asiento.Id,
                    CuentaContableId = cuentaInt.CuentaContableId,
                    NumeroLinea = numeroLinea++,
                    Descripcion = $"{cuentaInt.Descripcion ?? cuentaInt.ConceptoContable} - {documento.ReceptorNombre}",
                    Debe = cuentaInt.TipoMovimiento == "D" ? monto : 0,
                    Haber = cuentaInt.TipoMovimiento == "H" ? monto : 0,
                    Tercero = documento.ReceptorNombre,
                    ClienteId = documento.ClienteId,
                    DocumentoReferencia = documento.NumeroConsecutivo
                };
                movimientos.Add(movimiento);
            }

            // 8. Validar balance
            decimal totalDebe = movimientos.Sum(m => m.Debe);
            decimal totalHaber = movimientos.Sum(m => m.Haber);

            if (Math.Abs(totalDebe - totalHaber) > 0.01m)
            {
                _logger.LogError("Asiento desbalanceado para documento {DocumentoId}: Debe={Debe}, Haber={Haber}",
                    documento.Id, totalDebe, totalHaber);
                return null;
            }

            asiento.TotalDebe = totalDebe;
            asiento.TotalHaber = totalHaber;
            asiento.Movimientos = movimientos;

            // 9. Guardar asiento
            _context.AsientosContables.Add(asiento);
            _context.MovimientosContables.AddRange(movimientos);

            // 10. Opcionalmente aprobar automáticamente
            if (await _contabilidadUoW.ConfiguracionContableRepository
                .EstaHabilitadaAprobacionAutomaticaAsync(documento.EmpresaId))
            {
                asiento.Estado = "APR";
                asiento.FechaAprobacion = FechaCostaRicaHelper.Ahora;
                asiento.AprobadoPorId = userId;
            }

            // Actualizar consecutivo del período
            periodo.UltimoNumeroAsiento = asiento.Numero;
            periodo.CantidadAsientos++;
            periodo.TotalDebe += totalDebe;
            periodo.TotalHaber += totalHaber;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Asiento contable {AsientoId} generado para venta {DocumentoId}",
                asiento.Id, documento.Id);

            return asiento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando asiento para venta {DocumentoId}", documento.Id);
            return null;
        }
    }

    public async Task<AsientoContable?> GenerarAsientoCompraAsync(Gasto gasto, string userId)
    {
        try
        {
            if (!await EstaHabilitadoAsync(gasto.EmpresaId))
                return null;

            if (await ExisteAsientoParaDocumentoAsync(gasto.EmpresaId, ModulosContables.Compras, gasto.Id))
                return null;

            string tipoOperacion = gasto.FormaPago == FormaPago.Efectivo
                ? TiposOperacionContable.CompraContado
                : TiposOperacionContable.CompraCredito;

            var cuentasIntegracion = await _contabilidadUoW.CuentaIntegracionRepository
                .GetByModuloYTipoOperacionAsync(gasto.EmpresaId, ModulosContables.Compras, tipoOperacion);

            if (!cuentasIntegracion.Any())
            {
                _logger.LogWarning("No hay cuentas de integración para compras en empresa {EmpresaId}", gasto.EmpresaId);
                return null;
            }

            var periodo = await _contabilidadUoW.PeriodoContableRepository.GetAbiertoAsync(gasto.EmpresaId);
            if (periodo == null) return null;

            var asiento = await CrearAsientoBaseAsync(
                gasto.EmpresaId,
                periodo,
                $"Compra/Gasto - {gasto.NumeroDocumento}",
                gasto.NumeroDocumento,
                ModulosContables.Compras,
                gasto.Id,
                gasto.FechaGasto,
                userId);

            var movimientos = new List<MovimientoContable>();
            int numeroLinea = 1;

            foreach (var cuentaInt in cuentasIntegracion.OrderBy(c => c.Orden))
            {
                decimal monto = CalcularMontoCompra(gasto, cuentaInt.ConceptoContable);
                if (monto == 0) continue;

                movimientos.Add(new MovimientoContable
                {
                    Id = Guid.NewGuid(),
                    AsientoContableId = asiento.Id,
                    CuentaContableId = cuentaInt.CuentaContableId,
                    NumeroLinea = numeroLinea++,
                    Descripcion = $"{cuentaInt.Descripcion ?? cuentaInt.ConceptoContable} - {gasto.Descripcion}",
                    Debe = cuentaInt.TipoMovimiento == "D" ? monto : 0,
                    Haber = cuentaInt.TipoMovimiento == "H" ? monto : 0,
                    ProveedorId = gasto.ProveedorId,
                    DocumentoReferencia = gasto.NumeroDocumento
                });
            }

            decimal totalDebe = movimientos.Sum(m => m.Debe);
            decimal totalHaber = movimientos.Sum(m => m.Haber);

            if (Math.Abs(totalDebe - totalHaber) > 0.01m)
            {
                _logger.LogError("Asiento desbalanceado para gasto {GastoId}", gasto.Id);
                return null;
            }

            asiento.TotalDebe = totalDebe;
            asiento.TotalHaber = totalHaber;
            asiento.Movimientos = movimientos;

            _context.AsientosContables.Add(asiento);
            _context.MovimientosContables.AddRange(movimientos);

            if (await _contabilidadUoW.ConfiguracionContableRepository.EstaHabilitadaAprobacionAutomaticaAsync(gasto.EmpresaId))
            {
                asiento.Estado = "APR";
                asiento.FechaAprobacion = FechaCostaRicaHelper.Ahora;
                asiento.AprobadoPorId = userId;
            }

            periodo.UltimoNumeroAsiento = asiento.Numero;
            periodo.CantidadAsientos++;
            periodo.TotalDebe += totalDebe;
            periodo.TotalHaber += totalHaber;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Asiento contable {AsientoId} generado para compra/gasto {GastoId}", asiento.Id, gasto.Id);
            return asiento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando asiento para compra {GastoId}", gasto.Id);
            return null;
        }
    }

    public async Task<AsientoContable?> GenerarAsientoPagoGastoAsync(Gasto gasto, decimal montoPago, string userId)
    {
        try
        {
            if (!await EstaHabilitadoAsync(gasto.EmpresaId))
                return null;

            // Para pagos usamos un ID compuesto: GastoId + fecha para permitir múltiples pagos
            var documentoOrigenId = gasto.Id;

            var cuentasIntegracion = await _contabilidadUoW.CuentaIntegracionRepository
                .GetByModuloYTipoOperacionAsync(gasto.EmpresaId, ModulosContables.CuentasPorPagar, TiposOperacionContable.PagoProveedor);

            if (!cuentasIntegracion.Any())
            {
                _logger.LogWarning("No hay cuentas de integración para pago proveedor en empresa {EmpresaId}", gasto.EmpresaId);
                return null;
            }

            var periodo = await _contabilidadUoW.PeriodoContableRepository.GetAbiertoAsync(gasto.EmpresaId);
            if (periodo == null) return null;

            var asiento = await CrearAsientoBaseAsync(
                gasto.EmpresaId,
                periodo,
                $"Pago Proveedor - {gasto.NumeroDocumento}",
                gasto.NumeroDocumento,
                ModulosContables.CuentasPorPagar,
                documentoOrigenId,
                gasto.FechaPago ?? FechaCostaRicaHelper.Ahora,
                userId);

            var movimientos = new List<MovimientoContable>();
            int numeroLinea = 1;

            foreach (var cuentaInt in cuentasIntegracion.OrderBy(c => c.Orden))
            {
                movimientos.Add(new MovimientoContable
                {
                    Id = Guid.NewGuid(),
                    AsientoContableId = asiento.Id,
                    CuentaContableId = cuentaInt.CuentaContableId,
                    NumeroLinea = numeroLinea++,
                    Descripcion = $"{cuentaInt.Descripcion ?? cuentaInt.ConceptoContable} - {gasto.Descripcion}",
                    Debe = cuentaInt.TipoMovimiento == "D" ? montoPago : 0,
                    Haber = cuentaInt.TipoMovimiento == "H" ? montoPago : 0,
                    ProveedorId = gasto.ProveedorId,
                    DocumentoReferencia = gasto.NumeroDocumento
                });
            }

            decimal totalDebe = movimientos.Sum(m => m.Debe);
            decimal totalHaber = movimientos.Sum(m => m.Haber);

            if (Math.Abs(totalDebe - totalHaber) > 0.01m)
            {
                _logger.LogError("Asiento desbalanceado para pago gasto {GastoId}", gasto.Id);
                return null;
            }

            asiento.TotalDebe = totalDebe;
            asiento.TotalHaber = totalHaber;
            asiento.Movimientos = movimientos;

            _context.AsientosContables.Add(asiento);
            _context.MovimientosContables.AddRange(movimientos);

            if (await _contabilidadUoW.ConfiguracionContableRepository.EstaHabilitadaAprobacionAutomaticaAsync(gasto.EmpresaId))
            {
                asiento.Estado = "APR";
                asiento.FechaAprobacion = FechaCostaRicaHelper.Ahora;
                asiento.AprobadoPorId = userId;
            }

            periodo.UltimoNumeroAsiento = asiento.Numero;
            periodo.CantidadAsientos++;
            periodo.TotalDebe += totalDebe;
            periodo.TotalHaber += totalHaber;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Asiento contable {AsientoId} generado para pago de gasto {GastoId}", asiento.Id, gasto.Id);
            return asiento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando asiento para pago de gasto {GastoId}", gasto.Id);
            return null;
        }
    }

    public async Task<AsientoContable?> GenerarAsientoCobroAsync(CuentaPorCobrar cuenta, AbonoCobranza abono, string userId)
    {
        try
        {
            if (!await EstaHabilitadoAsync(cuenta.EmpresaId))
                return null;

            // Usamos el ID del abono como documento origen para permitir múltiples cobros
            if (await ExisteAsientoParaDocumentoAsync(cuenta.EmpresaId, ModulosContables.CuentasPorCobrar, abono.Id))
                return null;

            var cuentasIntegracion = await _contabilidadUoW.CuentaIntegracionRepository
                .GetByModuloYTipoOperacionAsync(cuenta.EmpresaId, ModulosContables.CuentasPorCobrar, TiposOperacionContable.ReciboPago);

            if (!cuentasIntegracion.Any())
            {
                _logger.LogWarning("No hay cuentas de integración para cobro en empresa {EmpresaId}", cuenta.EmpresaId);
                return null;
            }

            var periodo = await _contabilidadUoW.PeriodoContableRepository.GetAbiertoAsync(cuenta.EmpresaId);
            if (periodo == null) return null;

            var asiento = await CrearAsientoBaseAsync(
                cuenta.EmpresaId,
                periodo,
                $"Cobro Cliente - {cuenta.NumeroConsecutivo}",
                abono.NumeroReferencia ?? cuenta.NumeroConsecutivo,
                ModulosContables.CuentasPorCobrar,
                abono.Id,
                abono.FechaPago,
                userId);

            var movimientos = new List<MovimientoContable>();
            int numeroLinea = 1;

            foreach (var cuentaInt in cuentasIntegracion.OrderBy(c => c.Orden))
            {
                movimientos.Add(new MovimientoContable
                {
                    Id = Guid.NewGuid(),
                    AsientoContableId = asiento.Id,
                    CuentaContableId = cuentaInt.CuentaContableId,
                    NumeroLinea = numeroLinea++,
                    Descripcion = $"{cuentaInt.Descripcion ?? cuentaInt.ConceptoContable} - {cuenta.NombreCliente}",
                    Debe = cuentaInt.TipoMovimiento == "D" ? abono.Monto : 0,
                    Haber = cuentaInt.TipoMovimiento == "H" ? abono.Monto : 0,
                    Tercero = cuenta.NombreCliente,
                    ClienteId = cuenta.ClienteId,
                    DocumentoReferencia = cuenta.NumeroConsecutivo
                });
            }

            decimal totalDebe = movimientos.Sum(m => m.Debe);
            decimal totalHaber = movimientos.Sum(m => m.Haber);

            if (Math.Abs(totalDebe - totalHaber) > 0.01m)
            {
                _logger.LogError("Asiento desbalanceado para cobro {AbonoId}", abono.Id);
                return null;
            }

            asiento.TotalDebe = totalDebe;
            asiento.TotalHaber = totalHaber;
            asiento.Movimientos = movimientos;

            _context.AsientosContables.Add(asiento);
            _context.MovimientosContables.AddRange(movimientos);

            if (await _contabilidadUoW.ConfiguracionContableRepository.EstaHabilitadaAprobacionAutomaticaAsync(cuenta.EmpresaId))
            {
                asiento.Estado = "APR";
                asiento.FechaAprobacion = FechaCostaRicaHelper.Ahora;
                asiento.AprobadoPorId = userId;
            }

            periodo.UltimoNumeroAsiento = asiento.Numero;
            periodo.CantidadAsientos++;
            periodo.TotalDebe += totalDebe;
            periodo.TotalHaber += totalHaber;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Asiento contable {AsientoId} generado para cobro {AbonoId}", asiento.Id, abono.Id);
            return asiento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando asiento para cobro {AbonoId}", abono.Id);
            return null;
        }
    }

    public async Task<AsientoContable?> GenerarAsientoMovimientoBancarioAsync(MovimientoBancario movimiento, string userId)
    {
        try
        {
            if (!await EstaHabilitadoAsync(movimiento.EmpresaId))
                return null;

            if (await ExisteAsientoParaDocumentoAsync(movimiento.EmpresaId, ModulosContables.Bancos, movimiento.Id))
                return null;

            // Determinar tipo de operación según el tipo de movimiento
            string tipoOperacion = movimiento.TipoMovimiento switch
            {
                TiposMovimientoBancario.Deposito => TiposOperacionContable.Deposito,
                TiposMovimientoBancario.Retiro => TiposOperacionContable.Retiro,
                TiposMovimientoBancario.Transferencia => movimiento.Naturaleza == NaturalezaMovimiento.Credito
                    ? TiposOperacionContable.Deposito
                    : TiposOperacionContable.Retiro,
                TiposMovimientoBancario.Comision => TiposOperacionContable.ComisionBancaria,
                TiposMovimientoBancario.Interes => TiposOperacionContable.InteresBancario,
                _ => TiposOperacionContable.Deposito
            };

            var cuentasIntegracion = await _contabilidadUoW.CuentaIntegracionRepository
                .GetByModuloYTipoOperacionAsync(movimiento.EmpresaId, ModulosContables.Bancos, tipoOperacion);

            if (!cuentasIntegracion.Any())
            {
                _logger.LogWarning("No hay cuentas de integración para movimiento bancario {Tipo} en empresa {EmpresaId}",
                    tipoOperacion, movimiento.EmpresaId);
                return null;
            }

            var periodo = await _contabilidadUoW.PeriodoContableRepository.GetAbiertoAsync(movimiento.EmpresaId);
            if (periodo == null) return null;

            var asiento = await CrearAsientoBaseAsync(
                movimiento.EmpresaId,
                periodo,
                $"Movimiento Bancario - {movimiento.Numero}",
                movimiento.Numero,
                ModulosContables.Bancos,
                movimiento.Id,
                movimiento.Fecha,
                userId);

            var movimientos = new List<MovimientoContable>();
            int numeroLinea = 1;

            foreach (var cuentaInt in cuentasIntegracion.OrderBy(c => c.Orden))
            {
                movimientos.Add(new MovimientoContable
                {
                    Id = Guid.NewGuid(),
                    AsientoContableId = asiento.Id,
                    CuentaContableId = cuentaInt.CuentaContableId,
                    NumeroLinea = numeroLinea++,
                    Descripcion = $"{cuentaInt.Descripcion ?? cuentaInt.ConceptoContable} - {movimiento.Descripcion}",
                    Debe = cuentaInt.TipoMovimiento == "D" ? movimiento.Monto : 0,
                    Haber = cuentaInt.TipoMovimiento == "H" ? movimiento.Monto : 0,
                    Tercero = movimiento.Beneficiario,
                    DocumentoReferencia = movimiento.NumeroReferencia ?? movimiento.Numero
                });
            }

            decimal totalDebe = movimientos.Sum(m => m.Debe);
            decimal totalHaber = movimientos.Sum(m => m.Haber);

            if (Math.Abs(totalDebe - totalHaber) > 0.01m)
            {
                _logger.LogError("Asiento desbalanceado para movimiento bancario {MovimientoId}", movimiento.Id);
                return null;
            }

            asiento.TotalDebe = totalDebe;
            asiento.TotalHaber = totalHaber;
            asiento.Movimientos = movimientos;

            _context.AsientosContables.Add(asiento);
            _context.MovimientosContables.AddRange(movimientos);

            // Actualizar referencia en el movimiento bancario
            movimiento.AsientoContableId = asiento.Id;

            if (await _contabilidadUoW.ConfiguracionContableRepository.EstaHabilitadaAprobacionAutomaticaAsync(movimiento.EmpresaId))
            {
                asiento.Estado = "APR";
                asiento.FechaAprobacion = FechaCostaRicaHelper.Ahora;
                asiento.AprobadoPorId = userId;
            }

            periodo.UltimoNumeroAsiento = asiento.Numero;
            periodo.CantidadAsientos++;
            periodo.TotalDebe += totalDebe;
            periodo.TotalHaber += totalHaber;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Asiento contable {AsientoId} generado para movimiento bancario {MovimientoId}",
                asiento.Id, movimiento.Id);
            return asiento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando asiento para movimiento bancario {MovimientoId}", movimiento.Id);
            return null;
        }
    }

    public async Task<AsientoContable?> GenerarAsientoPlanillaAsync(Planilla planilla, string userId)
    {
        try
        {
            if (!await EstaHabilitadoAsync(planilla.EmpresaId))
                return null;

            if (await ExisteAsientoParaDocumentoAsync(planilla.EmpresaId, ModulosContables.Nomina, planilla.Id))
                return null;

            var cuentasIntegracion = await _contabilidadUoW.CuentaIntegracionRepository
                .GetByModuloYTipoOperacionAsync(planilla.EmpresaId, ModulosContables.Nomina, TiposOperacionContable.PagoPlanilla);

            if (!cuentasIntegracion.Any())
            {
                _logger.LogWarning("No hay cuentas de integración para pago planilla en empresa {EmpresaId}", planilla.EmpresaId);
                return null;
            }

            var periodo = await _contabilidadUoW.PeriodoContableRepository.GetAbiertoAsync(planilla.EmpresaId);
            if (periodo == null) return null;

            var asiento = await CrearAsientoBaseAsync(
                planilla.EmpresaId,
                periodo,
                $"Pago Planilla - {planilla.Codigo}",
                planilla.Codigo,
                ModulosContables.Nomina,
                planilla.Id,
                planilla.FechaPago ?? FechaCostaRicaHelper.Ahora,
                userId);

            var movimientos = new List<MovimientoContable>();
            int numeroLinea = 1;

            foreach (var cuentaInt in cuentasIntegracion.OrderBy(c => c.Orden))
            {
                decimal monto = CalcularMontoPlanilla(planilla, cuentaInt.ConceptoContable);
                if (monto == 0) continue;

                movimientos.Add(new MovimientoContable
                {
                    Id = Guid.NewGuid(),
                    AsientoContableId = asiento.Id,
                    CuentaContableId = cuentaInt.CuentaContableId,
                    NumeroLinea = numeroLinea++,
                    Descripcion = $"{cuentaInt.Descripcion ?? cuentaInt.ConceptoContable} - {planilla.PeriodoDescripcion}",
                    Debe = cuentaInt.TipoMovimiento == "D" ? monto : 0,
                    Haber = cuentaInt.TipoMovimiento == "H" ? monto : 0,
                    DocumentoReferencia = planilla.Codigo
                });
            }

            decimal totalDebe = movimientos.Sum(m => m.Debe);
            decimal totalHaber = movimientos.Sum(m => m.Haber);

            if (Math.Abs(totalDebe - totalHaber) > 0.01m)
            {
                _logger.LogError("Asiento desbalanceado para planilla {PlanillaId}", planilla.Id);
                return null;
            }

            asiento.TotalDebe = totalDebe;
            asiento.TotalHaber = totalHaber;
            asiento.Movimientos = movimientos;

            _context.AsientosContables.Add(asiento);
            _context.MovimientosContables.AddRange(movimientos);

            if (await _contabilidadUoW.ConfiguracionContableRepository.EstaHabilitadaAprobacionAutomaticaAsync(planilla.EmpresaId))
            {
                asiento.Estado = "APR";
                asiento.FechaAprobacion = FechaCostaRicaHelper.Ahora;
                asiento.AprobadoPorId = userId;
            }

            periodo.UltimoNumeroAsiento = asiento.Numero;
            periodo.CantidadAsientos++;
            periodo.TotalDebe += totalDebe;
            periodo.TotalHaber += totalHaber;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Asiento contable {AsientoId} generado para planilla {PlanillaId}", asiento.Id, planilla.Id);
            return asiento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando asiento para planilla {PlanillaId}", planilla.Id);
            return null;
        }
    }

    #region Métodos Auxiliares

    private async Task<AsientoContable> CrearAsientoBaseAsync(
        Guid empresaId,
        PeriodoContable periodo,
        string concepto,
        string? referencia,
        string moduloOrigen,
        Guid documentoOrigenId,
        DateTime fecha,
        string userId)
    {
        int nuevoNumero = periodo.UltimoNumeroAsiento + 1;

        return new AsientoContable
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Numero = nuevoNumero,
            Fecha = fecha,
            PeriodoContableId = periodo.Id,
            TipoAsiento = "DIA",
            Concepto = concepto,
            Referencia = referencia,
            ModuloOrigen = moduloOrigen,
            DocumentoOrigenId = documentoOrigenId,
            Estado = "BOR",
            FechaCreacion = FechaCostaRicaHelper.Ahora,
            CreadoPorId = userId
        };
    }

    private decimal CalcularMontoVenta(Documento documento, string conceptoContable)
    {
        return conceptoContable switch
        {
            ConceptosContables.CuentaVentas => documento.TotalGravado + documento.TotalExento,
            ConceptosContables.CuentaVentasExentas => documento.TotalExento,
            ConceptosContables.CuentaIvaDebito => documento.TotalImpuestos,
            ConceptosContables.CuentaClientes => documento.TotalVenta,
            ConceptosContables.CuentaCaja => documento.CondicionVenta == "01" ? documento.TotalVenta : 0,
            ConceptosContables.CuentaBancos => documento.CondicionVenta == "01" ? documento.TotalVenta : 0,
            ConceptosContables.CuentaDescuentos => documento.TotalDescuentos,
            _ => 0
        };
    }

    private decimal CalcularMontoCompra(Gasto gasto, string conceptoContable)
    {
        return conceptoContable switch
        {
            ConceptosContables.CuentaIvaCredito => gasto.MontoImpuesto,
            ConceptosContables.CuentaProveedores => gasto.MontoTotal,
            ConceptosContables.CuentaCaja => gasto.FormaPago == FormaPago.Efectivo ? gasto.MontoTotal : 0,
            ConceptosContables.CuentaBancos => gasto.FormaPago == FormaPago.Efectivo ? gasto.MontoTotal : 0,
            _ => gasto.MontoSubtotal // Para cuentas de gasto genéricas
        };
    }

    private decimal CalcularMontoPlanilla(Planilla planilla, string conceptoContable)
    {
        return conceptoContable switch
        {
            "CUENTA_SALARIOS" => planilla.TotalSalarioBruto,
            "CUENTA_SALARIOS_POR_PAGAR" => planilla.TotalSalarioNeto,
            "CUENTA_CCSS_PATRONAL" => planilla.TotalCCSSPatronal,
            "CUENTA_INS" => planilla.TotalINS,
            "CUENTA_CARGAS_SOCIALES" => planilla.TotalCargasSociales,
            ConceptosContables.CuentaBancos or "CUENTA_BANCOS" => planilla.TotalSalarioNeto,
            _ => 0
        };
    }

    #endregion
}
