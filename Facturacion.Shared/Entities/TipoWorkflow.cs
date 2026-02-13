using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Define los tipos de workflow disponibles en el sistema.
/// Cada tipo representa un proceso de aprobación diferente.
/// </summary>
public class TipoWorkflow
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
    // Identificación
    // =====================================================
    /// <summary>
    /// Código único del workflow: OC, REQ, ASI, PLA, FAC, NC, ND, VAC, GAS
    /// </summary>
    [Display(Name = "Código")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Codigo { get; set; } = null!;

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Configuración de Aprobación
    // =====================================================
    [Display(Name = "Requiere Aprobación")]
    public bool RequiereAprobacion { get; set; } = true;

    /// <summary>
    /// Monto mínimo para requerir aprobación
    /// </summary>
    [Display(Name = "Monto Mínimo Aprobación")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoMinimoAprobacion { get; set; }

    /// <summary>
    /// Monto máximo que no requiere aprobación (aprobación automática)
    /// </summary>
    [Display(Name = "Monto Máximo sin Aprobación")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoMaximoSinAprobacion { get; set; }

    // =====================================================
    // Notificaciones
    // =====================================================
    [Display(Name = "Notificar por Email")]
    public bool NotificarEmail { get; set; } = true;

    [Display(Name = "Notificar en Sistema")]
    public bool NotificarSistema { get; set; } = true;

    // =====================================================
    // Escalamiento
    // =====================================================
    /// <summary>
    /// Días antes de escalar al siguiente nivel
    /// </summary>
    [Display(Name = "Días para Escalamiento")]
    public int? DiasEscalamiento { get; set; }

    [Display(Name = "Escalamiento Automático")]
    public bool EscalamientoAutomatico { get; set; }

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
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<NivelAprobacion>? Niveles { get; set; }
    public ICollection<SolicitudAprobacion>? Solicitudes { get; set; }
}
