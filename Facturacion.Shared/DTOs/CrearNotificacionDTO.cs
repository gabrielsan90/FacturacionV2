using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para crear una nueva notificación
/// </summary>
public class CrearNotificacionDTO
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(450, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string UsuarioId { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoNotificacion TipoNotificacion { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Titulo { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Mensaje { get; set; } = null!;

    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Icono { get; set; }

    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Color { get; set; }

    public Guid? EntidadRelacionadaId { get; set; }

    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TipoEntidad { get; set; }

    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UrlAccion { get; set; }

    public bool Importante { get; set; }

    public DateTime? FechaExpiracion { get; set; }
}
