using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class UpdateRolDto
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Id { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
