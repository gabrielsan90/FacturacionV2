using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Detalle de productos/servicios solicitados en una requisición.
/// </summary>
public class RequisicionDetalle
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relación con Requisición
    // =====================================================
    [Display(Name = "Requisición")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid RequisicionId { get; set; }

    // =====================================================
    // Producto/Servicio
    // =====================================================
    [Display(Name = "Producto")]
    public Guid? ProductoId { get; set; }

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    [Display(Name = "Especificaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Especificaciones { get; set; }

    // =====================================================
    // Cantidades
    // =====================================================
    [Display(Name = "Cantidad Solicitada")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadSolicitada { get; set; }

    [Display(Name = "Unidad de Medida")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UnidadMedida { get; set; }

    [Display(Name = "Cantidad Cotizada")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadCotizada { get; set; }

    [Display(Name = "Cantidad Ordenada")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadOrdenada { get; set; }

    // =====================================================
    // Precios Estimados
    // =====================================================
    [Display(Name = "Precio Estimado")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal? PrecioEstimado { get; set; }

    [Display(Name = "Subtotal Estimado")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubtotalEstimado { get; set; }

    // =====================================================
    // Proveedor Sugerido
    // =====================================================
    [Display(Name = "Proveedor Sugerido")]
    public Guid? ProveedorSugeridoId { get; set; }

    // =====================================================
    // Control
    // =====================================================
    [Display(Name = "Número de Línea")]
    public int NumeroLinea { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public decimal CantidadPendienteCotizar => CantidadSolicitada - CantidadCotizada;

    [NotMapped]
    public decimal CantidadPendienteOrdenar => CantidadSolicitada - CantidadOrdenada;

    [NotMapped]
    public bool EstaCotizado => CantidadCotizada >= CantidadSolicitada;

    [NotMapped]
    public bool EstaOrdenado => CantidadOrdenada >= CantidadSolicitada;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Requisicion? Requisicion { get; set; }
    public Producto? Producto { get; set; }
    public Proveedor? ProveedorSugerido { get; set; }
}
