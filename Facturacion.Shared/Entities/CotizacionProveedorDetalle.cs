using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Detalle de items cotizados por el proveedor.
/// </summary>
public class CotizacionProveedorDetalle
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relaciones
    // =====================================================
    [Display(Name = "Cotización")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CotizacionProveedorId { get; set; }

    [Display(Name = "Detalle Requisición")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid RequisicionDetalleId { get; set; }

    [Display(Name = "Producto")]
    public Guid? ProductoId { get; set; }

    // =====================================================
    // Descripción
    // =====================================================
    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    [Display(Name = "Código Proveedor")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CodigoProveedor { get; set; }

    // =====================================================
    // Cantidades y Precios
    // =====================================================
    [Display(Name = "Cantidad")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Cantidad { get; set; }

    [Display(Name = "Unidad de Medida")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UnidadMedida { get; set; }

    [Display(Name = "Precio Unitario")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal PrecioUnitario { get; set; }

    [Display(Name = "% Descuento")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PorcentajeDescuento { get; set; }

    [Display(Name = "Monto Descuento")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoDescuento { get; set; }

    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Display(Name = "% IVA")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PorcentajeIVA { get; set; } = 13;

    [Display(Name = "Monto IVA")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoIVA { get; set; }

    [Display(Name = "Total")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    // =====================================================
    // Información Adicional
    // =====================================================
    [Display(Name = "Tiempo Entrega (Días)")]
    public int? TiempoEntregaDias { get; set; }

    [Display(Name = "Disponible Inmediato")]
    public bool DisponibleInmediato { get; set; }

    [Display(Name = "Marca")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Marca { get; set; }

    [Display(Name = "Modelo")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Modelo { get; set; }

    [Display(Name = "País de Origen")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PaisOrigen { get; set; }

    [Display(Name = "Garantía")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Garantia { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Número de Línea")]
    public int NumeroLinea { get; set; }

    // =====================================================
    // Navigation Properties
    // =====================================================
    public CotizacionProveedor? CotizacionProveedor { get; set; }
    public RequisicionDetalle? RequisicionDetalle { get; set; }
    public Producto? Producto { get; set; }
}
