namespace Facturacion.Shared.Enums;

/// <summary>
/// Estados del ciclo de vida de una cotización
/// </summary>
public enum EstadoCotizacion
{
    /// <summary>
    /// Cotización en edición, no enviada al cliente
    /// </summary>
    Borrador = 1,

    /// <summary>
    /// Cotización enviada al cliente, esperando respuesta
    /// </summary>
    Enviada = 2,

    /// <summary>
    /// Cotización aprobada por el cliente
    /// </summary>
    Aprobada = 3,

    /// <summary>
    /// Cotización rechazada por el cliente
    /// </summary>
    Rechazada = 4,

    /// <summary>
    /// Cotización convertida a factura (documento)
    /// </summary>
    Convertida = 5,

    /// <summary>
    /// Cotización vencida (superó la fecha de vencimiento sin aprobación)
    /// </summary>
    Vencida = 6
}
