using System.ComponentModel.DataAnnotations;
using Facturacion.Shared.Enums;

namespace Facturacion.Shared.Entities;

public class Consecutivo
{
    [Key]
    public Guid Id { get; set; }

    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    [Display(Name = "Sucursal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SucursalId { get; set; }

    [Display(Name = "Terminal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid TerminalId { get; set; }

    [Display(Name = "Tipo de Documento")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string TipoDocumento { get; set; } = null!;

    /// <summary>
    /// Ambiente de Hacienda para el cual aplica este consecutivo
    /// stag = Pruebas/Sandbox, prod = Producción
    /// Cada terminal debe tener consecutivos separados para pruebas y producción
    /// </summary>
    [Display(Name = "Ambiente")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Ambiente Ambiente { get; set; }

    [Display(Name = "Descripción")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    [Display(Name = "Clave Numeración Consecutiva")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string ClaveNumeracion { get; set; } = null!;

    [Display(Name = "Número Inicio")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public long NumeroInicio { get; set; }

    [Display(Name = "Número Fin")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public long NumeroFin { get; set; }

    [Display(Name = "Número Consecutivo Actual")]
    public long NumeroActual { get; set; }

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
    public Empresa? Empresa { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Terminal? Terminal { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }
}
