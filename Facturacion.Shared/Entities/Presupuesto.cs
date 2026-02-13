using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Define un presupuesto anual, trimestral o mensual.
/// Contiene las líneas de presupuesto por cuenta y centro de costo.
/// </summary>
public class Presupuesto
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
    // Período
    // =====================================================
    [Display(Name = "Año Fiscal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int AnioFiscal { get; set; }

    /// <summary>
    /// Tipo: ANU=Anual, TRI=Trimestral, MEN=Mensual
    /// </summary>
    [Display(Name = "Tipo de Presupuesto")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoPresupuesto { get; set; } = "ANU";

    // =====================================================
    // Estado y Workflow
    // =====================================================
    /// <summary>
    /// Estado: BOR=Borrador, REV=En Revisión, APR=Aprobado, VIG=Vigente, CER=Cerrado
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "BOR";

    [Display(Name = "Fecha de Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Aprobado Por")]
    public string? AprobadoPorId { get; set; }

    // =====================================================
    // Versionamiento
    // =====================================================
    /// <summary>
    /// Versión del presupuesto (para revisiones)
    /// </summary>
    [Display(Name = "Versión")]
    public int Version { get; set; } = 1;

    /// <summary>
    /// Presupuesto base del que se originó esta versión
    /// </summary>
    [Display(Name = "Presupuesto Base")]
    public Guid? PresupuestoBaseId { get; set; }

    // =====================================================
    // Totales
    // =====================================================
    [Display(Name = "Monto Total")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoTotal { get; set; }

    [Display(Name = "Monto Ejecutado")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoEjecutado { get; set; }

    // =====================================================
    // Observaciones
    // =====================================================
    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

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
    public string TipoPresupuestoDescripcion => TipoPresupuesto switch
    {
        "ANU" => "Anual",
        "TRI" => "Trimestral",
        "MEN" => "Mensual",
        _ => TipoPresupuesto
    };

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "BOR" => "Borrador",
        "REV" => "En Revisión",
        "APR" => "Aprobado",
        "VIG" => "Vigente",
        "CER" => "Cerrado",
        _ => Estado
    };

    [NotMapped]
    public decimal PorcentajeEjecucion => MontoTotal > 0 ? Math.Round((MontoEjecutado / MontoTotal) * 100, 2) : 0;

    [NotMapped]
    public decimal MontoDisponible => MontoTotal - MontoEjecutado;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Presupuesto? PresupuestoBase { get; set; }
    public User? AprobadoPor { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<LineaPresupuesto>? Lineas { get; set; }
    public ICollection<Presupuesto>? Versiones { get; set; }
}

/// <summary>
/// Constantes para tipos de presupuesto
/// </summary>
public static class TiposPresupuesto
{
    public const string Anual = "ANU";
    public const string Trimestral = "TRI";
    public const string Mensual = "MEN";
}

/// <summary>
/// Estados de presupuesto
/// </summary>
public static class EstadosPresupuesto
{
    public const string Borrador = "BOR";
    public const string EnRevision = "REV";
    public const string Aprobado = "APR";
    public const string Vigente = "VIG";
    public const string Cerrado = "CER";
}
