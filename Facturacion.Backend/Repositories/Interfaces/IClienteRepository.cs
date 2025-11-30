using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> GetAsync(Guid id);
    Task<IEnumerable<Cliente>> GetByEmpresaAsync(Guid empresaId);
    Task<Cliente?> GetByIdentificationAsync(Guid empresaId, string numeroIdentificacion);
    Task<Cliente> AddAsync(Cliente cliente);
    Task UpdateAsync(Cliente cliente);
    Task DeleteAsync(Guid id, string userId);
}
