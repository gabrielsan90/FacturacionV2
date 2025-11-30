using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de notificaciones
/// </summary>
public class NotificacionService : INotificacionService
{
    private readonly DataContext _context;

    public NotificacionService(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Crea una nueva notificación
    /// </summary>
    public async Task<Notificacion> CrearNotificacionAsync(Notificacion notificacion)
    {
        if (notificacion.Id == Guid.Empty)
            notificacion.Id = Guid.NewGuid();

        if (notificacion.FechaCreacion == default)
            notificacion.FechaCreacion = DateTime.Now;

        _context.Set<Notificacion>().Add(notificacion);
        await _context.SaveChangesAsync();

        return notificacion;
    }

    /// <summary>
    /// Obtiene las notificaciones de un usuario
    /// </summary>
    public async Task<IEnumerable<Notificacion>> ObtenerNotificacionesUsuarioAsync(
        string usuarioId,
        Guid empresaId,
        bool soloNoLeidas = false,
        int limite = 50)
    {
        var query = _context.Set<Notificacion>()
            .Where(n => n.UsuarioId == usuarioId && n.EmpresaId == empresaId);

        if (soloNoLeidas)
            query = query.Where(n => !n.Leida);

        // Excluir notificaciones expiradas
        query = query.Where(n => n.FechaExpiracion == null || n.FechaExpiracion > DateTime.Now);

        return await query
            .OrderByDescending(n => n.Importante)
            .ThenByDescending(n => n.FechaCreacion)
            .Take(limite)
            .ToListAsync();
    }

    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    public async Task MarcarComoLeidaAsync(Guid notificacionId)
    {
        var notificacion = await _context.Set<Notificacion>().FindAsync(notificacionId);
        if (notificacion != null)
        {
            notificacion.Leida = true;
            notificacion.FechaLeida = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Marca todas las notificaciones como leídas
    /// </summary>
    public async Task MarcarTodasComoLeidasAsync(string usuarioId, Guid empresaId)
    {
        var ahora = DateTime.Now;
        await _context.Set<Notificacion>()
            .Where(n => n.UsuarioId == usuarioId &&
                       n.EmpresaId == empresaId &&
                       !n.Leida)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Leida, true)
                .SetProperty(n => n.FechaLeida, ahora));
    }

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas
    /// </summary>
    public async Task<int> ObtenerConteoNoLeidasAsync(string usuarioId, Guid empresaId)
    {
        return await _context.Set<Notificacion>()
            .CountAsync(n => n.UsuarioId == usuarioId &&
                            n.EmpresaId == empresaId &&
                            !n.Leida &&
                            (n.FechaExpiracion == null || n.FechaExpiracion > DateTime.Now));
    }

    /// <summary>
    /// Elimina notificaciones expiradas
    /// </summary>
    public async Task EliminarExpiradasAsync()
    {
        var fechaLimite = DateTime.Now.AddDays(-90); // Eliminar notificaciones de más de 90 días

        await _context.Set<Notificacion>()
            .Where(n => n.Leida &&
                       (n.FechaExpiracion != null && n.FechaExpiracion < DateTime.Now) ||
                       n.FechaCreacion < fechaLimite)
            .ExecuteDeleteAsync();
    }

    /// <summary>
    /// Crea notificación de documento aceptado
    /// </summary>
    public async Task NotificarDocumentoAceptadoAsync(Documento documento)
    {
        await CrearNotificacionAsync(new Notificacion
        {
            EmpresaId = documento.EmpresaId,
            UsuarioId = documento.UsuarioCreacionId!,
            TipoNotificacion = TipoNotificacion.DocumentoAceptado,
            Titulo = "Documento aceptado por Hacienda",
            Mensaje = $"El documento {documento.NumeroConsecutivo} fue aceptado exitosamente.",
            Icono = "fa-check-circle",
            Color = "success",
            EntidadRelacionadaId = documento.Id,
            TipoEntidad = "Documento",
            UrlAccion = $"/Documentos?id={documento.Id}",
            Importante = false,
            FechaCreacion = DateTime.Now,
            FechaExpiracion = DateTime.Now.AddDays(7)
        });
    }

    /// <summary>
    /// Crea notificación de documento rechazado
    /// </summary>
    public async Task NotificarDocumentoRechazadoAsync(Documento documento, string razon)
    {
        await CrearNotificacionAsync(new Notificacion
        {
            EmpresaId = documento.EmpresaId,
            UsuarioId = documento.UsuarioCreacionId!,
            TipoNotificacion = TipoNotificacion.DocumentoRechazado,
            Titulo = "Documento RECHAZADO por Hacienda",
            Mensaje = $"El documento {documento.NumeroConsecutivo} fue rechazado: {razon}",
            Icono = "fa-times-circle",
            Color = "danger",
            EntidadRelacionadaId = documento.Id,
            TipoEntidad = "Documento",
            UrlAccion = $"/Documentos?id={documento.Id}",
            Importante = true,
            FechaCreacion = DateTime.Now
        });
    }

    /// <summary>
    /// Crea notificación de certificado próximo a vencer
    /// </summary>
    public async Task NotificarCertificadoPorVencerAsync(Empresa empresa, int diasParaVencer)
    {
        var admins = await _context.UsuariosEmpresas
            .Where(ue => ue.EmpresaId == empresa.Id)
            .Select(ue => ue.UserId)
            .ToListAsync();

        foreach (var adminId in admins)
        {
            await CrearNotificacionAsync(new Notificacion
            {
                EmpresaId = empresa.Id,
                UsuarioId = adminId,
                TipoNotificacion = diasParaVencer > 0
                    ? TipoNotificacion.CertificadoPorVencer
                    : TipoNotificacion.CertificadoVencido,
                Titulo = diasParaVencer > 0
                    ? $"Certificado vence en {diasParaVencer} días"
                    : "¡Certificado VENCIDO!",
                Mensaje = diasParaVencer > 0
                    ? $"El certificado digital de {empresa.NombreComercial} vencerá pronto. Renuévelo para evitar interrupciones."
                    : $"El certificado digital de {empresa.NombreComercial} está vencido. No podrá emitir documentos electrónicos.",
                Icono = "fa-certificate",
                Color = diasParaVencer <= 7 ? "danger" : "warning",
                UrlAccion = $"/Empresas?id={empresa.Id}&tab=certificado",
                Importante = diasParaVencer <= 7,
                FechaCreacion = DateTime.Now
            });
        }
    }

    /// <summary>
    /// Crea notificación de stock bajo
    /// </summary>
    public async Task NotificarStockBajoAsync(Producto producto, Sucursal sucursal, decimal stockActual)
    {
        var admins = await _context.UsuariosEmpresas
            .Where(ue => ue.EmpresaId == sucursal.EmpresaId)
            .Select(ue => ue.UserId)
            .ToListAsync();

        foreach (var adminId in admins)
        {
            await CrearNotificacionAsync(new Notificacion
            {
                EmpresaId = sucursal.EmpresaId,
                UsuarioId = adminId,
                TipoNotificacion = TipoNotificacion.InventarioBajo,
                Titulo = "Stock bajo",
                Mensaje = $"El producto '{producto.Nombre}' tiene stock bajo ({stockActual:N2}) en {sucursal.Nombre}.",
                Icono = "fa-box-open",
                Color = stockActual <= 0 ? "danger" : "warning",
                EntidadRelacionadaId = producto.Id,
                TipoEntidad = "Producto",
                UrlAccion = $"/Inventario?productoId={producto.Id}",
                Importante = stockActual <= 0,
                FechaCreacion = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddDays(3)
            });
        }
    }
}
