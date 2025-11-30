using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para adjuntos de correo electrónico
/// Contiene archivo en formato base64
/// </summary>
public class AdjuntoEmailDTO
{
    [Display(Name = "Nombre del Archivo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(255, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string NombreArchivo { get; set; } = null!;

    [Display(Name = "Contenido Base64")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string ContenidoBase64 { get; set; } = null!;

    [Display(Name = "Tipo de Contenido")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoContenido { get; set; } = null!;
}
