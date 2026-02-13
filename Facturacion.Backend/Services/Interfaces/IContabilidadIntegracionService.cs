using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio de integración contable para generación automática de asientos.
/// Genera asientos contables cuando ocurren operaciones transaccionales
/// (ventas, gastos, cobros, pagos, movimientos bancarios, planilla).
/// </summary>
public interface IContabilidadIntegracionService
{
    /// <summary>
    /// Genera un asiento contable para una venta (documento aceptado por Hacienda).
    /// </summary>
    /// <param name="documento">Documento de venta (Factura, Tiquete, etc.)</param>
    /// <param name="userId">ID del usuario que realiza la operación</param>
    /// <returns>Asiento generado o null si no está habilitado</returns>
    Task<AsientoContable?> GenerarAsientoVentaAsync(Documento documento, string userId);

    /// <summary>
    /// Genera un asiento contable para una compra/gasto aprobado.
    /// </summary>
    /// <param name="gasto">Gasto registrado</param>
    /// <param name="userId">ID del usuario que realiza la operación</param>
    /// <returns>Asiento generado o null si no está habilitado</returns>
    Task<AsientoContable?> GenerarAsientoCompraAsync(Gasto gasto, string userId);

    /// <summary>
    /// Genera un asiento contable para el pago de un gasto.
    /// </summary>
    /// <param name="gasto">Gasto pagado</param>
    /// <param name="montoPago">Monto del pago realizado</param>
    /// <param name="userId">ID del usuario que realiza la operación</param>
    /// <returns>Asiento generado o null si no está habilitado</returns>
    Task<AsientoContable?> GenerarAsientoPagoGastoAsync(Gasto gasto, decimal montoPago, string userId);

    /// <summary>
    /// Genera un asiento contable para un cobro a cliente (abono a CxC).
    /// </summary>
    /// <param name="cuenta">Cuenta por cobrar</param>
    /// <param name="abono">Abono aplicado</param>
    /// <param name="userId">ID del usuario que realiza la operación</param>
    /// <returns>Asiento generado o null si no está habilitado</returns>
    Task<AsientoContable?> GenerarAsientoCobroAsync(CuentaPorCobrar cuenta, AbonoCobranza abono, string userId);

    /// <summary>
    /// Genera un asiento contable para un movimiento bancario.
    /// </summary>
    /// <param name="movimiento">Movimiento bancario</param>
    /// <param name="userId">ID del usuario que realiza la operación</param>
    /// <returns>Asiento generado o null si no está habilitado</returns>
    Task<AsientoContable?> GenerarAsientoMovimientoBancarioAsync(MovimientoBancario movimiento, string userId);

    /// <summary>
    /// Genera un asiento contable para el pago de una planilla.
    /// </summary>
    /// <param name="planilla">Planilla pagada</param>
    /// <param name="userId">ID del usuario que realiza la operación</param>
    /// <returns>Asiento generado o null si no está habilitado</returns>
    Task<AsientoContable?> GenerarAsientoPlanillaAsync(Planilla planilla, string userId);

    /// <summary>
    /// Verifica si la generación automática de asientos está habilitada para una empresa.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>True si está habilitada</returns>
    Task<bool> EstaHabilitadoAsync(Guid empresaId);

    /// <summary>
    /// Verifica si ya existe un asiento para un documento específico.
    /// Usado para garantizar idempotencia.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="moduloOrigen">Módulo origen (VEN, COM, CXC, CXP, BAN, NOM)</param>
    /// <param name="documentoOrigenId">ID del documento origen</param>
    /// <returns>True si ya existe un asiento</returns>
    Task<bool> ExisteAsientoParaDocumentoAsync(Guid empresaId, string moduloOrigen, Guid documentoOrigenId);
}
