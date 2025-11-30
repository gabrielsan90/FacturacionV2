using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface ICategoriaRepository
{
    Task<Categoria?> GetAsync(Guid id);
    Task<IEnumerable<Categoria>> GetByEmpresaAsync(Guid empresaId);
    Task<Categoria?> GetByNombreAsync(Guid empresaId, string nombre);
    Task<Categoria> AddAsync(Categoria categoria);
    Task UpdateAsync(Categoria categoria);
    Task DeleteAsync(Guid id, string userId);
}
