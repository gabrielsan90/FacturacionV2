using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IPeriodoContableRepository
{
    Task<PeriodoContable?> GetAsync(Guid id);
    Task<IEnumerable<PeriodoContable>> GetByEmpresaAsync(Guid empresaId);
    Task<PeriodoContable?> GetAbiertoAsync(Guid empresaId);
    Task<PeriodoContable> AddAsync(PeriodoContable periodo);
    Task UpdateAsync(PeriodoContable periodo);
    Task DeleteAsync(Guid id, string userId);
    Task CerrarPeriodoAsync(Guid id, string userId);
}
