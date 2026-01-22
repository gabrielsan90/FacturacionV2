using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Otro cargo del documento (v4.4)
/// Permite agregar cargos adicionales al documento como timbres, transporte, etc.
/// Según Hacienda v4.4
/// </summary>
public class DocumentoOtroCargo
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
    // DATOS DEL CARGO
    // ========================================

    /// <summary>
    /// Tipo de documento del cargo según catálogo Hacienda
    /// Ejemplos:
    /// 01-Gastos de transporte (flete)
    /// 02-Gastos de exportación
    /// 03-Gastos de cobranza
    /// 04-Descuento por pronto pago
    /// 05-Interés por mora
    /// 06-Gastos administrativos
    /// 07-Gastos de instalación
    /// 08-Gastos de embalaje
    /// 09-Impuestos no incluidos en líneas (timbres, etc.)
    /// 10-Otros
    /// 99-Otros
    /// </summary>
    [Display(Name = "Tipo de Documento")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string TipoDocumento { get; set; } = null!;

    /// <summary>
    /// Tipo de documento otro - Obligatorio cuando TipoDocumento = 99
    /// Descripción del tipo de documento cuando se usa código 99 (Otros)
    /// </summary>
    [Display(Name = "Tipo Documento Otro")]
    [MaxLength(160, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TipoDocumentoOtro { get; set; }

    /// <summary>
    /// Detalle o descripción del cargo
    /// </summary>
    [Display(Name = "Detalle")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Detalle { get; set; }

    /// <summary>
    /// Monto del cargo
    /// </summary>
    [Display(Name = "Monto")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Monto { get; set; }

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
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
