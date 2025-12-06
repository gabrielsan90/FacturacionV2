using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize]
public class InventarioModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<InventarioModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public InventarioModel(IHttpClientFactory httpClientFactory, ILogger<InventarioModel> logger)
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
    /// Handler to get all inventarios for the current empresa
    /// </summary>
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Inventarios/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var inventarios = JsonSerializer.Deserialize<List<object>>(content, _jsonOptions);
                return new JsonResult(new { data = inventarios });
            }

            _logger.LogWarning("Failed to load inventarios. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventarios");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    /// <summary>
    /// Handler to get productos with inventory control enabled
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
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Productos/empresa/{empresaId}");

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
    /// Handler to get sucursales for the current empresa
    /// </summary>
    public async Task<IActionResult> OnGetSucursalesAsync()
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
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Sucursales/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var sucursales = JsonSerializer.Deserialize<List<object>>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = sucursales });
            }

            return new JsonResult(new { success = false, data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sucursales");
            return new JsonResult(new { success = false, data = new List<object>() });
        }
    }

    /// <summary>
    /// Handler to get inventarios with low stock
    /// </summary>
    public async Task<IActionResult> OnGetLowStockAsync()
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
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Inventarios/bajostock/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var lowStockItems = JsonSerializer.Deserialize<List<object>>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = lowStockItems });
            }

            return new JsonResult(new { success = false, data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading low stock items");
            return new JsonResult(new { success = false, data = new List<object>() });
        }
    }

    /// <summary>
    /// Handler to get a single inventario by ID
    /// </summary>
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return new JsonResult(new { success = false, message = "ID no proporcionado" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Inventarios/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var inventario = JsonSerializer.Deserialize<object>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = inventario });
            }

            return new JsonResult(new { success = false, message = "Inventario no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventario details for ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar el inventario" });
        }
    }

    /// <summary>
    /// Handler to create a new inventario
    /// </summary>
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement inventarioData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = inventarioData.GetRawText();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/Inventarios", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Inventario creado correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to create inventario. Status: {StatusCode}, Message: {Message}",
                response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventario");
            return new JsonResult(new { success = false, message = "Ocurrió un error al crear el inventario" });
        }
    }

    /// <summary>
    /// Handler to save an inventory adjustment
    /// </summary>
    public async Task<IActionResult> OnPostAjusteAsync(string id, [FromBody] JsonElement ajusteData)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return new JsonResult(new { success = false, message = "ID no proporcionado" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = ajusteData.GetRawText();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/Inventarios/{id}/ajuste", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Ajuste realizado correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to create ajuste for inventario {Id}. Status: {StatusCode}, Message: {Message}",
                id, response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ajuste for inventario {Id}", id);
            return new JsonResult(new { success = false, message = "Ocurrió un error al realizar el ajuste" });
        }
    }

    /// <summary>
    /// Handler to save an inventory transfer
    /// </summary>
    public async Task<IActionResult> OnPostTrasladoAsync(string id, [FromBody] JsonElement trasladoData)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return new JsonResult(new { success = false, message = "ID no proporcionado" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = trasladoData.GetRawText();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/Inventarios/{id}/traslado", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Traslado realizado correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to create traslado for inventario {Id}. Status: {StatusCode}, Message: {Message}",
                id, response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating traslado for inventario {Id}", id);
            return new JsonResult(new { success = false, message = "Ocurrió un error al realizar el traslado" });
        }
    }

    /// <summary>
    /// Handler to delete an inventario
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return new JsonResult(new { success = false, message = "ID no proporcionado" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/Inventarios/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Inventario eliminado correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete inventario {Id}. Status: {StatusCode}, Message: {Message}",
                id, response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventario {Id}", id);
            return new JsonResult(new { success = false, message = "Ocurrió un error al eliminar el inventario" });
        }
    }
}
