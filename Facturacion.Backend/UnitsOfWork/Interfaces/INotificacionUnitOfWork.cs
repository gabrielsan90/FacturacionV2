using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface INotificacionUnitOfWork
{
    Task<ActionResponse<IEnumerable<NotificacionDTO>>> GetByUsuarioAsync(string usuarioId, Guid empresaId);
    Task<ActionResponse<IEnumerable<NotificacionDTO>>> GetNoLeidasAsync(string usuarioId, Guid empresaId);
    Task<ActionResponse<int>> GetCountNoLeidasAsync(string usuarioId, Guid empresaId);
    Task<ActionResponse<ResumenNotificacionesDTO>> GetResumenAsync(string usuarioId, Guid empresaId);
    Task<ActionResponse<NotificacionDTO>> GetByIdAsync(Guid id);
    Task<ActionResponse<NotificacionDTO>> CreateAsync(CrearNotificacionDTO dto);
    Task<ActionResponse<bool>> MarcarComoLeidaAsync(Guid id);
    Task<ActionResponse<bool>> MarcarTodasComoLeidasAsync(string usuarioId, Guid empresaId);
    Task<ActionResponse<bool>> DeleteAsync(Guid id);
    Task<ActionResponse<bool>> DeleteExpiradasAsync(Guid empresaId);
}
