using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Representa una cuenta bancaria de la empresa.
/// Vinculada a un banco y a una cuenta contable.
/// </summary>
public class CuentaBancaria
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
    [Display(Name = "Número de Cuenta")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string NumeroCuenta { get; set; } = null!;

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Banco")]
    public Guid? BancoId { get; set; }

    /// <summary>
    /// Nombre del banco (si no existe en catálogo)
    /// </summary>
    [Display(Name = "Nombre del Banco")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreBanco { get; set; }

    // =====================================================
    // Tipo y Configuración
    // =====================================================
    /// <summary>
    /// Tipo: AHO=Ahorro, CTE=Corriente, INV=Inversión
    /// </summary>
    [Display(Name = "Tipo de Cuenta")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoCuenta { get; set; } = "CTE";

    [Display(Name = "Sucursal")]
    public Guid? SucursalId { get; set; }

    [Display(Name = "Moneda")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Moneda { get; set; } = "CRC";

    // =====================================================
    // Saldos
    // =====================================================
    [Display(Name = "Saldo Inicial")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoInicial { get; set; }

    [Display(Name = "Saldo Actual")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoActual { get; set; }

    [Display(Name = "Fecha de Apertura")]
    public DateTime? FechaApertura { get; set; }

    // =====================================================
    // Información Bancaria
    // =====================================================
    /// <summary>
    /// Número IBAN o CLABE
    /// </summary>
    [Display(Name = "Número IBAN/CLABE")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroClabe { get; set; }

    [Display(Name = "Código SWIFT")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? SwiftCode { get; set; }

    /// <summary>
    /// Nombre del contacto en el banco
    /// </summary>
    [Display(Name = "Contacto")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Contacto { get; set; }

    [Display(Name = "Teléfono")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Telefono { get; set; }

    // =====================================================
    // Contabilidad
    // =====================================================
    /// <summary>
    /// Cuenta contable asociada a esta cuenta bancaria
    /// </summary>
    [Display(Name = "Cuenta Contable")]
    public Guid? CuentaContableId { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

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
    public string TipoCuentaDescripcion => TipoCuenta switch
    {
        "AHO" => "Ahorro",
        "CTE" => "Corriente",
        "INV" => "Inversión",
        _ => TipoCuenta
    };

    [NotMapped]
    public string CuentaCompleta => $"{NumeroCuenta} - {Nombre}";

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Banco? Banco { get; set; }
    public Sucursal? Sucursal { get; set; }
    public CuentaContable? CuentaContable { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<MovimientoBancario>? Movimientos { get; set; }
    public ICollection<ConciliacionBancaria>? Conciliaciones { get; set; }
    public ICollection<ExtractoBancario>? Extractos { get; set; }
    public ICollection<ReglaConciliacion>? ReglasConciliacion { get; set; }
}

/// <summary>
/// Constantes para tipos de cuenta bancaria
/// </summary>
public static class TiposCuentaBancaria
{
    public const string Ahorro = "AHO";
    public const string Corriente = "CTE";
    public const string Inversion = "INV";
}
