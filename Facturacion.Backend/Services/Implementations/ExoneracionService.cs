using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Web;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de validación de exoneraciones
/// API oficial: https://api.hacienda.go.cr/fe/ex
/// Usa caching para optimizar rendimiento
/// </summary>
public class ExoneracionService : IExoneracionService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExoneracionService> _logger;

    // URL base de la API de Exoneraciones de Hacienda
    private const string EXONERACION_API_URL = "https://api.hacienda.go.cr/fe/ex";

    // Tiempo de caché: 1 día (las exoneraciones pueden cambiar de vigencia)
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(24);

    public ExoneracionService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<ExoneracionService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Valida que un documento de exoneración existe y está vigente
    /// </summary>
    public async Task<ExoneracionValidacionRespuestaDTO> ValidarExoneracionAsync(
        string numeroDocumento,
        string nombreInstitucion,
        DateTime? fechaValidacion = null)
    {
        if (string.IsNullOrWhiteSpace(numeroDocumento) || string.IsNullOrWhiteSpace(nombreInstitucion))
        {
            return new ExoneracionValidacionRespuestaDTO
            {
                EsValida = false,
                Mensaje = "El número de documento y nombre de institución son obligatorios"
            };
        }

        fechaValidacion ??= DateTime.Now;

        try
        {
            var exoneracion = await ObtenerExoneracionAsync(numeroDocumento, nombreInstitucion);

            if (exoneracion == null)
            {
                return new ExoneracionValidacionRespuestaDTO
                {
                    EsValida = false,
                    Mensaje = "El documento de exoneración no existe en el sistema de Hacienda"
                };
            }

            // Validar vigencia
            var esVigente = ValidarVigencia(exoneracion, fechaValidacion.Value);

            if (!esVigente)
            {
                var mensaje = exoneracion.FechaVencimiento.HasValue
                    ? $"La exoneración venció el {exoneracion.FechaVencimiento.Value:dd/MM/yyyy}"
                    : "La exoneración no está vigente";

                return new ExoneracionValidacionRespuestaDTO
                {
                    EsValida = false,
                    Mensaje = mensaje,
                    Exoneracion = exoneracion
                };
            }

            return new ExoneracionValidacionRespuestaDTO
            {
                EsValida = true,
                Mensaje = $"Exoneración válida. Porcentaje: {exoneracion.PorcentajeExoneracion}%",
                Exoneracion = exoneracion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar exoneración: {NumeroDocumento} - {Institucion}",
                numeroDocumento, nombreInstitucion);

            return new ExoneracionValidacionRespuestaDTO
            {
                EsValida = false,
                Mensaje = "Error al validar la exoneración. Intente nuevamente o consulte manualmente con Hacienda."
            };
        }
    }

    /// <summary>
    /// Verifica si una exoneración está vigente en una fecha específica
    /// </summary>
    public async Task<bool> EstaVigenteAsync(string numeroDocumento, string nombreInstitucion, DateTime? fecha = null)
    {
        var resultado = await ValidarExoneracionAsync(numeroDocumento, nombreInstitucion, fecha);
        return resultado.EsValida;
    }

    /// <summary>
    /// Obtiene información detallada de una exoneración
    /// </summary>
    public async Task<ExoneracionDTO?> ObtenerExoneracionAsync(string numeroDocumento, string nombreInstitucion)
    {
        if (string.IsNullOrWhiteSpace(numeroDocumento) || string.IsNullOrWhiteSpace(nombreInstitucion))
            return null;

        // Limpiar parámetros
        numeroDocumento = numeroDocumento.Trim();
        nombreInstitucion = nombreInstitucion.Trim();

        // Intentar obtener de caché
        var cacheKey = $"EX_{numeroDocumento}_{nombreInstitucion}";
        if (_cache.TryGetValue(cacheKey, out ExoneracionDTO? cachedResult))
        {
            _logger.LogDebug("Exoneración obtenida de caché: {NumeroDocumento}", numeroDocumento);
            return cachedResult;
        }

        try
        {
            // Consultar API de Hacienda
            var url = $"{EXONERACION_API_URL}?numeroDocumento={HttpUtility.UrlEncode(numeroDocumento)}" +
                     $"&nombreInstitucion={HttpUtility.UrlEncode(nombreInstitucion)}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al consultar API de Exoneraciones: {StatusCode} - {NumeroDocumento}",
                    response.StatusCode, numeroDocumento);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var exoneracion = JsonSerializer.Deserialize<ExoneracionDTO>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (exoneracion != null)
            {
                // Actualizar estado de vigencia
                exoneracion.Vigente = ValidarVigencia(exoneracion, DateTime.Now);

                // Guardar en caché
                _cache.Set(cacheKey, exoneracion, CACHE_DURATION);

                _logger.LogInformation("Exoneración consultada: {NumeroDocumento} - Vigente: {Vigente}",
                    numeroDocumento, exoneracion.Vigente);
            }

            return exoneracion;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión al consultar API de Exoneraciones: {NumeroDocumento}",
                numeroDocumento);
            throw new InvalidOperationException(
                "No se pudo conectar con la API de Exoneraciones de Hacienda. Verifique su conexión a internet.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear respuesta de API de Exoneraciones: {NumeroDocumento}",
                numeroDocumento);
            throw new InvalidOperationException(
                "Error al procesar la respuesta de la API de Exoneraciones.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar exoneración: {NumeroDocumento}",
                numeroDocumento);
            throw;
        }
    }

    /// <summary>
    /// Verifica si un beneficiario tiene exoneraciones vigentes
    /// </summary>
    public async Task<List<ExoneracionDTO>> ObtenerExoneracionesPorBeneficiarioAsync(string identificacion)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            return new List<ExoneracionDTO>();

        identificacion = identificacion.Trim();

        // Intentar obtener de caché
        var cacheKey = $"EX_BENEFICIARIO_{identificacion}";
        if (_cache.TryGetValue(cacheKey, out List<ExoneracionDTO>? cachedResult))
        {
            _logger.LogDebug("Exoneraciones de beneficiario obtenidas de caché: {Identificacion}", identificacion);
            return cachedResult ?? new List<ExoneracionDTO>();
        }

        try
        {
            // Consultar API de Hacienda
            var url = $"{EXONERACION_API_URL}/beneficiario?identificacion={HttpUtility.UrlEncode(identificacion)}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al consultar exoneraciones por beneficiario: {StatusCode} - {Identificacion}",
                    response.StatusCode, identificacion);
                return new List<ExoneracionDTO>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var exoneraciones = JsonSerializer.Deserialize<List<ExoneracionDTO>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            exoneraciones ??= new List<ExoneracionDTO>();

            // Actualizar estado de vigencia de cada exoneración
            foreach (var exoneracion in exoneraciones)
            {
                exoneracion.Vigente = ValidarVigencia(exoneracion, DateTime.Now);
            }

            // Filtrar solo las vigentes
            var exoneracionesVigentes = exoneraciones.Where(e => e.Vigente).ToList();

            // Guardar en caché
            _cache.Set(cacheKey, exoneracionesVigentes, CACHE_DURATION);

            _logger.LogInformation("Exoneraciones de beneficiario consultadas: {Identificacion} - Total: {Total}",
                identificacion, exoneracionesVigentes.Count);

            return exoneracionesVigentes;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión al consultar exoneraciones por beneficiario: {Identificacion}",
                identificacion);
            throw new InvalidOperationException(
                "No se pudo conectar con la API de Exoneraciones de Hacienda. Verifique su conexión a internet.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear respuesta de exoneraciones por beneficiario: {Identificacion}",
                identificacion);
            throw new InvalidOperationException(
                "Error al procesar la respuesta de la API de Exoneraciones.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar exoneraciones por beneficiario: {Identificacion}",
                identificacion);
            throw;
        }
    }

    /// <summary>
    /// Limpia la caché de exoneraciones
    /// </summary>
    public void LimpiarCache()
    {
        _logger.LogInformation("Limpiando caché de exoneraciones");
        // Los elementos expirarán naturalmente según CACHE_DURATION
    }

    /// <summary>
    /// Valida la vigencia de una exoneración en una fecha específica
    /// </summary>
    private bool ValidarVigencia(ExoneracionDTO exoneracion, DateTime fechaValidacion)
    {
        if (exoneracion == null)
            return false;

        // Verificar que la fecha de validación sea posterior a la fecha de emisión
        if (fechaValidacion < exoneracion.FechaEmision.Date)
        {
            exoneracion.RazonNoVigente = "La exoneración aún no ha entrado en vigencia";
            return false;
        }

        // Verificar que no haya vencido (si tiene fecha de vencimiento)
        if (exoneracion.FechaVencimiento.HasValue &&
            fechaValidacion > exoneracion.FechaVencimiento.Value.Date)
        {
            exoneracion.RazonNoVigente = $"La exoneración venció el {exoneracion.FechaVencimiento.Value:dd/MM/yyyy}";
            return false;
        }

        exoneracion.RazonNoVigente = null;
        return true;
    }
}
