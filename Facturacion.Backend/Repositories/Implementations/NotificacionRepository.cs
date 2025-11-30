using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class NotificacionRepository : INotificacionRepository
{
    private readonly DataContext _context;

    public NotificacionRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Notificacion?> GetAsync(Guid id)
    {
        return await _context.Notificaciones
            .Include(n => n.Empresa)
            .Include(n => n.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Notificacion>> GetByUsuarioAsync(string usuarioId, Guid empresaId)
    {
        var ahora = DateTime.UtcNow;

        return await _context.Notificaciones
            .Include(n => n.Empresa)
            .Include(n => n.Usuario)
            .Where(n => n.UsuarioId == usuarioId &&
                       n.EmpresaId == empresaId &&
                       (!n.FechaExpiracion.HasValue || n.FechaExpiracion.Value > ahora))
            .OrderByDescending(n => n.FechaCreacion)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Notificacion>> GetNoLeidasByUsuarioAsync(string usuarioId, Guid empresaId)
    {
        var ahora = DateTime.UtcNow;

        return await _context.Notificaciones
            .Include(n => n.Empresa)
            .Include(n => n.Usuario)
            .Where(n => n.UsuarioId == usuarioId &&
                       n.EmpresaId == empresaId &&
                       !n.Leida &&
                       (!n.FechaExpiracion.HasValue || n.FechaExpiracion.Value > ahora))
            .OrderByDescending(n => n.Importante)
            .ThenByDescending(n => n.FechaCreacion)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Notificacion>> GetByTipoAsync(string usuarioId, Guid empresaId, TipoNotificacion tipo)
    {
        var ahora = DateTime.UtcNow;

        return await _context.Notificaciones
            .Include(n => n.Empresa)
            .Include(n => n.Usuario)
            .Where(n => n.UsuarioId == usuarioId &&
                       n.EmpresaId == empresaId &&
                       n.TipoNotificacion == tipo &&
                       (!n.FechaExpiracion.HasValue || n.FechaExpiracion.Value > ahora))
            .OrderByDescending(n => n.FechaCreacion)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetCountNoLeidasAsync(string usuarioId, Guid empresaId)
    {
        var ahora = DateTime.UtcNow;

        return await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId &&
                       n.EmpresaId == empresaId &&
                       !n.Leida &&
                       (!n.FechaExpiracion.HasValue || n.FechaExpiracion.Value > ahora))
            .CountAsync();
    }

    public async Task<ResumenNotificacionesDTO> GetResumenAsync(string usuarioId, Guid empresaId)
    {
        var ahora = DateTime.UtcNow;
        var hoy = DateTime.Today;
        var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);

        var notificaciones = await _context.Notificaciones
            .Include(n => n.Empresa)
            .Include(n => n.Usuario)
            .Where(n => n.UsuarioId == usuarioId &&
                       n.EmpresaId == empresaId &&
                       (!n.FechaExpiracion.HasValue || n.FechaExpiracion.Value > ahora))
            .AsNoTracking()
            .ToListAsync();

        var totalNoLeidas = notificaciones.Count(n => !n.Leida);
        var totalImportantes = notificaciones.Count(n => n.Importante && !n.Leida);
        var totalHoy = notificaciones.Count(n => n.FechaCreacion.Date == hoy);
        var totalSemana = notificaciones.Count(n => n.FechaCreacion >= inicioSemana);

        var recientes = notificaciones
            .OrderByDescending(n => n.Importante)
            .ThenByDescending(n => n.FechaCreacion)
            .Take(10)
            .Select(n => ConvertirADTO(n))
            .ToList();

        return new ResumenNotificacionesDTO
        {
            TotalNoLeidas = totalNoLeidas,
            TotalImportantes = totalImportantes,
            TotalHoy = totalHoy,
            TotalSemana = totalSemana,
            NotificacionesRecientes = recientes
        };
    }

    public async Task<Notificacion> AddAsync(Notificacion notificacion)
    {
        notificacion.Id = Guid.NewGuid();
        notificacion.FechaCreacion = DateTime.UtcNow;
        notificacion.Leida = false;

        // Asignar icono y color por defecto según el tipo si no se especifica
        if (string.IsNullOrEmpty(notificacion.Icono) || string.IsNullOrEmpty(notificacion.Color))
        {
            var (icono, color) = ObtenerIconoYColor(notificacion.TipoNotificacion);
            notificacion.Icono ??= icono;
            notificacion.Color ??= color;
        }

        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();
        return notificacion;
    }

    public async Task MarcarComoLeidaAsync(Guid id)
    {
        var notificacion = await _context.Notificaciones.FindAsync(id);
        if (notificacion != null && !notificacion.Leida)
        {
            notificacion.Leida = true;
            notificacion.FechaLeida = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarcarTodasComoLeidasAsync(string usuarioId, Guid empresaId)
    {
        var notificaciones = await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId &&
                       n.EmpresaId == empresaId &&
                       !n.Leida)
            .ToListAsync();

        var ahora = DateTime.UtcNow;
        foreach (var notificacion in notificaciones)
        {
            notificacion.Leida = true;
            notificacion.FechaLeida = ahora;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var notificacion = await _context.Notificaciones.FindAsync(id);
        if (notificacion != null)
        {
            _context.Notificaciones.Remove(notificacion);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteExpiradasAsync(Guid empresaId)
    {
        var ahora = DateTime.UtcNow;
        var expiradas = await _context.Notificaciones
            .Where(n => n.EmpresaId == empresaId &&
                       n.FechaExpiracion.HasValue &&
                       n.FechaExpiracion.Value <= ahora)
            .ToListAsync();

        _context.Notificaciones.RemoveRange(expiradas);
        await _context.SaveChangesAsync();
    }

    // Métodos auxiliares

    private static (string Icono, string Color) ObtenerIconoYColor(TipoNotificacion tipo)
    {
        return tipo switch
        {
            TipoNotificacion.DocumentoAceptado => ("fa-check-circle", "success"),
            TipoNotificacion.DocumentoRechazado => ("fa-times-circle", "danger"),
            TipoNotificacion.DocumentoPendiente => ("fa-clock", "warning"),
            TipoNotificacion.PagoRecibido => ("fa-money-bill-wave", "success"),
            TipoNotificacion.PagoPendiente => ("fa-exclamation-circle", "warning"),
            TipoNotificacion.InventarioBajo => ("fa-boxes", "danger"),
            TipoNotificacion.GastoAprobado => ("fa-check-double", "success"),
            TipoNotificacion.GastoRechazado => ("fa-ban", "danger"),
            TipoNotificacion.GastoRequiereAprobacion => ("fa-clipboard-check", "info"),
            TipoNotificacion.NuevoUsuario => ("fa-user-plus", "info"),
            TipoNotificacion.Sistema => ("fa-cog", "secondary"),
            TipoNotificacion.Advertencia => ("fa-exclamation-triangle", "warning"),
            TipoNotificacion.Error => ("fa-exclamation-circle", "danger"),
            _ => ("fa-bell", "info")
        };
    }

    private static NotificacionDTO ConvertirADTO(Notificacion notificacion)
    {
        var ahora = DateTime.UtcNow;
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
        var diferencia = DateTime.UtcNow - fechaCreacion;

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
