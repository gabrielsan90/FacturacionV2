using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class ModuloPrivilegiosDto
{
    public int ModuloId { get; set; }

    [Display(Name = "Nombre del Módulo")]
    public string NombreModulo { get; set; } = null!;

    [Display(Name = "Orden")]
    public int Orden { get; set; }

    [Display(Name = "Icono")]
    public string? Icono { get; set; }

    [Display(Name = "Privilegios")]
    public List<PrivilegioDto> Privilegios { get; set; } = new();
}
