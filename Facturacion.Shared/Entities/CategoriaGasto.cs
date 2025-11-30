using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Categoría de gastos para clasificación y reporte
/// </summary>
public class CategoriaGasto
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

    [Display(Name = "Activa")]
    public bool Activa { get; set; } = true;

    // Navegación
    public ICollection<Gasto>? Gastos { get; set; } = new List<Gasto>();
}
