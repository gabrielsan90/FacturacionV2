using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Lote de productos para trazabilidad y control de vencimientos.
/// Permite seguimiento de productos por lote, fechas de vencimiento y cuarentena.
/// </summary>
public class Lote
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
    // Datos principales
    // =====================================================
    [Display(Name = "Número de Lote")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string NumeroLote { get; set; } = null!;

    [Display(Name = "Producto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProductoId { get; set; }

    // =====================================================
    // Fechas de control
    // =====================================================
    [Display(Name = "Fecha de Fabricación")]
    public DateTime? FechaFabricacion { get; set; }

    [Display(Name = "Fecha de Vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    // =====================================================
    // Origen del lote
    // =====================================================
    [Display(Name = "Proveedor")]
    public Guid? ProveedorId { get; set; }

    [Display(Name = "Factura de Compra")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroFacturaCompra { get; set; }

    // =====================================================
    // Cantidades y costos
    // =====================================================
    [Display(Name = "Cantidad Inicial")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadInicial { get; set; }

    [Display(Name = "Cantidad Actual")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal CantidadActual { get; set; }

    [Display(Name = "Costo Unitario")]
    [Column(TypeName = "decimal(18,6)")]
    public decimal CostoUnitario { get; set; }

    // =====================================================
    // Estado y cuarentena
    // =====================================================
    /// <summary>
    /// Estado del lote: ACT=Activo, AGO=Agotado, VEN=Vencido, CUA=Cuarentena
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "ACT";

    [Display(Name = "En Cuarentena")]
    public bool EnCuarentena { get; set; }

    [Display(Name = "Motivo Cuarentena")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MotivoCuarentena { get; set; }

    [Display(Name = "Fecha Cuarentena")]
    public DateTime? FechaCuarentena { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

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
    public bool EstaVencido => FechaVencimiento.HasValue && FechaVencimiento.Value.Date < DateTime.Today;

    [NotMapped]
    public int? DiasParaVencer => FechaVencimiento.HasValue
        ? (int)(FechaVencimiento.Value.Date - DateTime.Today).TotalDays
        : null;

    [NotMapped]
    public bool ProximoAVencer => DiasParaVencer.HasValue && DiasParaVencer.Value <= 30 && DiasParaVencer.Value > 0;

    [NotMapped]
    public decimal ValorTotal => CantidadActual * CostoUnitario;

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "ACT" => "Activo",
        "AGO" => "Agotado",
        "VEN" => "Vencido",
        "CUA" => "Cuarentena",
        _ => Estado
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Producto? Producto { get; set; }
    public Proveedor? Proveedor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<Inventario>? Inventarios { get; set; }
}
