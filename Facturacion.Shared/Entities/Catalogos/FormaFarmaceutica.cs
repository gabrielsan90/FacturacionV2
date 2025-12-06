using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

/// <summary>
/// Catálogo oficial de Hacienda: Formas Farmacéuticas
/// Obligatorio desde 01/12/2024 para productos farmacéuticos según v4.4
/// </summary>
public class FormaFarmaceutica
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Código")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Codigo { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
