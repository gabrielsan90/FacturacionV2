using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IEmpleadoRepository
{
    Task<Empleado?> GetAsync(Guid id);
    Task<IEnumerable<Empleado>> GetByEmpresaAsync(Guid empresaId);
    Task<IEnumerable<Empleado>> GetByDepartamentoAsync(Guid departamentoId);
    Task<IEnumerable<Empleado>> GetActivosAsync(Guid empresaId);
    Task<Empleado?> GetByIdentificacionAsync(Guid empresaId, string identificacion);
    Task<Empleado?> GetByCodigoAsync(Guid empresaId, string codigo);
    Task<IEnumerable<Empleado>> GetByEstadoAsync(Guid empresaId, string estado);
    Task<IEnumerable<Empleado>> GetSubordinadosAsync(Guid jefeId);
    Task<Empleado> AddAsync(Empleado empleado);
    Task UpdateAsync(Empleado empleado);
    Task DeleteAsync(Guid id, string userId);
}
