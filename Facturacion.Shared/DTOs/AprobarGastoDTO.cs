using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para aprobar o rechazar un gasto
/// </summary>
public class AprobarGastoDTO
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid GastoId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public bool Aprobado { get; set; }

    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Notas { get; set; }
}
