namespace Facturacion.Shared.DTOs;

/// <summary>
/// Resultado de la consulta de estado de un documento en Hacienda
/// </summary>
public class ResultadoConsulta
{
    /// <summary>
    /// Indica si la consulta fue exitosa
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
    /// Estado actual del documento en Hacienda
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// Fecha de la consulta
    /// </summary>
    public DateTime FechaConsulta { get; set; }

    /// <summary>
    /// Respuesta completa de Hacienda
    /// </summary>
    public HaciendaRespuesta? RespuestaHacienda { get; set; }
}
