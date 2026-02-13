using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IPlanillaRepository
{
    Task<Planilla?> GetAsync(Guid id);
    Task<IEnumerable<Planilla>> GetByEmpresaAsync(Guid empresaId);
    Task<IEnumerable<Planilla>> GetByPeriodoAsync(Guid empresaId, int anio, int mes);
    Task<Planilla?> GetByPeriodoEspecificoAsync(Guid empresaId, int anio, int mes, int periodo);
    Task<IEnumerable<Planilla>> GetByEstadoAsync(Guid empresaId, string estado);
    Task<IEnumerable<Planilla>> GetByTipoAsync(Guid empresaId, string tipoPlanilla);
    Task<IEnumerable<Planilla>> GetByAnioAsync(Guid empresaId, int anio);
    Task<Planilla> AddAsync(Planilla planilla);
    Task UpdateAsync(Planilla planilla);
    Task DeleteAsync(Guid id, string userId);
}
