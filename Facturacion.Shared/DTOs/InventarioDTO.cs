using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class InventarioDTO
{
    [Required(ErrorMessage = "El producto es obligatorio.")]
    public Guid ProductoId { get; set; }

    [Required(ErrorMessage = "La sucursal es obligatoria.")]
    public Guid SucursalId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "La cantidad inicial debe ser mayor o igual a cero.")]
    public decimal? CantidadInicial { get; set; }

    [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres.")]
    public string? Observaciones { get; set; }
}
