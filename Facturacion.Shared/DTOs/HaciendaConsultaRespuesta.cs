using System.Text.Json.Serialization;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para la respuesta JSON de Hacienda al consultar el estado de un documento
/// Esta es la estructura que devuelve Hacienda cuando se hace GET a /recepcion/{clave}
/// </summary>
public class HaciendaConsultaRespuesta
{
    /// <summary>
    /// Clave numérica del documento (50 dígitos)
    /// </summary>
    [JsonPropertyName("clave")]
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Fecha de la respuesta en formato ISO 8601
    /// Ejemplo: "2024-11-29T10:30:00-06:00"
    /// </summary>
    [JsonPropertyName("fecha")]
    public string Fecha { get; set; } = null!;

    /// <summary>
    /// Indicador de estado del documento
    /// Valores posibles: "aceptado", "rechazado", "procesando", "recibido"
    /// </summary>
    [JsonPropertyName("ind-estado")]
    public string IndEstado { get; set; } = null!;

    /// <summary>
    /// XML de respuesta de Hacienda codificado en Base64
    /// Este XML contiene el mensaje de aceptación/rechazo de Hacienda
    /// Debe decodificarse de Base64 y parsearse como XML
    /// </summary>
    [JsonPropertyName("respuesta-xml")]
    public string? RespuestaXml { get; set; }
}
