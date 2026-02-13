using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Registra cada acción tomada en el proceso de aprobación.
/// Historial completo de aprobaciones, rechazos y devoluciones.
/// </summary>
public class AccionAprobacion
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Multi-Tenancy
    // =====================================================
    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    // =====================================================
    // Solicitud
    // =====================================================
    [Display(Name = "Solicitud de Aprobación")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SolicitudAprobacionId { get; set; }

    // =====================================================
    // Nivel de Aprobación
    // =====================================================
    [Display(Name = "Nivel de Aprobación")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid NivelAprobacionId { get; set; }

    /// <summary>
    /// Número del nivel en el momento de la acción
    /// </summary>
    [Display(Name = "Número de Nivel")]
    public int NumeroNivel { get; set; }

    // =====================================================
    // Acción
    // =====================================================
    /// <summary>
    /// Tipo de acción: APR=Aprobar, REC=Rechazar, DEV=Devolver, ESC=Escalar, COM=Comentar, REA=Reasignar
    /// </summary>
    [Display(Name = "Tipo de Acción")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoAccion { get; set; } = null!;

    [Display(Name = "Fecha de Acción")]
    public DateTime FechaAccion { get; set; }

    // =====================================================
    // Usuario que Ejecuta
    // =====================================================
    [Display(Name = "Usuario")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string UsuarioId { get; set; } = null!;

    /// <summary>
    /// Indica si actuó en nombre de otro (delegación)
    /// </summary>
    [Display(Name = "Es Delegado")]
    public bool EsDelegado { get; set; }

    /// <summary>
    /// Usuario original si hubo delegación
    /// </summary>
    [Display(Name = "Usuario Original")]
    public string? UsuarioOriginalId { get; set; }

    // =====================================================
    // Comentarios y Justificación
    // =====================================================
    [Display(Name = "Comentario")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Comentario { get; set; }

    /// <summary>
    /// Motivo del rechazo o devolución (obligatorio en esos casos)
    /// </summary>
    [Display(Name = "Motivo")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Motivo { get; set; }

    // =====================================================
    // Estado Resultante
    // =====================================================
    /// <summary>
    /// Estado de la solicitud antes de la acción
    /// </summary>
    [Display(Name = "Estado Anterior")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? EstadoAnterior { get; set; }

    /// <summary>
    /// Estado de la solicitud después de la acción
    /// </summary>
    [Display(Name = "Estado Resultante")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? EstadoResultante { get; set; }

    /// <summary>
    /// Nivel al que pasó después de la acción (si aplica)
    /// </summary>
    [Display(Name = "Nivel Siguiente")]
    public Guid? NivelSiguienteId { get; set; }

    // =====================================================
    // Información Adicional
    // =====================================================
    /// <summary>
    /// Si la acción fue automática (por escalamiento o reglas)
    /// </summary>
    [Display(Name = "Acción Automática")]
    public bool AccionAutomatica { get; set; }

    /// <summary>
    /// Regla que disparó la acción automática
    /// </summary>
    [Display(Name = "Regla Aplicada")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ReglaAplicada { get; set; }

    /// <summary>
    /// IP desde donde se realizó la acción
    /// </summary>
    [Display(Name = "Dirección IP")]
    [MaxLength(45, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? DireccionIP { get; set; }

    /// <summary>
    /// Información del dispositivo/navegador
    /// </summary>
    [Display(Name = "User Agent")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? UserAgent { get; set; }

    // =====================================================
    // Reasignación
    // =====================================================
    /// <summary>
    /// Usuario al que se reasigna (cuando TipoAccion = REA)
    /// </summary>
    [Display(Name = "Reasignado A")]
    public string? ReasignadoAId { get; set; }

    /// <summary>
    /// Motivo de la reasignación
    /// </summary>
    [Display(Name = "Motivo de Reasignación")]
    [MaxLength(300, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MotivoReasignacion { get; set; }

    // =====================================================
    // Tiempos
    // =====================================================
    /// <summary>
    /// Tiempo que tomó tomar la decisión desde que recibió la solicitud
    /// </summary>
    [Display(Name = "Tiempo de Respuesta (horas)")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal? TiempoRespuestaHoras { get; set; }

    /// <summary>
    /// Si la acción fue dentro del tiempo límite
    /// </summary>
    [Display(Name = "Dentro del Límite")]
    public bool DentroDelLimite { get; set; } = true;

    // =====================================================
    // Notificaciones
    // =====================================================
    [Display(Name = "Notificación Enviada")]
    public bool NotificacionEnviada { get; set; }

    [Display(Name = "Fecha de Notificación")]
    public DateTime? FechaNotificacion { get; set; }

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    // =====================================================
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public string TipoAccionDescripcion => TipoAccion switch
    {
        "APR" => "Aprobado",
        "REC" => "Rechazado",
        "DEV" => "Devuelto",
        "ESC" => "Escalado",
        "COM" => "Comentario",
        "REA" => "Reasignado",
        _ => TipoAccion
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public SolicitudAprobacion? SolicitudAprobacion { get; set; }
    public NivelAprobacion? NivelAprobacion { get; set; }
    public NivelAprobacion? NivelSiguiente { get; set; }
    public User? Usuario { get; set; }
    public User? UsuarioOriginal { get; set; }
    public User? ReasignadoA { get; set; }
}

/// <summary>
/// Tipos de acción en el workflow
/// </summary>
public static class TiposAccionAprobacion
{
    public const string Aprobar = "APR";
    public const string Rechazar = "REC";
    public const string Devolver = "DEV";
    public const string Escalar = "ESC";
    public const string Comentar = "COM";
    public const string Reasignar = "REA";
}
