using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Representa un período fiscal completo (año fiscal).
/// Agrupa los períodos contables mensuales.
/// Costa Rica: Año fiscal de octubre a septiembre para empresas.
/// </summary>
public class PeriodoFiscal
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
    /// Año fiscal (ej: 2024, 2025)
    /// </summary>
    [Display(Name = "Año Fiscal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int AnioFiscal { get; set; }

    /// <summary>
    /// Nombre descriptivo del período fiscal
    /// Ej: "Período Fiscal 2024-2025"
    /// </summary>
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Fecha Inicio")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaInicio { get; set; }

    [Display(Name = "Fecha Fin")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaFin { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: ABT=Abierto, CER=Cerrado, AUD=En Auditoría
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "ABT";

    [Display(Name = "Fecha Cierre")]
    public DateTime? FechaCierre { get; set; }

    [Display(Name = "Cerrado Por")]
    public string? CerradoPorId { get; set; }

    // =====================================================
    // Asientos de Cierre/Apertura
    // =====================================================
    /// <summary>
    /// ID del asiento de cierre del ejercicio
    /// </summary>
    [Display(Name = "Asiento Cierre")]
    public Guid? AsientoCierreId { get; set; }

    /// <summary>
    /// ID del asiento de apertura del siguiente ejercicio
    /// </summary>
    [Display(Name = "Asiento Apertura")]
    public Guid? AsientoAperturaId { get; set; }

    // =====================================================
    // Resultado del Ejercicio
    // =====================================================
    /// <summary>
    /// Resultado del ejercicio (utilidad positiva, pérdida negativa)
    /// </summary>
    [Display(Name = "Resultado del Ejercicio")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ResultadoEjercicio { get; set; }

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
    public string EstadoDescripcion => Estado switch
    {
        "ABT" => "Abierto",
        "CER" => "Cerrado",
        "AUD" => "En Auditoría",
        _ => Estado
    };

    [NotMapped]
    public bool PuedeCerrar => Estado == "ABT";

    [NotMapped]
    public bool EstaAbierto => Estado == "ABT";

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public AsientoContable? AsientoCierre { get; set; }
    public AsientoContable? AsientoApertura { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? CerradoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<PeriodoContable>? PeriodosContables { get; set; }
}
