using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Mensajes del receptor de documentos electrónicos
/// Tipos: 05-Mensaje Receptor Aceptación, 06-Mensaje Receptor Aceptación Parcial, 07-Mensaje Receptor Rechazo
/// El receptor (cliente) puede aceptar, aceptar parcialmente o rechazar un documento recibido
/// Según Hacienda v4.4
/// </summary>
public class DocumentoReceptorMensaje
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIÓN CON DOCUMENTO ORIGINAL
    // ========================================

    /// <summary>
    /// Documento original que se está aceptando/rechazando
    /// Este es el documento recibido (FE, ND, NC, TE, etc.)
    /// </summary>
    [Display(Name = "Documento Original")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoOriginalId { get; set; }

    // ========================================
    // CLAVE DEL MENSAJE
    // ========================================

    /// <summary>
    /// Clave numérica de 50 dígitos del mensaje de respuesta
    /// Similar a la clave del documento, pero con tipo 05, 06 o 07
    /// </summary>
    [Display(Name = "Clave del Mensaje")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    [MinLength(50, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string ClaveMensaje { get; set; } = null!;

    /// <summary>
    /// Número consecutivo del mensaje (20 dígitos)
    /// Formato: XXX-YYYYY-ZZ-AAAAAAAAAA (donde ZZ = 05, 06 o 07)
    /// </summary>
    [Display(Name = "Número Consecutivo")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NumeroConsecutivo { get; set; } = null!;

    // ========================================
    // TIPO DE MENSAJE
    // ========================================

    /// <summary>
    /// Tipo de mensaje del receptor
    /// 1 = Aceptado (tipo 05)
    /// 2 = Aceptado Parcialmente (tipo 06)
    /// 3 = Rechazado (tipo 07)
    /// </summary>
    [Display(Name = "Tipo de Mensaje")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int TipoMensaje { get; set; }

    // ========================================
    // INFORMACIÓN DEL MENSAJE
    // ========================================

    /// <summary>
    /// Fecha de emisión del mensaje de respuesta
    /// </summary>
    [Display(Name = "Fecha de Emisión")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Código del mensaje según catálogo Hacienda
    /// Para aceptación: 01-Aceptado
    /// Para aceptación parcial: 02-Aceptado parcialmente
    /// Para rechazo: 03-Rechazado
    /// </summary>
    [Display(Name = "Código del Mensaje")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string CodigoMensaje { get; set; } = null!;

    /// <summary>
    /// Detalle o razón del mensaje
    /// Obligatorio para rechazo y aceptación parcial
    /// Ejemplo: "Producto no solicitado", "Precio incorrecto", "Cantidad errónea"
    /// </summary>
    [Display(Name = "Detalle del Mensaje")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? DetalleMensaje { get; set; }

    // ========================================
    // IMPUESTO TOTAL (PARA ACEPTACIÓN PARCIAL)
    // ========================================

    /// <summary>
    /// Monto total del impuesto aceptado (solo para aceptación parcial tipo 06)
    /// Indica cuánto del impuesto original se acepta
    /// </summary>
    [Display(Name = "Monto Total Impuesto Aceptado")]
    public decimal? MontoTotalImpuestoAceptado { get; set; }

    /// <summary>
    /// Monto total aceptado (solo para aceptación parcial tipo 06)
    /// </summary>
    [Display(Name = "Monto Total Aceptado")]
    public decimal? MontoTotalAceptado { get; set; }

    // ========================================
    // XML DEL MENSAJE
    // ========================================

    /// <summary>
    /// XML generado del mensaje (sin firma)
    /// </summary>
    [Display(Name = "XML Generado")]
    public string? XmlGenerado { get; set; }

    /// <summary>
    /// XML firmado digitalmente del mensaje (XAdES-BES)
    /// </summary>
    [Display(Name = "XML Firmado")]
    public string? XmlFirmado { get; set; }

    /// <summary>
    /// Fecha de firma digital del mensaje
    /// </summary>
    [Display(Name = "Fecha de Firma")]
    public DateTime? FechaFirma { get; set; }

    // ========================================
    // COMUNICACIÓN CON HACIENDA
    // ========================================

    /// <summary>
    /// Fecha de envío del mensaje a Hacienda
    /// </summary>
    [Display(Name = "Fecha de Envío a Hacienda")]
    public DateTime? FechaEnvioHacienda { get; set; }

    /// <summary>
    /// Fecha de respuesta de Hacienda al mensaje
    /// </summary>
    [Display(Name = "Fecha de Respuesta Hacienda")]
    public DateTime? FechaRespuestaHacienda { get; set; }

    /// <summary>
    /// Mensaje de respuesta de Hacienda
    /// </summary>
    [Display(Name = "Mensaje Hacienda")]
    public string? MensajeHacienda { get; set; }

    /// <summary>
    /// Estado del mensaje (Pendiente, Aceptado, Rechazado)
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Estado { get; set; }

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

    public Documento? DocumentoOriginal { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
