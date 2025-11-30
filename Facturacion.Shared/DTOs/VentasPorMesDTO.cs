using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// Ventas agrupadas por mes para análisis de tendencias
/// </summary>
public class VentasPorMesDTO
{
    [Display(Name = "Mes")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int Mes { get; set; }

    [Display(Name = "Año")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int Ano { get; set; }

    [Display(Name = "Total de Ventas")]
    public decimal TotalVentas { get; set; }

    [Display(Name = "Cantidad de Documentos")]
    public int CantidadDocumentos { get; set; }

    [Display(Name = "Promedio de Venta")]
    public decimal PromedioVenta { get; set; }
}
