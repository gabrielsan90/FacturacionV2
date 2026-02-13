using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IIncapacidadRepository
{
    Task<Incapacidad?> GetAsync(Guid id);
    Task<IEnumerable<Incapacidad>> GetByEmpleadoAsync(Guid empleadoId);
    Task<IEnumerable<Incapacidad>> GetByEmpresaAsync(Guid empresaId);
    Task<IEnumerable<Incapacidad>> GetByTipoAsync(Guid empresaId, string tipo);
    Task<IEnumerable<Incapacidad>> GetByPeriodoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<Incapacidad>> GetActivasAsync(Guid empresaId);
    Task<Incapacidad> AddAsync(Incapacidad incapacidad);
    Task UpdateAsync(Incapacidad incapacidad);
}
