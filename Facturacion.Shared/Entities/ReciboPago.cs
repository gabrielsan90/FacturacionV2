using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Recibo Electrónico de Pago (REP) - Nuevo en v4.4
/// Registra pagos recibidos sobre ventas a crédito
/// Resolución MH-DGT-RES-0027-2024
/// </summary>
public class ReciboPago
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON DOCUMENTO REP
    // ========================================

    /// <summary>
    /// Relación con el documento REP (tipo 10)
    /// </summary>
    [Display(Name = "Documento REP")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoId { get; set; }

    // ========================================
    // DOCUMENTO ORIGINAL A CRÉDITO
    // ========================================

    /// <summary>
    /// Documento original que se está pagando (FE, TE a crédito)
    /// </summary>
    [Display(Name = "Documento Original")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoOriginalId { get; set; }

    /// <summary>
    /// Clave del documento original (50 dígitos)
    /// </summary>
    [Display(Name = "Clave Documento Original")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    [MinLength(50, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    public string ClaveDocumentoOriginal { get; set; } = null!;

    /// <summary>
    /// Número consecutivo del documento original
    /// </summary>
    [Display(Name = "Número Consecutivo Original")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string NumeroConsecutivoOriginal { get; set; } = null!;

    /// <summary>
    /// Tipo de documento original (FE=01, TE=04, etc.)
    /// </summary>
    [Display(Name = "Tipo Documento Original")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoDocumentoOriginal { get; set; } = null!;

    // ========================================
    // INFORMACIÓN DEL PAGO
    // ========================================

    /// <summary>
    /// Fecha en que se recibió el pago
    /// </summary>
    [Display(Name = "Fecha de Pago")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaPago { get; set; }

    /// <summary>
    /// Monto pagado en este recibo (con 5 decimales)
    /// </summary>
    [Display(Name = "Monto Pagado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoPagado { get; set; }

    /// <summary>
    /// Saldo pendiente del documento después de este pago
    /// </summary>
    [Display(Name = "Saldo Pendiente")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal SaldoPendiente { get; set; }

    /// <summary>
    /// Moneda del pago (debe coincidir con documento original)
    /// </summary>
    [Display(Name = "Moneda")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoMoneda Moneda { get; set; }

    /// <summary>
    /// Tipo de cambio aplicado (si no es CRC)
    /// </summary>
    [Display(Name = "Tipo de Cambio")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? TipoCambio { get; set; }

    // ========================================
    // OBSERVACIONES
    // ========================================

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
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

    public Documento? Documento { get; set; }  // REP document
    public Documento? DocumentoOriginal { get; set; }  // Original credit document
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
