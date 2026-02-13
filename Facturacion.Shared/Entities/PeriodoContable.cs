using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Período contable mensual.
/// Permite controlar apertura/cierre de meses para registro de asientos.
/// </summary>
public class PeriodoContable
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
    // Período Fiscal
    // =====================================================
    [Display(Name = "Período Fiscal")]
    public Guid? PeriodoFiscalId { get; set; }

    // =====================================================
    // Identificación
    // =====================================================
    [Display(Name = "Año")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int Anio { get; set; }

    [Display(Name = "Mes")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12.")]
    public int Mes { get; set; }

    /// <summary>
    /// Nombre descriptivo del período (Ej: "Enero 2024")
    /// </summary>
    [Display(Name = "Nombre")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Nombre { get; set; }

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
    /// Estado: ABT=Abierto, CER=Cerrado
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
    // Consecutivo de Asientos
    // =====================================================
    /// <summary>
    /// Último número de asiento utilizado en el período
    /// </summary>
    [Display(Name = "Último Número Asiento")]
    public int UltimoNumeroAsiento { get; set; }

    // =====================================================
    // Totales del Período
    // =====================================================
    [Display(Name = "Total Debe")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDebe { get; set; }

    [Display(Name = "Total Haber")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalHaber { get; set; }

    [Display(Name = "Cantidad de Asientos")]
    public int CantidadAsientos { get; set; }

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
        _ => Estado
    };

    [NotMapped]
    public bool EstaAbierto => Estado == "ABT";

    [NotMapped]
    public bool PuedeCerrar => Estado == "ABT";

    [NotMapped]
    public string NombreMes => Mes switch
    {
        1 => "Enero",
        2 => "Febrero",
        3 => "Marzo",
        4 => "Abril",
        5 => "Mayo",
        6 => "Junio",
        7 => "Julio",
        8 => "Agosto",
        9 => "Septiembre",
        10 => "Octubre",
        11 => "Noviembre",
        12 => "Diciembre",
        _ => $"Mes {Mes}"
    };

    [NotMapped]
    public string PeriodoNombre => $"{NombreMes} {Anio}";

    [NotMapped]
    public decimal Diferencia => TotalDebe - TotalHaber;

    [NotMapped]
    public bool EstaBalanceado => Math.Abs(Diferencia) < 0.01m;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public PeriodoFiscal? PeriodoFiscal { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? CerradoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<AsientoContable>? Asientos { get; set; }
}
