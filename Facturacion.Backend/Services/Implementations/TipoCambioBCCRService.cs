using Facturacion.Backend.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de tipo de cambio del BCCR
/// Usa el servicio web oficial del Banco Central de Costa Rica
/// URL: https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx
/// </summary>
public class TipoCambioBCCRService : ITipoCambioBCCRService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TipoCambioBCCRService> _logger;

    // Indicadores del BCCR
    private const string INDICADOR_COMPRA_DOLAR = "317"; // Tipo de cambio de compra del dólar
    private const string INDICADOR_VENTA_DOLAR = "318";  // Tipo de cambio de venta del dólar
    private const string INDICADOR_COMPRA_EURO = "333";  // Tipo de cambio de compra del euro
    private const string INDICADOR_VENTA_EURO = "334";   // Tipo de cambio de venta del euro

    private const string BCCR_WS_URL = "https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx/ObtenerIndicadoresEconomicos";

    // Credenciales del BCCR (públicas para acceso básico)
    private const string BCCR_NOMBRE = "N";
    private const string BCCR_CORREO = "facturacion@sistema.cr";
    private const string BCCR_TOKEN = "";
    private const string BCCR_SUBNIVELES = "N";

    public TipoCambioBCCRService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<TipoCambioBCCRService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el tipo de cambio de compra del dólar
    /// </summary>
    public async Task<decimal> ObtenerTipoCambioCompraAsync(DateTime fecha)
    {
        return await ObtenerIndicadorAsync(INDICADOR_COMPRA_DOLAR, fecha);
    }

    /// <summary>
    /// Obtiene el tipo de cambio de venta del dólar
    /// </summary>
    public async Task<decimal> ObtenerTipoCambioVentaAsync(DateTime fecha)
    {
        return await ObtenerIndicadorAsync(INDICADOR_VENTA_DOLAR, fecha);
    }

    /// <summary>
    /// Obtiene ambos tipos de cambio (compra y venta)
    /// </summary>
    public async Task<(decimal Compra, decimal Venta)> ObtenerTiposCambioAsync(DateTime fecha)
    {
        var compra = await ObtenerTipoCambioCompraAsync(fecha);
        var venta = await ObtenerTipoCambioVentaAsync(fecha);
        return (compra, venta);
    }

    /// <summary>
    /// Obtiene el tipo de cambio de venta para una moneda específica
    /// </summary>
    public async Task<decimal> ObtenerTipoCambioMonedaAsync(string codigoMoneda, DateTime fecha)
    {
        if (string.IsNullOrWhiteSpace(codigoMoneda))
            throw new ArgumentException("El código de moneda es obligatorio", nameof(codigoMoneda));

        // Para CRC siempre es 1
        if (codigoMoneda.Equals("CRC", StringComparison.OrdinalIgnoreCase))
            return 1.00000m;

        string indicador = codigoMoneda.ToUpperInvariant() switch
        {
            "USD" => INDICADOR_VENTA_DOLAR,
            "EUR" => INDICADOR_VENTA_EURO,
            _ => throw new NotSupportedException($"La moneda {codigoMoneda} no está soportada para consulta de tipo de cambio")
        };

        return await ObtenerIndicadorAsync(indicador, fecha);
    }

    /// <summary>
    /// Obtiene un indicador económico del BCCR
    /// </summary>
    private async Task<decimal> ObtenerIndicadorAsync(string indicador, DateTime fecha)
    {
        // Crear clave de caché
        var cacheKey = $"BCCR_{indicador}_{fecha:yyyyMMdd}";

        // Intentar obtener de caché
        if (_cache.TryGetValue(cacheKey, out decimal valorCache))
        {
            _logger.LogDebug("Tipo de cambio obtenido de caché: {Indicador} = {Valor}", indicador, valorCache);
            return valorCache;
        }

        try
        {
            var valor = await ConsultarBCCRAsync(indicador, fecha);

            // Guardar en caché por 24 horas
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24));

            _cache.Set(cacheKey, valor, cacheOptions);

            _logger.LogInformation("Tipo de cambio consultado del BCCR: {Indicador} = {Valor} para fecha {Fecha}",
                indicador, valor, fecha.ToShortDateString());

            return valor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar tipo de cambio del BCCR para indicador {Indicador}", indicador);

            // Si falla, intentar con la fecha más reciente disponible en caché
            var tipoCambioFallback = await ObtenerTipoCambioFallbackAsync(indicador, fecha);
            if (tipoCambioFallback.HasValue)
            {
                _logger.LogWarning("Usando tipo de cambio de respaldo: {Valor}", tipoCambioFallback.Value);
                return tipoCambioFallback.Value;
            }

            throw new InvalidOperationException(
                $"No se pudo obtener el tipo de cambio del BCCR. Verifique la conexión a internet o ingrese el tipo de cambio manualmente.",
                ex);
        }
    }

    /// <summary>
    /// Consulta el servicio web del BCCR
    /// </summary>
    private async Task<decimal> ConsultarBCCRAsync(string indicador, DateTime fecha)
    {
        var fechaStr = fecha.ToString("dd/MM/yyyy");

        // Construir URL con parámetros
        var url = $"{BCCR_WS_URL}?" +
                  $"Indicador={indicador}" +
                  $"&FechaInicio={fechaStr}" +
                  $"&FechaFinal={fechaStr}" +
                  $"&Nombre={BCCR_NOMBRE}" +
                  $"&SubNiveles={BCCR_SUBNIVELES}" +
                  $"&CorreoElectronico={Uri.EscapeDataString(BCCR_CORREO)}" +
                  $"&Token={BCCR_TOKEN}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        // Parsear la respuesta XML
        return ParsearRespuestaBCCR(content);
    }

    /// <summary>
    /// Parsea la respuesta XML del BCCR
    /// </summary>
    private decimal ParsearRespuestaBCCR(string xmlContent)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);

            // El BCCR devuelve los datos en formato XML con namespace
            XNamespace ns = "http://ws.sdde.bccr.fi.cr";

            var valor = doc.Descendants(ns + "NUM_VALOR").FirstOrDefault()?.Value;

            if (string.IsNullOrWhiteSpace(valor))
            {
                // Intentar sin namespace
                valor = doc.Descendants("NUM_VALOR").FirstOrDefault()?.Value;
            }

            if (string.IsNullOrWhiteSpace(valor))
            {
                // Intentar extraer de CDATA o texto plano
                var rawContent = doc.ToString();
                var numValorMatch = System.Text.RegularExpressions.Regex.Match(
                    rawContent, @"NUM_VALOR[^>]*>([0-9,\.]+)<");

                if (numValorMatch.Success)
                    valor = numValorMatch.Groups[1].Value;
            }

            if (string.IsNullOrWhiteSpace(valor))
                throw new InvalidOperationException("No se encontró el valor del tipo de cambio en la respuesta del BCCR");

            // Limpiar el valor (puede venir con coma como separador decimal)
            valor = valor.Replace(",", ".");

            if (decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var resultado))
            {
                return Math.Round(resultado, 5);
            }

            throw new InvalidOperationException($"No se pudo parsear el valor del tipo de cambio: {valor}");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Error al parsear la respuesta del BCCR", ex);
        }
    }

    /// <summary>
    /// Intenta obtener un tipo de cambio de respaldo (del día anterior)
    /// </summary>
    private async Task<decimal?> ObtenerTipoCambioFallbackAsync(string indicador, DateTime fecha)
    {
        // Buscar en los últimos 7 días
        for (int i = 1; i <= 7; i++)
        {
            var fechaAnterior = fecha.AddDays(-i);
            var cacheKey = $"BCCR_{indicador}_{fechaAnterior:yyyyMMdd}";

            if (_cache.TryGetValue(cacheKey, out decimal valor))
            {
                return valor;
            }
        }

        // Si no hay datos en caché, intentar consultar el día anterior
        try
        {
            var fechaAnterior = fecha.AddDays(-1);
            return await ConsultarBCCRAsync(indicador, fechaAnterior);
        }
        catch
        {
            return null;
        }
    }
}
