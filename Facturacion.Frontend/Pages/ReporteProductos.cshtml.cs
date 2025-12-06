using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ReporteProductosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReporteProductosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteProductosModel(IHttpClientFactory httpClientFactory, ILogger<ReporteProductosModel> logger)
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

    public async Task<IActionResult> OnGetDataAsync(string? categoriaId, string? estado, string? stockBajo)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/productos?categoriaId={categoriaId}&estado={estado}&stockBajo={stockBajo}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = data ?? new List<dynamic>() });
            }

            _logger.LogWarning("Failed to load reporte productos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reporte productos");
            return new JsonResult(new { data = new List<dynamic>() });
        }
    }

    public async Task<IActionResult> OnGetResumenAsync(string? categoriaId, string? estado, string? stockBajo)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/productos/resumen?categoriaId={categoriaId}&estado={estado}&stockBajo={stockBajo}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data ?? new { totalProductos = 0, totalActivos = 0, totalStockBajo = 0, valorInventario = 0 });
            }

            _logger.LogWarning("Failed to load resumen. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { totalProductos = 0, totalActivos = 0, totalStockBajo = 0, valorInventario = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resumen");
            return new JsonResult(new { totalProductos = 0, totalActivos = 0, totalStockBajo = 0, valorInventario = 0 });
        }
    }

    public async Task<IActionResult> OnGetCategoriasAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/categorias");

            if (response.IsSuccessStatusCode)
            {
                var categorias = await response.Content.ReadFromJsonAsync<List<Categoria>>(_jsonOptions);
                return new JsonResult(categorias ?? new List<Categoria>());
            }

            _logger.LogWarning("Failed to load categorias. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<Categoria>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorias");
            return new JsonResult(new List<Categoria>());
        }
    }
}
