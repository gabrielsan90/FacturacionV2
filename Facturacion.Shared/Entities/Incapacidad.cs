using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Control de incapacidades médicas de empleados.
/// </summary>
public class Incapacidad
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
    // Tipo y período
    // =====================================================
    /// <summary>
    /// Tipo: ENF=Enfermedad, MAT=Maternidad, PAT=Paternidad, RIE=Riesgo de Trabajo
    /// </summary>
    [Display(Name = "Tipo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Tipo { get; set; } = "ENF";

    [Display(Name = "Fecha de Inicio")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaInicio { get; set; }

    [Display(Name = "Fecha de Fin")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaFin { get; set; }

    [Display(Name = "Días")]
    public int Dias { get; set; }

    // =====================================================
    // Documentación
    // =====================================================
    [Display(Name = "Número de Documento")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroDocumento { get; set; }

    [Display(Name = "Institución Emisora")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? InstitucionEmisora { get; set; }

    [Display(Name = "Diagnóstico")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Diagnostico { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Archivo Adjunto")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ArchivoAdjunto { get; set; }

    // =====================================================
    // Pago (según legislación Costa Rica)
    // =====================================================
    /// <summary>
    /// Porcentaje que paga la CCSS (normalmente 60% a partir del 4to día)
    /// </summary>
    [Display(Name = "% Pago CCSS")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PorcentajePagoCCSS { get; set; }

    /// <summary>
    /// Porcentaje que paga el patrono (normalmente 40% complementario + 100% primeros 3 días)
    /// </summary>
    [Display(Name = "% Pago Patrono")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PorcentajePagoPatrono { get; set; } = 100;

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
    public string TipoDescripcion => Tipo switch
    {
        "ENF" => "Enfermedad",
        "MAT" => "Maternidad",
        "PAT" => "Paternidad",
        "RIE" => "Riesgo de Trabajo",
        _ => Tipo
    };

    [NotMapped]
    public bool EstaActiva => DateTime.Now >= FechaInicio && DateTime.Now <= FechaFin;

    [NotMapped]
    public int DiasTranscurridos
    {
        get
        {
            if (DateTime.Now < FechaInicio) return 0;
            if (DateTime.Now > FechaFin) return Dias;
            return (DateTime.Now - FechaInicio).Days + 1;
        }
    }

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empleado? Empleado { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
}
