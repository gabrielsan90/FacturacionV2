using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IActivoFijoRepository
{
    Task<ActionResponse<ActivoFijo>> GetAsync(Guid id);
    Task<ActionResponse<ActivoFijo>> GetWithDepreciacionesAsync(Guid id);
    Task<ActionResponse<IEnumerable<ActivoFijo>>> GetByEmpresaAsync(Guid empresaId);
    Task<ActionResponse<IEnumerable<ActivoFijo>>> GetByCategoriaAsync(Guid categoriaActivoId);
    Task<ActionResponse<IEnumerable<ActivoFijo>>> GetByUbicacionAsync(Guid empresaId, Guid? sucursalId, Guid? departamentoId);
    Task<ActionResponse<IEnumerable<ActivoFijo>>> GetActivosAsync(Guid empresaId);
    Task<ActionResponse<ActivoFijo>> GetByCodigoAsync(Guid empresaId, string codigo);
    Task<ActionResponse<ActivoFijo>> AddAsync(ActivoFijo activo);
    Task<ActionResponse<ActivoFijo>> UpdateAsync(ActivoFijo activo);
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);
    Task<ActionResponse<bool>> DarDeBajaAsync(Guid id, string usuarioId, string motivo, DateTime fechaBaja);
}
