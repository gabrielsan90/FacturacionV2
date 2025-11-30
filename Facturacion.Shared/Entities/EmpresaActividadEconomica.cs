using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class EmpresaActividadEconomica
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Empresa")]
    public Guid EmpresaId { get; set; }

    [Display(Name = "Actividad Económica")]
    public int ActividadEconomicaId { get; set; }

    [Display(Name = "Es Principal")]
    public bool EsPrincipal { get; set; }

    // Navegación
    public Empresa? Empresa { get; set; }
    public ActividadEconomica? ActividadEconomica { get; set; }
}
