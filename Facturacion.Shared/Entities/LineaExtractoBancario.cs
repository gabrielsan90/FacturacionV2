using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Representa una línea/transacción del extracto bancario importado.
/// </summary>
public class LineaExtractoBancario
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
    // Extracto
    // =====================================================
    [Display(Name = "Extracto Bancario")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ExtractoBancarioId { get; set; }

    /// <summary>
    /// Número de línea dentro del extracto
    /// </summary>
    [Display(Name = "Número de Línea")]
    public int NumeroLinea { get; set; }

    // =====================================================
    // Fechas
    // =====================================================
    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Fecha valor (fecha efectiva del movimiento)
    /// </summary>
    [Display(Name = "Fecha Valor")]
    public DateTime? FechaValor { get; set; }

    // =====================================================
    // Referencias
    // =====================================================
    /// <summary>
    /// Referencia externa del banco
    /// </summary>
    [Display(Name = "Referencia Externa")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ReferenciaExterna { get; set; }

    [Display(Name = "Número de Documento")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroDocumento { get; set; }

    [Display(Name = "Descripción")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Descripcion { get; set; } = null!;

    // =====================================================
    // Montos
    // =====================================================
    [Display(Name = "Débito")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Debito { get; set; }

    [Display(Name = "Crédito")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Credito { get; set; }

    [Display(Name = "Saldo Acumulado")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoAcumulado { get; set; }

    // =====================================================
    // Clasificación
    // =====================================================
    /// <summary>
    /// Tipo de transacción inferido: DEP, RET, TRA, CHE, COM, INT, OTR
    /// </summary>
    [Display(Name = "Tipo de Transacción")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TipoTransaccion { get; set; }

    // =====================================================
    // Conciliación
    // =====================================================
    /// <summary>
    /// Movimiento bancario con el que fue conciliado
    /// </summary>
    [Display(Name = "Movimiento Bancario")]
    public Guid? MovimientoBancarioId { get; set; }

    /// <summary>
    /// Estado: PEN=Pendiente, CON=Conciliado, DIF=Diferencia, IGN=Ignorado
    /// </summary>
    [Display(Name = "Estado de Conciliación")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string EstadoConciliacion { get; set; } = "PEN";

    [Display(Name = "Fecha de Conciliación")]
    public DateTime? FechaConciliacion { get; set; }

    [Display(Name = "Conciliado Por")]
    public string? ConciliadoPorId { get; set; }

    [Display(Name = "Nota de Conciliación")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NotaConciliacion { get; set; }

    // =====================================================
    // Matching Automático
    // =====================================================
    /// <summary>
    /// Porcentaje de confianza del match automático (0-100)
    /// </summary>
    [Display(Name = "Confianza Match")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal? ConfianzaMatch { get; set; }

    /// <summary>
    /// Si fue conciliado automáticamente por regla
    /// </summary>
    [Display(Name = "Conciliación Automática")]
    public bool ConciliacionAutomatica { get; set; }

    [Display(Name = "Regla de Conciliación")]
    public Guid? ReglaConciliacionId { get; set; }

    // =====================================================
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public decimal Monto => Credito - Debito;

    [NotMapped]
    public bool EsCredito => Credito > 0;

    [NotMapped]
    public bool EsDebito => Debito > 0;

    [NotMapped]
    public string TipoTransaccionDescripcion => TipoTransaccion switch
    {
        "DEP" => "Depósito",
        "RET" => "Retiro",
        "TRA" => "Transferencia",
        "CHE" => "Cheque",
        "COM" => "Comisión",
        "INT" => "Interés",
        "OTR" => "Otro",
        _ => TipoTransaccion ?? "Sin clasificar"
    };

    [NotMapped]
    public string EstadoConciliacionDescripcion => EstadoConciliacion switch
    {
        "PEN" => "Pendiente",
        "CON" => "Conciliado",
        "DIF" => "Diferencia",
        "IGN" => "Ignorado",
        _ => EstadoConciliacion
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public ExtractoBancario? ExtractoBancario { get; set; }
    public MovimientoBancario? MovimientoBancario { get; set; }
    public ReglaConciliacion? ReglaConciliacion { get; set; }
    public User? ConciliadoPor { get; set; }
}

/// <summary>
/// Estados de conciliación de línea
/// </summary>
public static class EstadosConciliacionLinea
{
    public const string Pendiente = "PEN";
    public const string Conciliado = "CON";
    public const string Diferencia = "DIF";
    public const string Ignorado = "IGN";
}
