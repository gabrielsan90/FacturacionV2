using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para operaciones CRUD de TipoWorkflow
/// </summary>
public interface ITipoWorkflowRepository
{
    /// <summary>
    /// Obtiene un tipo de workflow por su ID
    /// </summary>
    Task<ActionResponse<TipoWorkflow>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todos los tipos de workflow de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<TipoWorkflow>>> GetAllAsync(Guid empresaId);

    /// <summary>
    /// Obtiene solo los tipos de workflow activos de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<TipoWorkflow>>> GetActivosAsync(Guid empresaId);

    /// <summary>
    /// Obtiene un tipo de workflow por su código
    /// </summary>
    Task<ActionResponse<TipoWorkflow>> GetByCodigoAsync(Guid empresaId, string codigo);

    /// <summary>
    /// Crea un nuevo tipo de workflow
    /// </summary>
    Task<ActionResponse<TipoWorkflow>> AddAsync(TipoWorkflow tipoWorkflow);

    /// <summary>
    /// Actualiza un tipo de workflow existente
    /// </summary>
    Task<ActionResponse<TipoWorkflow>> UpdateAsync(TipoWorkflow tipoWorkflow);

    /// <summary>
    /// Elimina (soft delete) un tipo de workflow
    /// </summary>
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);

    /// <summary>
    /// Activa o desactiva un tipo de workflow
    /// </summary>
    Task<ActionResponse<TipoWorkflow>> ToggleActivoAsync(Guid id);
}
