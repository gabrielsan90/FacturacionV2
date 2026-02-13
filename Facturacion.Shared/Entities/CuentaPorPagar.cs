using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Representa una cuenta por pagar a un proveedor.
/// Registra facturas de proveedores pendientes de pago.
/// </summary>
public class CuentaPorPagar
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
    // Proveedor
    // =====================================================
    [Display(Name = "Proveedor")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProveedorId { get; set; }

    // =====================================================
    // Identificación del Documento
    // =====================================================
    [Display(Name = "Número de Factura")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string NumeroFactura { get; set; } = null!;

    [Display(Name = "Fecha de Factura")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaFactura { get; set; }

    [Display(Name = "Fecha de Vencimiento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaVencimiento { get; set; }

    // =====================================================
    // Montos
    // =====================================================
    [Display(Name = "Monto Original")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoOriginal { get; set; }

    [Display(Name = "Saldo Pendiente")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoSaldo { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: PEN=Pendiente, PAR=Parcial, PAG=Pagada, VEN=Vencida
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "PEN";

    // =====================================================
    // Relaciones Opcionales
    // =====================================================
    [Display(Name = "Orden de Compra")]
    public Guid? OrdenCompraId { get; set; }

    // =====================================================
    // Información Adicional
    // =====================================================
    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Moneda")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Moneda { get; set; } = "CRC";

    [Display(Name = "Tipo de Cambio")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal TipoCambio { get; set; } = 1;

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
    public string EstadoDescripcion => Estado switch
    {
        "PEN" => "Pendiente",
        "PAR" => "Parcial",
        "PAG" => "Pagada",
        "VEN" => "Vencida",
        _ => Estado
    };

    [NotMapped]
    public bool EstaVencida => Estado != "PAG" && FechaVencimiento < DateTime.Now.Date;

    [NotMapped]
    public int DiasVencido
    {
        get
        {
            if (!EstaVencida) return 0;
            return (DateTime.Now.Date - FechaVencimiento.Date).Days;
        }
    }

    [NotMapped]
    public decimal MontoAbonado => MontoOriginal - MontoSaldo;

    [NotMapped]
    public decimal PorcentajePagado => MontoOriginal > 0 ? (MontoAbonado / MontoOriginal) * 100 : 0;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Proveedor? Proveedor { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<AbonoPago>? Abonos { get; set; } = new List<AbonoPago>();
}

/// <summary>
/// Constantes para estados de cuenta por pagar
/// </summary>
public static class EstadoCuentaPorPagar
{
    public const string Pendiente = "PEN";
    public const string Parcial = "PAR";
    public const string Pagada = "PAG";
    public const string Vencida = "VEN";
}
