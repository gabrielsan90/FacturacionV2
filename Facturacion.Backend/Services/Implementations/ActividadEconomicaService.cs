using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Web;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de consulta de actividades económicas (CIIU)
/// API oficial: https://api.hacienda.go.cr/fe/ae
/// Usa caching para optimizar rendimiento
/// </summary>
public class ActividadEconomicaService : IActividadEconomicaService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ActividadEconomicaService> _logger;

    // URL base de la API de Actividades Económicas de Hacienda
    private const string AE_API_URL = "https://api.hacienda.go.cr/fe/ae";

    // Tiempo de caché: 30 días (las actividades económicas no cambian frecuentemente)
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromDays(30);
    private const string CACHE_KEY_ALL = "AE_ALL_ACTIVAS";

    public ActividadEconomicaService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<ActividadEconomicaService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Valida que un código de actividad económica existe y está activo
    /// </summary>
    public async Task<bool> ValidarCodigoAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return false;

        // Validar formato básico (generalmente 6 dígitos, pero puede variar)
        codigo = codigo.Trim();
        if (string.IsNullOrEmpty(codigo) || !codigo.All(char.IsDigit))
        {
            _logger.LogWarning("Código de actividad económica inválido (formato): {Codigo}", codigo);
            return false;
        }

        try
        {
            var actividad = await ObtenerPorCodigoAsync(codigo);
            return actividad != null && actividad.Activa;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar código de actividad económica: {Codigo}", codigo);
            return false;
        }
    }

    /// <summary>
    /// Obtiene información detallada de una actividad económica específica
    /// </summary>
    public async Task<ActividadEconomicaDTO?> ObtenerPorCodigoAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        // Limpiar código
        codigo = codigo.Trim();

        // Intentar obtener de caché
        var cacheKey = $"AE_{codigo}";
        if (_cache.TryGetValue(cacheKey, out ActividadEconomicaDTO? cachedResult))
        {
            _logger.LogDebug("Actividad económica obtenida de caché: {Codigo}", codigo);
            return cachedResult;
        }

        try
        {
            // Consultar API de Hacienda
            var url = $"{AE_API_URL}?codigo={codigo}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al consultar API de Actividades Económicas: {StatusCode} - {Codigo}",
                    response.StatusCode, codigo);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ActividadEconomicaBusquedaRespuestaDTO>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var actividad = resultado?.Actividades?.FirstOrDefault();

            if (actividad != null)
            {
                // Guardar en caché
                _cache.Set(cacheKey, actividad, CACHE_DURATION);
                _logger.LogInformation("Actividad económica consultada: {Codigo} - {Descripcion}",
                    codigo, actividad.Descripcion);
            }

            return actividad;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión al consultar API de Actividades Económicas: {Codigo}", codigo);
            throw new InvalidOperationException(
                "No se pudo conectar con la API de Actividades Económicas de Hacienda. Verifique su conexión a internet.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear respuesta de API de Actividades Económicas: {Codigo}", codigo);
            throw new InvalidOperationException(
                "Error al procesar la respuesta de la API de Actividades Económicas.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar actividad económica: {Codigo}", codigo);
            throw;
        }
    }

    /// <summary>
    /// Busca actividades económicas por descripción
    /// </summary>
    public async Task<ActividadEconomicaBusquedaRespuestaDTO> BuscarPorDescripcionAsync(string descripcion, int top = 10)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return new ActividadEconomicaBusquedaRespuestaDTO();
        }

        return await BuscarAsync(new ActividadEconomicaBusquedaDTO
        {
            Descripcion = descripcion,
            Top = top
        });
    }

    /// <summary>
    /// Realiza una búsqueda general de actividades económicas
    /// </summary>
    public async Task<ActividadEconomicaBusquedaRespuestaDTO> BuscarAsync(ActividadEconomicaBusquedaDTO parametros)
    {
        if (parametros == null)
        {
            return new ActividadEconomicaBusquedaRespuestaDTO();
        }

        try
        {
            // Construir URL con parámetros de búsqueda
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(parametros.Codigo))
                queryParams.Add($"codigo={HttpUtility.UrlEncode(parametros.Codigo)}");

            if (!string.IsNullOrWhiteSpace(parametros.Descripcion))
                queryParams.Add($"descripcion={HttpUtility.UrlEncode(parametros.Descripcion)}");

            if (parametros.Top > 0)
                queryParams.Add($"top={parametros.Top}");

            var url = $"{AE_API_URL}?{string.Join("&", queryParams)}";

            _logger.LogDebug("Buscando actividades económicas: {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al buscar en API de Actividades Económicas: {StatusCode}",
                    response.StatusCode);
                return new ActividadEconomicaBusquedaRespuestaDTO();
            }

            var content = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ActividadEconomicaBusquedaRespuestaDTO>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _logger.LogInformation("Búsqueda de actividades económicas completada: {Total} resultados encontrados",
                resultado?.Total ?? 0);

            return resultado ?? new ActividadEconomicaBusquedaRespuestaDTO();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión al buscar en API de Actividades Económicas");
            throw new InvalidOperationException(
                "No se pudo conectar con la API de Actividades Económicas de Hacienda. Verifique su conexión a internet.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear respuesta de búsqueda de actividades económicas");
            throw new InvalidOperationException(
                "Error al procesar la respuesta de la API de Actividades Económicas.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar actividades económicas");
            throw;
        }
    }

    /// <summary>
    /// Obtiene todas las actividades económicas activas
    /// </summary>
    public async Task<List<ActividadEconomicaDTO>> ObtenerTodasActivasAsync()
    {
        // Intentar obtener de caché
        if (_cache.TryGetValue(CACHE_KEY_ALL, out List<ActividadEconomicaDTO>? cachedResult))
        {
            _logger.LogDebug("Lista de actividades económicas obtenida de caché");
            return cachedResult ?? new List<ActividadEconomicaDTO>();
        }

        try
        {
            // Consultar API de Hacienda
            var url = $"{AE_API_URL}?activa=true";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al obtener todas las actividades económicas: {StatusCode}",
                    response.StatusCode);
                return new List<ActividadEconomicaDTO>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ActividadEconomicaBusquedaRespuestaDTO>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var actividades = resultado?.Actividades ?? new List<ActividadEconomicaDTO>();

            // Guardar en caché
            _cache.Set(CACHE_KEY_ALL, actividades, CACHE_DURATION);

            _logger.LogInformation("Se obtuvieron {Total} actividades económicas activas", actividades.Count);

            return actividades;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión al obtener actividades económicas");
            throw new InvalidOperationException(
                "No se pudo conectar con la API de Actividades Económicas de Hacienda. Verifique su conexión a internet.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear respuesta de actividades económicas");
            throw new InvalidOperationException(
                "Error al procesar la respuesta de la API de Actividades Económicas.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener actividades económicas");
            throw;
        }
    }

    /// <summary>
    /// Limpia la caché de actividades económicas
    /// </summary>
    public void LimpiarCache()
    {
        _logger.LogInformation("Limpiando caché de actividades económicas");
        _cache.Remove(CACHE_KEY_ALL);
        // Los elementos individuales expirarán naturalmente según CACHE_DURATION
    }
}
