namespace Facturacion.Shared.DTOs;

/// <summary>
/// Resultado del proceso de recepción de un documento electrónico
/// </summary>
public class ResultadoRecepcion
{
    /// <summary>
    /// Indica si la recepción fue exitosa
    /// </summary>
    public bool Exitoso { get; set; }

    /// <summary>
    /// Mensaje principal del resultado
    /// </summary>
    public string Mensaje { get; set; } = null!;

    /// <summary>
    /// ID del documento creado en la base de datos (si fue exitoso)
    /// </summary>
    public Guid? DocumentoId { get; set; }

    /// <summary>
    /// Clave del documento recibido
    /// </summary>
    public string? Clave { get; set; }

    /// <summary>
    /// Nombre del emisor (proveedor)
    /// </summary>
    public string? EmisorNombre { get; set; }

    /// <summary>
    /// Total de venta del documento
    /// </summary>
    public decimal? TotalVenta { get; set; }

    /// <summary>
    /// Tipo de documento recibido
    /// </summary>
    public string? TipoDocumento { get; set; }

    /// <summary>
    /// Fecha de emisión del documento
    /// </summary>
    public DateTime? FechaEmision { get; set; }

    /// <summary>
    /// Advertencias durante la recepción (no impiden el proceso)
    /// </summary>
    public List<string> Advertencias { get; set; } = new();

    /// <summary>
    /// Errores que impidieron la recepción
    /// </summary>
    public List<string> Errores { get; set; } = new();
}
