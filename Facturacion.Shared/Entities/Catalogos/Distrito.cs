using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

public class Distrito
{
    public int Id { get; set; }

    public int CantonId { get; set; }

    [Required]
    [MaxLength(6)]
    public string Codigo { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Descripcion { get; set; } = null!;

    public bool Activo { get; set; } = true;

    // Navigation properties
    public Canton Canton { get; set; } = null!;
    public ICollection<Barrio> Barrios { get; set; } = new List<Barrio>();
}
