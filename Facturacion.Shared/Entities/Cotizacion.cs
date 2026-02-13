using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Cotización o presupuesto para un cliente
/// Puede convertirse en un Documento (factura) cuando es aprobada
/// </summary>
public class Cotizacion
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIONES CON ARQUITECTURA MULTI-TENANT
    // ========================================

    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    [Display(Name = "Sucursal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SucursalId { get; set; }

    [Display(Name = "Terminal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid TerminalId { get; set; }

    // ========================================
    // NUMERACIÓN
    // ========================================

    /// <summary>
    /// Número consecutivo de la cotización
    /// Formato: COT-YYYY-MM-NNNN (ejemplo: COT-2025-02-0001)
    /// </summary>
    [Display(Name = "Número de Cotización")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Numero { get; set; } = null!;

    // ========================================
    // ESTADO Y FECHAS
    // ========================================

    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public EstadoCotizacion Estado { get; set; }

    [Display(Name = "Fecha de Emisión")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Fecha hasta la cual la cotización es válida
    /// Después de esta fecha, pasa automáticamente a estado Vencida
    /// </summary>
    [Display(Name = "Fecha de Vencimiento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaVencimiento { get; set; }

    /// <summary>
    /// Fecha en que se envió la cotización al cliente
    /// </summary>
    [Display(Name = "Fecha de Envío")]
    public DateTime? FechaEnvio { get; set; }

    /// <summary>
    /// Fecha en que el cliente aprobó la cotización
    /// </summary>
    [Display(Name = "Fecha de Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    /// <summary>
    /// Fecha en que el cliente rechazó la cotización
    /// </summary>
    [Display(Name = "Fecha de Rechazo")]
    public DateTime? FechaRechazo { get; set; }

    /// <summary>
    /// Fecha en que se convirtió a factura (documento)
    /// </summary>
    [Display(Name = "Fecha de Conversión")]
    public DateTime? FechaConversion { get; set; }

    // ========================================
    // RECEPTOR (Cliente)
    // ========================================

    [Display(Name = "Cliente")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Tipo de identificación del cliente (duplicado para histórico)
    /// </summary>
    [Display(Name = "Cliente - Tipo de Identificación")]
    public TipoIdentificacion? ClienteTipoIdentificacion { get; set; }

    /// <summary>
    /// Número de identificación del cliente (duplicado para histórico)
    /// </summary>
    [Display(Name = "Cliente - Número de Identificación")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ClienteNumeroIdentificacion { get; set; }

    /// <summary>
    /// Nombre del cliente (duplicado para histórico)
    /// </summary>
    [Display(Name = "Cliente - Nombre")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ClienteNombre { get; set; }

    /// <summary>
    /// Email del cliente
    /// </summary>
    [Display(Name = "Cliente - Email")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [EmailAddress(ErrorMessage = "El campo {0} debe ser un email válido.")]
    public string? ClienteEmail { get; set; }

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    [Display(Name = "Cliente - Teléfono")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ClienteTelefono { get; set; }

    // ========================================
    // CONDICIONES COMERCIALES
    // ========================================

    /// <summary>
    /// Condición de venta (01-Contado, 02-Crédito, etc.)
    /// </summary>
    [Display(Name = "Condición de Venta")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string CondicionVenta { get; set; } = null!;

    /// <summary>
    /// Plazo de crédito en días (si CondicionVenta = 02)
    /// </summary>
    [Display(Name = "Plazo de Crédito (días)")]
    public int? PlazoCreditoDias { get; set; }

    /// <summary>
    /// Medio de pago principal
    /// </summary>
    [Display(Name = "Medio de Pago")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string MedioPago { get; set; } = null!;

    // ========================================
    // MONEDA Y TIPO DE CAMBIO
    // ========================================

    [Display(Name = "Moneda")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoMoneda Moneda { get; set; }

    /// <summary>
    /// Tipo de cambio (obligatorio si Moneda != CRC)
    /// Hasta 5 decimales
    /// </summary>
    [Display(Name = "Tipo de Cambio")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? TipoCambio { get; set; }

    // ========================================
    // TOTALES
    // ========================================

    /// <summary>
    /// Subtotal (antes de impuestos y descuentos)
    /// </summary>
    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Total de descuentos aplicados
    /// </summary>
    [Display(Name = "Total Descuentos")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalDescuentos { get; set; }

    /// <summary>
    /// Total de impuestos
    /// </summary>
    [Display(Name = "Total Impuestos")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalImpuestos { get; set; }

    /// <summary>
    /// Total de la cotización (Subtotal - Descuentos + Impuestos)
    /// </summary>
    [Display(Name = "Total")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Total { get; set; }

    // ========================================
    // INFORMACIÓN ADICIONAL
    // ========================================

    /// <summary>
    /// Notas o condiciones generales de la cotización
    /// </summary>
    [Display(Name = "Notas")]
    [MaxLength(2000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Notas { get; set; }

    /// <summary>
    /// Observaciones internas (no visibles para el cliente)
    /// </summary>
    [Display(Name = "Observaciones Internas")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ObservacionesInternas { get; set; }

    // ========================================
    // CONVERSIÓN A DOCUMENTO
    // ========================================

    /// <summary>
    /// ID del documento (factura) generado a partir de esta cotización
    /// </summary>
    [Display(Name = "Documento Generado")]
    public Guid? DocumentoGeneradoId { get; set; }

    // ========================================
    // AUDIT TRAIL
    // ========================================

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // ========================================
    // SOFT DELETE
    // ========================================

    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================

    public Empresa? Empresa { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Terminal? Terminal { get; set; }
    public Cliente? Cliente { get; set; }
    public Documento? DocumentoGenerado { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    /// <summary>
    /// Líneas de detalle de la cotización
    /// </summary>
    public ICollection<CotizacionDetalle> Detalles { get; set; } = new List<CotizacionDetalle>();
}
