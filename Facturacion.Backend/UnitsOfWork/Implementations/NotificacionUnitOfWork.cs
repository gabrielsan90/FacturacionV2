using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class NotificacionUnitOfWork : INotificacionUnitOfWork
{
    private readonly DataContext _context;
    private INotificacionRepository? _notificacionRepository;

    public NotificacionUnitOfWork(DataContext context)
    {
        _context = context;
    }

    private INotificacionRepository NotificacionRepository
    {
        get
        {
            _notificacionRepository ??= new NotificacionRepository(_context);
            return _notificacionRepository;
        }
    }

    public async Task<ActionResponse<IEnumerable<NotificacionDTO>>> GetByUsuarioAsync(string usuarioId, Guid empresaId)
    {
        try
        {
            var notificaciones = await NotificacionRepository.GetByUsuarioAsync(usuarioId, empresaId);
            var dtos = notificaciones.Select(n => ConvertirADTO(n)).ToList();

            return new ActionResponse<IEnumerable<NotificacionDTO>>
            {
                WasSuccess = true,
                Result = dtos
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<NotificacionDTO>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<NotificacionDTO>>> GetNoLeidasAsync(string usuarioId, Guid empresaId)
    {
        try
        {
            var notificaciones = await NotificacionRepository.GetNoLeidasByUsuarioAsync(usuarioId, empresaId);
            var dtos = notificaciones.Select(n => ConvertirADTO(n)).ToList();

            return new ActionResponse<IEnumerable<NotificacionDTO>>
            {
                WasSuccess = true,
                Result = dtos
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<NotificacionDTO>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<int>> GetCountNoLeidasAsync(string usuarioId, Guid empresaId)
    {
        try
        {
            var count = await NotificacionRepository.GetCountNoLeidasAsync(usuarioId, empresaId);

            return new ActionResponse<int>
            {
                WasSuccess = true,
                Result = count
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<int>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ResumenNotificacionesDTO>> GetResumenAsync(string usuarioId, Guid empresaId)
    {
        try
        {
            var resumen = await NotificacionRepository.GetResumenAsync(usuarioId, empresaId);

            return new ActionResponse<ResumenNotificacionesDTO>
            {
                WasSuccess = true,
                Result = resumen
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ResumenNotificacionesDTO>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<NotificacionDTO>> GetByIdAsync(Guid id)
    {
        try
        {
            var notificacion = await NotificacionRepository.GetAsync(id);

            if (notificacion == null)
            {
                return new ActionResponse<NotificacionDTO>
                {
                    WasSuccess = false,
                    Message = "Notificación no encontrada."
                };
            }

            return new ActionResponse<NotificacionDTO>
            {
                WasSuccess = true,
                Result = ConvertirADTO(notificacion)
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<NotificacionDTO>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<NotificacionDTO>> CreateAsync(CrearNotificacionDTO dto)
    {
        try
        {
            var notificacion = new Notificacion
            {
                EmpresaId = dto.EmpresaId,
                UsuarioId = dto.UsuarioId,
                TipoNotificacion = dto.TipoNotificacion,
                Titulo = dto.Titulo,
                Mensaje = dto.Mensaje,
                Icono = dto.Icono,
                Color = dto.Color,
                EntidadRelacionadaId = dto.EntidadRelacionadaId,
                TipoEntidad = dto.TipoEntidad,
                UrlAccion = dto.UrlAccion,
                Importante = dto.Importante,
                FechaExpiracion = dto.FechaExpiracion
            };

            var notificacionCreada = await NotificacionRepository.AddAsync(notificacion);

            return new ActionResponse<NotificacionDTO>
            {
                WasSuccess = true,
                Result = ConvertirADTO(notificacionCreada),
                Message = "Notificación creada exitosamente."
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<NotificacionDTO>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> MarcarComoLeidaAsync(Guid id)
    {
        try
        {
            await NotificacionRepository.MarcarComoLeidaAsync(id);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Notificación marcada como leída."
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> MarcarTodasComoLeidasAsync(string usuarioId, Guid empresaId)
    {
        try
        {
            await NotificacionRepository.MarcarTodasComoLeidasAsync(usuarioId, empresaId);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Todas las notificaciones han sido marcadas como leídas."
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            await NotificacionRepository.DeleteAsync(id);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Notificación eliminada exitosamente."
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> DeleteExpiradasAsync(Guid empresaId)
    {
        try
        {
            await NotificacionRepository.DeleteExpiradasAsync(empresaId);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Notificaciones expiradas eliminadas exitosamente."
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    // Método auxiliar
    private static NotificacionDTO ConvertirADTO(Notificacion notificacion)
    {
        var ahora = FechaCostaRicaHelper.Ahora;
        var expirada = notificacion.FechaExpiracion.HasValue && notificacion.FechaExpiracion.Value <= ahora;

        return new NotificacionDTO
        {
            Id = notificacion.Id,
            EmpresaId = notificacion.EmpresaId,
            UsuarioId = notificacion.UsuarioId,
            TipoNotificacion = notificacion.TipoNotificacion,
            Titulo = notificacion.Titulo,
            Mensaje = notificacion.Mensaje,
            Icono = notificacion.Icono,
            Color = notificacion.Color,
            Leida = notificacion.Leida,
            FechaLeida = notificacion.FechaLeida,
            EntidadRelacionadaId = notificacion.EntidadRelacionadaId,
            TipoEntidad = notificacion.TipoEntidad,
            UrlAccion = notificacion.UrlAccion,
            Importante = notificacion.Importante,
            FechaCreacion = notificacion.FechaCreacion,
            FechaExpiracion = notificacion.FechaExpiracion,
            TiempoTranscurrido = CalcularTiempoTranscurrido(notificacion.FechaCreacion),
            Expirada = expirada
        };
    }

    private static string CalcularTiempoTranscurrido(DateTime fechaCreacion)
    {
        var diferencia = FechaCostaRicaHelper.Ahora - fechaCreacion;

        if (diferencia.TotalMinutes < 1)
            return "Hace un momento";

        if (diferencia.TotalMinutes < 60)
            return $"Hace {(int)diferencia.TotalMinutes} {((int)diferencia.TotalMinutes == 1 ? "minuto" : "minutos")}";

        if (diferencia.TotalHours < 24)
            return $"Hace {(int)diferencia.TotalHours} {((int)diferencia.TotalHours == 1 ? "hora" : "horas")}";

        if (diferencia.TotalDays < 7)
            return $"Hace {(int)diferencia.TotalDays} {((int)diferencia.TotalDays == 1 ? "día" : "días")}";

        if (diferencia.TotalDays < 30)
            return $"Hace {(int)(diferencia.TotalDays / 7)} {((int)(diferencia.TotalDays / 7) == 1 ? "semana" : "semanas")}";

        if (diferencia.TotalDays < 365)
            return $"Hace {(int)(diferencia.TotalDays / 30)} {((int)(diferencia.TotalDays / 30) == 1 ? "mes" : "meses")}";

        return $"Hace {(int)(diferencia.TotalDays / 365)} {((int)(diferencia.TotalDays / 365) == 1 ? "año" : "años")}";
    }
}
