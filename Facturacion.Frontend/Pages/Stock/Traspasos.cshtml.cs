using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Stock;

/// <summary>
/// PageModel para gestionar traspasos de inventario entre bodegas.
/// Permite crear, ver, enviar y recibir traspasos con workflow de estados.
/// Estados: PEN=Pendiente, TRA=En Tránsito, REC=Recibido, ANU=Anulado
/// </summary>
[Authorize]
public class TraspasosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TraspasosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public TraspasosModel(IHttpClientFactory httpClientFactory, ILogger<TraspasosModel> logger)
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
    /// Handler to get all traspasos for the current empresa
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

            var response = await client.GetAsync($"/api/trasladosinventario/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var traspasos = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                return new JsonResult(new { data = traspasos });
            }

            _logger.LogWarning("Failed to load traspasos. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading traspasos");
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

    /// <summary>
    /// Handler to get a single traspaso by ID with all details
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

            var response = await client.GetAsync($"/api/trasladosinventario/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var traspaso = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = traspaso });
            }

            _logger.LogWarning("Traspaso not found. ID: {Id}, Status: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { success = false, message = "Traspaso no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading traspaso details. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar el traspaso" });
        }
    }

    /// <summary>
    /// Handler to create or update a traspaso (only in Pendiente state)
    /// </summary>
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement traspasoData)
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

            // Add EmpresaId and Numero placeholder to the traspaso data
            var traspasoDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(traspasoData.GetRawText());
            if (traspasoDict != null)
            {
                traspasoDict["empresaId"] = JsonDocument.Parse($"\"{empresaId}\"").RootElement;
                // Set placeholder for server-generated Numero to pass API model validation
                if (!traspasoDict.ContainsKey("numero") || traspasoDict["numero"].ValueKind == JsonValueKind.Null
                    || (traspasoDict["numero"].ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(traspasoDict["numero"].GetString())))
                {
                    traspasoDict["numero"] = JsonDocument.Parse("\"AUTO\"").RootElement;
                }
            }
            var json = JsonSerializer.Serialize(traspasoDict);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Check if we're creating or updating
            var idProperty = traspasoData.GetProperty("id");
            var id = idProperty.GetString();
            var isNew = string.IsNullOrEmpty(id) || id == "00000000-0000-0000-0000-000000000000";

            HttpResponseMessage response;
            if (isNew)
            {
                response = await client.PostAsync("/api/trasladosinventario", content);
            }
            else
            {
                response = await client.PutAsync($"/api/trasladosinventario/{id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                var message = isNew ? "Traspaso creado correctamente" : "Traspaso actualizado correctamente";
                return new JsonResult(new { success = true, message });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save traspaso. Status: {StatusCode}, Message: {Message}",
                response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving traspaso");
            return new JsonResult(new { success = false, message = "Ocurrió un error al guardar el traspaso" });
        }
    }

    /// <summary>
    /// Handler to send (enviar) a traspaso - changes state from PEN to TRA
    /// Decreases inventory in origin bodega
    /// </summary>
    public async Task<IActionResult> OnPostEnviarAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/trasladosinventario/{id}/enviar", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Traspaso enviado correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to send traspaso {Id}. Status: {StatusCode}, Message: {Message}",
                id, response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending traspaso {Id}", id);
            return new JsonResult(new { success = false, message = "Ocurrió un error al enviar el traspaso" });
        }
    }

    /// <summary>
    /// Handler to receive (recibir) a traspaso - changes state from TRA to REC
    /// Increases inventory in destination bodega
    /// </summary>
    public async Task<IActionResult> OnPostRecibirAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/trasladosinventario/{id}/recibir", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Traspaso recibido correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to receive traspaso {Id}. Status: {StatusCode}, Message: {Message}",
                id, response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving traspaso {Id}", id);
            return new JsonResult(new { success = false, message = "Ocurrió un error al recibir el traspaso" });
        }
    }

    /// <summary>
    /// Handler to get stock for a product in a specific bodega (via its sucursal)
    /// </summary>
    public async Task<IActionResult> OnGetStockAsync(Guid productoId, Guid bodegaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Get bodega to find its sucursalId
            var bodegaResponse = await client.GetAsync($"/api/bodegas/{bodegaId}");
            if (!bodegaResponse.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = false, stock = 0m });
            }

            var bodegaContent = await bodegaResponse.Content.ReadAsStringAsync();
            var bodega = JsonSerializer.Deserialize<JsonElement>(bodegaContent, _jsonOptions);
            var sucursalId = bodega.TryGetProperty("sucursalId", out var sId) ? sId.GetString() : null;
            if (string.IsNullOrEmpty(sucursalId))
            {
                return new JsonResult(new { success = false, stock = 0m });
            }

            // Get inventory for the empresa and find matching product+sucursal
            var empresaId = User.FindFirstValue("EmpresaId");
            var invResponse = await client.GetAsync($"/api/inventarios/empresa/{empresaId}");
            if (!invResponse.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = false, stock = 0m });
            }

            var invContent = await invResponse.Content.ReadAsStringAsync();
            var inventarios = JsonSerializer.Deserialize<JsonElement>(invContent, _jsonOptions);

            decimal stock = 0;
            if (inventarios.ValueKind == JsonValueKind.Array)
            {
                foreach (var inv in inventarios.EnumerateArray())
                {
                    var pId = inv.TryGetProperty("productoId", out var p) ? p.GetString() : null;
                    var bId = inv.TryGetProperty("bodegaId", out var b) && b.ValueKind == JsonValueKind.String ? b.GetString() : null;
                    var sIdInv = inv.TryGetProperty("sucursalId", out var s) ? s.GetString() : null;

                    // Match by bodegaId first, fallback to sucursalId
                    if (pId == productoId.ToString())
                    {
                        if (bId == bodegaId.ToString())
                        {
                            stock = inv.TryGetProperty("cantidadActual", out var c) ? c.GetDecimal() : 0;
                            break;
                        }
                        if (bId == null && sIdInv == sucursalId)
                        {
                            stock = inv.TryGetProperty("cantidadActual", out var c) ? c.GetDecimal() : 0;
                            // Don't break — keep looking for exact bodega match
                        }
                    }
                }
            }

            return new JsonResult(new { success = true, stock });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock for producto {ProductoId} bodega {BodegaId}", productoId, bodegaId);
            return new JsonResult(new { success = false, stock = 0m });
        }
    }

    /// <summary>
    /// Handler to delete a traspaso (soft delete)
    /// Only pending traspasos can be deleted
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

            var response = await client.DeleteAsync($"/api/trasladosinventario/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Traspaso eliminado correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete traspaso {Id}. Status: {StatusCode}, Message: {Message}",
                id, response.StatusCode, errorMessage);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting traspaso {Id}", id);
            return new JsonResult(new { success = false, message = "Ocurrió un error al eliminar el traspaso" });
        }
    }
}
