using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Entidad para el sistema de notificaciones in-app
/// Permite enviar alertas y mensajes a los usuarios dentro de la aplicación
/// </summary>
public class Notificacion
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// ID de la empresa a la que pertenece esta notificación (multi-tenant)
    /// </summary>
    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    /// <summary>
    /// ID del usuario destinatario de la notificación
    /// </summary>
    [Display(Name = "Usuario")]
    [MaxLength(450, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string UsuarioId { get; set; } = null!;

    /// <summary>
    /// Tipo de notificación
    /// </summary>
    [Display(Name = "Tipo de Notificación")]
    public TipoNotificacion TipoNotificacion { get; set; }

    /// <summary>
    /// Título de la notificación
    /// </summary>
    [Display(Name = "Título")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Titulo { get; set; } = null!;

    /// <summary>
    /// Mensaje detallado de la notificación
    /// </summary>
    [Display(Name = "Mensaje")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Mensaje { get; set; } = null!;

    /// <summary>
    /// Clase de icono para la UI (ej: "fa-file-invoice", "fa-exclamation-triangle")
    /// </summary>
    [Display(Name = "Icono")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Icono { get; set; }

    /// <summary>
    /// Código de color para la UI (ej: "success", "warning", "danger", "info")
    /// </summary>
    [Display(Name = "Color")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Color { get; set; }

    /// <summary>
    /// Indica si la notificación ha sido leída
    /// </summary>
    [Display(Name = "Leída")]
    public bool Leida { get; set; }

    /// <summary>
    /// Fecha y hora en que se marcó como leída
    /// </summary>
    [Display(Name = "Fecha Leída")]
    public DateTime? FechaLeida { get; set; }

    /// <summary>
    /// ID de la entidad relacionada (DocumentoId, GastoId, etc.)
    /// </summary>
    [Display(Name = "Entidad Relacionada ID")]
    public Guid? EntidadRelacionadaId { get; set; }

    /// <summary>
    /// Tipo de entidad relacionada ("Documento", "Gasto", "Inventario", etc.)
    /// </summary>
    [Display(Name = "Tipo de Entidad")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TipoEntidad { get; set; }

    /// <summary>
    /// URL de acción para navegación en el frontend (deep link)
    /// </summary>
    [Display(Name = "URL de Acción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UrlAccion { get; set; }

    /// <summary>
    /// Marca la notificación como importante/prioritaria
    /// </summary>
    [Display(Name = "Importante")]
    public bool Importante { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la notificación
    /// </summary>
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de expiración opcional de la notificación
    /// </summary>
    [Display(Name = "Fecha de Expiración")]
    public DateTime? FechaExpiracion { get; set; }

    // Navegación
    public Empresa? Empresa { get; set; }
    public User? Usuario { get; set; }
}
