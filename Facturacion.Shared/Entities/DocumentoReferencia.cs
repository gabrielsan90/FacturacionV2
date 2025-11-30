using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Referencias a otros documentos electrónicos
/// Obligatorio para Notas de Débito (02) y Notas de Crédito (03)
/// Según Hacienda v4.4
/// </summary>
public class DocumentoReferencia
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
    // DOCUMENTO REFERENCIADO
    // ========================================

    /// <summary>
    /// Tipo de documento referenciado (01-FE, 02-ND, 03-NC, 04-TE, 08-FEC, 09-FEE)
    /// </summary>
    [Display(Name = "Tipo de Documento Referenciado")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string TipoDocumentoReferenciado { get; set; } = null!;

    /// <summary>
    /// Número del documento referenciado (formato: XXX-YYYYY-ZZ-AAAAAAAAAA)
    /// </summary>
    [Display(Name = "Número Documento Referenciado")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NumeroDocumentoReferenciado { get; set; } = null!;

    /// <summary>
    /// Fecha de emisión del documento referenciado
    /// </summary>
    [Display(Name = "Fecha Emisión Documento Referenciado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaEmisionDocumentoReferenciado { get; set; }

    /// <summary>
    /// Clave numérica de 50 dígitos del documento referenciado
    /// Opcional pero recomendado para validación
    /// </summary>
    [Display(Name = "Clave Documento Referenciado")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    [MinLength(50, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    public string? ClaveDocumentoReferenciado { get; set; }

    // ========================================
    // TIPO DE REFERENCIA
    // ========================================

    /// <summary>
    /// Código de tipo de referencia según catálogo Hacienda
    /// 01-Anula Documento de Referencia
    /// 02-Corrige el texto del Documento de Referencia
    /// 03-Corrige el monto del Documento de Referencia
    /// 04-Referencia a otro documento
    /// 05-Sustituye comprobante provisional por contingencia
    /// 99-Otros
    /// </summary>
    [Display(Name = "Código de Referencia")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoReferenciaDocumento CodigoReferencia { get; set; }

    /// <summary>
    /// Razón o motivo de la referencia (texto descriptivo)
    /// Ejemplo: "Anulación por error en precio", "Descuento posterior a la venta"
    /// </summary>
    [Display(Name = "Razón de la Referencia")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string RazonReferencia { get; set; } = null!;

    // ========================================
    // RELACIÓN OPCIONAL CON DOCUMENTO EN LA BASE DE DATOS
    // ========================================

    /// <summary>
    /// Relación con el documento referenciado si existe en la base de datos
    /// Opcional: puede referenciar documentos externos o antiguos no en el sistema
    /// </summary>
    [Display(Name = "Documento Referenciado (Base de Datos)")]
    public Guid? DocumentoReferenciadoId { get; set; }

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
    public Documento? DocumentoReferenciado { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
