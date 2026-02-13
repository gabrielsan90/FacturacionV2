using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Plan de cuentas contable con estructura jerárquica.
/// Soporta múltiples niveles (Grupo, Subgrupo, Cuenta, Subcuenta).
/// </summary>
public class CuentaContable
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
    /// Código jerárquico de la cuenta (Ej: 1.1.01.001)
    /// </summary>
    [Display(Name = "Código")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Codigo { get; set; } = null!;

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Clasificación
    // =====================================================
    /// <summary>
    /// Tipo de cuenta: ACT=Activo, PAS=Pasivo, CAP=Capital, ING=Ingreso, GAS=Gasto, COS=Costo
    /// </summary>
    [Display(Name = "Tipo de Cuenta")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoCuenta { get; set; } = null!;

    /// <summary>
    /// Naturaleza del saldo: D=Deudora, A=Acreedora
    /// </summary>
    [Display(Name = "Naturaleza")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(1, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Naturaleza { get; set; } = null!;

    /// <summary>
    /// Nivel en la jerarquía: 1=Grupo, 2=Subgrupo, 3=Cuenta, 4=Subcuenta
    /// </summary>
    [Display(Name = "Nivel")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int Nivel { get; set; } = 1;

    [Display(Name = "Cuenta Padre")]
    public Guid? CuentaPadreId { get; set; }

    // =====================================================
    // Control de Movimientos
    // =====================================================
    /// <summary>
    /// Solo las cuentas de detalle aceptan movimientos
    /// </summary>
    [Display(Name = "Acepta Movimientos")]
    public bool AceptaMovimientos { get; set; } = true;

    /// <summary>
    /// Requiere centro de costo en los movimientos
    /// </summary>
    [Display(Name = "Requiere Centro de Costo")]
    public bool RequiereCentroCosto { get; set; }

    /// <summary>
    /// Requiere tercero (cliente/proveedor) en los movimientos
    /// </summary>
    [Display(Name = "Requiere Tercero")]
    public bool RequiereTercero { get; set; }

    // =====================================================
    // Saldos
    // =====================================================
    [Display(Name = "Saldo Inicial")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoInicial { get; set; }

    [Display(Name = "Saldo Actual")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoActual { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

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
        "ACT" => "Activo",
        "PAS" => "Pasivo",
        "CAP" => "Capital",
        "ING" => "Ingreso",
        "GAS" => "Gasto",
        "COS" => "Costo",
        _ => TipoCuenta
    };

    [NotMapped]
    public string NaturalezaDescripcion => Naturaleza switch
    {
        "D" => "Deudora",
        "A" => "Acreedora",
        _ => Naturaleza
    };

    [NotMapped]
    public string NivelDescripcion => Nivel switch
    {
        1 => "Grupo",
        2 => "Subgrupo",
        3 => "Cuenta",
        4 => "Subcuenta",
        _ => $"Nivel {Nivel}"
    };

    [NotMapped]
    public string CodigoNombre => $"{Codigo} - {Nombre}";

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public CuentaContable? CuentaPadre { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<CuentaContable>? CuentasHijas { get; set; }
    public ICollection<MovimientoContable>? Movimientos { get; set; }
}
