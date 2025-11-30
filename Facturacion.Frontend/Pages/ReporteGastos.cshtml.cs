using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ReporteGastosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReporteGastosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteGastosModel(IHttpClientFactory httpClientFactory, ILogger<ReporteGastosModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public void OnGet()
    {
        // Page initialization
    }

    public async Task<IActionResult> OnGetDataAsync(string? fechaInicio, string? fechaFin, string? categoriaGasto)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/gastos?fechaInicio={fechaInicio}&fechaFin={fechaFin}&categoriaGasto={categoriaGasto}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = data ?? new List<dynamic>() });
            }

            _logger.LogWarning("Failed to load reporte gastos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reporte gastos");
            return new JsonResult(new { data = new List<dynamic>() });
        }
    }

    public async Task<IActionResult> OnGetResumenAsync(string? fechaInicio, string? fechaFin, string? categoriaGasto)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/gastos/resumen?fechaInicio={fechaInicio}&fechaFin={fechaFin}&categoriaGasto={categoriaGasto}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data ?? new { totalGastos = 0, totalDocumentos = 0, promedioGasto = 0, totalProveedores = 0 });
            }

            _logger.LogWarning("Failed to load resumen gastos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { totalGastos = 0, totalDocumentos = 0, promedioGasto = 0, totalProveedores = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resumen gastos");
            return new JsonResult(new { totalGastos = 0, totalDocumentos = 0, promedioGasto = 0, totalProveedores = 0 });
        }
    }

    public async Task<IActionResult> OnGetGraficosAsync(string? fechaInicio, string? fechaFin, string? categoriaGasto)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/gastos/graficos?fechaInicio={fechaInicio}&fechaFin={fechaFin}&categoriaGasto={categoriaGasto}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data ?? new { meses = new { labels = new List<string>(), valores = new List<decimal>() }, categorias = new List<dynamic>() });
            }

            _logger.LogWarning("Failed to load graficos gastos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { meses = new { labels = new List<string>(), valores = new List<decimal>() }, categorias = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading graficos gastos");
            return new JsonResult(new { meses = new { labels = new List<string>(), valores = new List<decimal>() }, categorias = new List<dynamic>() });
        }
    }

    public async Task<IActionResult> OnGetTopProveedoresAsync(string? fechaInicio, string? fechaFin, string? categoriaGasto)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/gastos/top-proveedores?fechaInicio={fechaInicio}&fechaFin={fechaFin}&categoriaGasto={categoriaGasto}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(data ?? new List<dynamic>());
            }

            _logger.LogWarning("Failed to load top proveedores. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<dynamic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading top proveedores");
            return new JsonResult(new List<dynamic>());
        }
    }
}
