using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IPuestoRepository
{
    Task<Puesto?> GetAsync(Guid id);
    Task<IEnumerable<Puesto>> GetByEmpresaAsync(Guid empresaId);
    Task<IEnumerable<Puesto>> GetActivosAsync(Guid empresaId);
    Task<Puesto?> GetByCodigoAsync(Guid empresaId, string codigo);
    Task<IEnumerable<Puesto>> GetByDepartamentoAsync(Guid departamentoId);
    Task<IEnumerable<Puesto>> GetByNivelJerarquicoAsync(Guid empresaId, int nivelJerarquico);
    Task<Puesto> AddAsync(Puesto puesto);
    Task UpdateAsync(Puesto puesto);
    Task DeleteAsync(Guid id, string userId);
}
