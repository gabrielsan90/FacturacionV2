using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Acciones de personal: promociones, aumentos, traslados, amonestaciones, etc.
/// Mantiene el historial de cambios laborales del empleado.
/// </summary>
public class AccionPersonal
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
    // Tipo y descripción
    // =====================================================
    /// <summary>
    /// Tipo: PRO=Promoción, AUM=Aumento, TRA=Traslado, AMO=Amonestación,
    /// SUS=Suspensión, CAP=Capacitación, CAM=Cambio Contrato, REI=Reingreso, BAJ=Baja, OTR=Otro
    /// </summary>
    [Display(Name = "Tipo de Acción")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoAccion { get; set; } = null!;

    [Display(Name = "Fecha de Acción")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaAccion { get; set; }

    [Display(Name = "Fecha Efectiva")]
    public DateTime? FechaEfectiva { get; set; }

    [Display(Name = "Motivo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Motivo { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // =====================================================
    // Datos anteriores (historial)
    // =====================================================
    [Display(Name = "Departamento Anterior")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? DepartamentoAnterior { get; set; }

    [Display(Name = "Puesto Anterior")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PuestoAnterior { get; set; }

    [Display(Name = "Salario Anterior")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalarioAnterior { get; set; }

    [Display(Name = "Estado Anterior")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? EstadoAnterior { get; set; }

    // =====================================================
    // Datos nuevos
    // =====================================================
    [Display(Name = "Departamento Nuevo")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? DepartamentoNuevo { get; set; }

    [Display(Name = "Puesto Nuevo")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PuestoNuevo { get; set; }

    [Display(Name = "Salario Nuevo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalarioNuevo { get; set; }

    [Display(Name = "Estado Nuevo")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? EstadoNuevo { get; set; }

    // =====================================================
    // Para suspensiones/amonestaciones
    // =====================================================
    [Display(Name = "Días Afectados")]
    public int? DiasAfectados { get; set; }

    [Display(Name = "Monto Afectado")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoAfectado { get; set; }

    // =====================================================
    // Documentación
    // =====================================================
    [Display(Name = "Archivo Adjunto")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ArchivoAdjunto { get; set; }

    // =====================================================
    // Estado de la acción
    // =====================================================
    /// <summary>
    /// Estado: PEN=Pendiente, APR=Aprobada, APL=Aplicada, REC=Rechazada, ANU=Anulada
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "PEN";

    // =====================================================
    // Aprobación
    // =====================================================
    [Display(Name = "Aprobado Por")]
    public Guid? AprobadoPorId { get; set; }

    [Display(Name = "Fecha de Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Observaciones de Aprobación")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ObservacionesAprobacion { get; set; }

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
    public string TipoAccionDescripcion => TipoAccion switch
    {
        "PRO" => "Promoción",
        "AUM" => "Aumento",
        "TRA" => "Traslado",
        "AMO" => "Amonestación",
        "SUS" => "Suspensión",
        "CAP" => "Capacitación",
        "CAM" => "Cambio de Contrato",
        "REI" => "Reingreso",
        "BAJ" => "Baja",
        "OTR" => "Otro",
        _ => TipoAccion
    };

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "PEN" => "Pendiente",
        "APR" => "Aprobada",
        "APL" => "Aplicada",
        "REC" => "Rechazada",
        "ANU" => "Anulada",
        _ => Estado
    };

    [NotMapped]
    public decimal? DiferenciaSalarial => SalarioNuevo.HasValue && SalarioAnterior.HasValue
        ? SalarioNuevo.Value - SalarioAnterior.Value
        : null;

    [NotMapped]
    public decimal? PorcentajeAumento => SalarioNuevo.HasValue && SalarioAnterior.HasValue && SalarioAnterior.Value > 0
        ? ((SalarioNuevo.Value - SalarioAnterior.Value) / SalarioAnterior.Value) * 100
        : null;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empleado? Empleado { get; set; }
    public Empleado? AprobadoPor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
}
