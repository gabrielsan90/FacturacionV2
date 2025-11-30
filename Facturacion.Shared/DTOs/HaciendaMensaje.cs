namespace Facturacion.Shared.DTOs;

/// <summary>
/// Mensaje individual de respuesta de Hacienda
/// </summary>
public class HaciendaMensaje
{
    /// <summary>
    /// Código del mensaje según catálogo de Hacienda
    /// </summary>
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Mensaje descriptivo
    /// </summary>
    public string Mensaje { get; set; } = null!;

    /// <summary>
    /// Detalle adicional del mensaje
    /// </summary>
    public string? Detalle { get; set; }

    /// <summary>
    /// Tipo de mensaje: "error", "advertencia", "info"
    /// </summary>
    public string Tipo { get; set; } = null!;
}
