using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Detalle de productos en una orden de compra.
/// </summary>
public class OrdenCompraDetalle
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relaciones
    // =====================================================
    [Display(Name = "Orden de Compra")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid OrdenCompraId { get; set; }

    [Display(Name = "Producto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProductoId { get; set; }

    // =====================================================
    // Cantidades
    // =====================================================
    [Display(Name = "Cantidad")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Cantidad { get; set; }

    [Display(Name = "Cantidad Recibida")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadRecibida { get; set; }

    // =====================================================
    // Precios
    // =====================================================
    [Display(Name = "Precio Unitario")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal PrecioUnitario { get; set; }

    [Display(Name = "% Descuento")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PorcentajeDescuento { get; set; }

    [Display(Name = "Descuento")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; }

    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Display(Name = "% IVA")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PorcentajeIVA { get; set; } = 13;

    [Display(Name = "Impuesto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Impuesto { get; set; }

    [Display(Name = "Total Línea")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalLinea { get; set; }

    // =====================================================
    // Control
    // =====================================================
    [Display(Name = "Observaciones")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Número de Línea")]
    public int NumeroLinea { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public decimal CantidadPendiente => Cantidad - CantidadRecibida;

    [NotMapped]
    public bool EstaCompleto => CantidadRecibida >= Cantidad;

    [NotMapped]
    public decimal PorcentajeRecibido => Cantidad > 0
        ? (CantidadRecibida / Cantidad) * 100
        : 0;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public OrdenCompra? OrdenCompra { get; set; }
    public Producto? Producto { get; set; }
}
