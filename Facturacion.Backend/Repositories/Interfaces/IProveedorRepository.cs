using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IProveedorRepository
{
    Task<Proveedor?> GetAsync(Guid id);
    Task<IEnumerable<Proveedor>> GetByEmpresaAsync(Guid empresaId);
    Task<Proveedor?> GetByIdentificationAsync(Guid empresaId, string numeroIdentificacion);
    Task<Proveedor> AddAsync(Proveedor proveedor);
    Task UpdateAsync(Proveedor proveedor);
    Task DeleteAsync(Guid id, string userId);
}
