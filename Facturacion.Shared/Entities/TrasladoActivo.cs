using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Traslados de activos fijos entre sucursales o responsables.
/// Mantiene historial de movimientos del activo.
/// </summary>
public class TrasladoActivo
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

    [Display(Name = "Activo Fijo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ActivoFijoId { get; set; }

    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Fecha { get; set; }

    // =====================================================
    // Origen
    // =====================================================
    [Display(Name = "Sucursal Origen")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SucursalOrigenId { get; set; }

    [Display(Name = "Responsable Origen")]
    public Guid? ResponsableOrigenId { get; set; }

    // =====================================================
    // Destino
    // =====================================================
    [Display(Name = "Sucursal Destino")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SucursalDestinoId { get; set; }

    [Display(Name = "Responsable Destino")]
    public Guid? ResponsableDestinoId { get; set; }

    // =====================================================
    // Detalles
    // =====================================================
    [Display(Name = "Motivo")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Motivo { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Estado y Aprobación
    // =====================================================
    /// <summary>
    /// Estado: PEN=Pendiente, APR=Aprobado, REC=Recibido, CAN=Cancelado
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "PEN";

    [Display(Name = "Fecha de Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Aprobado Por")]
    public string? AprobadoPorId { get; set; }

    [Display(Name = "Fecha de Recepción")]
    public DateTime? FechaRecepcion { get; set; }

    [Display(Name = "Recibido Por")]
    public string? RecibidoPorId { get; set; }

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
    // Propiedades calculadas
    // =====================================================
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "PEN" => "Pendiente",
        "APR" => "Aprobado",
        "REC" => "Recibido",
        "CAN" => "Cancelado",
        _ => Estado
    };

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool EsMismoResponsable => ResponsableOrigenId == ResponsableDestinoId;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool EsMismaSucursal => SucursalOrigenId == SucursalDestinoId;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public ActivoFijo? ActivoFijo { get; set; }
    public Sucursal? SucursalOrigen { get; set; }
    public Sucursal? SucursalDestino { get; set; }
    public Empleado? ResponsableOrigen { get; set; }
    public Empleado? ResponsableDestino { get; set; }
    public User? AprobadoPor { get; set; }
    public User? RecibidoPor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
}
