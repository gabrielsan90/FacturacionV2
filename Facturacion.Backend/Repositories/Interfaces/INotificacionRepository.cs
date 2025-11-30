using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface INotificacionRepository
{
    Task<Notificacion?> GetAsync(Guid id);
    Task<IEnumerable<Notificacion>> GetByUsuarioAsync(string usuarioId, Guid empresaId);
    Task<IEnumerable<Notificacion>> GetNoLeidasByUsuarioAsync(string usuarioId, Guid empresaId);
    Task<IEnumerable<Notificacion>> GetByTipoAsync(string usuarioId, Guid empresaId, TipoNotificacion tipo);
    Task<int> GetCountNoLeidasAsync(string usuarioId, Guid empresaId);
    Task<ResumenNotificacionesDTO> GetResumenAsync(string usuarioId, Guid empresaId);
    Task<Notificacion> AddAsync(Notificacion notificacion);
    Task MarcarComoLeidaAsync(Guid id);
    Task MarcarTodasComoLeidasAsync(string usuarioId, Guid empresaId);
    Task DeleteAsync(Guid id);
    Task DeleteExpiradasAsync(Guid empresaId);
}
