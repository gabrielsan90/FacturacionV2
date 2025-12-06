namespace Facturacion.Shared.DTOs;

/// <summary>
/// Resultado de la validación de un XML contra un esquema XSD
/// </summary>
public class ResultadoValidacionXsd
{
    /// <summary>
    /// Indica si el XML es válido según el esquema XSD
    /// </summary>
    public bool EsValido { get; set; }

    /// <summary>
    /// Lista de errores de validación (vacía si es válido)
    /// </summary>
    public List<string> Errores { get; set; } = new List<string>();

    /// <summary>
    /// Lista de advertencias de validación (no impiden el envío)
    /// </summary>
    public List<string> Advertencias { get; set; } = new List<string>();

    /// <summary>
    /// Ruta del archivo XSD utilizado para la validación
    /// </summary>
    public string? RutaXsd { get; set; }

    /// <summary>
    /// Tipo de documento validado
    /// </summary>
    public string? TipoDocumento { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Mensaje => EsValido
        ? "El documento XML es válido según el esquema XSD v4.4"
        : $"El documento XML tiene {Errores.Count} error(es) de validación";
}
