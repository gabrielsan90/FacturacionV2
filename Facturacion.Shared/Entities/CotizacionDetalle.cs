using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Línea de detalle de una cotización
/// Representa un producto o servicio incluido en la cotización
/// </summary>
public class CotizacionDetalle
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON COTIZACIÓN
    // ========================================

    [Display(Name = "Cotización")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CotizacionId { get; set; }

    // ========================================
    // NÚMERO DE LÍNEA
    // ========================================

    /// <summary>
    /// Número secuencial de la línea (1, 2, 3, ...)
    /// </summary>
    [Display(Name = "Número de Línea")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int NumeroLinea { get; set; }

    // ========================================
    // PRODUCTO/SERVICIO
    // ========================================

    /// <summary>
    /// Relación con el catálogo de productos (opcional, puede ser producto libre)
    /// </summary>
    [Display(Name = "Producto")]
    public Guid? ProductoId { get; set; }

    /// <summary>
    /// Código del producto o servicio
    /// </summary>
    [Display(Name = "Código")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Codigo { get; set; }

    /// <summary>
    /// Descripción detallada del producto o servicio
    /// </summary>
    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    // ========================================
    // CANTIDAD Y UNIDAD
    // ========================================

    /// <summary>
    /// Cantidad del producto o servicio (hasta 3 decimales)
    /// </summary>
    [Display(Name = "Cantidad")]
    [Column(TypeName = "decimal(18, 3)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Código de unidad de medida
    /// </summary>
    [Display(Name = "Unidad de Medida")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UnidadMedida { get; set; }

    // ========================================
    // PRECIOS
    // ========================================

    /// <summary>
    /// Precio unitario del producto o servicio (hasta 5 decimales)
    /// </summary>
    [Display(Name = "Precio Unitario")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Monto total antes de descuentos e impuestos (Cantidad × PrecioUnitario)
    /// </summary>
    [Display(Name = "Monto Total")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal MontoTotal { get; set; }

    // ========================================
    // DESCUENTOS
    // ========================================

    /// <summary>
    /// Monto de descuento aplicado a esta línea
    /// </summary>
    [Display(Name = "Monto Descuento")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoDescuento { get; set; }

    /// <summary>
    /// Porcentaje de descuento aplicado
    /// </summary>
    [Display(Name = "Porcentaje Descuento")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? PorcentajeDescuento { get; set; }

    /// <summary>
    /// Naturaleza del descuento (si aplica)
    /// </summary>
    [Display(Name = "Naturaleza Descuento")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NaturalezaDescuento { get; set; }

    // ========================================
    // SUBTOTAL (después de descuentos, antes de impuestos)
    // ========================================

    /// <summary>
    /// Subtotal de la línea (MontoTotal - MontoDescuento)
    /// </summary>
    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal SubTotal { get; set; }

    // ========================================
    // IMPUESTOS
    // ========================================

    /// <summary>
    /// Código de tarifa de IVA (01-No sujeto, 02-Tarifa 0%, 03-Tarifa reducida 1%,
    /// 04-Tarifa reducida 2%, 05-Tarifa reducida 4%, 06-Tarifa reducida 8%,
    /// 07-Tarifa general 13%, 08-Tarifa 2%, 09-Tarifa 4%)
    /// </summary>
    [Display(Name = "Código Tarifa IVA")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CodigoTarifaIVA { get; set; }

    /// <summary>
    /// Tarifa de IVA aplicada (porcentaje)
    /// </summary>
    [Display(Name = "Tarifa IVA")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? TarifaIVA { get; set; }

    /// <summary>
    /// Monto del IVA calculado
    /// </summary>
    [Display(Name = "Monto IVA")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoIVA { get; set; }

    /// <summary>
    /// Monto total de impuestos de la línea
    /// </summary>
    [Display(Name = "Total Impuestos")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalImpuestos { get; set; }

    // ========================================
    // TOTAL DE LA LÍNEA (después de impuestos)
    // ========================================

    /// <summary>
    /// Total de la línea (SubTotal + TotalImpuestos)
    /// </summary>
    [Display(Name = "Total Línea")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal TotalLinea { get; set; }

    // ========================================
    // INFORMACIÓN ADICIONAL
    // ========================================

    /// <summary>
    /// Notas adicionales de la línea
    /// </summary>
    [Display(Name = "Notas")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Notas { get; set; }

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================

    public Cotizacion? Cotizacion { get; set; }
    public Producto? Producto { get; set; }
}
