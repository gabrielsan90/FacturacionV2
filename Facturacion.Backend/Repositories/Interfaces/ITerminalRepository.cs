using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface ITerminalRepository
{
    Task<Terminal?> GetAsync(Guid id);
    Task<IEnumerable<Terminal>> GetBySucursalAsync(Guid sucursalId);
    Task<Terminal?> GetByCodigoAsync(Guid sucursalId, string codigo);
    Task<Terminal> AddAsync(Terminal terminal);
    Task UpdateAsync(Terminal terminal);
    Task DeleteAsync(Guid id, string userId);
}
