using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Actividad CRM para seguimiento de clientes y oportunidades.
/// Incluye llamadas, reuniones, emails, tareas, visitas y demostraciones.
/// </summary>
public class ActividadCRM
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
    // Tipo y datos principales
    // =====================================================
    /// <summary>
    /// Tipo de actividad:
    /// LLA=Llamada, REU=Reunión, EMA=Email, TAR=Tarea, VIS=Visita, DEM=Demostración, NOT=Nota
    /// </summary>
    [Display(Name = "Tipo de Actividad")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoActividad { get; set; } = null!;

    [Display(Name = "Asunto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Asunto { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(2000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Relaciones
    // =====================================================
    [Display(Name = "Oportunidad")]
    public Guid? OportunidadId { get; set; }

    [Display(Name = "Cliente")]
    public Guid? ClienteId { get; set; }

    // =====================================================
    // Programación
    // =====================================================
    [Display(Name = "Fecha Programada")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaProgramada { get; set; }

    [Display(Name = "Hora Programada")]
    public TimeSpan? HoraProgramada { get; set; }

    [Display(Name = "Duración (minutos)")]
    public int DuracionMinutos { get; set; } = 30;

    [Display(Name = "Fecha Realizada")]
    public DateTime? FechaRealizada { get; set; }

    [Display(Name = "Asignado A")]
    public string? AsignadoAId { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: PEN=Pendiente, PRO=En Progreso, COM=Completada, CAN=Cancelada
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "PEN";

    /// <summary>
    /// Prioridad: ALT=Alta, MED=Media, BAJ=Baja
    /// </summary>
    [Display(Name = "Prioridad")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Prioridad { get; set; } = "MED";

    [Display(Name = "Resultado")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Resultado { get; set; }

    // =====================================================
    // Recordatorio
    // =====================================================
    [Display(Name = "Recordatorio")]
    public bool Recordatorio { get; set; }

    [Display(Name = "Minutos Antes")]
    public int? MinutosAntes { get; set; }

    [Display(Name = "Reminder Enviado")]
    public bool ReminderEnviado { get; set; }

    [Display(Name = "Ubicación")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Ubicacion { get; set; }

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
    // Soft Delete
    // =====================================================
    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public bool EstaVencida => Estado == "PEN" && FechaProgramada < DateTime.Now;

    [NotMapped]
    public bool EsHoy => FechaProgramada.Date == DateTime.Today;

    [NotMapped]
    public string TipoActividadDescripcion => TipoActividad switch
    {
        "LLA" => "Llamada",
        "REU" => "Reunión",
        "EMA" => "Email",
        "TAR" => "Tarea",
        "VIS" => "Visita",
        "DEM" => "Demostración",
        "NOT" => "Nota",
        _ => TipoActividad
    };

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "PEN" => "Pendiente",
        "PRO" => "En Progreso",
        "COM" => "Completada",
        "CAN" => "Cancelada",
        _ => Estado
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Oportunidad? Oportunidad { get; set; }
    public Cliente? Cliente { get; set; }
    public User? AsignadoA { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
