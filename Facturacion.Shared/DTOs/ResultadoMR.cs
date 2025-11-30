using Facturacion.Shared.Enums;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// Resultado del proceso de generación y envío de Mensaje Receptor
/// </summary>
public class ResultadoMR
{
    /// <summary>
    /// Indica si el proceso fue exitoso
    /// </summary>
    public bool Exitoso { get; set; }

    /// <summary>
    /// Mensaje principal del resultado
    /// </summary>
    public string Mensaje { get; set; } = null!;

    /// <summary>
    /// Clave del Mensaje Receptor generado (50 dígitos)
    /// </summary>
    public string? ClaveMensaje { get; set; }

    /// <summary>
    /// Tipo de mensaje enviado
    /// </summary>
    public TipoMensajeReceptor? TipoMensaje { get; set; }

    /// <summary>
    /// Código de mensaje enviado
    /// </summary>
    public CodigoMensajeReceptor? CodigoMensaje { get; set; }

    /// <summary>
    /// Estado del mensaje en Hacienda
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// ID del DocumentoReceptorMensaje creado
    /// </summary>
    public Guid? MensajeId { get; set; }

    /// <summary>
    /// Clave del documento original
    /// </summary>
    public string? ClaveDocumentoOriginal { get; set; }

    /// <summary>
    /// Fecha de envío a Hacienda
    /// </summary>
    public DateTime? FechaEnvio { get; set; }

    /// <summary>
    /// Errores que impidieron el envío
    /// </summary>
    public List<string> Errores { get; set; } = new();

    /// <summary>
    /// XML generado del mensaje
    /// </summary>
    public string? XmlGenerado { get; set; }

    /// <summary>
    /// XML firmado del mensaje
    /// </summary>
    public string? XmlFirmado { get; set; }

    /// <summary>
    /// Respuesta de Hacienda
    /// </summary>
    public string? RespuestaHacienda { get; set; }
}
