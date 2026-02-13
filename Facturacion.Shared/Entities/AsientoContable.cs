using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Asiento contable (partida de diario).
/// Puede ser manual o generado automáticamente por otros módulos.
/// </summary>
public class AsientoContable
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
    /// Número secuencial del asiento dentro del período
    /// </summary>
    [Display(Name = "Número")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int Numero { get; set; }

    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Fecha { get; set; }

    [Display(Name = "Período Contable")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid PeriodoContableId { get; set; }

    // =====================================================
    // Tipo y Concepto
    // =====================================================
    /// <summary>
    /// Tipo: DIA=Diario, AJU=Ajuste, CIE=Cierre, APE=Apertura
    /// </summary>
    [Display(Name = "Tipo de Asiento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoAsiento { get; set; } = "DIA";

    [Display(Name = "Concepto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Concepto { get; set; } = null!;

    /// <summary>
    /// Referencia externa (número de factura, recibo, etc.)
    /// </summary>
    [Display(Name = "Referencia")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Referencia { get; set; }

    // =====================================================
    // Origen del Asiento
    // =====================================================
    /// <summary>
    /// Módulo origen: VEN, COM, BAN, INV, CXC, CXP, NOM, ACT, MAN (manual)
    /// </summary>
    [Display(Name = "Módulo Origen")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ModuloOrigen { get; set; }

    /// <summary>
    /// ID del documento origen (factura, recibo, etc.)
    /// </summary>
    [Display(Name = "Documento Origen")]
    public Guid? DocumentoOrigenId { get; set; }

    // =====================================================
    // Totales
    // =====================================================
    [Display(Name = "Total Debe")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDebe { get; set; }

    [Display(Name = "Total Haber")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalHaber { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: BOR=Borrador, APR=Aprobado, ANU=Anulado
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "BOR";

    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Aprobación
    // =====================================================
    [Display(Name = "Fecha Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Aprobado Por")]
    public string? AprobadoPorId { get; set; }

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
    public string TipoAsientoDescripcion => TipoAsiento switch
    {
        "DIA" => "Diario",
        "AJU" => "Ajuste",
        "CIE" => "Cierre",
        "APE" => "Apertura",
        _ => TipoAsiento
    };

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "BOR" => "Borrador",
        "APR" => "Aprobado",
        "ANU" => "Anulado",
        _ => Estado
    };

    [NotMapped]
    public string ModuloOrigenDescripcion => ModuloOrigen switch
    {
        "VEN" => "Ventas",
        "COM" => "Compras",
        "BAN" => "Bancos",
        "INV" => "Inventario",
        "CXC" => "Cuentas por Cobrar",
        "CXP" => "Cuentas por Pagar",
        "NOM" => "Nómina",
        "ACT" => "Activos Fijos",
        "MAN" => "Manual",
        _ => ModuloOrigen ?? "Manual"
    };

    [NotMapped]
    public decimal Diferencia => TotalDebe - TotalHaber;

    [NotMapped]
    public bool EstaBalanceado => Math.Abs(Diferencia) < 0.01m;

    [NotMapped]
    public bool PuedeEditar => Estado == "BOR";

    [NotMapped]
    public bool PuedeAprobar => Estado == "BOR" && EstaBalanceado;

    [NotMapped]
    public bool PuedeAnular => Estado == "APR";

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public PeriodoContable? PeriodoContable { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? AprobadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<MovimientoContable>? Movimientos { get; set; }
}
