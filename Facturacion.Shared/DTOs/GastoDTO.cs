using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para crear y actualizar gastos
/// </summary>
public class GastoDTO
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    public Guid? ProveedorId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int CategoriaGastoId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string NumeroDocumento { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaGasto { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Descripcion { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Range(0.00001, 999999999999.99999, ErrorMessage = "El {0} debe ser mayor a cero.")]
    public decimal MontoSubtotal { get; set; }

    [Range(0, 999999999999.99999, ErrorMessage = "El {0} no puede ser negativo.")]
    public decimal MontoImpuesto { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Range(0.00001, 999999999999.99999, ErrorMessage = "El {0} debe ser mayor a cero.")]
    public decimal MontoTotal { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoMoneda Moneda { get; set; }

    public decimal? TipoCambio { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public FormaPago FormaPago { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public EstadoPago EstadoPago { get; set; }

    [Range(0, 999999999999.99999, ErrorMessage = "El {0} no puede ser negativo.")]
    public decimal MontoPagado { get; set; }

    public DateTime? FechaPago { get; set; }

    public string? Comprobante { get; set; }
}
