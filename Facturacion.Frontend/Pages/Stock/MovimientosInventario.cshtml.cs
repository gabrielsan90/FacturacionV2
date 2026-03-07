using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Stock;

/// <summary>
/// PageModel para visualizar movimientos de inventario.
/// Solo consulta - los movimientos se crean desde otros módulos (ventas, compras, ajustes, etc.)
/// </summary>
[Authorize]
public class MovimientosInventarioModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MovimientosInventarioModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MovimientosInventarioModel(IHttpClientFactory httpClientFactory, ILogger<MovimientosInventarioModel> logger)
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
    /// Handler to get all movimientos with optional filters
    /// </summary>
    public async Task<IActionResult> OnGetDataAsync(
        string? productoId = null,
        string? bodegaId = null,
        string? tipoMovimiento = null,
        string? fechaDesde = null,
        string? fechaHasta = null)
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                _logger.LogWarning("EmpresaId not found in user claims");
                return new JsonResult(new { data = new List<object>() });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Build query string with filters matching backend param names
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(productoId))
                queryParams.Add($"productoId={productoId}");

            if (!string.IsNullOrEmpty(bodegaId))
                queryParams.Add($"sucursalId={bodegaId}");

            if (!string.IsNullOrEmpty(tipoMovimiento))
                queryParams.Add($"tipoMovimiento={tipoMovimiento}");

            if (!string.IsNullOrEmpty(fechaDesde))
                queryParams.Add($"fechaInicio={fechaDesde}");

            if (!string.IsNullOrEmpty(fechaHasta))
                queryParams.Add($"fechaFin={fechaHasta}");

            var qs = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await client.GetAsync($"/api/movimientosinventario/empresa/{empresaId}{qs}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var movimientos = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                return new JsonResult(new { data = movimientos });
            }

            _logger.LogWarning("Failed to load movimientos. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading movimientos inventario");
            return new JsonResult(new { data = new List<object>() });
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
