using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class PrivilegioDto
{
    public int Id { get; set; }

    [Display(Name = "Módulo")]
    public int ModuloId { get; set; }

    [Display(Name = "Nombre del Módulo")]
    public string NombreModulo { get; set; } = null!;

    [Display(Name = "Acción")]
    public AccionPrivilegio Accion { get; set; }

    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }
}
