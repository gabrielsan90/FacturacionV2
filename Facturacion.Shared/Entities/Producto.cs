using Facturacion.Shared.Entities.Catalogos;
using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Productos y Servicios de una empresa
/// Soporte para facturación electrónica de Costa Rica (Hacienda)
/// </summary>
public class Producto
{
    [Key]
    public Guid Id { get; set; }

    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    [Display(Name = "Tipo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoProducto Tipo { get; set; }

    [Display(Name = "Código")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Codigo { get; set; } = null!;

    [Display(Name = "Nombre")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    [Display(Name = "Unidad de Medida")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int UnidadMedidaId { get; set; }

    [Display(Name = "Precio de Venta")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal PrecioVenta { get; set; }

    [Display(Name = "Costo")]
    public decimal? Costo { get; set; }

    [Display(Name = "Impuesto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int ImpuestoId { get; set; }

    [Display(Name = "Controlar Inventario")]
    public bool ControlarInventario { get; set; }

    [Display(Name = "Stock Mínimo")]
    public decimal? StockMinimo { get; set; }

    [Display(Name = "Categoría")]
    public Guid? CategoriaId { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // Soft Delete
    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // Navegación
    public Empresa? Empresa { get; set; }
    public UnidadMedida? UnidadMedida { get; set; }
    public Impuesto? Impuesto { get; set; }
    public Categoria? Categoria { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
