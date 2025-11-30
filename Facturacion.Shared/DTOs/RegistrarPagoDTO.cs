using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para registrar un pago parcial o total
/// </summary>
public class RegistrarPagoDTO
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid GastoId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Range(0.00001, 999999999999.99999, ErrorMessage = "El {0} debe ser mayor a cero.")]
    public decimal MontoPago { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaPago { get; set; }

    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }
}
