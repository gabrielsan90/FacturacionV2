using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Solicitud interna de compra de productos o servicios.
/// Inicia el ciclo de compras y requiere aprobación antes de generar OC.
/// </summary>
public class Requisicion
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
    public DateTime Fecha { get; set; }

    // =====================================================
    // Solicitante y Destino
    // =====================================================
    [Display(Name = "Solicitante")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string SolicitanteId { get; set; } = null!;

    [Display(Name = "Departamento")]
    public Guid? DepartamentoId { get; set; }

    [Display(Name = "Sucursal")]
    public Guid? SucursalId { get; set; }

    // =====================================================
    // Prioridad y Fechas
    // =====================================================
    /// <summary>
    /// Prioridad: ALT=Alta, MED=Media, BAJ=Baja, URG=Urgente
    /// </summary>
    [Display(Name = "Prioridad")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Prioridad { get; set; } = "MED";

    [Display(Name = "Fecha Requerida")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaRequerida { get; set; }

    // =====================================================
    // Justificación y Montos
    // =====================================================
    [Display(Name = "Justificación")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Justificacion { get; set; }

    [Display(Name = "Monto Estimado")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoEstimado { get; set; }

    [Display(Name = "Moneda")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Moneda { get; set; } = "CRC";

    // =====================================================
    // Estado y Aprobación
    // =====================================================
    /// <summary>
    /// Estado: BOR=Borrador, ENV=Enviada, APR=Aprobada, REC=Rechazada,
    /// COT=En Cotización, OC=Orden Compra Generada, ANU=Anulada
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "BOR";

    [Display(Name = "Aprobado Por")]
    public string? AprobadoPorId { get; set; }

    [Display(Name = "Fecha Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

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
    public string PrioridadDescripcion => Prioridad switch
    {
        "ALT" => "Alta",
        "MED" => "Media",
        "BAJ" => "Baja",
        "URG" => "Urgente",
        _ => Prioridad
    };

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "BOR" => "Borrador",
        "ENV" => "Enviada",
        "APR" => "Aprobada",
        "REC" => "Rechazada",
        "COT" => "En Cotización",
        "OC" => "Orden Compra Generada",
        "ANU" => "Anulada",
        _ => Estado
    };

    [NotMapped]
    public bool PuedeEditar => Estado == "BOR";

    [NotMapped]
    public bool PuedeAprobar => Estado == "ENV";

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public User? Solicitante { get; set; }
    public Departamento? Departamento { get; set; }
    public Sucursal? Sucursal { get; set; }
    public User? AprobadoPor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<RequisicionDetalle>? Detalles { get; set; }
    public ICollection<CotizacionProveedor>? Cotizaciones { get; set; }
}
