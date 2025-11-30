using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// Documentos pendientes de envío o rechazados por Hacienda
/// </summary>
public class DocumentosPendientesDTO
{
    [Display(Name = "Documento Id")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoId { get; set; }

    [Display(Name = "Número Consecutivo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NumeroConsecutivo { get; set; } = null!;

    [Display(Name = "Tipo de Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string TipoDocumento { get; set; } = null!;

    [Display(Name = "Clave")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Clave { get; set; } = null!;

    [Display(Name = "Fecha de Emisión")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaEmision { get; set; }

    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Estado { get; set; } = null!;

    [Display(Name = "Total")]
    public decimal Total { get; set; }

    [Display(Name = "Motivo de Rechazo")]
    public string? MotivoRechazo { get; set; }
}
