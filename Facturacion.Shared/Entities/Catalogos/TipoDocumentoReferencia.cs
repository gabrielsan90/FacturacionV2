using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

/// <summary>
/// Catálogo oficial de Hacienda: Tipos de Documento de Referencia
/// Usado en información de referencia de NC/ND/REP
/// </summary>
public class TipoDocumentoReferencia
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Código del tipo de documento (01-14, 99)
    /// </summary>
    [Display(Name = "Código")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Descripción del tipo de documento
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
