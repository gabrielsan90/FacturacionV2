using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Detalle de productos en un traslado de inventario.
/// </summary>
public class TrasladoInventarioDetalle
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relación con encabezado
    // =====================================================
    [Display(Name = "Traslado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid TrasladoInventarioId { get; set; }

    // =====================================================
    // Producto y lote
    // =====================================================
    [Display(Name = "Producto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProductoId { get; set; }

    [Display(Name = "Lote")]
    public Guid? LoteId { get; set; }

    // =====================================================
    // Cantidades
    // =====================================================
    [Display(Name = "Cantidad Solicitada")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadSolicitada { get; set; }

    [Display(Name = "Cantidad Enviada")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadEnviada { get; set; }

    [Display(Name = "Cantidad Recibida")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadRecibida { get; set; }

    [Display(Name = "Costo Unitario")]
    [Column(TypeName = "decimal(18,6)")]
    public decimal CostoUnitario { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public decimal DiferenciaCantidad => CantidadRecibida - CantidadSolicitada;

    [NotMapped]
    public bool TieneDiferencia => DiferenciaCantidad != 0;

    [NotMapped]
    public decimal ValorTotal => CantidadSolicitada * CostoUnitario;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public TrasladoInventario? TrasladoInventario { get; set; }
    public Producto? Producto { get; set; }
    public Lote? Lote { get; set; }
}
