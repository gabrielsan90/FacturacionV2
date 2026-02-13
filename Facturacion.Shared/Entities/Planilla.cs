using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Planilla de pagos.
/// Agrupa los pagos a empleados por período.
/// </summary>
public class Planilla
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

    [Display(Name = "Año")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int Anio { get; set; }

    [Display(Name = "Mes")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre {1} y {2}.")]
    public int Mes { get; set; }

    /// <summary>
    /// Período: 1=Primera quincena, 2=Segunda quincena (o 1 si es mensual)
    /// </summary>
    [Display(Name = "Período")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int Periodo { get; set; } = 1;

    /// <summary>
    /// Tipo: ORD=Ordinaria, EXT=Extraordinaria, AGU=Aguinaldo, LIQ=Liquidación
    /// </summary>
    [Display(Name = "Tipo de Planilla")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoPlanilla { get; set; } = "ORD";

    [Display(Name = "Descripción")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Fechas del período
    // =====================================================
    [Display(Name = "Fecha de Inicio")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaInicio { get; set; }

    [Display(Name = "Fecha de Fin")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaFin { get; set; }

    [Display(Name = "Fecha de Pago")]
    public DateTime? FechaPago { get; set; }

    // =====================================================
    // Totales
    // =====================================================
    [Display(Name = "Total Salario Bruto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSalarioBruto { get; set; }

    [Display(Name = "Total Deducciones")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDeducciones { get; set; }

    [Display(Name = "Total Salario Neto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSalarioNeto { get; set; }

    [Display(Name = "Total Cargas Sociales")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCargasSociales { get; set; }

    // =====================================================
    // Desglose cargas patronales (Costa Rica)
    // =====================================================
    [Display(Name = "Total CCSS Patronal")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCCSSPatronal { get; set; }

    [Display(Name = "Total INS")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalINS { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: BOR=Borrador, CAL=Calculada, APR=Aprobada, PAG=Pagada, ANU=Anulada
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "BOR";

    [Display(Name = "Fecha de Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Aprobado Por")]
    public string? AprobadoPorId { get; set; }

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
    public string TipoPlanillaDescripcion => TipoPlanilla switch
    {
        "ORD" => "Ordinaria",
        "EXT" => "Extraordinaria",
        "AGU" => "Aguinaldo",
        "LIQ" => "Liquidación",
        _ => TipoPlanilla
    };

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "BOR" => "Borrador",
        "CAL" => "Calculada",
        "APR" => "Aprobada",
        "PAG" => "Pagada",
        "ANU" => "Anulada",
        _ => Estado
    };

    [NotMapped]
    public string PeriodoDescripcion => $"{Anio}-{Mes:D2} P{Periodo}";

    [NotMapped]
    public decimal CostoTotal => TotalSalarioBruto + TotalCargasSociales;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public User? AprobadoPor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<DetallePlanilla>? Detalles { get; set; }
}
