using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

/// <summary>
/// Catálogo oficial de Hacienda: Medios de Pago (01-99)
/// Referencia: Anexo 4.6 - Medios de pago
/// </summary>
public class MedioPago
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
