namespace Facturacion.Shared.Enums;

/// <summary>
/// Estado de pago de un gasto
/// </summary>
public enum EstadoPago
{
    /// <summary>
    /// Pago pendiente
    /// </summary>
    Pendiente = 1,

    /// <summary>
    /// Pago completado
    /// </summary>
    Pagado = 2,

    /// <summary>
    /// Pago parcial
    /// </summary>
    Parcial = 3
}
