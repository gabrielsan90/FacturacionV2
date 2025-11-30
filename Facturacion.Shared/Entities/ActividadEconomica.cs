using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class ActividadEconomica
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Código CIIU4")]
    [MaxLength(6, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string CodigoCIIU4 { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    [Display(Name = "Activa")]
    public bool Activa { get; set; }

    // Navegación
    public ICollection<EmpresaActividadEconomica>? EmpresasActividades { get; set; } = new List<EmpresaActividadEconomica>();
}
