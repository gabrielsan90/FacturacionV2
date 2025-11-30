using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities.Catalogos;

public class Canton
{
    public int Id { get; set; }

    public int ProvinciaId { get; set; }

    [Required]
    [MaxLength(4)]
    public string Codigo { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Descripcion { get; set; } = null!;

    public bool Activo { get; set; } = true;

    // Navigation properties
    public Provincia Provincia { get; set; } = null!;
    public ICollection<Distrito> Distritos { get; set; } = new List<Distrito>();
}
