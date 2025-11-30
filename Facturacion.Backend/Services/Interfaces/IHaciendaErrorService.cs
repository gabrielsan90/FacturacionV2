namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para manejar y traducir errores de Hacienda
/// Recomendación 13 de Hacienda v4.4
/// </summary>
public interface IHaciendaErrorService
{
    /// <summary>
    /// Obtiene un mensaje amigable para un código de error de Hacienda
    /// </summary>
    /// <param name="codigoError">Código de error de Hacienda</param>
    /// <returns>Mensaje amigable para el usuario</returns>
    string ObtenerMensajeAmigable(string codigoError);

    /// <summary>
    /// Obtiene la acción recomendada para un código de error
    /// </summary>
    /// <param name="codigoError">Código de error de Hacienda</param>
    /// <returns>Acción recomendada</returns>
    string ObtenerAccionRecomendada(string codigoError);

    /// <summary>
    /// Determina si el error es recuperable (se puede reintentar)
    /// </summary>
    /// <param name="codigoError">Código de error</param>
    /// <returns>True si se puede reintentar</returns>
    bool EsErrorRecuperable(string codigoError);

    /// <summary>
    /// Parsea un mensaje de error de Hacienda y extrae información estructurada
    /// </summary>
    /// <param name="mensajeHacienda">Mensaje de Hacienda</param>
    /// <returns>Información estructurada del error</returns>
    ErrorHaciendaInfo ParsearMensajeHacienda(string mensajeHacienda);
}

/// <summary>
/// Información estructurada de un error de Hacienda
/// </summary>
public class ErrorHaciendaInfo
{
    public string CodigoError { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string MensajeAmigable { get; set; } = string.Empty;
    public string AccionRecomendada { get; set; } = string.Empty;
    public bool EsRecuperable { get; set; }
    public string? CampoAfectado { get; set; }
    public string? DetallesTecnicos { get; set; }
}
