using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Recepción de productos de una orden de compra.
/// Registra la entrada de mercadería al inventario.
/// </summary>
public class RecepcionCompra
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

    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Fecha { get; set; }

    // =====================================================
    // Orden de Compra y Bodega
    // =====================================================
    [Display(Name = "Orden de Compra")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid OrdenCompraId { get; set; }

    [Display(Name = "Bodega")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid BodegaId { get; set; }

    // =====================================================
    // Factura del Proveedor
    // =====================================================
    [Display(Name = "Número Factura Proveedor")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroFacturaProveedor { get; set; }

    [Display(Name = "Fecha Factura Proveedor")]
    public DateTime? FechaFacturaProveedor { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: APL=Aplicada, ANU=Anulada
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "APL";

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
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "APL" => "Aplicada",
        "ANU" => "Anulada",
        _ => Estado
    };

    [NotMapped]
    public bool PuedeAnular => Estado == "APL";

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }
    public Bodega? Bodega { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<RecepcionCompraDetalle>? Detalles { get; set; }
}
