using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IRequisicionRepository
{
    Task<ActionResponse<Requisicion>> GetAsync(Guid id);
    Task<ActionResponse<Requisicion>> GetWithDetallesAsync(Guid id);
    Task<ActionResponse<IEnumerable<Requisicion>>> GetByEmpresaAsync(Guid empresaId);
    Task<ActionResponse<IEnumerable<Requisicion>>> GetBySolicitanteAsync(string solicitanteId);
    Task<ActionResponse<IEnumerable<Requisicion>>> GetByEstadoAsync(Guid empresaId, string estado);
    Task<ActionResponse<IEnumerable<Requisicion>>> GetPendientesAprobacionAsync(Guid empresaId);
    Task<ActionResponse<IEnumerable<Requisicion>>> GetAprobadasSinOCAsync(Guid empresaId);
    Task<ActionResponse<Requisicion>> AddAsync(Requisicion requisicion);
    Task<ActionResponse<Requisicion>> UpdateAsync(Requisicion requisicion);
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);
    Task<ActionResponse<Requisicion>> AprobarAsync(Guid id, string usuarioId);
    Task<ActionResponse<Requisicion>> RechazarAsync(Guid id, string usuarioId, string motivo);
    Task<ActionResponse<Requisicion>> AnularAsync(Guid id, string usuarioId, string motivo);
    Task<ActionResponse<string>> GenerarNumeroAsync(Guid empresaId);
}
