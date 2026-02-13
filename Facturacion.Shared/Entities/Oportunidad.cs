using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Oportunidad de venta CRM.
/// Seguimiento de prospectos y negociaciones comerciales.
/// </summary>
public class Oportunidad
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
    // Datos principales
    // =====================================================
    [Display(Name = "Número")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Numero { get; set; } = null!;

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Cliente y contacto
    // =====================================================
    [Display(Name = "Cliente")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ClienteId { get; set; }

    [Display(Name = "Nombre Contacto")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreContacto { get; set; }

    [Display(Name = "Teléfono Contacto")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TelefonoContacto { get; set; }

    [Display(Name = "Email Contacto")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [EmailAddress(ErrorMessage = "El campo {0} debe ser un email válido.")]
    public string? EmailContacto { get; set; }

    // =====================================================
    // Valores y probabilidad
    // =====================================================
    [Display(Name = "Monto Estimado")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoEstimado { get; set; }

    [Display(Name = "Probabilidad de Cierre (%)")]
    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public decimal ProbabilidadCierre { get; set; } = 50;

    // =====================================================
    // Pipeline
    // =====================================================
    [Display(Name = "Etapa")]
    public Guid? EtapaPipelineId { get; set; }

    /// <summary>
    /// Código de etapa para referencia rápida:
    /// PRO=Prospecto, CAL=Calificado, PRP=Propuesta, NEG=Negociación, GAN=Ganado, PER=Perdido
    /// </summary>
    [Display(Name = "Código Etapa")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string CodigoEtapa { get; set; } = "PRO";

    // =====================================================
    // Fechas
    // =====================================================
    [Display(Name = "Fecha Cierre Estimada")]
    public DateTime? FechaCierreEstimada { get; set; }

    [Display(Name = "Fecha Cierre Real")]
    public DateTime? FechaCierreReal { get; set; }

    // =====================================================
    // Vendedor asignado
    // =====================================================
    [Display(Name = "Vendedor")]
    public string? VendedorId { get; set; }

    // =====================================================
    // Origen
    // =====================================================
    /// <summary>
    /// Origen: WEB=Web, REF=Referido, FER=Feria, LLA=Llamada, RED=Redes Sociales, OTR=Otro
    /// </summary>
    [Display(Name = "Origen")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Origen { get; set; }

    [Display(Name = "Detalle Origen")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? OrigenDetalle { get; set; }

    // =====================================================
    // Cierre
    // =====================================================
    [Display(Name = "Motivo Ganado/Perdido")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MotivoGanoPerdido { get; set; }

    [Display(Name = "Competidor")]
    public Guid? CompetidorId { get; set; }

    /// <summary>
    /// Prioridad: ALT=Alta, MED=Media, BAJ=Baja
    /// </summary>
    [Display(Name = "Prioridad")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Prioridad { get; set; } = "MED";

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
    public decimal MontoPonderado => MontoEstimado * (ProbabilidadCierre / 100);

    [NotMapped]
    public string PrioridadDescripcion => Prioridad switch
    {
        "ALT" => "Alta",
        "MED" => "Media",
        "BAJ" => "Baja",
        _ => Prioridad
    };

    [NotMapped]
    public string OrigenDescripcion => Origen switch
    {
        "WEB" => "Web",
        "REF" => "Referido",
        "FER" => "Feria",
        "LLA" => "Llamada",
        "RED" => "Redes Sociales",
        "OTR" => "Otro",
        _ => Origen ?? ""
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Cliente? Cliente { get; set; }
    public EtapaPipeline? EtapaPipeline { get; set; }
    public User? Vendedor { get; set; }
    public Competidor? Competidor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<ActividadCRM>? Actividades { get; set; }
    public ICollection<NotaOportunidad>? Notas { get; set; }
    public ICollection<HistorialEtapaOportunidad>? HistorialEtapas { get; set; }
}
