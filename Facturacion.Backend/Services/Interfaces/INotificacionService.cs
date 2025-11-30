using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para gestión de notificaciones del sistema
/// Recomendación 11 de Hacienda v4.4
/// </summary>
public interface INotificacionService
{
    /// <summary>
    /// Crea una nueva notificación
    /// </summary>
    Task<Notificacion> CrearNotificacionAsync(Notificacion notificacion);

    /// <summary>
    /// Obtiene las notificaciones de un usuario
    /// </summary>
    Task<IEnumerable<Notificacion>> ObtenerNotificacionesUsuarioAsync(
        string usuarioId,
        Guid empresaId,
        bool soloNoLeidas = false,
        int limite = 50);

    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    Task MarcarComoLeidaAsync(Guid notificacionId);

    /// <summary>
    /// Marca todas las notificaciones como leídas
    /// </summary>
    Task MarcarTodasComoLeidasAsync(string usuarioId, Guid empresaId);

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas
    /// </summary>
    Task<int> ObtenerConteoNoLeidasAsync(string usuarioId, Guid empresaId);

    /// <summary>
    /// Elimina notificaciones expiradas
    /// </summary>
    Task EliminarExpiradasAsync();

    /// <summary>
    /// Crea notificación de documento aceptado
    /// </summary>
    Task NotificarDocumentoAceptadoAsync(Documento documento);

    /// <summary>
    /// Crea notificación de documento rechazado
    /// </summary>
    Task NotificarDocumentoRechazadoAsync(Documento documento, string razon);

    /// <summary>
    /// Crea notificación de certificado próximo a vencer
    /// </summary>
    Task NotificarCertificadoPorVencerAsync(Empresa empresa, int diasParaVencer);

    /// <summary>
    /// Crea notificación de stock bajo
    /// </summary>
    Task NotificarStockBajoAsync(Producto producto, Sucursal sucursal, decimal stockActual);
}
