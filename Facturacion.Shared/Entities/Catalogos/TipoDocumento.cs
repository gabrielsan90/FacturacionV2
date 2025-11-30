using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

/// <summary>
/// Catálogo oficial de Hacienda: Tipos de Documento para Facturación Electrónica v4.4
/// Referencia: Anexo 4.1 - Tipos de documentos electrónicos
/// </summary>
public class TipoDocumento
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

    [Display(Name = "Abreviatura")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Abreviatura { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }
}
