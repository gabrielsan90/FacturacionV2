using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Representa una conciliación bancaria mensual.
/// Compara los movimientos del sistema con el estado de cuenta del banco.
/// </summary>
public class ConciliacionBancaria
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
    [Display(Name = "Número")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Numero { get; set; } = null!;

    [Display(Name = "Cuenta Bancaria")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CuentaBancariaId { get; set; }

    // =====================================================
    // Período
    // =====================================================
    [Display(Name = "Mes")]
    public int Mes { get; set; }

    [Display(Name = "Año")]
    public int Anio { get; set; }

    [Display(Name = "Fecha de Inicio")]
    public DateTime FechaInicio { get; set; }

    [Display(Name = "Fecha de Fin")]
    public DateTime FechaFin { get; set; }

    // =====================================================
    // Saldos
    // =====================================================
    /// <summary>
    /// Saldo inicial según los libros del sistema
    /// </summary>
    [Display(Name = "Saldo Inicial Libros")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoInicialLibros { get; set; }

    /// <summary>
    /// Saldo final según los libros del sistema
    /// </summary>
    [Display(Name = "Saldo Final Libros")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoFinalLibros { get; set; }

    /// <summary>
    /// Saldo según el estado de cuenta del banco
    /// </summary>
    [Display(Name = "Saldo Estado de Cuenta")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoEstadoCuenta { get; set; }

    // =====================================================
    // Partidas en Tránsito
    // =====================================================
    [Display(Name = "Depósitos en Tránsito")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DepositosEnTransito { get; set; }

    [Display(Name = "Cheques en Tránsito")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ChequesEnTransito { get; set; }

    /// <summary>
    /// Notas de crédito del banco no registradas
    /// </summary>
    [Display(Name = "Notas de Crédito")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal NotasCredito { get; set; }

    /// <summary>
    /// Notas de débito del banco no registradas
    /// </summary>
    [Display(Name = "Notas de Débito")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal NotasDebito { get; set; }

    /// <summary>
    /// Diferencia resultante de la conciliación
    /// </summary>
    [Display(Name = "Diferencia")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Diferencia { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: PEN=Pendiente, PAR=Parcial, CON=Conciliada, CER=Cerrada
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "PEN";

    [Display(Name = "Fecha de Conciliación")]
    public DateTime? FechaConciliacion { get; set; }

    [Display(Name = "Conciliado Por")]
    public string? ConciliadoPorId { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
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
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "PEN" => "Pendiente",
        "PAR" => "Parcialmente Conciliada",
        "CON" => "Conciliada",
        "CER" => "Cerrada",
        _ => Estado
    };

    [NotMapped]
    public string Periodo => $"{Mes:00}/{Anio}";

    [NotMapped]
    public decimal SaldoConciliado => SaldoEstadoCuenta + DepositosEnTransito - ChequesEnTransito + NotasCredito - NotasDebito;

    [NotMapped]
    public bool EstaConciliada => Math.Abs(Diferencia) < 0.01m;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public CuentaBancaria? CuentaBancaria { get; set; }
    public User? ConciliadoPor { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }

    public ICollection<MovimientoBancario>? Movimientos { get; set; }
    public ICollection<ExtractoBancario>? Extractos { get; set; }
}

/// <summary>
/// Estados de conciliación bancaria
/// </summary>
public static class EstadosConciliacionBancaria
{
    public const string Pendiente = "PEN";
    public const string Parcial = "PAR";
    public const string Conciliada = "CON";
    public const string Cerrada = "CER";
}
