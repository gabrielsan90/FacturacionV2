using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para operaciones CRUD de ReglaConciliacion
/// </summary>
public interface IReglaConciliacionRepository
{
    /// <summary>
    /// Obtiene una regla de conciliación por su ID
    /// </summary>
    Task<ActionResponse<ReglaConciliacion>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todas las reglas de conciliación de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<ReglaConciliacion>>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene solo las reglas activas de una empresa, ordenadas por prioridad
    /// </summary>
    Task<ActionResponse<IEnumerable<ReglaConciliacion>>> GetActivasAsync(Guid empresaId);

    /// <summary>
    /// Obtiene las reglas de conciliación de una cuenta bancaria específica
    /// </summary>
    Task<ActionResponse<IEnumerable<ReglaConciliacion>>> GetByCuentaBancariaAsync(Guid cuentaBancariaId);

    /// <summary>
    /// Crea una nueva regla de conciliación
    /// </summary>
    Task<ActionResponse<ReglaConciliacion>> AddAsync(ReglaConciliacion regla);

    /// <summary>
    /// Actualiza una regla de conciliación existente
    /// </summary>
    Task<ActionResponse<ReglaConciliacion>> UpdateAsync(ReglaConciliacion regla);

    /// <summary>
    /// Elimina una regla de conciliación (eliminación física)
    /// </summary>
    Task<ActionResponse<bool>> DeleteAsync(Guid id);

    /// <summary>
    /// Activa o desactiva una regla de conciliación
    /// </summary>
    Task<ActionResponse<ReglaConciliacion>> ToggleActivaAsync(Guid id);
}
