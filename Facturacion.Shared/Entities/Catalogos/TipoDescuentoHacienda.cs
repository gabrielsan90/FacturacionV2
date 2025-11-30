using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

/// <summary>
/// Catálogo oficial de Hacienda v4.4: Tipos de Descuento
/// 11 códigos específicos para clasificar descuentos
/// Obligatorio cuando hay descuento en el documento
/// </summary>
public class TipoDescuentoHacienda
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Código del tipo de descuento (01-11)
    /// </summary>
    [Display(Name = "Código")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Descripción del tipo de descuento
    /// </summary>
    [Display(Name = "Descripción")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Indica si el código está activo
    /// </summary>
    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
