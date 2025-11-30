using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

/// <summary>
/// Catálogo oficial de Hacienda v4.4: Tarifas de IVA (CodigoTarifa)
/// Define las tarifas aplicables del Impuesto al Valor Agregado
/// </summary>
public class TarifaIVA
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Código de la tarifa IVA (01-11)
    /// </summary>
    [Display(Name = "Código")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Descripción de la tarifa
    /// </summary>
    [Display(Name = "Descripción")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Porcentaje de la tarifa (0%, 1%, 2%, 4%, 8%, 13%)
    /// </summary>
    [Display(Name = "Porcentaje")]
    [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
    public decimal Porcentaje { get; set; }

    /// <summary>
    /// Indica si la tarifa está activa
    /// </summary>
    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
