namespace Facturacion.Shared.DTOs;

/// <summary>
/// Respuesta de la API de Hacienda al enviar o consultar un documento
/// </summary>
public class HaciendaRespuesta
{
    /// <summary>
    /// Clave numérica del documento (50 dígitos)
    /// </summary>
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Fecha de la respuesta
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Indicador de estado: "aceptado", "rechazado", "procesando"
    /// </summary>
    public string IndEstado { get; set; } = null!;

    /// <summary>
    /// XML de respuesta completo de Hacienda
    /// </summary>
    public string? RespuestaXml { get; set; }

    /// <summary>
    /// Lista de mensajes (errores, advertencias, info)
    /// </summary>
    public List<HaciendaMensaje> Mensajes { get; set; } = new List<HaciendaMensaje>();
}
