namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para consultar el tipo de cambio oficial del BCCR
/// Recomendación 9 de Hacienda v4.4
/// </summary>
public interface ITipoCambioBCCRService
{
    /// <summary>
    /// Obtiene el tipo de cambio de compra del dólar para una fecha específica
    /// </summary>
    /// <param name="fecha">Fecha para la cual obtener el tipo de cambio</param>
    /// <returns>Tipo de cambio de compra</returns>
    Task<decimal> ObtenerTipoCambioCompraAsync(DateTime fecha);

    /// <summary>
    /// Obtiene el tipo de cambio de venta del dólar para una fecha específica
    /// </summary>
    /// <param name="fecha">Fecha para la cual obtener el tipo de cambio</param>
    /// <returns>Tipo de cambio de venta</returns>
    Task<decimal> ObtenerTipoCambioVentaAsync(DateTime fecha);

    /// <summary>
    /// Obtiene ambos tipos de cambio (compra y venta) para una fecha específica
    /// </summary>
    /// <param name="fecha">Fecha para la cual obtener el tipo de cambio</param>
    /// <returns>Tupla con tipo de cambio de compra y venta</returns>
    Task<(decimal Compra, decimal Venta)> ObtenerTiposCambioAsync(DateTime fecha);

    /// <summary>
    /// Obtiene el tipo de cambio de venta para una moneda específica
    /// </summary>
    /// <param name="codigoMoneda">Código de moneda (USD, EUR, etc.)</param>
    /// <param name="fecha">Fecha para la cual obtener el tipo de cambio</param>
    /// <returns>Tipo de cambio de venta</returns>
    Task<decimal> ObtenerTipoCambioMonedaAsync(string codigoMoneda, DateTime fecha);
}
