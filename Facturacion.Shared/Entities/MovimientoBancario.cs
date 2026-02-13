using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Representa un movimiento (depósito, retiro, transferencia) en una cuenta bancaria.
/// </summary>
public class MovimientoBancario
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

    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; }

    // =====================================================
    // Tipo de Movimiento
    // =====================================================
    /// <summary>
    /// Tipo: DEP=Depósito, RET=Retiro, TRA=Transferencia, CHE=Cheque, COM=Comisión, INT=Interés, OTR=Otro
    /// </summary>
    [Display(Name = "Tipo de Movimiento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoMovimiento { get; set; } = "DEP";

    /// <summary>
    /// Naturaleza: CRE=Crédito (entrada), DEB=Débito (salida)
    /// </summary>
    [Display(Name = "Naturaleza")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Naturaleza { get; set; } = "CRE";

    // =====================================================
    // Montos
    // =====================================================
    [Display(Name = "Monto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [Display(Name = "Saldo Anterior")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoAnterior { get; set; }

    [Display(Name = "Saldo Nuevo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoNuevo { get; set; }

    // =====================================================
    // Información del Movimiento
    // =====================================================
    [Display(Name = "Beneficiario")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Beneficiario { get; set; }

    [Display(Name = "Número de Referencia")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroReferencia { get; set; }

    [Display(Name = "Número de Documento")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroDocumento { get; set; }

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Documento Origen
    // =====================================================
    /// <summary>
    /// Tipo de documento origen: FAC, NC, ND, PAG, REC, etc.
    /// </summary>
    [Display(Name = "Tipo Documento Origen")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TipoDocumentoOrigen { get; set; }

    [Display(Name = "ID Documento Origen")]
    public Guid? DocumentoOrigenId { get; set; }

    // =====================================================
    // Conciliación
    // =====================================================
    [Display(Name = "Conciliado")]
    public bool Conciliado { get; set; }

    [Display(Name = "Fecha de Conciliación")]
    public DateTime? FechaConciliacion { get; set; }

    [Display(Name = "Conciliación")]
    public Guid? ConciliacionId { get; set; }

    /// <summary>
    /// Estado: REG=Registrado, CON=Conciliado, ANU=Anulado
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "REG";

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Integración Contable
    // =====================================================
    /// <summary>
    /// Asiento contable generado por este movimiento (si aplica)
    /// </summary>
    [Display(Name = "Asiento Contable")]
    public Guid? AsientoContableId { get; set; }

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
    public string TipoMovimientoDescripcion => TipoMovimiento switch
    {
        "DEP" => "Depósito",
        "RET" => "Retiro",
        "TRA" => "Transferencia",
        "CHE" => "Cheque",
        "COM" => "Comisión",
        "INT" => "Interés",
        "OTR" => "Otro",
        _ => TipoMovimiento
    };

    [NotMapped]
    public string NaturalezaDescripcion => Naturaleza switch
    {
        "CRE" => "Crédito",
        "DEB" => "Débito",
        _ => Naturaleza
    };

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "REG" => "Registrado",
        "CON" => "Conciliado",
        "ANU" => "Anulado",
        _ => Estado
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public CuentaBancaria? CuentaBancaria { get; set; }
    public ConciliacionBancaria? Conciliacion { get; set; }
    public AsientoContable? AsientoContable { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }

    public ICollection<LineaExtractoBancario>? LineasExtracto { get; set; }
}

/// <summary>
/// Constantes para tipos de movimiento bancario
/// </summary>
public static class TiposMovimientoBancario
{
    public const string Deposito = "DEP";
    public const string Retiro = "RET";
    public const string Transferencia = "TRA";
    public const string Cheque = "CHE";
    public const string Comision = "COM";
    public const string Interes = "INT";
    public const string Otro = "OTR";
}

/// <summary>
/// Naturaleza del movimiento bancario
/// </summary>
public static class NaturalezaMovimiento
{
    public const string Credito = "CRE";
    public const string Debito = "DEB";
}

/// <summary>
/// Estados de movimiento bancario
/// </summary>
public static class EstadosMovimientoBancario
{
    public const string Registrado = "REG";
    public const string Conciliado = "CON";
    public const string Anulado = "ANU";
}
