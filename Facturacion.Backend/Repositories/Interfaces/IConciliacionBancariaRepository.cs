using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para operaciones CRUD de ConciliacionBancaria
/// </summary>
public interface IConciliacionBancariaRepository
{
    /// <summary>
    /// Obtiene una conciliación bancaria por su ID
    /// </summary>
    Task<ActionResponse<ConciliacionBancaria>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todas las conciliaciones de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<ConciliacionBancaria>>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene conciliaciones de una cuenta bancaria específica
    /// </summary>
    Task<ActionResponse<IEnumerable<ConciliacionBancaria>>> GetByCuentaBancariaAsync(Guid cuentaBancariaId);

    /// <summary>
    /// Obtiene una conciliación por período (mes/año)
    /// </summary>
    Task<ActionResponse<ConciliacionBancaria>> GetByPeriodoAsync(Guid cuentaBancariaId, int mes, int anio);

    /// <summary>
    /// Obtiene conciliaciones pendientes de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<ConciliacionBancaria>>> GetPendientesAsync(Guid empresaId);

    /// <summary>
    /// Crea una nueva conciliación bancaria
    /// </summary>
    Task<ActionResponse<ConciliacionBancaria>> AddAsync(ConciliacionBancaria conciliacion);

    /// <summary>
    /// Actualiza una conciliación bancaria existente
    /// </summary>
    Task<ActionResponse<ConciliacionBancaria>> UpdateAsync(ConciliacionBancaria conciliacion);

    /// <summary>
    /// Marca una conciliación como cerrada
    /// </summary>
    Task<ActionResponse<ConciliacionBancaria>> CerrarConciliacionAsync(Guid id, string usuarioId);

    /// <summary>
    /// Elimina una conciliación bancaria (soft delete mediante cambio de estado)
    /// </summary>
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);
}
