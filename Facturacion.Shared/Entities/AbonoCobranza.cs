using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Abono o pago parcial aplicado a una cuenta por cobrar
/// Registra cada pago recibido del cliente
/// </summary>
public class AbonoCobranza
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON CUENTA POR COBRAR
    // ========================================

    [Display(Name = "Cuenta Por Cobrar")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CuentaPorCobrarId { get; set; }

    // ========================================
    // INFORMACIÓN DEL PAGO
    // ========================================

    /// <summary>
    /// Monto del pago aplicado
    /// </summary>
    [Display(Name = "Monto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal Monto { get; set; }

    /// <summary>
    /// Fecha en que se recibió el pago
    /// </summary>
    [Display(Name = "Fecha de Pago")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaPago { get; set; }

    /// <summary>
    /// Método de pago utilizado
    /// 01-Efectivo, 02-Tarjeta, 03-Cheque, 04-Transferencia, 05-Recaudado por terceros, 06-SINPE Móvil, 99-Otros
    /// </summary>
    [Display(Name = "Método de Pago")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string MetodoPago { get; set; } = null!;

    /// <summary>
    /// Número de referencia del pago (número de cheque, transacción, autorización tarjeta, etc.)
    /// </summary>
    [Display(Name = "Número de Referencia")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroReferencia { get; set; }

    /// <summary>
    /// Tipo de cambio aplicado al momento del pago (si aplica)
    /// </summary>
    [Display(Name = "Tipo de Cambio")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? TipoCambio { get; set; }

    /// <summary>
    /// Moneda del pago (normalmente debe coincidir con la cuenta por cobrar)
    /// </summary>
    [Display(Name = "Moneda")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoMoneda Moneda { get; set; }

    // ========================================
    // INFORMACIÓN ADICIONAL
    // ========================================

    /// <summary>
    /// Notas u observaciones sobre el pago
    /// </summary>
    [Display(Name = "Notas")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Notas { get; set; }

    /// <summary>
    /// Relación con Recibo Electrónico de Pago (REP) - si se generó
    /// </summary>
    [Display(Name = "Recibo de Pago")]
    public Guid? ReciboPagoId { get; set; }

    /// <summary>
    /// Relación con MovimientoBancario (si el pago fue conciliado)
    /// </summary>
    [Display(Name = "Movimiento Bancario")]
    public Guid? MovimientoBancarioId { get; set; }

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

    public CuentaPorCobrar? CuentaPorCobrar { get; set; }
    public ReciboPago? ReciboPago { get; set; }
    public MovimientoBancario? MovimientoBancario { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
