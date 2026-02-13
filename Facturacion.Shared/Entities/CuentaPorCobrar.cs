using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Cuenta por cobrar generada por un documento de venta a crédito
/// Registra el saldo pendiente y los abonos aplicados
/// </summary>
public class CuentaPorCobrar
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIONES CON ARQUITECTURA MULTI-TENANT
    // ========================================

    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    // ========================================
    // DOCUMENTO ORIGEN
    // ========================================

    /// <summary>
    /// Documento que generó esta cuenta por cobrar (Factura o Tiquete a crédito)
    /// </summary>
    [Display(Name = "Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoId { get; set; }

    // ========================================
    // CLIENTE
    // ========================================

    [Display(Name = "Cliente")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ClienteId { get; set; }

    // ========================================
    // MONTOS Y SALDO
    // ========================================

    /// <summary>
    /// Monto original del documento (total de la factura/tiquete)
    /// </summary>
    [Display(Name = "Monto Original")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoOriginal { get; set; }

    /// <summary>
    /// Saldo pendiente de pago (se reduce con cada abono)
    /// </summary>
    [Display(Name = "Monto Saldo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoSaldo { get; set; }

    /// <summary>
    /// Moneda de la cuenta por cobrar (debe coincidir con el documento)
    /// </summary>
    [Display(Name = "Moneda")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoMoneda Moneda { get; set; }

    /// <summary>
    /// Tipo de cambio aplicado al momento de creación (si no es CRC)
    /// </summary>
    [Display(Name = "Tipo de Cambio")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? TipoCambio { get; set; }

    // ========================================
    // FECHAS
    // ========================================

    /// <summary>
    /// Fecha de emisión del documento (para cálculo de antigüedad)
    /// </summary>
    [Display(Name = "Fecha Emisión")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Fecha de vencimiento del pago
    /// </summary>
    [Display(Name = "Fecha Vencimiento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaVencimiento { get; set; }

    /// <summary>
    /// Fecha del último pago aplicado
    /// </summary>
    [Display(Name = "Fecha Último Pago")]
    public DateTime? FechaUltimoPago { get; set; }

    // ========================================
    // ESTADO
    // ========================================

    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public EstadoCuentaPorCobrar Estado { get; set; }

    /// <summary>
    /// Motivo del castigo de baja (si aplica)
    /// </summary>
    [Display(Name = "Motivo Castigo Baja")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MotivoCastigoBaja { get; set; }

    // ========================================
    // INFORMACIÓN ADICIONAL
    // ========================================

    /// <summary>
    /// Número de consecutivo del documento (para referencia)
    /// </summary>
    [Display(Name = "Número Consecutivo")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroConsecutivo { get; set; }

    /// <summary>
    /// Nombre del cliente (duplicado para histórico y reportes)
    /// </summary>
    [Display(Name = "Nombre Cliente")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreCliente { get; set; }

    /// <summary>
    /// Días de crédito otorgados
    /// </summary>
    [Display(Name = "Días Crédito")]
    public int DiasCredito { get; set; }

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

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
    public Documento? Documento { get; set; }
    public Cliente? Cliente { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    /// <summary>
    /// Lista de abonos/pagos aplicados a esta cuenta
    /// </summary>
    public ICollection<AbonoCobranza> Abonos { get; set; } = new List<AbonoCobranza>();
}
