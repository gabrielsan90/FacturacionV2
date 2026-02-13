using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Stock;

/// <summary>
/// PageModel para gestionar bodegas (almacenes).
/// Permite crear, editar, listar y eliminar bodegas por sucursal.
/// </summary>
[Authorize]
public class BodegasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BodegasModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public BodegasModel(IHttpClientFactory httpClientFactory, ILogger<BodegasModel> logger)
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
    /// Handler to get all bodegas for the current empresa
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

            var response = await client.GetAsync($"/api/bodegas/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var bodegas = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                return new JsonResult(new { data = bodegas });
            }

            _logger.LogWarning("Failed to load bodegas. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bodegas");
            return new JsonResult(new { data = new List<object>() });
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/sucursales/empresa/{empresaId}");

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
    /// Handler to get a single bodega by ID with all details
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

            var response = await client.GetAsync($"/api/bodegas/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var bodega = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = bodega });
            }

            _logger.LogWarning("Bodega not found. ID: {Id}, Status: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { success = false, message = "Bodega no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bodega details. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar la bodega" });
        }
    }

    /// <summary>
    /// Handler to create or update a bodega
    /// </summary>
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement bodegaData)
    {
        try
        {
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

            // Add EmpresaId to the bodega data
            var bodegaDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bodegaData.GetRawText());
            if (bodegaDict != null)
            {
                bodegaDict["empresaId"] = JsonDocument.Parse($"\"{empresaId}\"").RootElement;
            }
            var json = JsonSerializer.Serialize(bodegaDict);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Check if we're creating or updating
            var idProperty = bodegaData.GetProperty("id");
            var id = idProperty.GetString();
            var isNew = string.IsNullOrEmpty(id) || id == "00000000-0000-0000-0000-000000000000";

            HttpResponseMessage response;
            if (isNew)
            {
                response = await client.PostAsync("/api/bodegas", content);
            }
            else
            {
                response = await client.PutAsync($"/api/bodegas/{id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                var message = isNew ? "Bodega creada correctamente" : "Bodega actualizada correctamente";
                return new JsonResult(new { success = true, message });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save bodega. Status: {StatusCode}, Message: {Message}",
                response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving bodega");
            return new JsonResult(new { success = false, message = "Ocurrió un error al guardar la bodega" });
        }
    }

    /// <summary>
    /// Handler to delete a bodega (soft delete)
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

            var response = await client.DeleteAsync($"/api/bodegas/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Bodega eliminada correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete bodega {Id}. Status: {StatusCode}, Message: {Message}",
                id, response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bodega {Id}", id);
            return new JsonResult(new { success = false, message = "Ocurrió un error al eliminar la bodega" });
        }
    }
}
