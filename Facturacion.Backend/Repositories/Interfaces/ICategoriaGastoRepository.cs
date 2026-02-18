using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface ICategoriaGastoRepository
{
    Task<CategoriaGasto?> GetAsync(int id);
    Task<IEnumerable<CategoriaGasto>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<CategoriaGasto>> GetByEmpresaAsync(Guid empresaId, bool includeInactive = false);
    Task<IEnumerable<CategoriaGasto>> GetActivasAsync();
    Task<IEnumerable<CategoriaGasto>> GetActivasByEmpresaAsync(Guid empresaId);
    Task<CategoriaGasto?> GetByNombreAsync(string nombre);
    Task<CategoriaGasto?> GetByNombreYEmpresaAsync(string nombre, Guid empresaId);
    Task<CategoriaGasto> AddAsync(CategoriaGasto categoria);
    Task UpdateAsync(CategoriaGasto categoria);
    Task DeleteAsync(int id);
    Task<int> CountByEmpresaAsync(Guid empresaId);
}
