using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Control de vacaciones de empleados.
/// </summary>
public class Vacacion
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relación con empleado
    // =====================================================
    [Display(Name = "Empleado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpleadoId { get; set; }

    // =====================================================
    // Período de vacaciones
    // =====================================================
    [Display(Name = "Fecha de Inicio")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaInicio { get; set; }

    [Display(Name = "Fecha de Fin")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaFin { get; set; }

    [Display(Name = "Días Hábiles")]
    public int DiasHabiles { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: SOL=Solicitada, APR=Aprobada, REC=Rechazada, DIS=Disfrutada, CAN=Cancelada
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "SOL";

    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Aprobación
    // =====================================================
    [Display(Name = "Aprobado Por")]
    public string? AprobadoPorId { get; set; }

    [Display(Name = "Fecha de Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Motivo de Rechazo")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MotivoRechazo { get; set; }

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
    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "SOL" => "Solicitada",
        "APR" => "Aprobada",
        "REC" => "Rechazada",
        "DIS" => "Disfrutada",
        "CAN" => "Cancelada",
        _ => Estado
    };

    [NotMapped]
    public bool PuedeAprobar => Estado == "SOL";

    [NotMapped]
    public bool PuedeRechazar => Estado == "SOL";

    [NotMapped]
    public bool PuedeCancelar => Estado == "SOL" || Estado == "APR";

    [NotMapped]
    public int DiasTranscurridos
    {
        get
        {
            if (Estado != "APR" && Estado != "DIS") return 0;
            if (DateTime.Now < FechaInicio) return 0;
            if (DateTime.Now > FechaFin) return DiasHabiles;
            return (DateTime.Now - FechaInicio).Days + 1;
        }
    }

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empleado? Empleado { get; set; }
    public User? AprobadoPor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
}
