using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class User : IdentityUser
{
    [Display(Name = "Nombre Completo")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string FullName { get; set; } = null!;

    [Display(Name = "Documento")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Document { get; set; } = null!;
}
