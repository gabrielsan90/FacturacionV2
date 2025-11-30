using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Medios de pago utilizados en un documento
/// Permite registrar pagos mixtos (efectivo + tarjeta, etc.)
/// Según Hacienda v4.4
/// </summary>
public class DocumentoMedioPago
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON DOCUMENTO
    // ========================================

    [Display(Name = "Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoId { get; set; }

    // ========================================
    // MEDIO DE PAGO
    // ========================================

    /// <summary>
    /// Código del medio de pago según catálogo Hacienda
    /// 01-Efectivo, 02-Tarjeta, 03-Cheque, 04-Transferencia - depósito bancario,
    /// 05-Recaudado por terceros, 06-SINPE Móvil (NUEVO v4.4), 99-Otros
    /// </summary>
    [Display(Name = "Código Medio de Pago")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string CodigoMedioPago { get; set; } = null!;

    /// <summary>
    /// Relación con el catálogo de medios de pago de Hacienda (opcional)
    /// </summary>
    [Display(Name = "Medio de Pago")]
    public int? MedioPagoId { get; set; }

    // ========================================
    // MONTO
    // ========================================

    /// <summary>
    /// Monto pagado con este medio de pago (hasta 5 decimales)
    /// </summary>
    [Display(Name = "Monto")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Monto { get; set; }

    // ========================================
    // INFORMACIÓN ADICIONAL
    // ========================================

    /// <summary>
    /// Descripción o detalle adicional del medio de pago
    /// Ejemplo: "Tarjeta Visa terminada en 4532", "Transferencia SINPE"
    /// </summary>
    [Display(Name = "Descripción")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Número de referencia del pago (número de transacción, cheque, etc.)
    /// </summary>
    [Display(Name = "Número de Referencia")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroReferencia { get; set; }

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

    public Documento? Documento { get; set; }
    public Catalogos.MedioPago? MedioPago { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
