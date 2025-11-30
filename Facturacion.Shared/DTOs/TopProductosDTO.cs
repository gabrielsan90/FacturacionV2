using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// Productos más vendidos por cantidad y monto
/// </summary>
public class TopProductosDTO
{
    [Display(Name = "Producto Id")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProductoId { get; set; }

    [Display(Name = "Nombre del Producto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NombreProducto { get; set; } = null!;

    [Display(Name = "Cantidad Vendida")]
    public decimal CantidadVendida { get; set; }

    [Display(Name = "Total de Ventas")]
    public decimal TotalVentas { get; set; }

    [Display(Name = "Última Venta")]
    public DateTime? UltimaVenta { get; set; }
}
