using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Orden de compra a proveedores.
/// Documento oficial para solicitar productos/servicios.
/// </summary>
public class OrdenCompra
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
    // Proveedor y Destino
    // =====================================================
    [Display(Name = "Proveedor")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProveedorId { get; set; }

    [Display(Name = "Sucursal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SucursalId { get; set; }

    [Display(Name = "Bodega Destino")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid BodegaDestinoId { get; set; }

    // =====================================================
    // Moneda y Totales
    // =====================================================
    [Display(Name = "Moneda")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Moneda { get; set; } = "CRC";

    [Display(Name = "Tipo Cambio")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal TipoCambio { get; set; } = 1;

    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Display(Name = "Descuento")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; }

    [Display(Name = "Impuesto")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Impuesto { get; set; }

    [Display(Name = "Total")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    // =====================================================
    // Entrega y Pago
    // =====================================================
    [Display(Name = "Fecha Entrega Esperada")]
    public DateTime? FechaEntregaEsperada { get; set; }

    /// <summary>
    /// Condición: CON=Contado, CRE=Crédito
    /// </summary>
    [Display(Name = "Condición de Pago")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CondicionPago { get; set; }

    [Display(Name = "Días de Crédito")]
    public int DiasCredito { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: BOR=Borrador, PEN=Pendiente, APR=Aprobada, PAR=Parcial, COM=Completada, ANU=Anulada
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "BOR";

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

    [Display(Name = "Fecha de Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Aprobado Por")]
    public string? AprobadoPorId { get; set; }

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
        "BOR" => "Borrador",
        "PEN" => "Pendiente",
        "APR" => "Aprobada",
        "PAR" => "Parcial",
        "COM" => "Completada",
        "ANU" => "Anulada",
        _ => Estado
    };

    [NotMapped]
    public bool PuedeEditar => Estado == "BOR";

    [NotMapped]
    public bool PuedeAprobar => Estado == "PEN";

    [NotMapped]
    public bool PuedeRecibir => Estado == "APR" || Estado == "PAR";

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Proveedor? Proveedor { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Bodega? BodegaDestino { get; set; }
    public User? CreadoPor { get; set; }
    public User? ModificadoPor { get; set; }
    public User? AprobadoPor { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<OrdenCompraDetalle>? Detalles { get; set; }
    public ICollection<RecepcionCompra>? Recepciones { get; set; }
}
