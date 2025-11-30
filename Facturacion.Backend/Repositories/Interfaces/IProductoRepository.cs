using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IProductoRepository
{
    Task<Producto?> GetAsync(Guid id);
    Task<IEnumerable<Producto>> GetByEmpresaAsync(Guid empresaId);
    Task<Producto?> GetByCodigoAsync(Guid empresaId, string codigo);
    Task<IEnumerable<Producto>> GetByCategoriaAsync(Guid categoriaId);
    Task<Producto> AddAsync(Producto producto);
    Task UpdateAsync(Producto producto);
    Task DeleteAsync(Guid id, string userId);
}
