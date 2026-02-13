using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Define los niveles de aprobación para cada tipo de workflow.
/// Cada nivel tiene un orden y puede tener límites de monto.
/// </summary>
public class NivelAprobacion
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
    // Tipo de Workflow
    // =====================================================
    [Display(Name = "Tipo de Workflow")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid TipoWorkflowId { get; set; }

    // =====================================================
    // Configuración del Nivel
    // =====================================================
    /// <summary>
    /// Orden del nivel en la cadena de aprobación (1, 2, 3...)
    /// </summary>
    [Display(Name = "Orden")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int Orden { get; set; }

    [Display(Name = "Nombre del Nivel")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(300, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Tipo de Aprobador
    // =====================================================
    /// <summary>
    /// Tipo de aprobador: USU=Usuario específico, ROL=Rol, DEP=Departamento, JEF=Jefe Inmediato
    /// </summary>
    [Display(Name = "Tipo de Aprobador")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoAprobador { get; set; } = "USU";

    /// <summary>
    /// ID del usuario aprobador (cuando TipoAprobador = USU)
    /// </summary>
    [Display(Name = "Usuario Aprobador")]
    public string? UsuarioAprobadorId { get; set; }

    /// <summary>
    /// Nombre del rol aprobador (cuando TipoAprobador = ROL)
    /// </summary>
    [Display(Name = "Rol Aprobador")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? RolAprobador { get; set; }

    /// <summary>
    /// ID del departamento aprobador (cuando TipoAprobador = DEP)
    /// </summary>
    [Display(Name = "Departamento Aprobador")]
    public Guid? DepartamentoAprobadorId { get; set; }

    // =====================================================
    // Límites de Monto
    // =====================================================
    /// <summary>
    /// Monto mínimo que puede aprobar este nivel
    /// </summary>
    [Display(Name = "Monto Mínimo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoMinimo { get; set; }

    /// <summary>
    /// Monto máximo que puede aprobar este nivel
    /// </summary>
    [Display(Name = "Monto Máximo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoMaximo { get; set; }

    // =====================================================
    // Configuración de Aprobación
    // =====================================================
    /// <summary>
    /// Si se requieren todos los aprobadores del nivel o solo uno
    /// </summary>
    [Display(Name = "Requiere Todos los Aprobadores")]
    public bool RequiereTodosAprobadores { get; set; }

    /// <summary>
    /// Cantidad mínima de aprobadores requeridos
    /// </summary>
    [Display(Name = "Aprobadores Mínimos")]
    public int AprobadoresMinimos { get; set; } = 1;

    /// <summary>
    /// Si puede rechazar o solo aprobar/escalar
    /// </summary>
    [Display(Name = "Puede Rechazar")]
    public bool PuedeRechazar { get; set; } = true;

    /// <summary>
    /// Si puede devolver al nivel anterior
    /// </summary>
    [Display(Name = "Puede Devolver")]
    public bool PuedeDevolver { get; set; } = true;

    // =====================================================
    // Escalamiento
    // =====================================================
    /// <summary>
    /// Días límite para responder antes de escalar
    /// </summary>
    [Display(Name = "Días Límite")]
    public int? DiasLimite { get; set; }

    /// <summary>
    /// ID del nivel al que escala si no se responde a tiempo
    /// </summary>
    [Display(Name = "Nivel Escalamiento")]
    public Guid? NivelEscalamientoId { get; set; }

    // =====================================================
    // Notificaciones
    // =====================================================
    [Display(Name = "Notificar por Email")]
    public bool NotificarEmail { get; set; } = true;

    [Display(Name = "Notificar en Sistema")]
    public bool NotificarSistema { get; set; } = true;

    /// <summary>
    /// Días antes del vencimiento para enviar recordatorio
    /// </summary>
    [Display(Name = "Días para Recordatorio")]
    public int? DiasRecordatorio { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Creado Por")]
    public string? CreadoPorId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Modificado Por")]
    public string? ModificadoPorId { get; set; }

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
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public string TipoAprobadorDescripcion => TipoAprobador switch
    {
        "USU" => "Usuario Específico",
        "ROL" => "Rol",
        "DEP" => "Departamento",
        "JEF" => "Jefe Inmediato",
        _ => TipoAprobador
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public TipoWorkflow? TipoWorkflow { get; set; }
    public User? UsuarioAprobador { get; set; }
    public Departamento? DepartamentoAprobador { get; set; }
    public NivelAprobacion? NivelEscalamiento { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<SolicitudAprobacion>? Solicitudes { get; set; }
    public ICollection<AccionAprobacion>? Acciones { get; set; }
}

/// <summary>
/// Constantes para tipos de aprobador
/// </summary>
public static class TiposAprobador
{
    public const string Usuario = "USU";
    public const string Rol = "ROL";
    public const string Departamento = "DEP";
    public const string JefeInmediato = "JEF";
}
