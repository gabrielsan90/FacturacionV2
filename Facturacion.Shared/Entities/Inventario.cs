using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Inventario de productos por sucursal
/// Registro único por producto por sucursal (composite unique index)
/// Sólo aplica para productos con ControlarInventario=true
/// </summary>
public class Inventario
{
    [Key]
    public Guid Id { get; set; }

    [Display(Name = "Producto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProductoId { get; set; }

    [Display(Name = "Sucursal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SucursalId { get; set; }

    /// <summary>
    /// Bodega específica dentro de la sucursal (opcional para compatibilidad con datos existentes)
    /// </summary>
    [Display(Name = "Bodega")]
    public Guid? BodegaId { get; set; }

    /// <summary>
    /// Lote del producto (opcional, para productos con control de lotes)
    /// </summary>
    [Display(Name = "Lote")]
    public Guid? LoteId { get; set; }

    [Display(Name = "Cantidad Actual")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal CantidadActual { get; set; }

    [Display(Name = "Cantidad Reservada")]
    public decimal? CantidadReservada { get; set; }

    /// <summary>
    /// Costo promedio ponderado del producto en esta ubicación
    /// </summary>
    [Display(Name = "Costo Promedio")]
    [Column(TypeName = "decimal(18,6)")]
    public decimal CostoPromedio { get; set; }

    [Display(Name = "Última Actualización")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime UltimaActualizacion { get; set; }

    // Audit Trail
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

    // Computed Property - NOT mapped to database
    [NotMapped]
    [Display(Name = "Cantidad Disponible")]
    public decimal CantidadDisponible => CantidadActual - (CantidadReservada ?? 0);

    // Navigation Properties
    public Producto? Producto { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Bodega? Bodega { get; set; }
    public Lote? Lote { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<MovimientoInventario>? Movimientos { get; set; } = new List<MovimientoInventario>();
}
