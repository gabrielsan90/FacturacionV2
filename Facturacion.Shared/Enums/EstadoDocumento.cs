namespace Facturacion.Shared.Enums;

/// <summary>
/// Estados del documento electrónico en su ciclo de vida
/// </summary>
public enum EstadoDocumento
{
    /// <summary>
    /// Documento en edición, no enviado a Hacienda
    /// </summary>
    Borrador = 1,

    /// <summary>
    /// Documento generado, listo para enviar a Hacienda
    /// </summary>
    Pendiente = 2,

    /// <summary>
    /// Documento enviado a Hacienda, esperando respuesta
    /// </summary>
    Procesando = 3,

    /// <summary>
    /// Documento aceptado por Hacienda
    /// </summary>
    Aceptado = 4,

    /// <summary>
    /// Documento rechazado por Hacienda
    /// </summary>
    Rechazado = 5,

    /// <summary>
    /// Documento en modo contingencia (sin conexión con Hacienda)
    /// </summary>
    Contingencia = 6,

    /// <summary>
    /// Documento anulado
    /// </summary>
    Anulado = 7,

    /// <summary>
    /// Error técnico al enviar/procesar el documento (problemas de conexión, autenticación, etc.)
    /// No es un rechazo de Hacienda, es un error del sistema que requiere reintento
    /// </summary>
    Error = 8
}
