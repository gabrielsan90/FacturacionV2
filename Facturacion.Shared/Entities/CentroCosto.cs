using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Define los centros de costo para asignación de gastos e ingresos.
/// Estructura jerárquica con niveles.
/// </summary>
public class CentroCosto
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
    // Tipo y Jerarquía
    // =====================================================
    /// <summary>
    /// Tipo: COS=Centro de Costo, BEN=Centro de Beneficio, AUX=Auxiliar
    /// </summary>
    [Display(Name = "Tipo")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Tipo { get; set; } = "COS";

    /// <summary>
    /// Centro de costo padre (para estructura jerárquica)
    /// </summary>
    [Display(Name = "Centro Padre")]
    public Guid? PadreId { get; set; }

    /// <summary>
    /// Nivel en la jerarquía (1=raíz, 2, 3, 4...)
    /// </summary>
    [Display(Name = "Nivel")]
    public int Nivel { get; set; } = 1;

    // =====================================================
    // Ubicación y Responsable
    // =====================================================
    [Display(Name = "Sucursal")]
    public Guid? SucursalId { get; set; }

    [Display(Name = "Departamento")]
    public Guid? DepartamentoId { get; set; }

    /// <summary>
    /// Usuario responsable del centro de costo
    /// </summary>
    [Display(Name = "Responsable")]
    public string? ResponsableId { get; set; }

    // =====================================================
    // Configuración
    // =====================================================
    /// <summary>
    /// Si acepta movimientos directos o solo es agrupador
    /// </summary>
    [Display(Name = "Acepta Movimientos")]
    public bool AceptaMovimientos { get; set; } = true;

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
    public string TipoDescripcion => Tipo switch
    {
        "COS" => "Centro de Costo",
        "BEN" => "Centro de Beneficio",
        "AUX" => "Auxiliar",
        _ => Tipo
    };

    [NotMapped]
    public string CodigoNombre => $"{Codigo} - {Nombre}";

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public CentroCosto? Padre { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Departamento? Departamento { get; set; }
    public User? Responsable { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<CentroCosto>? Hijos { get; set; }
    public ICollection<LineaPresupuesto>? LineasPresupuesto { get; set; }
    public ICollection<MovimientoContable>? MovimientosContables { get; set; }
}

/// <summary>
/// Constantes para tipos de centro de costo
/// </summary>
public static class TiposCentroCosto
{
    public const string CentroCosto = "COS";
    public const string CentroBeneficio = "BEN";
    public const string Auxiliar = "AUX";
}
