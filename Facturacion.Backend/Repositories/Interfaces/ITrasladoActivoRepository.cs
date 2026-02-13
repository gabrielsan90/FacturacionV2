using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface ITrasladoActivoRepository
{
    Task<ActionResponse<TrasladoActivo>> GetAsync(Guid id);
    Task<ActionResponse<IEnumerable<TrasladoActivo>>> GetByActivoAsync(Guid activoFijoId);
    Task<ActionResponse<IEnumerable<TrasladoActivo>>> GetByEmpresaAsync(Guid empresaId);
    Task<ActionResponse<IEnumerable<TrasladoActivo>>> GetPendientesAsync(Guid empresaId);
    Task<ActionResponse<TrasladoActivo>> AddAsync(TrasladoActivo traslado);
    Task<ActionResponse<TrasladoActivo>> UpdateAsync(TrasladoActivo traslado);
    Task<ActionResponse<bool>> AprobarTrasladoAsync(Guid id, string usuarioId);
    Task<ActionResponse<bool>> RecibirTrasladoAsync(Guid id, string usuarioId);
    Task<ActionResponse<bool>> CancelarTrasladoAsync(Guid id);
}
