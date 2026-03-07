using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

public class PedidoVentaDetalle
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON PEDIDO DE VENTA
    // ========================================

    [Display(Name = "Pedido de Venta")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid PedidoVentaId { get; set; }

    // ========================================
    // NÚMERO DE LÍNEA
    // ========================================

    [Display(Name = "Número de Línea")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int NumeroLinea { get; set; }

    // ========================================
    // PRODUCTO/SERVICIO
    // ========================================

    [Display(Name = "Producto")]
    public Guid? ProductoId { get; set; }

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    // ========================================
    // CANTIDAD Y PRECIO
    // ========================================

    [Display(Name = "Cantidad")]
    [Column(TypeName = "decimal(18, 3)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Cantidad { get; set; }

    [Display(Name = "Precio Unitario")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal PrecioUnitario { get; set; }

    [Display(Name = "Monto Total")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal MontoTotal { get; set; }

    // ========================================
    // DESCUENTOS
    // ========================================

    [Display(Name = "Porcentaje Descuento")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal PorcentajeDescuento { get; set; }

    [Display(Name = "Monto Descuento")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoDescuento { get; set; }

    // ========================================
    // SUBTOTAL E IMPUESTOS
    // ========================================

    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Código tarifa IVA (01=No sujeto, 02=Exento, 03=1%, 04=2%, 05=4%, 08=13%)
    /// </summary>
    [Display(Name = "Código Tarifa IVA")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string CodigoTarifaIVA { get; set; } = "08";

    [Display(Name = "Tarifa IVA")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TarifaIVA { get; set; } = 13;

    [Display(Name = "Monto IVA")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoIVA { get; set; }

    // ========================================
    // TOTAL DE LA LÍNEA
    // ========================================

    [Display(Name = "Total Línea")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal TotalLinea { get; set; }

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================

    public PedidoVenta? PedidoVenta { get; set; }
    public Producto? Producto { get; set; }
}
