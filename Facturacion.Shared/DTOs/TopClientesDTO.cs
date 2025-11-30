using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// Clientes principales ordenados por volumen de compras
/// </summary>
public class TopClientesDTO
{
    [Display(Name = "Cliente Id")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid ClienteId { get; set; }

    [Display(Name = "Nombre del Cliente")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NombreCliente { get; set; } = null!;

    [Display(Name = "Total de Compras")]
    public decimal TotalCompras { get; set; }

    [Display(Name = "Cantidad de Documentos")]
    public int CantidadDocumentos { get; set; }

    [Display(Name = "Última Compra")]
    public DateTime? UltimaCompra { get; set; }
}
