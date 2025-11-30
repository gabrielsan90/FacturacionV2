namespace Facturacion.Shared.DTOs;

/// <summary>
/// Resultado del proceso completo de generación, firma y envío a Hacienda
/// </summary>
public class ResultadoEnvio
{
    /// <summary>
    /// Indica si el proceso fue exitoso
    /// </summary>
    public bool Exitoso { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Mensaje { get; set; } = null!;

    /// <summary>
    /// Clave numérica del documento (50 dígitos)
    /// </summary>
    public string? Clave { get; set; }

    /// <summary>
    /// Estado del documento: "Aceptado", "Rechazado", "Procesando"
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// Lista de errores (si los hay)
    /// </summary>
    public List<string> Errores { get; set; } = new List<string>();

    /// <summary>
    /// XML generado (sin firma)
    /// </summary>
    public string? XmlGenerado { get; set; }

    /// <summary>
    /// XML firmado digitalmente
    /// </summary>
    public string? XmlFirmado { get; set; }

    /// <summary>
    /// Respuesta completa de Hacienda
    /// </summary>
    public HaciendaRespuesta? RespuestaHacienda { get; set; }
}
