using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class Rol : IdentityRole
{
    [Display(Name = "Nombre")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    [Display(Name = "Es Sistema")]
    public bool EsSistema { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    // Navegación
    public User? UsuarioCreacion { get; set; }
    public ICollection<RolPrivilegio>? RolesPrivilegios { get; set; } = new List<RolPrivilegio>();
}
