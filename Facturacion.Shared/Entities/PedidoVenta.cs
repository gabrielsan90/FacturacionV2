using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

public class PedidoVenta
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN MULTI-TENANT
    // ========================================

    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    // ========================================
    // NUMERACIÓN
    // ========================================

    [Display(Name = "Número de Pedido")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Numero { get; set; } = null!;

    // ========================================
    // ESTADO Y FECHAS
    // ========================================

    [Display(Name = "Estado")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Estado { get; set; } = "BORRADOR";

    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Fecha { get; set; }

    [Display(Name = "Fecha de Vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    [Display(Name = "Fecha de Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    // ========================================
    // CLIENTE
    // ========================================

    [Display(Name = "Cliente")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ClienteId { get; set; }

    // ========================================
    // CONDICIONES COMERCIALES
    // ========================================

    /// <summary>
    /// Condición de venta (01=Contado, 02=Crédito, 03=Consignación, etc.)
    /// </summary>
    [Display(Name = "Condición de Venta")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string CondicionVenta { get; set; } = "01";

    /// <summary>
    /// Plazo de crédito en días (si CondicionVenta = 02)
    /// </summary>
    [Display(Name = "Plazo Crédito (días)")]
    public int? PlazoCreditoDias { get; set; }

    /// <summary>
    /// Medio de pago (01=Efectivo, 02=Tarjeta, 03=Cheque, 04=Transferencia, etc.)
    /// </summary>
    [Display(Name = "Medio de Pago")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string MedioPago { get; set; } = "01";

    // ========================================
    // MONEDA
    // ========================================

    [Display(Name = "Moneda")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Moneda { get; set; } = "CRC";

    /// <summary>
    /// Tipo de cambio (obligatorio si Moneda != CRC)
    /// </summary>
    [Display(Name = "Tipo de Cambio")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? TipoCambio { get; set; }

    // ========================================
    // TOTALES
    // ========================================

    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal Subtotal { get; set; }

    [Display(Name = "Total Descuentos")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalDescuentos { get; set; }

    [Display(Name = "Total Impuestos")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalImpuestos { get; set; }

    [Display(Name = "Total")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal Total { get; set; }

    // ========================================
    // INFORMACIÓN ADICIONAL
    // ========================================

    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // ========================================
    // APROBACIÓN
    // ========================================

    [Display(Name = "Usuario Aprobación")]
    public string? UsuarioAprobacionId { get; set; }

    // ========================================
    // CONVERSIÓN A FACTURA
    // ========================================

    [Display(Name = "Documento Generado")]
    public Guid? DocumentoGeneradoId { get; set; }

    // ========================================
    // AUDIT TRAIL
    // ========================================

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // ========================================
    // SOFT DELETE
    // ========================================

    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================

    public Empresa? Empresa { get; set; }
    public Cliente? Cliente { get; set; }
    public Documento? DocumentoGenerado { get; set; }
    public User? UsuarioAprobacion { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<PedidoVentaDetalle> Detalles { get; set; } = new List<PedidoVentaDetalle>();
}
