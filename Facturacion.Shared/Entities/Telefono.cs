using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class Telefono
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Empresa")]
    public Guid? EmpresaId { get; set; }

    [Display(Name = "Cliente")]
    public Guid? ClienteId { get; set; }

    [Display(Name = "Proveedor")]
    public Guid? ProveedorId { get; set; }

    [Display(Name = "Código País")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string CodigoPais { get; set; } = null!;

    [Display(Name = "Número de Teléfono")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NumeroTelefono { get; set; } = null!;

    [Display(Name = "Es Principal")]
    public bool EsPrincipal { get; set; }

    [Display(Name = "Tipo")]
    public TipoTelefono Tipo { get; set; }

    // Navegación
    public Empresa? Empresa { get; set; }
    public Cliente? Cliente { get; set; }
    public Proveedor? Proveedor { get; set; }
}
