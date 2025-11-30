using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface ICategoriaGastoRepository
{
    Task<CategoriaGasto?> GetAsync(int id);
    Task<IEnumerable<CategoriaGasto>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<CategoriaGasto>> GetActivasAsync();
    Task<CategoriaGasto?> GetByNombreAsync(string nombre);
    Task<CategoriaGasto> AddAsync(CategoriaGasto categoria);
    Task UpdateAsync(CategoriaGasto categoria);
    Task DeleteAsync(int id);
}
