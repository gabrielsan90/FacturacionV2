using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Detalle de productos en un ajuste de inventario.
/// </summary>
public class AjusteInventarioDetalle
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relación con encabezado
    // =====================================================
    [Display(Name = "Ajuste")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid AjusteInventarioId { get; set; }

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
    /// <summary>
    /// Cantidad registrada en el sistema antes del ajuste
    /// </summary>
    [Display(Name = "Cantidad Sistema")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadSistema { get; set; }

    /// <summary>
    /// Cantidad física contada o cantidad a ajustar
    /// </summary>
    [Display(Name = "Cantidad Física")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadFisica { get; set; }

    /// <summary>
    /// Diferencia calculada (CantidadFisica - CantidadSistema)
    /// </summary>
    [Display(Name = "Diferencia")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Diferencia { get; set; }

    [Display(Name = "Costo Unitario")]
    [Column(TypeName = "decimal(18,6)")]
    public decimal CostoUnitario { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public decimal ValorDiferencia => Diferencia * CostoUnitario;

    [NotMapped]
    public bool TieneDiferencia => Diferencia != 0;

    [NotMapped]
    public string TipoDiferencia => Diferencia switch
    {
        > 0 => "Sobrante",
        < 0 => "Faltante",
        _ => "Sin diferencia"
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public AjusteInventario? AjusteInventario { get; set; }
    public Producto? Producto { get; set; }
    public Lote? Lote { get; set; }
}
