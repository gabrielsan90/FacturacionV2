using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Catálogo de puestos de trabajo.
/// Define estructura salarial, requisitos y funciones.
/// </summary>
public class Puesto
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
    // Relaciones organizacionales
    // =====================================================
    [Display(Name = "Departamento")]
    public Guid? DepartamentoId { get; set; }

    /// <summary>
    /// Nivel jerárquico: 1=Ejecutivo, 2=Gerencial, 3=Supervisión, 4=Operativo
    /// </summary>
    [Display(Name = "Nivel Jerárquico")]
    public int NivelJerarquico { get; set; } = 4;

    [Display(Name = "Puesto Superior")]
    public Guid? PuestoSuperiorId { get; set; }

    // =====================================================
    // Estructura salarial
    // =====================================================
    [Display(Name = "Salario Mínimo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalarioMinimo { get; set; }

    [Display(Name = "Salario Máximo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalarioMaximo { get; set; }

    // =====================================================
    // Requisitos y competencias
    // =====================================================
    [Display(Name = "Requisitos")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Requisitos { get; set; }

    [Display(Name = "Funciones")]
    [MaxLength(2000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Funciones { get; set; }

    [Display(Name = "Competencias")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Competencias { get; set; }

    // =====================================================
    // Características del puesto
    // =====================================================
    [Display(Name = "Es de Confianza")]
    public bool EsDeConfianza { get; set; }

    [Display(Name = "Tiene Personal a Cargo")]
    public bool TienePersonalACargo { get; set; }

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
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public string NivelJerarquicoDescripcion => NivelJerarquico switch
    {
        1 => "Ejecutivo",
        2 => "Gerencial",
        3 => "Supervisión",
        4 => "Operativo",
        _ => $"Nivel {NivelJerarquico}"
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Departamento? Departamento { get; set; }
    public Puesto? PuestoSuperior { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<Empleado>? Empleados { get; set; }
    public ICollection<Puesto>? PuestosSubordinados { get; set; }
}
