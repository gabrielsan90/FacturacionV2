using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IEmpresaRepository
{
    Task<ActionResponse<Empresa>> GetAsync(Guid id);
    Task<ActionResponse<IEnumerable<Empresa>>> GetAsync();
    Task<ActionResponse<IEnumerable<Empresa>>> GetAllActiveAsync();
    Task<ActionResponse<Empresa>> GetByIdentificationAsync(string numeroIdentificacion);
    Task<ActionResponse<Empresa>> AddAsync(Empresa empresa);
    Task<ActionResponse<Empresa>> UpdateAsync(Empresa empresa);
    Task<ActionResponse<Empresa>> DeleteAsync(Guid id);
}
