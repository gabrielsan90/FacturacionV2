using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO base para envío de correos electrónicos genéricos
/// Soporta destinatarios múltiples, CC, CCO y adjuntos
/// </summary>
public class EmailDTO
{
    [Display(Name = "Para (Destinatarios)")]
    [Required(ErrorMessage = "Debe especificar al menos un destinatario.")]
    public List<string> Para { get; set; } = new List<string>();

    [Display(Name = "CC (Copia)")]
    public List<string>? CC { get; set; }

    [Display(Name = "CCO (Copia Oculta)")]
    public List<string>? CCO { get; set; }

    [Display(Name = "Asunto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Asunto { get; set; } = null!;

    [Display(Name = "Cuerpo (HTML)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Cuerpo { get; set; } = null!;

    [Display(Name = "Adjuntos")]
    public List<AdjuntoEmailDTO>? Adjuntos { get; set; }
}
