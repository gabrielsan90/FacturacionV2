namespace Facturacion.Shared.Enums;

/// <summary>
/// Forma de pago para gastos
/// </summary>
public enum FormaPago
{
    /// <summary>
    /// Efectivo
    /// </summary>
    Efectivo = 1,

    /// <summary>
    /// Transferencia bancaria
    /// </summary>
    Transferencia = 2,

    /// <summary>
    /// Tarjeta de crédito o débito
    /// </summary>
    Tarjeta = 3,

    /// <summary>
    /// Cheque
    /// </summary>
    Cheque = 4
}
