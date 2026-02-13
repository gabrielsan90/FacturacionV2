using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para gestión de Cuentas por Pagar
/// </summary>
public interface ICuentaPorPagarRepository
{
    /// <summary>
    /// Obtiene una cuenta por pagar por su ID con sus abonos
    /// </summary>
    Task<ActionResponse<CuentaPorPagar>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todas las cuentas por pagar de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaPorPagar>>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene todas las cuentas por pagar de un proveedor
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaPorPagar>>> GetByProveedorAsync(Guid proveedorId);

    /// <summary>
    /// Obtiene cuentas por pagar pendientes o parciales de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaPorPagar>>> GetPendientesAsync(Guid empresaId);

    /// <summary>
    /// Obtiene cuentas por pagar vencidas de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaPorPagar>>> GetVencidasAsync(Guid empresaId);

    /// <summary>
    /// Obtiene cuentas por pagar por rango de fecha de vencimiento
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaPorPagar>>> GetByFechaVencimientoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Obtiene el saldo pendiente total de un proveedor
    /// </summary>
    Task<ActionResponse<decimal>> GetSaldoPendienteProveedorAsync(Guid proveedorId);

    /// <summary>
    /// Crea una nueva cuenta por pagar
    /// </summary>
    Task<ActionResponse<CuentaPorPagar>> AddAsync(CuentaPorPagar cuenta);

    /// <summary>
    /// Actualiza una cuenta por pagar
    /// </summary>
    Task<ActionResponse<CuentaPorPagar>> UpdateAsync(CuentaPorPagar cuenta);

    /// <summary>
    /// Aplica un pago a una cuenta por pagar
    /// </summary>
    Task<ActionResponse<bool>> AplicarPagoAsync(Guid cuentaId, decimal monto, string usuarioId);

    /// <summary>
    /// Anula una cuenta por pagar (soft delete con motivo)
    /// </summary>
    Task<ActionResponse<bool>> AnularAsync(Guid id, string usuarioId, string motivo);
}
