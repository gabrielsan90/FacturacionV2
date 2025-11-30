using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

public class Barrio
{
    public int Id { get; set; }

    public int DistritoId { get; set; }

    [Required]
    [MaxLength(9)]
    public string Codigo { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Descripcion { get; set; } = null!;

    public bool Activo { get; set; } = true;

    // Navigation property
    public Distrito Distrito { get; set; } = null!;
}
