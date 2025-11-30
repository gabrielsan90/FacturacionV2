using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class Email
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Empresa")]
    public Guid? EmpresaId { get; set; }

    [Display(Name = "Cliente")]
    public Guid? ClienteId { get; set; }

    [Display(Name = "Proveedor")]
    public Guid? ProveedorId { get; set; }

    [Display(Name = "Dirección de Email")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [EmailAddress(ErrorMessage = "El campo {0} debe ser un email válido.")]
    public string DireccionEmail { get; set; } = null!;

    [Display(Name = "Es Principal")]
    public bool EsPrincipal { get; set; }

    [Display(Name = "Tipo")]
    public TipoEmail Tipo { get; set; }

    // Navegación
    public Empresa? Empresa { get; set; }
    public Cliente? Cliente { get; set; }
    public Proveedor? Proveedor { get; set; }
}
