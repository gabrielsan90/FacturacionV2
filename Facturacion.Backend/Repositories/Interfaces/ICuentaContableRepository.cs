using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface ICuentaContableRepository
{
    Task<CuentaContable?> GetAsync(Guid id);
    Task<IEnumerable<CuentaContable>> GetByEmpresaAsync(Guid empresaId);
    Task<CuentaContable?> GetByCodigoAsync(Guid empresaId, string codigo);
    Task<IEnumerable<CuentaContable>> GetArbolAsync(Guid empresaId);
    Task<CuentaContable> AddAsync(CuentaContable cuenta);
    Task UpdateAsync(CuentaContable cuenta);
    Task DeleteAsync(Guid id, string userId);
    Task ToggleActivaAsync(Guid id);
}
