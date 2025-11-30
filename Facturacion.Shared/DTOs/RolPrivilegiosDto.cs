using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class RolPrivilegiosDto
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string RolId { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Privilegios")]
    public List<int> PrivilegioIds { get; set; } = new();
}
