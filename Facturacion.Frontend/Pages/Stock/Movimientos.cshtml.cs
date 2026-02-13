using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Stock;

/// <summary>
/// PageModel para visualizar el historial de movimientos de inventario
/// Los movimientos son inmutables y solo permiten visualización (no edición ni eliminación)
/// </summary>
[Authorize]
public class MovimientosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MovimientosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MovimientosModel(IHttpClientFactory httpClientFactory, ILogger<MovimientosModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    }

    public string EmpresaId { get; set; } = "";

    public void OnGet()
    {
        // Get the current user's empresa from claims
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    /// <summary>
    /// Handler to retrieve filtered inventory movements
    /// Supports filtering by date range, movement type, product, and branch
    /// </summary>
    public async Task<IActionResult> OnGetDataAsync(
        string? fechaInicio,
        string? fechaFin,
        int? tipoMovimiento,
        Guid? productoId,
        Guid? sucursalId)
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

            // Build query string with filters
            // Add end of day time to fechaFin to include records from that day
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(fechaInicio))
                queryParams.Add($"fechaInicio={Uri.EscapeDataString(fechaInicio)}");
            if (!string.IsNullOrEmpty(fechaFin))
            {
                // Add end of day time to include all records from fechaFin
                var fechaFinConHora = fechaFin + "T23:59:59";
                queryParams.Add($"fechaFin={Uri.EscapeDataString(fechaFinConHora)}");
            }
            if (tipoMovimiento.HasValue)
                queryParams.Add($"tipoMovimiento={tipoMovimiento.Value}");
            if (productoId.HasValue)
                queryParams.Add($"productoId={productoId.Value}");
            if (sucursalId.HasValue)
                queryParams.Add($"sucursalId={sucursalId.Value}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/MovimientosInventario/empresa/{empresaId}{queryString}";

            _logger.LogInformation("Fetching movimientos from: {Url}", url);

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Read as JsonElement to preserve the JSON structure
                var jsonContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response length: {Length} chars", jsonContent.Length);

                var movimientos = JsonSerializer.Deserialize<JsonElement>(jsonContent, _jsonOptions);
                return new JsonResult(new { data = movimientos });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to load movimientos. Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading movimientos data");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    /// <summary>
    /// Handler to retrieve detailed information about a specific inventory movement
    /// </summary>
    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/MovimientosInventario/{id}");

            if (response.IsSuccessStatusCode)
            {
                // Read as JsonElement to preserve the JSON structure
                var jsonContent = await response.Content.ReadAsStringAsync();
                var movimiento = JsonSerializer.Deserialize<JsonElement>(jsonContent, _jsonOptions);
                return new JsonResult(new { success = true, data = movimiento });
            }

            _logger.LogWarning("Movement not found. ID: {Id}, Status: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { success = false, message = "Movimiento no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading movement details. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar los detalles del movimiento" });
        }
    }

    /// <summary>
    /// Handler to retrieve products list for filter dropdown
    /// </summary>
    public async Task<IActionResult> OnGetProductosAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new List<object>());
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Productos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var productos = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(productos ?? new List<object>());
            }

            _logger.LogWarning("Failed to load productos for filter. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading productos for filter");
            return new JsonResult(new List<object>());
        }
    }

    /// <summary>
    /// Handler to retrieve branches list for filter dropdown
    /// </summary>
    public async Task<IActionResult> OnGetSucursalesAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new List<object>());
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Sucursales/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var sucursales = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(sucursales ?? new List<object>());
            }

            _logger.LogWarning("Failed to load sucursales for filter. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sucursales for filter");
            return new JsonResult(new List<object>());
        }
    }
}
