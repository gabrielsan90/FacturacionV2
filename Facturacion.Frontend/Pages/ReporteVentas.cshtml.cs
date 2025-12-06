using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ReporteVentasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReporteVentasModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteVentasModel(IHttpClientFactory httpClientFactory, ILogger<ReporteVentasModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public void OnGet()
    {
        // Page initialization
    }

    public async Task<IActionResult> OnGetResumenAsync(string? fechaInicio, string? fechaFin)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/ventas/resumen?fechaInicio={fechaInicio}&fechaFin={fechaFin}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data ?? new { totalVentas = 0, totalDocumentos = 0, promedioVenta = 0, clientesUnicos = 0 });
            }

            _logger.LogWarning("Failed to load resumen ventas. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { totalVentas = 0, totalDocumentos = 0, promedioVenta = 0, clientesUnicos = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resumen ventas");
            return new JsonResult(new { totalVentas = 0, totalDocumentos = 0, promedioVenta = 0, clientesUnicos = 0 });
        }
    }

    public async Task<IActionResult> OnGetGraficoVentasAsync(string? fechaInicio, string? fechaFin, string? agruparPor)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/ventas/grafico?fechaInicio={fechaInicio}&fechaFin={fechaFin}&agruparPor={agruparPor}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data ?? new { labels = new List<string>(), valores = new List<decimal>(), topProductos = new List<dynamic>() });
            }

            _logger.LogWarning("Failed to load grafico ventas. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { labels = new List<string>(), valores = new List<decimal>(), topProductos = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading grafico ventas");
            return new JsonResult(new { labels = new List<string>(), valores = new List<decimal>(), topProductos = new List<dynamic>() });
        }
    }

    public async Task<IActionResult> OnGetTopClientesAsync(string? fechaInicio, string? fechaFin)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/ventas/top-clientes?fechaInicio={fechaInicio}&fechaFin={fechaFin}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(data ?? new List<dynamic>());
            }

            _logger.LogWarning("Failed to load top clientes. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<dynamic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading top clientes");
            return new JsonResult(new List<dynamic>());
        }
    }

    public async Task<IActionResult> OnGetTopProductosAsync(string? fechaInicio, string? fechaFin)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/ventas/top-productos?fechaInicio={fechaInicio}&fechaFin={fechaFin}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(data ?? new List<dynamic>());
            }

            _logger.LogWarning("Failed to load top productos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<dynamic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading top productos");
            return new JsonResult(new List<dynamic>());
        }
    }
}
