using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Detalle de evaluación por cotización en un comparativo.
/// </summary>
public class ComparativoCotizacionDetalle
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relaciones
    // =====================================================
    [Display(Name = "Comparativo")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ComparativoCotizacionId { get; set; }

    [Display(Name = "Cotización")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CotizacionProveedorId { get; set; }

    // =====================================================
    // Puntuaciones (0-100)
    // =====================================================
    [Display(Name = "Puntuación Precio")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PuntuacionPrecio { get; set; }

    [Display(Name = "Puntuación Tiempo Entrega")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PuntuacionTiempoEntrega { get; set; }

    [Display(Name = "Puntuación Calidad")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PuntuacionCalidad { get; set; }

    [Display(Name = "Puntuación Condiciones Pago")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PuntuacionCondicionesPago { get; set; }

    [Display(Name = "Puntuación Total")]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PuntuacionTotal { get; set; }

    // =====================================================
    // Ranking
    // =====================================================
    [Display(Name = "Ranking")]
    public int Ranking { get; set; }

    [Display(Name = "Cumple Requisitos")]
    public bool CumpleRequisitos { get; set; } = true;

    [Display(Name = "Motivo No Cumple")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? MotivoNoCumple { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public bool EsMejorOpcion => Ranking == 1 && CumpleRequisitos;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public ComparativoCotizacion? ComparativoCotizacion { get; set; }
    public CotizacionProveedor? CotizacionProveedor { get; set; }
}
