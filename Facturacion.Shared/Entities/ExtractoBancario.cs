using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Representa un extracto bancario importado del banco para conciliación.
/// </summary>
public class ExtractoBancario
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
    [Display(Name = "Cuenta Bancaria")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CuentaBancariaId { get; set; }

    [Display(Name = "Número")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Numero { get; set; } = null!;

    // =====================================================
    // Período del Extracto
    // =====================================================
    [Display(Name = "Fecha de Inicio")]
    public DateTime FechaInicio { get; set; }

    [Display(Name = "Fecha de Fin")]
    public DateTime FechaFin { get; set; }

    // =====================================================
    // Saldos
    // =====================================================
    [Display(Name = "Saldo Inicial")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoInicial { get; set; }

    [Display(Name = "Saldo Final")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoFinal { get; set; }

    [Display(Name = "Total Créditos")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCreditos { get; set; }

    [Display(Name = "Total Débitos")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDebitos { get; set; }

    [Display(Name = "Cantidad de Transacciones")]
    public int CantidadTransacciones { get; set; }

    // =====================================================
    // Importación
    // =====================================================
    [Display(Name = "Fecha de Importación")]
    public DateTime FechaImportacion { get; set; }

    /// <summary>
    /// Ruta o nombre del archivo importado
    /// </summary>
    [Display(Name = "Archivo Origen")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ArchivoOrigen { get; set; }

    /// <summary>
    /// Formato: CSV, OFX, MT940, XLSX
    /// </summary>
    [Display(Name = "Formato de Archivo")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? FormatoArchivo { get; set; }

    // =====================================================
    // Estado y Conciliación
    // =====================================================
    /// <summary>
    /// Estado: PEN=Pendiente, PAR=Parcialmente Conciliado, CON=Conciliado, CER=Cerrado
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "PEN";

    [Display(Name = "Conciliación")]
    public Guid? ConciliacionBancariaId { get; set; }

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
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "PEN" => "Pendiente",
        "PAR" => "Parcialmente Conciliado",
        "CON" => "Conciliado",
        "CER" => "Cerrado",
        _ => Estado
    };

    [NotMapped]
    public string Periodo => $"{FechaInicio:dd/MM/yyyy} - {FechaFin:dd/MM/yyyy}";

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public CuentaBancaria? CuentaBancaria { get; set; }
    public ConciliacionBancaria? ConciliacionBancaria { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }

    public ICollection<LineaExtractoBancario>? Lineas { get; set; }
}

/// <summary>
/// Formatos de archivo soportados para importación
/// </summary>
public static class FormatosExtracto
{
    public const string CSV = "CSV";
    public const string OFX = "OFX";
    public const string MT940 = "MT940";
    public const string XLSX = "XLSX";
}

/// <summary>
/// Estados del extracto bancario
/// </summary>
public static class EstadosExtractoBancario
{
    public const string Pendiente = "PEN";
    public const string Parcial = "PAR";
    public const string Conciliado = "CON";
    public const string Cerrado = "CER";
}
