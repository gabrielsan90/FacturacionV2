using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// Flujo de caja diario con ingresos, egresos y saldos
/// </summary>
public class FlujoCajaDTO
{
    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime Fecha { get; set; }

    [Display(Name = "Ingresos")]
    public decimal Ingresos { get; set; }

    [Display(Name = "Egresos")]
    public decimal Egresos { get; set; }

    [Display(Name = "Saldo Diario")]
    public decimal SaldoDiario { get; set; }

    [Display(Name = "Saldo Acumulado")]
    public decimal SaldoAcumulado { get; set; }
}
