using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class RolDto
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Display(Name = "Es Sistema")]
    public bool EsSistema { get; set; }

    [Display(Name = "Empresa")]
    public Guid? EmpresaId { get; set; }

    [Display(Name = "Empresa")]
    public string? EmpresaNombre { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; }

    [Display(Name = "Cantidad de Privilegios")]
    public int CantidadPrivilegios { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }
}
