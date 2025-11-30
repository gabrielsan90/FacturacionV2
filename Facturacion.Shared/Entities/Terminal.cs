using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class Terminal
{
    [Key]
    public Guid Id { get; set; }

    [Display(Name = "Sucursal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SucursalId { get; set; }

    [Display(Name = "Código")]
    [MaxLength(10, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Codigo { get; set; } = null!;

    [Display(Name = "Nombre")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Nombre { get; set; } = null!;

    // Control
    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // Audit Trail
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // Soft Delete
    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // Navigation Properties
    public Sucursal? Sucursal { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<Consecutivo>? Consecutivos { get; set; } = new List<Consecutivo>();
    public ICollection<Documento>? Documentos { get; set; } = new List<Documento>();
}
