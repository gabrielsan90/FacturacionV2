using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class Privilegio
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Módulo")]
    public int ModuloId { get; set; }

    [Display(Name = "Acción")]
    public AccionPrivilegio Accion { get; set; }

    [Display(Name = "Nombre")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Descripcion { get; set; }

    // Navegación
    public Modulo? Modulo { get; set; }
    public ICollection<RolPrivilegio>? RolesPrivilegios { get; set; } = new List<RolPrivilegio>();
}
