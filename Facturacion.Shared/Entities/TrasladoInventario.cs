using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Traslado de inventario entre bodegas.
/// Permite mover productos de una bodega origen a una bodega destino.
/// </summary>
public class TrasladoInventario
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

    // =====================================================
    // Bodegas origen y destino
    // =====================================================
    [Display(Name = "Bodega Origen")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid BodegaOrigenId { get; set; }

    [Display(Name = "Bodega Destino")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid BodegaDestinoId { get; set; }

    // =====================================================
    // Estado y fechas
    // =====================================================
    /// <summary>
    /// Estado: PEN=Pendiente, TRA=En Tránsito, REC=Recibido, ANU=Anulado
    /// </summary>
    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "PEN";

    [Display(Name = "Fecha de Envío")]
    public DateTime? FechaEnvio { get; set; }

    [Display(Name = "Fecha de Recepción")]
    public DateTime? FechaRecepcion { get; set; }

    [Display(Name = "Enviado Por")]
    public string? EnviadoPorId { get; set; }

    [Display(Name = "Recibido Por")]
    public string? RecibidoPorId { get; set; }

    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
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
    public bool PuedeEnviar => Estado == "PEN";

    [NotMapped]
    public bool PuedeRecibir => Estado == "TRA";

    [NotMapped]
    public bool PuedeAnular => Estado == "PEN" || Estado == "TRA";

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "PEN" => "Pendiente",
        "TRA" => "En Tránsito",
        "REC" => "Recibido",
        "ANU" => "Anulado",
        _ => Estado
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Bodega? BodegaOrigen { get; set; }
    public Bodega? BodegaDestino { get; set; }
    public User? EnviadoPor { get; set; }
    public User? RecibidoPor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<TrasladoInventarioDetalle>? Detalles { get; set; } = new List<TrasladoInventarioDetalle>();
}
