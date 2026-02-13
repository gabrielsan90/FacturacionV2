using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Solicitud de aprobación para un documento específico.
/// Rastrea el estado de aprobación a través de los niveles del workflow.
/// </summary>
public class SolicitudAprobacion
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
    // Documento Origen
    // =====================================================
    /// <summary>
    /// Módulo de origen: OC, REQ, ASI, PLA, FAC, NC, ND, VAC, GAS
    /// </summary>
    [Display(Name = "Módulo Origen")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string ModuloOrigen { get; set; } = null!;

    /// <summary>
    /// Tipo de documento: ORDEN_COMPRA, REQUISICION, ASIENTO, PLANILLA, etc.
    /// </summary>
    [Display(Name = "Tipo de Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoDocumento { get; set; } = null!;

    /// <summary>
    /// ID del documento a aprobar (Guid del registro)
    /// </summary>
    [Display(Name = "ID del Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DocumentoId { get; set; }

    /// <summary>
    /// Número del documento para referencia
    /// </summary>
    [Display(Name = "Número de Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string NumeroDocumento { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Monto
    // =====================================================
    [Display(Name = "Monto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [Display(Name = "Moneda")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Moneda { get; set; } = "CRC";

    [Display(Name = "Monto en Moneda Base")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoMonedaBase { get; set; }

    // =====================================================
    // Estado de la Solicitud
    // =====================================================
    /// <summary>
    /// Estado: PEN=Pendiente, APR=Aprobado, REC=Rechazado, DEV=Devuelto, CAN=Cancelado
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "PEN";

    /// <summary>
    /// Nivel actual de aprobación
    /// </summary>
    [Display(Name = "Nivel Actual")]
    public Guid? NivelActualId { get; set; }

    /// <summary>
    /// Número del nivel actual (1, 2, 3...)
    /// </summary>
    [Display(Name = "Número de Nivel")]
    public int NumeroNivel { get; set; } = 1;

    /// <summary>
    /// Total de niveles requeridos
    /// </summary>
    [Display(Name = "Total de Niveles")]
    public int TotalNiveles { get; set; }

    // =====================================================
    // Fechas Importantes
    // =====================================================
    [Display(Name = "Fecha de Solicitud")]
    public DateTime FechaSolicitud { get; set; }

    [Display(Name = "Fecha Límite")]
    public DateTime? FechaLimite { get; set; }

    [Display(Name = "Fecha de Resolución")]
    public DateTime? FechaResolucion { get; set; }

    [Display(Name = "Fecha de Última Acción")]
    public DateTime? FechaUltimaAccion { get; set; }

    // =====================================================
    // Solicitante
    // =====================================================
    [Display(Name = "Solicitante")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string SolicitanteId { get; set; } = null!;

    [Display(Name = "Departamento Solicitante")]
    public Guid? DepartamentoSolicitanteId { get; set; }

    // =====================================================
    // Prioridad y Urgencia
    // =====================================================
    /// <summary>
    /// Prioridad: BAJ=Baja, NOR=Normal, ALT=Alta, URG=Urgente
    /// </summary>
    [Display(Name = "Prioridad")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Prioridad { get; set; } = "NOR";

    [Display(Name = "Es Urgente")]
    public bool EsUrgente { get; set; }

    [Display(Name = "Motivo de Urgencia")]
    [MaxLength(300, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MotivoUrgencia { get; set; }

    // =====================================================
    // Escalamiento
    // =====================================================
    [Display(Name = "Fue Escalada")]
    public bool FueEscalada { get; set; }

    [Display(Name = "Fecha de Escalamiento")]
    public DateTime? FechaEscalamiento { get; set; }

    [Display(Name = "Veces Escalada")]
    public int VecesEscalada { get; set; }

    // =====================================================
    // Observaciones
    // =====================================================
    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Motivo de rechazo o devolución
    /// </summary>
    [Display(Name = "Motivo de Rechazo")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MotivoRechazo { get; set; }

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
    public string EstadoDescripcion => Estado switch
    {
        "PEN" => "Pendiente",
        "APR" => "Aprobado",
        "REC" => "Rechazado",
        "DEV" => "Devuelto",
        "CAN" => "Cancelado",
        _ => Estado
    };

    [NotMapped]
    public string PrioridadDescripcion => Prioridad switch
    {
        "BAJ" => "Baja",
        "NOR" => "Normal",
        "ALT" => "Alta",
        "URG" => "Urgente",
        _ => Prioridad
    };

    [NotMapped]
    public bool EstaVencida => Estado == "PEN" && FechaLimite.HasValue && FechaLimite.Value < DateTime.Now;

    [NotMapped]
    public int DiasEnProceso => (DateTime.Now - FechaSolicitud).Days;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public TipoWorkflow? TipoWorkflow { get; set; }
    public NivelAprobacion? NivelActual { get; set; }
    public User? Solicitante { get; set; }
    public Departamento? DepartamentoSolicitante { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<AccionAprobacion>? Acciones { get; set; }
}

/// <summary>
/// Estados de solicitud de aprobación
/// </summary>
public static class EstadosSolicitudAprobacion
{
    public const string Pendiente = "PEN";
    public const string Aprobado = "APR";
    public const string Rechazado = "REC";
    public const string Devuelto = "DEV";
    public const string Cancelado = "CAN";
}

/// <summary>
/// Prioridades de solicitud
/// </summary>
public static class PrioridadesSolicitud
{
    public const string Baja = "BAJ";
    public const string Normal = "NOR";
    public const string Alta = "ALT";
    public const string Urgente = "URG";
}
