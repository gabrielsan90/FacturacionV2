using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// Estado del inventario de productos con alertas de stock
/// </summary>
public class EstadoInventarioDTO
{
    [Display(Name = "Producto Id")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProductoId { get; set; }

    [Display(Name = "Nombre del Producto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NombreProducto { get; set; } = null!;

    [Display(Name = "Stock Actual")]
    public decimal StockActual { get; set; }

    [Display(Name = "Stock Mínimo")]
    public decimal StockMinimo { get; set; }

    [Display(Name = "Stock Máximo")]
    public decimal StockMaximo { get; set; }

    [Display(Name = "Valor de Inventario")]
    public decimal ValorInventario { get; set; }

    [Display(Name = "Estado del Stock")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string EstadoStock { get; set; } = null!;
}
