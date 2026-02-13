using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para gestión de Cuentas por Cobrar
/// </summary>
public interface ICuentaPorCobrarRepository
{
    /// <summary>
    /// Obtiene una cuenta por cobrar por su ID con sus abonos
    /// </summary>
    Task<ActionResponse<CuentaPorCobrar>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todas las cuentas por cobrar de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaPorCobrar>>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene todas las cuentas por cobrar de un cliente
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaPorCobrar>>> GetByClienteAsync(Guid clienteId);

    /// <summary>
    /// Obtiene cuentas por cobrar pendientes o parciales de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaPorCobrar>>> GetPendientesAsync(Guid empresaId);

    /// <summary>
    /// Obtiene cuentas por cobrar vencidas de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaPorCobrar>>> GetVencidasAsync(Guid empresaId);

    /// <summary>
    /// Obtiene cuentas por cobrar por rango de fecha de vencimiento
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaPorCobrar>>> GetByFechaVencimientoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Obtiene una cuenta por cobrar por DocumentoId
    /// </summary>
    Task<ActionResponse<CuentaPorCobrar>> GetByDocumentoIdAsync(Guid documentoId);

    /// <summary>
    /// Obtiene el saldo pendiente total de un cliente
    /// </summary>
    Task<ActionResponse<decimal>> GetSaldoPendienteClienteAsync(Guid clienteId);

    /// <summary>
    /// Obtiene el reporte de antigüedad de saldos de una empresa
    /// </summary>
    Task<ActionResponse<object>> GetAntiguedadSaldosAsync(Guid empresaId);

    /// <summary>
    /// Crea una nueva cuenta por cobrar
    /// </summary>
    Task<ActionResponse<CuentaPorCobrar>> AddAsync(CuentaPorCobrar cuentaPorCobrar);

    /// <summary>
    /// Actualiza una cuenta por cobrar
    /// </summary>
    Task<ActionResponse<CuentaPorCobrar>> UpdateAsync(CuentaPorCobrar cuentaPorCobrar);

    /// <summary>
    /// Aplica un abono a una cuenta por cobrar
    /// </summary>
    Task<ActionResponse<CuentaPorCobrar>> AplicarAbonoAsync(Guid cuentaPorCobrarId, AbonoCobranza abono);

    /// <summary>
    /// Aplica un cobro a una cuenta por cobrar
    /// </summary>
    Task<ActionResponse<bool>> AplicarCobroAsync(Guid cuentaId, decimal monto, string usuarioId);

    /// <summary>
    /// Elimina una cuenta por cobrar (soft delete)
    /// </summary>
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string userId);

    /// <summary>
    /// Anula una cuenta por cobrar (soft delete con motivo)
    /// </summary>
    Task<ActionResponse<bool>> AnularAsync(Guid id, string usuarioId, string motivo);
}

