using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class AjusteInventarioDTO
{
    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    public decimal Cantidad { get; set; }

    [MaxLength(100, ErrorMessage = "La referencia no puede exceder 100 caracteres.")]
    public string? Referencia { get; set; }

    [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres.")]
    public string? Observaciones { get; set; }
}
