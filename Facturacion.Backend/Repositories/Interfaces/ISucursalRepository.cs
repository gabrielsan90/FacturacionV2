using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface ISucursalRepository
{
    Task<Sucursal?> GetAsync(Guid id);
    Task<IEnumerable<Sucursal>> GetByEmpresaAsync(Guid empresaId);
    Task<Sucursal?> GetByCodigoAsync(Guid empresaId, string codigo);
    Task<Sucursal?> GetPrincipalAsync(Guid empresaId);
    Task<Sucursal> AddAsync(Sucursal sucursal);
    Task UpdateAsync(Sucursal sucursal);
    Task DeleteAsync(Guid id, string userId);
}
