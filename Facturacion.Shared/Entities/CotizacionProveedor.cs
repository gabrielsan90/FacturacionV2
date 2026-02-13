using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Solicitud de cotización enviada a un proveedor.
/// Permite comparar ofertas de múltiples proveedores.
/// </summary>
public class CotizacionProveedor
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

    [Display(Name = "Requisición")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid RequisicionId { get; set; }

    [Display(Name = "Proveedor")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ProveedorId { get; set; }

    // =====================================================
    // Fechas
    // =====================================================
    [Display(Name = "Fecha Solicitud")]
    public DateTime FechaSolicitud { get; set; }

    [Display(Name = "Fecha Vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    [Display(Name = "Fecha Respuesta")]
    public DateTime? FechaRespuesta { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: ENV=Enviada, REC=Recibida, APR=Aprobada, REJ=Rechazada, VEN=Vencida, SEL=Seleccionada
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "ENV";

    [Display(Name = "Referencia Proveedor")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ReferenciaProveedor { get; set; }

    // =====================================================
    // Montos
    // =====================================================
    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoSubtotal { get; set; }

    [Display(Name = "Impuestos")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoImpuestos { get; set; }

    [Display(Name = "Total")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoTotal { get; set; }

    [Display(Name = "Moneda")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Moneda { get; set; } = "CRC";

    [Display(Name = "Tipo Cambio")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal? TipoCambio { get; set; }

    // =====================================================
    // Condiciones
    // =====================================================
    [Display(Name = "Tiempo Entrega (Días)")]
    public int? TiempoEntregaDias { get; set; }

    [Display(Name = "Validez (Días)")]
    public int? ValidezDias { get; set; }

    [Display(Name = "Condiciones de Pago")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CondicionesPago { get; set; }

    [Display(Name = "Lugar de Entrega")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? LugarEntrega { get; set; }

    [Display(Name = "Incluye Flete")]
    public bool IncluyeFlete { get; set; }

    [Display(Name = "Monto Flete")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoFlete { get; set; }

    // =====================================================
    // Selección
    // =====================================================
    [Display(Name = "Seleccionada")]
    public bool Seleccionada { get; set; }

    [Display(Name = "Puntuación Total")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal? PuntuacionTotal { get; set; }

    // =====================================================
    // Observaciones y Archivo
    // =====================================================
    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Nombre Archivo")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NombreArchivo { get; set; }

    [Display(Name = "Ruta Archivo")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? RutaArchivo { get; set; }

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Registrado Por")]
    public string? RegistradoPorId { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "ENV" => "Enviada",
        "REC" => "Recibida",
        "APR" => "Aprobada",
        "REJ" => "Rechazada",
        "VEN" => "Vencida",
        "SEL" => "Seleccionada",
        _ => Estado
    };

    [NotMapped]
    public bool EstaVencida => FechaVencimiento.HasValue && DateTime.Now > FechaVencimiento.Value && Estado == "ENV";

    [NotMapped]
    public int? DiasParaVencer => FechaVencimiento.HasValue
        ? (int)(FechaVencimiento.Value - DateTime.Now).TotalDays
        : null;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Requisicion? Requisicion { get; set; }
    public Proveedor? Proveedor { get; set; }
    public User? RegistradoPor { get; set; }
    public User? UsuarioModificacion { get; set; }

    public ICollection<CotizacionProveedorDetalle>? Detalles { get; set; }
}
