using Facturacion.Shared.Enums;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO completo de notificación
/// </summary>
public class NotificacionDTO
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string UsuarioId { get; set; } = null!;
    public TipoNotificacion TipoNotificacion { get; set; }
    public string Titulo { get; set; } = null!;
    public string Mensaje { get; set; } = null!;
    public string? Icono { get; set; }
    public string? Color { get; set; }
    public bool Leida { get; set; }
    public DateTime? FechaLeida { get; set; }
    public Guid? EntidadRelacionadaId { get; set; }
    public string? TipoEntidad { get; set; }
    public string? UrlAccion { get; set; }
    public bool Importante { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaExpiracion { get; set; }

    // Propiedades calculadas
    public string TiempoTranscurrido { get; set; } = null!;
    public bool Expirada { get; set; }
}
