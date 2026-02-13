using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Línea de movimiento dentro de un asiento contable.
/// Cada movimiento afecta una cuenta contable con débito o haber.
/// </summary>
public class MovimientoContable
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relaciones
    // =====================================================
    [Display(Name = "Asiento Contable")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid AsientoContableId { get; set; }

    [Display(Name = "Cuenta Contable")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CuentaContableId { get; set; }

    // =====================================================
    // Control de Línea
    // =====================================================
    [Display(Name = "Número de Línea")]
    public int NumeroLinea { get; set; }

    // =====================================================
    // Descripción
    // =====================================================
    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Montos
    // =====================================================
    [Display(Name = "Debe")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Debe { get; set; }

    [Display(Name = "Haber")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Haber { get; set; }

    // =====================================================
    // Centro de Costo y Tercero
    // =====================================================
    /// <summary>
    /// Centro de costo para análisis de gastos
    /// </summary>
    [Display(Name = "Centro de Costo")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CentroCosto { get; set; }

    /// <summary>
    /// Nombre del tercero (cliente/proveedor) relacionado
    /// </summary>
    [Display(Name = "Tercero")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Tercero { get; set; }

    /// <summary>
    /// ID del cliente si el movimiento está relacionado con uno
    /// </summary>
    [Display(Name = "Cliente")]
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// ID del proveedor si el movimiento está relacionado con uno
    /// </summary>
    [Display(Name = "Proveedor")]
    public Guid? ProveedorId { get; set; }

    // =====================================================
    // Referencia
    // =====================================================
    /// <summary>
    /// Referencia del documento (factura, recibo, etc.)
    /// </summary>
    [Display(Name = "Documento Referencia")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? DocumentoReferencia { get; set; }

    // =====================================================
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public decimal Saldo => Debe - Haber;

    [NotMapped]
    public string TipoMovimiento => Debe > 0 ? "Débito" : "Crédito";

    [NotMapped]
    public decimal Monto => Debe > 0 ? Debe : Haber;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public AsientoContable? AsientoContable { get; set; }
    public CuentaContable? CuentaContable { get; set; }
    public Cliente? Cliente { get; set; }
    public Proveedor? Proveedor { get; set; }
}
