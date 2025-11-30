using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Registro inmutable de movimientos de inventario
/// Cada cambio en CantidadActual debe generar un MovimientoInventario
/// NO tiene soft delete - los movimientos son permanentes para auditoría
/// </summary>
public class MovimientoInventario
{
    [Key]
    public Guid Id { get; set; }

    [Display(Name = "Inventario")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid InventarioId { get; set; }

    [Display(Name = "Tipo de Movimiento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoMovimientoInventario TipoMovimiento { get; set; }

    [Display(Name = "Cantidad")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Cantidad { get; set; }

    [Display(Name = "Cantidad Anterior")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal CantidadAnterior { get; set; }

    [Display(Name = "Cantidad Nueva")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal CantidadNueva { get; set; }

    [Display(Name = "Referencia")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Referencia { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Fecha del Movimiento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Fecha { get; set; }

    // Foreign Keys para trazabilidad (opcionales)
    [Display(Name = "Sucursal Origen")]
    public Guid? SucursalOrigenId { get; set; }

    [Display(Name = "Sucursal Destino")]
    public Guid? SucursalDestinoId { get; set; }

    [Display(Name = "Cliente")]
    public Guid? ClienteId { get; set; }

    [Display(Name = "Proveedor")]
    public Guid? ProveedorId { get; set; }

    [Display(Name = "Documento")]
    public Guid? DocumentoId { get; set; }

    // Audit Trail (solo creación - NO modificación ni eliminación)
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string UsuarioCreacionId { get; set; } = null!;

    // Navigation Properties
    public Inventario? Inventario { get; set; }
    public Sucursal? SucursalOrigen { get; set; }
    public Sucursal? SucursalDestino { get; set; }
    public Cliente? Cliente { get; set; }
    public Proveedor? Proveedor { get; set; }
    public User? UsuarioCreacion { get; set; }
}
