using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para operaciones CRUD de ExtractoBancario
/// </summary>
public interface IExtractoBancarioRepository
{
    /// <summary>
    /// Obtiene un extracto bancario por su ID con sus líneas
    /// </summary>
    Task<ActionResponse<ExtractoBancario>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todos los extractos de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<ExtractoBancario>>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene extractos de una cuenta bancaria específica
    /// </summary>
    Task<ActionResponse<IEnumerable<ExtractoBancario>>> GetByCuentaBancariaAsync(Guid cuentaBancariaId);

    /// <summary>
    /// Obtiene extractos pendientes de conciliar
    /// </summary>
    Task<ActionResponse<IEnumerable<ExtractoBancario>>> GetPendientesAsync(Guid empresaId);

    /// <summary>
    /// Obtiene extractos por rango de fechas
    /// </summary>
    Task<ActionResponse<IEnumerable<ExtractoBancario>>> GetByFechaRangeAsync(Guid cuentaBancariaId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Crea un nuevo extracto bancario
    /// </summary>
    Task<ActionResponse<ExtractoBancario>> AddAsync(ExtractoBancario extracto);

    /// <summary>
    /// Actualiza un extracto bancario existente
    /// </summary>
    Task<ActionResponse<ExtractoBancario>> UpdateAsync(ExtractoBancario extracto);

    /// <summary>
    /// Marca un extracto como conciliado
    /// </summary>
    Task<ActionResponse<ExtractoBancario>> MarcarComoConciliadoAsync(Guid id, Guid conciliacionId);

    /// <summary>
    /// Elimina un extracto bancario (soft delete mediante cambio de estado)
    /// </summary>
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);
}
