namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para respuesta de consulta de código CABYS
/// API: https://api.hacienda.go.cr/fe/cabys
/// </summary>
public class CabysDTO
{
    /// <summary>
    /// Código CABYS (13 dígitos)
    /// </summary>
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Descripción del producto o servicio
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Porcentaje de impuesto aplicable (generalmente 13.00 para IVA en Costa Rica)
    /// </summary>
    public decimal Impuesto { get; set; }

    /// <summary>
    /// Categoría del producto o servicio
    /// </summary>
    public string? Categoria { get; set; }
}

/// <summary>
/// DTO para respuesta de búsqueda de códigos CABYS
/// </summary>
public class CabysBusquedaRespuestaDTO
{
    /// <summary>
    /// Lista de códigos CABYS encontrados
    /// </summary>
    public List<CabysDTO> Cabys { get; set; } = new();

    /// <summary>
    /// Total de resultados encontrados
    /// </summary>
    public int Total { get; set; }
}

/// <summary>
/// DTO para parámetros de búsqueda de CABYS
/// </summary>
public class CabysBusquedaDTO
{
    /// <summary>
    /// Buscar por código exacto (13 dígitos)
    /// </summary>
    public string? Codigo { get; set; }

    /// <summary>
    /// Buscar por descripción (texto parcial)
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Búsqueda general (código o descripción)
    /// </summary>
    public string? Q { get; set; }

    /// <summary>
    /// Número máximo de resultados a retornar
    /// </summary>
    public int Top { get; set; } = 10;
}
