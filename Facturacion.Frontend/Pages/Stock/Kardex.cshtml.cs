using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Stock;

/// <summary>
/// PageModel para el kardex valorado de inventario.
/// Permite visualizar el historial de movimientos con costos y saldos acumulados.
/// </summary>
[Authorize]
public class KardexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<KardexModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public KardexModel(IHttpClientFactory httpClientFactory, ILogger<KardexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public string EmpresaId { get; set; } = "";

    public void OnGet()
    {
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    /// <summary>
    /// Handler to get kardex data for a specific product
    /// </summary>
    public async Task<IActionResult> OnGetKardexAsync(
        string productoId,
        string? bodegaId = null,
        string? fechaDesde = null,
        string? fechaHasta = null)
    {
        try
        {
            if (string.IsNullOrEmpty(productoId))
            {
                return new JsonResult(new { success = false, message = "Debe especificar un producto" });
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró la empresa del usuario" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Build query string
            var queryParams = new List<string>
            {
                $"empresaId={empresaId}",
                $"productoId={productoId}"
            };

            if (!string.IsNullOrEmpty(bodegaId))
                queryParams.Add($"bodegaId={bodegaId}");

            if (!string.IsNullOrEmpty(fechaDesde))
                queryParams.Add($"fechaDesde={fechaDesde}");

            if (!string.IsNullOrEmpty(fechaHasta))
                queryParams.Add($"fechaHasta={fechaHasta}");

            var queryString = string.Join("&", queryParams);
            var response = await client.GetAsync($"/api/kardex?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var kardex = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                // Get producto details
                var productoResponse = await client.GetAsync($"/api/productos/{productoId}");
                JsonElement? producto = null;
                if (productoResponse.IsSuccessStatusCode)
                {
                    var productoContent = await productoResponse.Content.ReadAsStringAsync();
                    producto = JsonSerializer.Deserialize<JsonElement>(productoContent, _jsonOptions);
                }

                return new JsonResult(new { success = true, data = kardex, producto });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to load kardex. Status: {StatusCode}, Message: {Message}",
                response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading kardex. ProductoId: {ProductoId}", productoId);
            return new JsonResult(new { success = false, message = "Ocurrió un error al cargar el kardex" });
        }
    }

    /// <summary>
    /// Handler to get productos with inventory control enabled for the current empresa
    /// </summary>
    public async Task<IActionResult> OnGetProductosAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, data = new List<object>() });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/productos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var productos = JsonSerializer.Deserialize<List<object>>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = productos });
            }

            return new JsonResult(new { success = false, data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading productos");
            return new JsonResult(new { success = false, data = new List<object>() });
        }
    }

    /// <summary>
    /// Handler to get bodegas for the current empresa
    /// </summary>
    public async Task<IActionResult> OnGetBodegasAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, data = new List<object>() });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/bodegas/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var bodegas = JsonSerializer.Deserialize<List<object>>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = bodegas });
            }

            return new JsonResult(new { success = false, data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bodegas");
            return new JsonResult(new { success = false, data = new List<object>() });
        }
    }
}
