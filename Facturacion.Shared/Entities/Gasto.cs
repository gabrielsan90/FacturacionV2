using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Registro de gastos empresariales con control de pagos y aprobaciones
/// </summary>
public class Gasto
{
    [Key]
    public Guid Id { get; set; }

    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    [Display(Name = "Proveedor")]
    public Guid? ProveedorId { get; set; }

    [Display(Name = "Categoría de Gasto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int CategoriaGastoId { get; set; }

    [Display(Name = "Número de Documento")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NumeroDocumento { get; set; } = null!;

    [Display(Name = "Fecha del Gasto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaGasto { get; set; }

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Descripcion { get; set; } = null!;

    [Display(Name = "Subtotal")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Column(TypeName = "decimal(18,5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal MontoSubtotal { get; set; }

    [Display(Name = "Impuesto")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Column(TypeName = "decimal(18,5)")]
    public decimal MontoImpuesto { get; set; }

    [Display(Name = "Total")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Column(TypeName = "decimal(18,5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal MontoTotal { get; set; }

    [Display(Name = "Moneda")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoMoneda Moneda { get; set; }

    [Display(Name = "Tipo de Cambio")]
    [DisplayFormat(DataFormatString = "{0:N5}")]
    [Column(TypeName = "decimal(18,5)")]
    public decimal? TipoCambio { get; set; }

    [Display(Name = "Forma de Pago")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public FormaPago FormaPago { get; set; }

    [Display(Name = "Estado de Pago")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public EstadoPago EstadoPago { get; set; }

    [Display(Name = "Monto Pagado")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Column(TypeName = "decimal(18,5)")]
    public decimal MontoPagado { get; set; }

    [Display(Name = "Saldo Pendiente")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Column(TypeName = "decimal(18,5)")]
    public decimal SaldoPendiente { get; set; }

    [Display(Name = "Fecha de Pago")]
    public DateTime? FechaPago { get; set; }

    [Display(Name = "Aprobado")]
    public bool Aprobado { get; set; }

    [Display(Name = "Usuario Aprobación")]
    public string? UsuarioAprobacionId { get; set; }

    [Display(Name = "Fecha de Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Notas de Aprobación")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NotasAprobacion { get; set; }

    [Display(Name = "Retención Renta")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoRetencionRenta { get; set; }

    [Display(Name = "Retención IVA")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoRetencionIVA { get; set; }

    [Display(Name = "Comprobante")]
    public string? Comprobante { get; set; }

    // Auditoría
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
    public Proveedor? Proveedor { get; set; }
    public CategoriaGasto? CategoriaGasto { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
    public User? UsuarioAprobacion { get; set; }
}
