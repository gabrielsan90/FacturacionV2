using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Plantilla de asiento contable reutilizable.
/// Permite definir estructuras predefinidas para asientos recurrentes.
/// </summary>
public class PlantillaAsiento
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
    // Configuración del Asiento
    // =====================================================
    /// <summary>
    /// Tipo de asiento: DIA=Diario, AJU=Ajuste, CIE=Cierre, APE=Apertura
    /// </summary>
    [Display(Name = "Tipo de Asiento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoAsiento { get; set; } = "DIA";

    /// <summary>
    /// Template para el concepto del asiento (puede contener variables)
    /// </summary>
    [Display(Name = "Concepto Template")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ConceptoTemplate { get; set; }

    // =====================================================
    // Estado y Uso
    // =====================================================
    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    [Display(Name = "Última Utilización")]
    public DateTime? UltimaUtilizacion { get; set; }

    [Display(Name = "Veces Utilizada")]
    public int VecesUtilizada { get; set; }

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

    // =====================================================
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public string TipoAsientoDescripcion => TipoAsiento switch
    {
        "DIA" => "Diario",
        "AJU" => "Ajuste",
        "CIE" => "Cierre",
        "APE" => "Apertura",
        _ => TipoAsiento
    };

    [NotMapped]
    public int CantidadLineas => Lineas?.Count ?? 0;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }

    public ICollection<PlantillaAsientoLinea>? Lineas { get; set; }
}

/// <summary>
/// Línea de una plantilla de asiento contable.
/// Define la cuenta y el tipo de monto (fijo, variable o porcentaje).
/// </summary>
public class PlantillaAsientoLinea
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relaciones
    // =====================================================
    [Display(Name = "Plantilla")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid PlantillaAsientoId { get; set; }

    [Display(Name = "Cuenta Contable")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CuentaContableId { get; set; }

    // =====================================================
    // Control de Línea
    // =====================================================
    [Display(Name = "Orden")]
    public int Orden { get; set; }

    [Display(Name = "Descripción")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Tipo de Monto
    // =====================================================
    /// <summary>
    /// Tipo de monto: FIJO, VARIABLE, PORCENTAJE
    /// </summary>
    [Display(Name = "Tipo de Monto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoMonto { get; set; } = "FIJO";

    /// <summary>
    /// Monto fijo para Debe (si TipoMonto = FIJO)
    /// </summary>
    [Display(Name = "Monto Debe Fijo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoDebeFijo { get; set; }

    /// <summary>
    /// Monto fijo para Haber (si TipoMonto = FIJO)
    /// </summary>
    [Display(Name = "Monto Haber Fijo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoHaberFijo { get; set; }

    /// <summary>
    /// Naturaleza del monto variable: DEBE, HABER
    /// </summary>
    [Display(Name = "Naturaleza Variable")]
    [MaxLength(5, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NaturalezaVariable { get; set; }

    /// <summary>
    /// Porcentaje a aplicar (si TipoMonto = PORCENTAJE)
    /// </summary>
    [Display(Name = "Porcentaje")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal? Porcentaje { get; set; }

    // =====================================================
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public string TipoMontoDescripcion => TipoMonto switch
    {
        "FIJO" => "Monto Fijo",
        "VARIABLE" => "Monto Variable",
        "PORCENTAJE" => "Porcentaje",
        _ => TipoMonto
    };

    [NotMapped]
    public bool EsFijo => TipoMonto == "FIJO";

    [NotMapped]
    public bool EsVariable => TipoMonto == "VARIABLE";

    [NotMapped]
    public bool EsPorcentaje => TipoMonto == "PORCENTAJE";

    // =====================================================
    // Navigation Properties
    // =====================================================
    public PlantillaAsiento? PlantillaAsiento { get; set; }
    public CuentaContable? CuentaContable { get; set; }
}
