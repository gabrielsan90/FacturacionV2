namespace Facturacion.Shared.DTOs;

/// <summary>
/// Resultado de una operación de importación desde Excel
/// </summary>
public class ImportResultDTO
{
    /// <summary>
    /// Indica si la importación fue exitosa (puede tener errores parciales)
    /// </summary>
    public bool WasSuccess { get; set; }

    /// <summary>
    /// Mensaje general del resultado
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Total de filas procesadas en el archivo
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Número de registros importados exitosamente
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Número de registros con errores
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Lista de errores detallados por fila
    /// </summary>
    public List<ImportErrorDTO> Errors { get; set; } = new();
}

/// <summary>
/// Detalle de un error de importación
/// </summary>
public class ImportErrorDTO
{
    /// <summary>
    /// Número de fila en el archivo Excel (1-based)
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Nombre de la columna con error (si aplica)
    /// </summary>
    public string? Column { get; set; }

    /// <summary>
    /// Valor que causó el error
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Descripción del error
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
