namespace Facturacion.Shared.Enums;

/// <summary>
/// Estados de una cuenta por cobrar
/// </summary>
public enum EstadoCuentaPorCobrar
{
    /// <summary>
    /// Cuenta sin pagos aplicados
    /// </summary>
    Pendiente = 1,

    /// <summary>
    /// Cuenta con pagos parciales aplicados
    /// </summary>
    Parcial = 2,

    /// <summary>
    /// Cuenta totalmente pagada
    /// </summary>
    Pagada = 3,

    /// <summary>
    /// Cuenta vencida sin pagar
    /// </summary>
    Vencida = 4,

    /// <summary>
    /// Cuenta dada de baja por castigo contable
    /// </summary>
    CastigoBaja = 5
}
