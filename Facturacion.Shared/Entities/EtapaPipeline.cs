using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Configuración de las etapas del pipeline de ventas CRM.
/// Define las fases por las que pasa una oportunidad de venta.
/// </summary>
public class EtapaPipeline
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
    [Display(Name = "Código")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Codigo { get; set; } = null!;

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Configuración de etapa
    // =====================================================
    [Display(Name = "Orden")]
    public int Orden { get; set; }

    [Display(Name = "Color")]
    [MaxLength(7, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Color { get; set; }

    [Display(Name = "Probabilidad Sugerida (%)")]
    [Range(0, 100, ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
    public int ProbabilidadSugerida { get; set; } = 50;

    [Display(Name = "Es Etapa Final")]
    public bool EsFinal { get; set; }

    [Display(Name = "Es Ganada")]
    public bool EsGanada { get; set; }

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
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<Oportunidad>? Oportunidades { get; set; }
}
