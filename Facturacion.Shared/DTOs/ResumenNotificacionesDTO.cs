namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO con resumen de notificaciones del usuario
/// </summary>
public class ResumenNotificacionesDTO
{
    public int TotalNoLeidas { get; set; }
    public int TotalImportantes { get; set; }
    public int TotalHoy { get; set; }
    public int TotalSemana { get; set; }
    public List<NotificacionDTO> NotificacionesRecientes { get; set; } = new List<NotificacionDTO>();
}
