using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface ICentroCostoRepository
{
    Task<CentroCosto?> GetAsync(Guid id);
    Task<IEnumerable<CentroCosto>> GetByEmpresaAsync(Guid empresaId);
    Task<CentroCosto> AddAsync(CentroCosto centroCosto);
    Task UpdateAsync(CentroCosto centroCosto);
    Task DeleteAsync(Guid id, string userId);
}
