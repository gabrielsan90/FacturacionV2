using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Registro de comparación y selección de cotizaciones.
/// Documenta el proceso de adjudicación.
/// </summary>
public class ComparativoCotizacion
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

    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; }

    // =====================================================
    // Estado
    // =====================================================
    /// <summary>
    /// Estado: BOR=Borrador, REV=En Revisión, APR=Aprobado, ANU=Anulado
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "BOR";

    // =====================================================
    // Selección
    // =====================================================
    [Display(Name = "Cotización Seleccionada")]
    public Guid? CotizacionSeleccionadaId { get; set; }

    [Display(Name = "Justificación de Selección")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? JustificacionSeleccion { get; set; }

    // =====================================================
    // Criterios de Evaluación (Pesos 0-100)
    // =====================================================
    [Display(Name = "Criterios de Evaluación")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? CriteriosEvaluacion { get; set; }

    [Display(Name = "Peso Precio")]
    public int PesoPrecio { get; set; } = 40;

    [Display(Name = "Peso Tiempo Entrega")]
    public int PesoTiempoEntrega { get; set; } = 20;

    [Display(Name = "Peso Calidad")]
    public int PesoCalidad { get; set; } = 25;

    [Display(Name = "Peso Condiciones Pago")]
    public int PesoCondicionesPago { get; set; } = 15;

    // =====================================================
    // Aprobación
    // =====================================================
    [Display(Name = "Realizado Por")]
    public string? RealizadoPorId { get; set; }

    [Display(Name = "Aprobado Por")]
    public string? AprobadoPorId { get; set; }

    [Display(Name = "Fecha Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Orden de Compra generada
    // =====================================================
    [Display(Name = "Orden de Compra")]
    public Guid? OrdenCompraId { get; set; }

    // =====================================================
    // Audit Trail
    // =====================================================
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
        "BOR" => "Borrador",
        "REV" => "En Revisión",
        "APR" => "Aprobado",
        "ANU" => "Anulado",
        _ => Estado
    };

    [NotMapped]
    public int TotalPesos => PesoPrecio + PesoTiempoEntrega + PesoCalidad + PesoCondicionesPago;

    [NotMapped]
    public bool PesosValidos => TotalPesos == 100;

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Requisicion? Requisicion { get; set; }
    public CotizacionProveedor? CotizacionSeleccionada { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }
    public User? RealizadoPor { get; set; }
    public User? AprobadoPor { get; set; }
    public User? UsuarioModificacion { get; set; }

    public ICollection<ComparativoCotizacionDetalle>? Detalles { get; set; }
}
