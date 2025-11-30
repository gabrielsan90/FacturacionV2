using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class TrasladoInventarioDTO
{
    [Required(ErrorMessage = "La sucursal de destino es obligatoria.")]
    public Guid SucursalDestinoId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public decimal Cantidad { get; set; }

    [MaxLength(100, ErrorMessage = "La referencia no puede exceder 100 caracteres.")]
    public string? Referencia { get; set; }

    [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres.")]
    public string? Observaciones { get; set; }
}
