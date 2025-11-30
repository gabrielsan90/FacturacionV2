using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class RolPrivilegio
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Rol")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string RolId { get; set; } = null!;

    [Display(Name = "Privilegio")]
    public int PrivilegioId { get; set; }

    // Navegación
    public Rol? Rol { get; set; }
    public Privilegio? Privilegio { get; set; }
}
