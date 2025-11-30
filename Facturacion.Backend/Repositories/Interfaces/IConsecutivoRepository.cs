using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IConsecutivoRepository
{
    Task<Consecutivo?> GetAsync(Guid id);
    Task<IEnumerable<Consecutivo>> GetByTerminalAsync(Guid terminalId);
    Task<IEnumerable<Consecutivo>> GetByEmpresaAsync(Guid empresaId);
    Task<Consecutivo?> GetByClaveAsync(string claveNumeracion);
    Task<Consecutivo?> GetByTipoDocumentoAsync(Guid terminalId, string tipoDocumento);
    Task<Consecutivo> AddAsync(Consecutivo consecutivo);
    Task UpdateAsync(Consecutivo consecutivo);
    Task DeleteAsync(Guid id, string userId);
    Task<long> IncrementAsync(Guid id);
}
