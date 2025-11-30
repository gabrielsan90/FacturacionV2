using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

/// <summary>
/// Catálogo de Bienes y Servicios (CAByS) versión 2025
/// Obligatorio desde 01/06/2025 según Hacienda v4.4
/// Código de 13 dígitos
/// </summary>
public class CAByS
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Código CAByS de 13 dígitos
    /// </summary>
    [Display(Name = "Código CAByS")]
    [MaxLength(13, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [MinLength(13, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Descripción del bien o servicio
    /// </summary>
    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Categoría del catálogo (Bien o Servicio)
    /// </summary>
    [Display(Name = "Categoría")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Categoria { get; set; }

    /// <summary>
    /// Código del impuesto aplicable por defecto
    /// </summary>
    [Display(Name = "Código Impuesto")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CodigoImpuesto { get; set; }

    /// <summary>
    /// Tarifa del impuesto por defecto (porcentaje)
    /// </summary>
    [Display(Name = "Tarifa Impuesto")]
    public decimal? TarifaImpuesto { get; set; }

    /// <summary>
    /// Indica si el código está activo/vigente
    /// </summary>
    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
