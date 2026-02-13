using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IAsientoContableRepository
{
    Task<AsientoContable?> GetAsync(Guid id);
    Task<AsientoContable?> GetWithMovimientosAsync(Guid id);
    Task<IEnumerable<AsientoContable>> GetByEmpresaAsync(Guid empresaId);
    Task<IEnumerable<AsientoContable>> GetByPeriodoAsync(Guid periodoContableId);
    Task<AsientoContable> AddAsync(AsientoContable asiento);
    Task UpdateAsync(AsientoContable asiento);
    Task DeleteAsync(Guid id, string userId);
    Task AprobarAsync(Guid id, string userId);
    Task AnularAsync(Guid id, string userId);
}
