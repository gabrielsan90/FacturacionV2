using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class UpdateUserRolesDto
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public List<string> Roles { get; set; } = new();
}
