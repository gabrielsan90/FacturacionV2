using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IVacacionRepository
{
    Task<Vacacion?> GetAsync(Guid id);
    Task<IEnumerable<Vacacion>> GetByEmpleadoAsync(Guid empleadoId);
    Task<IEnumerable<Vacacion>> GetByEmpresaAsync(Guid empresaId);
    Task<IEnumerable<Vacacion>> GetPendientesAsync(Guid empresaId);
    Task<IEnumerable<Vacacion>> GetByPeriodoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<Vacacion>> GetByEstadoAsync(Guid empresaId, string estado);
    Task<IEnumerable<Vacacion>> GetActivasAsync(Guid empresaId);
    Task<Vacacion> AddAsync(Vacacion vacacion);
    Task UpdateAsync(Vacacion vacacion);
}
