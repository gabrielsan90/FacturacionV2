using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

public class Provincia
{
    public int Id { get; set; }

    [Required]
    [MaxLength(2)]
    public string Codigo { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Descripcion { get; set; } = null!;

    public bool Activo { get; set; } = true;

    // Navigation property
    public ICollection<Canton> Cantones { get; set; } = new List<Canton>();
}
