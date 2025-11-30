using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// Distribución de ventas por tipo de documento (FE, TE, NC, etc.)
/// </summary>
public class VentasPorTipoDocumentoDTO
{
    [Display(Name = "Tipo de Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string TipoDocumento { get; set; } = null!;

    [Display(Name = "Cantidad de Documentos")]
    public int Cantidad { get; set; }

    [Display(Name = "Total")]
    public decimal Total { get; set; }

    [Display(Name = "Porcentaje")]
    public decimal Porcentaje { get; set; }
}
