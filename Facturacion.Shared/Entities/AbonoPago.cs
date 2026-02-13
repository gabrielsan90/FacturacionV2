using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Representa un abono o pago realizado a una cuenta por pagar.
/// Registra cada transacción de pago parcial o total.
/// </summary>
public class AbonoPago
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Relación con Cuenta por Pagar
    // =====================================================
    [Display(Name = "Cuenta por Pagar")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid CuentaPorPagarId { get; set; }

    // =====================================================
    // Información del Pago
    // =====================================================
    [Display(Name = "Fecha de Pago")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaPago { get; set; }

    [Display(Name = "Monto")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    /// <summary>
    /// Método de Pago: EFE=Efectivo, CHE=Cheque, TRA=Transferencia, DEP=Depósito, TAR=Tarjeta
    /// </summary>
    [Display(Name = "Método de Pago")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string MetodoPago { get; set; } = null!;

    [Display(Name = "Número de Referencia")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroReferencia { get; set; }

    // =====================================================
    // Cuenta Bancaria (Opcional)
    // =====================================================
    [Display(Name = "Cuenta Bancaria")]
    public Guid? CuentaBancariaId { get; set; }

    // =====================================================
    // Notas
    // =====================================================
    [Display(Name = "Notas")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Notas { get; set; }

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Registrado Por")]
    public string? RegistradoPorId { get; set; }

    // =====================================================
    // Propiedades Calculadas
    // =====================================================
    [NotMapped]
    public string MetodoPagoDescripcion => MetodoPago switch
    {
        "EFE" => "Efectivo",
        "CHE" => "Cheque",
        "TRA" => "Transferencia",
        "DEP" => "Depósito",
        "TAR" => "Tarjeta",
        _ => MetodoPago
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public CuentaPorPagar? CuentaPorPagar { get; set; }
    public CuentaBancaria? CuentaBancaria { get; set; }
    public User? RegistradoPor { get; set; }
}

/// <summary>
/// Constantes para métodos de pago
/// </summary>
public static class MetodoPago
{
    public const string Efectivo = "EFE";
    public const string Cheque = "CHE";
    public const string Transferencia = "TRA";
    public const string Deposito = "DEP";
    public const string Tarjeta = "TAR";
}
