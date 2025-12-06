namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para respuesta de consulta de actividad económica
/// API: https://api.hacienda.go.cr/fe/ae
/// Basado en la Clasificación Industrial Internacional Uniforme (CIIU)
/// </summary>
public class ActividadEconomicaDTO
{
    /// <summary>
    /// Código de actividad económica (6 dígitos generalmente)
    /// Ejemplo: 620101 - Programación informática
    /// </summary>
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Descripción de la actividad económica
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Categoría o sección de la actividad
    /// </summary>
    public string? Categoria { get; set; }

    /// <summary>
    /// Estado de la actividad (activa/inactiva)
    /// </summary>
    public bool Activa { get; set; } = true;
}

/// <summary>
/// DTO para respuesta de búsqueda de actividades económicas
/// </summary>
public class ActividadEconomicaBusquedaRespuestaDTO
{
    /// <summary>
    /// Lista de actividades económicas encontradas
    /// </summary>
    public List<ActividadEconomicaDTO> Actividades { get; set; } = new();

    /// <summary>
    /// Total de resultados encontrados
    /// </summary>
    public int Total { get; set; }
}

/// <summary>
/// DTO para parámetros de búsqueda de actividad económica
/// </summary>
public class ActividadEconomicaBusquedaDTO
{
    /// <summary>
    /// Buscar por código exacto
    /// </summary>
    public string? Codigo { get; set; }

    /// <summary>
    /// Buscar por descripción (texto parcial)
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Número máximo de resultados a retornar
    /// </summary>
    public int Top { get; set; } = 10;
}
