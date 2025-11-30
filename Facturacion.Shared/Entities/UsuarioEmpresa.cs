using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Entities;

public class UsuarioEmpresa
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Usuario")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string UserId { get; set; } = null!;

    [Display(Name = "Empresa")]
    public Guid EmpresaId { get; set; }

    [Display(Name = "Fecha de Asignación")]
    public DateTime FechaAsignacion { get; set; }

    [Display(Name = "Asignado Por")]
    public string? AsignadoPorId { get; set; }

    // Navegación
    public User? User { get; set; }
    public Empresa? Empresa { get; set; }
    public User? AsignadoPor { get; set; }
}
