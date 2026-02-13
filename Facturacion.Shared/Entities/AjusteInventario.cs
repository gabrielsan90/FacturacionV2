using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Ajuste de inventario para corrección de cantidades.
/// Permite ajustes positivos, negativos o conteos físicos.
/// </summary>
public class AjusteInventario
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
    // Datos principales
    // =====================================================
    [Display(Name = "Número")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Numero { get; set; } = null!;

    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Fecha { get; set; }

    [Display(Name = "Bodega")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid BodegaId { get; set; }

    // =====================================================
    // Tipo y motivo
    // =====================================================
    /// <summary>
    /// Tipo de ajuste: POS=Positivo, NEG=Negativo, CON=Conteo Físico
    /// </summary>
    [Display(Name = "Tipo de Ajuste")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoAjuste { get; set; } = null!;

    [Display(Name = "Motivo")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Motivo { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // =====================================================
    // Estado y aprobación
    // =====================================================
    /// <summary>
    /// Estado: PEN=Pendiente, APR=Aprobado, ANU=Anulado
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "PEN";

    [Display(Name = "Fecha de Aprobación")]
    public DateTime? FechaAprobacion { get; set; }

    [Display(Name = "Aprobado Por")]
    public string? AprobadoPorId { get; set; }

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
    public bool PuedeAprobar => Estado == "PEN";

    [NotMapped]
    public bool PuedeAnular => Estado == "PEN";

    [NotMapped]
    public string TipoAjusteDescripcion => TipoAjuste switch
    {
        "POS" => "Ajuste Positivo",
        "NEG" => "Ajuste Negativo",
        "CON" => "Conteo Físico",
        _ => TipoAjuste
    };

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "PEN" => "Pendiente",
        "APR" => "Aprobado",
        "ANU" => "Anulado",
        _ => Estado
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Bodega? Bodega { get; set; }
    public User? AprobadoPor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<AjusteInventarioDetalle>? Detalles { get; set; } = new List<AjusteInventarioDetalle>();
}
