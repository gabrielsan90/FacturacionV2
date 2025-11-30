using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

/// <summary>
/// Catálogo oficial de Hacienda: Impuestos
/// Referencia: Anexo 4.2 - Códigos de impuestos
/// Incluye IVA (01), Exento (02), IVA Devuelto (03), Servicio 10% (04), etc.
/// </summary>
public class Impuesto
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Código")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Codigo { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    [Display(Name = "Porcentaje")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Porcentaje { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
