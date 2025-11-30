using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para enviar documentos electrónicos (facturas, tiquetes, etc.) por correo
/// Incluye XML y PDF adjuntos
/// </summary>
public class EnviarDocumentoEmailDTO
{
    [Display(Name = "ID del Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoId { get; set; }

    [Display(Name = "Emails Adicionales")]
    public List<string>? EmailsAdicionales { get; set; }

    [Display(Name = "Mensaje Personalizado")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Mensaje { get; set; }
}
