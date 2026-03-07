using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Facturacion.Frontend.Pages.Maestros;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class CategoriasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CategoriasModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Categoria Categoria { get; set; } = new();

    public CategoriasModel(IHttpClientFactory httpClientFactory, ILogger<CategoriasModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public void OnGet()
    {
        // Page initialization
    }

    // Handler for DataTable - Load all categorias
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new JsonResult(new { data = new List<Categoria>() });
            }

            var response = await client.GetAsync($"/api/Categorias/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var categorias = await response.Content.ReadFromJsonAsync<List<Categoria>>(_jsonOptions);
                return new JsonResult(new { data = categorias ?? new List<Categoria>() });
            }

            _logger.LogWarning("Failed to load categorias. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<Categoria>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorias");
            return new JsonResult(new { data = new List<Categoria>() });
        }
    }

    // Handler to get a single categoria by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/categorias/{id}");

            if (response.IsSuccessStatusCode)
            {
                var categoria = await response.Content.ReadFromJsonAsync<Categoria>(_jsonOptions);
                return new JsonResult(categoria);
            }

            _logger.LogWarning("Categoria not found with ID: {Id}", id);
            return new JsonResult(new { error = "Categoría no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categoria with ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar la categoría" });
        }
    }

    // Handler to save (create or update) a categoria
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement categoriaElement)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId) || !Guid.TryParse(empresaId, out var empresaGuid))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
            }

            // Use JsonNode to manipulate the JSON: inject empresaId, normalize id
            var node = JsonNode.Parse(categoriaElement.GetRawText())?.AsObject();
            if (node == null)
            {
                return new JsonResult(new { success = false, message = "No se recibieron los datos de la categoría." });
            }

            // Determine if new or update
            var idStr = node["id"]?.GetValue<string>() ?? "";
            bool isNew = !Guid.TryParse(idStr, out var idGuid) || idGuid == Guid.Empty;

            if (isNew)
            {
                node["id"] = Guid.Empty.ToString();
            }

            // Inject empresaId if missing
            var existingEmpresa = node["empresaId"]?.GetValue<string>() ?? "";
            if (!Guid.TryParse(existingEmpresa, out var existingEmpresaGuid) || existingEmpresaGuid == Guid.Empty)
            {
                node["empresaId"] = empresaGuid.ToString();
            }

            var json = node.ToJsonString();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            if (isNew)
            {
                response = await client.PostAsync("/api/categorias", content);
            }
            else
            {
                response = await client.PutAsync($"/api/categorias/{idGuid}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Categoria {Action} successfully", isNew ? "created" : "updated");
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Categoría creada exitosamente" : "Categoría actualizada exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save categoria. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            var mensajeError = ExtraerMensajeError(error, (int)response.StatusCode);
            return new JsonResult(new { success = false, message = mensajeError });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving categoria");
            return new JsonResult(new { success = false, message = "Error al guardar la categoría" });
        }
    }

    /// <summary>
    /// Extrae un mensaje de error legible de la respuesta del API
    /// </summary>
    private static string ExtraerMensajeError(string responseBody, int statusCode)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return statusCode switch
            {
                400 => "Datos inválidos. Verifique los campos e intente de nuevo.",
                403 => "No tiene permisos para realizar esta acción.",
                404 => "Recurso no encontrado.",
                _ => $"Error del servidor (código {statusCode})."
            };
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errors))
            {
                var mensajes = new List<string>();
                foreach (var field in errors.EnumerateObject())
                {
                    foreach (var msg in field.Value.EnumerateArray())
                    {
                        mensajes.Add(msg.GetString() ?? field.Name);
                    }
                }
                if (mensajes.Count > 0) return string.Join(" | ", mensajes);
            }

            if (root.TryGetProperty("message", out var message))
                return message.GetString() ?? responseBody;

            if (root.ValueKind == JsonValueKind.String)
                return root.GetString() ?? responseBody;
        }
        catch { /* Not JSON */ }

        if (responseBody.Contains("An error occurred while saving"))
            return "Error al guardar los datos. Verifique que todos los campos requeridos estén completos y no existan duplicados.";

        return responseBody;
    }

    // Handler to delete a categoria
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/categorias/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Categoria deleted successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Categoría eliminada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete categoria with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting categoria with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar la categoría" });
        }
    }
}
