using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface ICategoriaActivoRepository
{
    Task<ActionResponse<CategoriaActivo>> GetAsync(Guid id);
    Task<ActionResponse<IEnumerable<CategoriaActivo>>> GetByEmpresaAsync(Guid empresaId);
    Task<ActionResponse<IEnumerable<CategoriaActivo>>> GetActivasAsync(Guid empresaId);
    Task<ActionResponse<CategoriaActivo>> GetByCodigoAsync(Guid empresaId, string codigo);
    Task<ActionResponse<CategoriaActivo>> AddAsync(CategoriaActivo categoria);
    Task<ActionResponse<CategoriaActivo>> UpdateAsync(CategoriaActivo categoria);
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);
}
