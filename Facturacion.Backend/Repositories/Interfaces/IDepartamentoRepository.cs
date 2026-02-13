using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IDepartamentoRepository
{
    Task<Departamento?> GetAsync(Guid id);
    Task<IEnumerable<Departamento>> GetByEmpresaAsync(Guid empresaId);
    Task<IEnumerable<Departamento>> GetActivosAsync(Guid empresaId);
    Task<Departamento?> GetByCodigoAsync(Guid empresaId, string codigo);
    Task<IEnumerable<Departamento>> GetByPadreAsync(Guid departamentoPadreId);
    Task<Departamento> AddAsync(Departamento departamento);
    Task UpdateAsync(Departamento departamento);
    Task DeleteAsync(Guid id, string userId);
}
