using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

/// <summary>
/// Catálogo oficial de Hacienda v4.4: Códigos de Referencia
/// Usado en Notas de Crédito/Débito para indicar el motivo
/// Incluye nuevos códigos v4.4: 06-12
/// </summary>
public class CodigoReferencia
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Código de referencia (01-12, 99)
    /// </summary>
    [Display(Name = "Código")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Descripción del código de referencia
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
