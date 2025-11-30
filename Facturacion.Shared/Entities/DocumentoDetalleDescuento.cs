using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Descuentos aplicados a una línea de detalle del documento
/// Una línea puede tener múltiples descuentos
/// Según Hacienda v4.4
/// </summary>
public class DocumentoDetalleDescuento
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON DETALLE
    // ========================================

    [Display(Name = "Detalle del Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoDetalleId { get; set; }

    // ========================================
    // NATURALEZA DEL DESCUENTO
    // ========================================

    /// <summary>
    /// Descripción o motivo del descuento
    /// Ejemplo: "Descuento por volumen", "Promoción Black Friday", etc.
    /// </summary>
    [Display(Name = "Naturaleza del Descuento")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NaturalezaDescuento { get; set; } = null!;

    // ========================================
    // MONTO DEL DESCUENTO
    // ========================================

    /// <summary>
    /// Monto del descuento aplicado (hasta 5 decimales)
    /// </summary>
    [Display(Name = "Monto Descuento")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal MontoDescuento { get; set; }

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

    public DocumentoDetalle? DocumentoDetalle { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
