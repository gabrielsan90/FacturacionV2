using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Web;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de consulta de códigos CABYS
/// API oficial: https://api.hacienda.go.cr/fe/cabys
/// Usa caching para optimizar rendimiento
/// </summary>
public class CabysService : ICabysService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CabysService> _logger;

    // URL base de la API de CABYS de Hacienda
    private const string CABYS_API_URL = "https://api.hacienda.go.cr/fe/cabys";

    // Tiempo de caché: 7 días (los códigos CABYS no cambian frecuentemente)
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromDays(7);

    public CabysService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<CabysService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Valida que un código CABYS existe y es válido
    /// </summary>
    public async Task<bool> ValidarCodigoAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return false;

        // Validar formato: debe tener 13 dígitos
        if (codigo.Length != 13 || !codigo.All(char.IsDigit))
        {
            _logger.LogWarning("Código CABYS inválido (formato): {Codigo}", codigo);
            return false;
        }

        try
        {
            var cabys = await ObtenerPorCodigoAsync(codigo);
            return cabys != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar código CABYS: {Codigo}", codigo);
            return false;
        }
    }

    /// <summary>
    /// Obtiene información detallada de un código CABYS específico
    /// </summary>
    public async Task<CabysDTO?> ObtenerPorCodigoAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        // Limpiar y validar código
        codigo = codigo.Trim();
        if (codigo.Length != 13 || !codigo.All(char.IsDigit))
        {
            _logger.LogWarning("Código CABYS inválido: {Codigo}", codigo);
            return null;
        }

        // Intentar obtener de caché
        var cacheKey = $"CABYS_{codigo}";
        if (_cache.TryGetValue(cacheKey, out CabysDTO? cachedResult))
        {
            _logger.LogDebug("Código CABYS obtenido de caché: {Codigo}", codigo);
            return cachedResult;
        }

        try
        {
            // Consultar API de Hacienda
            var url = $"{CABYS_API_URL}?codigo={codigo}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al consultar API CABYS: {StatusCode} - {Codigo}",
                    response.StatusCode, codigo);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<CabysBusquedaRespuestaDTO>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var cabys = resultado?.Cabys?.FirstOrDefault();

            if (cabys != null)
            {
                // Guardar en caché
                _cache.Set(cacheKey, cabys, CACHE_DURATION);
                _logger.LogInformation("Código CABYS consultado: {Codigo} - {Descripcion}",
                    codigo, cabys.Descripcion);
            }

            return cabys;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión al consultar API CABYS: {Codigo}", codigo);
            throw new InvalidOperationException(
                "No se pudo conectar con la API de CABYS de Hacienda. Verifique su conexión a internet.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear respuesta de API CABYS: {Codigo}", codigo);
            throw new InvalidOperationException(
                "Error al procesar la respuesta de la API de CABYS.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar código CABYS: {Codigo}", codigo);
            throw;
        }
    }

    /// <summary>
    /// Busca códigos CABYS por descripción
    /// </summary>
    public async Task<CabysBusquedaRespuestaDTO> BuscarPorDescripcionAsync(string descripcion, int top = 10)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return new CabysBusquedaRespuestaDTO();
        }

        return await BuscarAsync(new CabysBusquedaDTO
        {
            Descripcion = descripcion,
            Top = top
        });
    }

    /// <summary>
    /// Realiza una búsqueda general de códigos CABYS
    /// </summary>
    public async Task<CabysBusquedaRespuestaDTO> BuscarAsync(CabysBusquedaDTO parametros)
    {
        if (parametros == null)
        {
            return new CabysBusquedaRespuestaDTO();
        }

        try
        {
            // Construir URL con parámetros de búsqueda
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(parametros.Codigo))
                queryParams.Add($"codigo={HttpUtility.UrlEncode(parametros.Codigo)}");

            if (!string.IsNullOrWhiteSpace(parametros.Descripcion))
                queryParams.Add($"descripcion={HttpUtility.UrlEncode(parametros.Descripcion)}");

            if (!string.IsNullOrWhiteSpace(parametros.Q))
                queryParams.Add($"q={HttpUtility.UrlEncode(parametros.Q)}");

            if (parametros.Top > 0)
                queryParams.Add($"top={parametros.Top}");

            var url = $"{CABYS_API_URL}?{string.Join("&", queryParams)}";

            _logger.LogDebug("Buscando códigos CABYS: {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al buscar en API CABYS: {StatusCode}", response.StatusCode);
                return new CabysBusquedaRespuestaDTO();
            }

            var content = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<CabysBusquedaRespuestaDTO>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _logger.LogInformation("Búsqueda CABYS completada: {Total} resultados encontrados",
                resultado?.Total ?? 0);

            return resultado ?? new CabysBusquedaRespuestaDTO();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión al buscar en API CABYS");
            throw new InvalidOperationException(
                "No se pudo conectar con la API de CABYS de Hacienda. Verifique su conexión a internet.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear respuesta de búsqueda CABYS");
            throw new InvalidOperationException(
                "Error al procesar la respuesta de la API de CABYS.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar códigos CABYS");
            throw;
        }
    }

    /// <summary>
    /// Obtiene el porcentaje de impuesto aplicable para un código CABYS
    /// </summary>
    public async Task<decimal?> ObtenerPorcentajeImpuestoAsync(string codigo)
    {
        var cabys = await ObtenerPorCodigoAsync(codigo);
        return cabys?.Impuesto;
    }

    /// <summary>
    /// Limpia la caché de códigos CABYS
    /// </summary>
    public void LimpiarCache()
    {
        _logger.LogInformation("Limpiando caché de códigos CABYS");
        // La caché de memoria no tiene una forma directa de limpiar solo las entradas con prefijo
        // En un escenario de producción, se podría usar IDistributedCache con Redis que soporta patrones
        // Por ahora, los elementos expirarán naturalmente según CACHE_DURATION
    }
}
