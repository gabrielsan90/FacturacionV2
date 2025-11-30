using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class UserManagementDto
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Display(Name = "Nombre Completo")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Display(Name = "Documento")]
    public string Document { get; set; } = null!;

    [Phone(ErrorMessage = "El formato del número de teléfono no es válido.")]
    [Display(Name = "Teléfono")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Contraseña")]
    public string? Password { get; set; }

    [Display(Name = "Confirmar Contraseña")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Email Confirmado")]
    public bool EmailConfirmed { get; set; }

    [Display(Name = "Roles")]
    public List<string> Roles { get; set; } = new();

    [Display(Name = "Empresas")]
    public List<Guid> EmpresaIds { get; set; } = new();
}
