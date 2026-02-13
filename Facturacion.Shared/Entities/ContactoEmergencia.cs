using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Contactos de emergencia para empleados.
/// </summary>
public class ContactoEmergencia
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relación con empleado
    // =====================================================
    [Display(Name = "Empleado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpleadoId { get; set; }

    // =====================================================
    // Datos del contacto
    // =====================================================
    [Display(Name = "Nombre Completo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string NombreCompleto { get; set; } = null!;

    [Display(Name = "Parentesco")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Parentesco { get; set; } = null!;

    [Display(Name = "Teléfono")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Telefono { get; set; } = null!;

    [Display(Name = "Teléfono Alternativo")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TelefonoAlternativo { get; set; }

    [Display(Name = "Dirección")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Direccion { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Es Principal")]
    public bool EsPrincipal { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empleado? Empleado { get; set; }
    public User? UsuarioCreacion { get; set; }
}
