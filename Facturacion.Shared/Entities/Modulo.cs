using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class Modulo
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Nombre")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    [Display(Name = "Orden")]
    public int Orden { get; set; }

    [Display(Name = "Icono")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Icono { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }

    // Navegación
    public ICollection<Privilegio>? Privilegios { get; set; } = new List<Privilegio>();
}
