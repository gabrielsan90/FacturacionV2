using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Historial de cambios de etapa en una oportunidad CRM.
/// Registra cada transición de etapa para análisis de velocidad del pipeline.
/// </summary>
public class HistorialEtapaOportunidad
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relación con oportunidad
    // =====================================================
    [Display(Name = "Oportunidad")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid OportunidadId { get; set; }

    // =====================================================
    // Datos del cambio
    // =====================================================
    [Display(Name = "Etapa Anterior")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? EtapaAnterior { get; set; }

    [Display(Name = "Etapa Nueva")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string EtapaNueva { get; set; } = null!;

    [Display(Name = "Etapa Anterior (Referencia)")]
    public Guid? EtapaAnteriorId { get; set; }

    [Display(Name = "Etapa Nueva (Referencia)")]
    public Guid? EtapaNuevaId { get; set; }

    // =====================================================
    // Valores al momento del cambio
    // =====================================================
    [Display(Name = "Monto al Cambio")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoAlCambio { get; set; }

    [Display(Name = "Probabilidad al Cambio (%)")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal ProbabilidadAlCambio { get; set; }

    // =====================================================
    // Métricas de tiempo
    // =====================================================
    [Display(Name = "Días en Etapa Anterior")]
    public int DiasEnEtapaAnterior { get; set; }

    [Display(Name = "Fecha del Cambio")]
    public DateTime FechaCambio { get; set; }

    [Display(Name = "Cambiado Por")]
    public string? CambiadoPorId { get; set; }

    [Display(Name = "Comentario")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Comentario { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public string EtapaAnteriorDescripcion => EtapaAnterior switch
    {
        "PRO" => "Prospecto",
        "CAL" => "Calificado",
        "PRP" => "Propuesta",
        "NEG" => "Negociación",
        "GAN" => "Ganado",
        "PER" => "Perdido",
        null => "(Inicio)",
        _ => EtapaAnterior
    };

    [NotMapped]
    public string EtapaNuevaDescripcion => EtapaNueva switch
    {
        "PRO" => "Prospecto",
        "CAL" => "Calificado",
        "PRP" => "Propuesta",
        "NEG" => "Negociación",
        "GAN" => "Ganado",
        "PER" => "Perdido",
        _ => EtapaNueva
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Oportunidad? Oportunidad { get; set; }
    public EtapaPipeline? EtapaAnteriorNav { get; set; }
    public EtapaPipeline? EtapaNuevaNav { get; set; }
    public User? CambiadoPor { get; set; }
}
