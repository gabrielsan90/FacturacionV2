using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Expediente digital para almacenar documentos de empleados.
/// </summary>
public class ExpedienteDigital
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relación con empleado
    // =====================================================
    [Display(Name = "Empleado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpleadoId { get; set; }

    // =====================================================
    // Datos del documento
    // =====================================================
    [Display(Name = "Nombre del Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string NombreDocumento { get; set; } = null!;

    /// <summary>
    /// Tipo: CED=Cédula, TIT=Título, CON=Contrato, CER=Certificado, LIC=Licencia, OTR=Otro
    /// </summary>
    [Display(Name = "Tipo de Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoDocumento { get; set; } = "OTR";

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Datos del archivo
    // =====================================================
    [Display(Name = "Ruta del Archivo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string RutaArchivo { get; set; } = null!;

    [Display(Name = "Nombre Original")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreArchivoOriginal { get; set; }

    [Display(Name = "Extensión")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Extension { get; set; }

    [Display(Name = "Tamaño (bytes)")]
    public long TamanoBytes { get; set; }

    [Display(Name = "Tipo MIME")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TipoMime { get; set; }

    // =====================================================
    // Datos del documento físico
    // =====================================================
    [Display(Name = "Fecha de Emisión")]
    public DateTime? FechaEmision { get; set; }

    [Display(Name = "Fecha de Vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    [Display(Name = "Número de Documento")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroDocumento { get; set; }

    [Display(Name = "Entidad Emisora")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? EntidadEmisora { get; set; }

    // =====================================================
    // Verificación
    // =====================================================
    [Display(Name = "Verificado")]
    public bool Verificado { get; set; }

    [Display(Name = "Fecha de Verificación")]
    public DateTime? FechaVerificacion { get; set; }

    [Display(Name = "Verificado Por")]
    public string? VerificadoPorId { get; set; }

    [Display(Name = "Es Confidencial")]
    public bool EsConfidencial { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empleado? Empleado { get; set; }
    public User? VerificadoPor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
}
