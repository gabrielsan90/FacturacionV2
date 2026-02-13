using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para operaciones CRUD de MovimientoBancario
/// </summary>
public interface IMovimientoBancarioRepository
{
    /// <summary>
    /// Obtiene un movimiento bancario por su ID
    /// </summary>
    Task<ActionResponse<MovimientoBancario>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todos los movimientos bancarios de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<MovimientoBancario>>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene movimientos bancarios de una cuenta específica
    /// </summary>
    Task<ActionResponse<IEnumerable<MovimientoBancario>>> GetByCuentaBancariaAsync(Guid cuentaBancariaId);

    /// <summary>
    /// Obtiene movimientos bancarios en un rango de fechas
    /// </summary>
    Task<ActionResponse<IEnumerable<MovimientoBancario>>> GetByFechaRangeAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Obtiene movimientos bancarios pendientes de conciliar de una cuenta
    /// </summary>
    Task<ActionResponse<IEnumerable<MovimientoBancario>>> GetPendientesConciliarAsync(Guid cuentaBancariaId);

    /// <summary>
    /// Obtiene movimientos bancarios ya conciliados de una cuenta
    /// </summary>
    Task<ActionResponse<IEnumerable<MovimientoBancario>>> GetConciliadosAsync(Guid cuentaBancariaId);

    /// <summary>
    /// Crea un nuevo movimiento bancario
    /// </summary>
    Task<ActionResponse<MovimientoBancario>> AddAsync(MovimientoBancario movimiento);

    /// <summary>
    /// Actualiza un movimiento bancario existente
    /// </summary>
    Task<ActionResponse<MovimientoBancario>> UpdateAsync(MovimientoBancario movimiento);

    /// <summary>
    /// Elimina un movimiento bancario (cambia estado a anulado)
    /// </summary>
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);

    /// <summary>
    /// Calcula el saldo actual de una cuenta bancaria
    /// </summary>
    Task<ActionResponse<decimal>> GetSaldoCuentaAsync(Guid cuentaBancariaId);

    /// <summary>
    /// Marca un movimiento como conciliado
    /// </summary>
    Task<ActionResponse<MovimientoBancario>> MarcarComoConciliadoAsync(Guid id, Guid conciliacionId);
}
