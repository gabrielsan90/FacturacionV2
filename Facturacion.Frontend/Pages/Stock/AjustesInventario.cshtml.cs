using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Stock;

/// <summary>
/// PageModel para gestionar ajustes de inventario.
/// Permite crear, ver y aplicar ajustes de inventario por bodega.
/// </summary>
[Authorize]
public class AjustesInventarioModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AjustesInventarioModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AjustesInventarioModel(IHttpClientFactory httpClientFactory, ILogger<AjustesInventarioModel> logger)
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
    /// Handler to get all ajustes de inventario for the current empresa
    /// </summary>
    public async Task<IActionResult> OnGetDataAsync()
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

            var response = await client.GetAsync($"/api/ajustesinventario/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ajustes = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                return new JsonResult(new { data = ajustes });
            }

            _logger.LogWarning("Failed to load ajustes inventario. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading ajustes inventario");
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
    /// Handler to get bodegas (warehouses) for the current empresa
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

            var response = await client.GetAsync($"/api/Bodegas/empresa/{empresaId}");

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

    /// <summary>
    /// Handler to get a single ajuste inventario by ID with all details
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

            var response = await client.GetAsync($"/api/ajustesinventario/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ajuste = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = ajuste });
            }

            _logger.LogWarning("Ajuste inventario not found. ID: {Id}, Status: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { success = false, message = "Ajuste no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading ajuste inventario details. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar el ajuste" });
        }
    }

    /// <summary>
    /// Handler to create or update an ajuste inventario
    /// </summary>
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement ajusteData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var json = ajusteData.GetRawText();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Check if we're creating or updating
            var idProperty = ajusteData.GetProperty("id");
            var id = idProperty.GetString();
            var isNew = string.IsNullOrEmpty(id) || id == "00000000-0000-0000-0000-000000000000";

            HttpResponseMessage response;
            if (isNew)
            {
                response = await client.PostAsync("/api/ajustesinventario", content);
            }
            else
            {
                response = await client.PutAsync($"/api/ajustesinventario/{id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                var message = isNew ? "Ajuste creado correctamente" : "Ajuste actualizado correctamente";
                return new JsonResult(new { success = true, message });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save ajuste inventario. Status: {StatusCode}, Message: {Message}",
                response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving ajuste inventario");
            return new JsonResult(new { success = false, message = "Ocurrió un error al guardar el ajuste" });
        }
    }

    /// <summary>
    /// Handler to apply (approve) an ajuste inventario
    /// This will update the inventory based on the adjustment
    /// </summary>
    public async Task<IActionResult> OnPostAplicarAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/ajustesinventario/{id}/aplicar", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Ajuste aplicado correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to apply ajuste inventario {Id}. Status: {StatusCode}, Message: {Message}",
                id, response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying ajuste inventario {Id}", id);
            return new JsonResult(new { success = false, message = "Ocurrió un error al aplicar el ajuste" });
        }
    }

    /// <summary>
    /// Handler to delete an ajuste inventario
    /// Only pending ajustes can be deleted
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/ajustesinventario/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Ajuste eliminado correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete ajuste inventario {Id}. Status: {StatusCode}, Message: {Message}",
                id, response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ajuste inventario {Id}", id);
            return new JsonResult(new { success = false, message = "Ocurrió un error al eliminar el ajuste" });
        }
    }
}
