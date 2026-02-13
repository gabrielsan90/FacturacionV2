using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Detalle de productos recibidos en una recepción de compra.
/// </summary>
public class RecepcionCompraDetalle
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relaciones
    // =====================================================
    [Display(Name = "Recepción")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid RecepcionCompraId { get; set; }

    [Display(Name = "Detalle Orden de Compra")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid OrdenCompraDetalleId { get; set; }

    [Display(Name = "Producto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProductoId { get; set; }

    // =====================================================
    // Cantidades y Costos
    // =====================================================
    [Display(Name = "Cantidad Recibida")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadRecibida { get; set; }

    [Display(Name = "Costo Unitario")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CostoUnitario { get; set; }

    [Display(Name = "Costo Total")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostoTotal { get; set; }

    // =====================================================
    // Control de Lote (opcional)
    // =====================================================
    [Display(Name = "Lote")]
    public Guid? LoteId { get; set; }

    [Display(Name = "Número de Lote")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroLote { get; set; }

    [Display(Name = "Fecha de Vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    // =====================================================
    // Control
    // =====================================================
    [Display(Name = "Observaciones")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Número de Línea")]
    public int NumeroLinea { get; set; }

    // =====================================================
    // Navigation Properties
    // =====================================================
    public RecepcionCompra? RecepcionCompra { get; set; }
    public OrdenCompraDetalle? OrdenCompraDetalle { get; set; }
    public Producto? Producto { get; set; }
    public Lote? Lote { get; set; }
}
