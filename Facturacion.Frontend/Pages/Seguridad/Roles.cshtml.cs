using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Seguridad;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class RolesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RolesModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RolesModel(IHttpClientFactory httpClientFactory, ILogger<RolesModel> logger)
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

    // Handler for DataTable - Load all roles
    public async Task<IActionResult> OnGetDataAsync()
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

            var response = await client.GetAsync("/api/roles");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                // API may return array directly or wrapped in ActionResponse<T>
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    return new ContentResult { Content = $"{{\"data\":{content}}}", ContentType = "application/json", StatusCode = 200 };
                }

                if (doc.RootElement.TryGetProperty("result", out var resultElement))
                {
                    return new JsonResult(new { data = resultElement });
                }

                return new JsonResult(new { data = doc.RootElement });
            }

            _logger.LogWarning("Failed to load roles. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading roles");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    // Handler to get a single role by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
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

            var response = await client.GetAsync($"/api/roles/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("result", out var resultElement))
                {
                    return new JsonResult(new { success = true, data = resultElement });
                }
                return new JsonResult(new { success = true, data = doc.RootElement });
            }

            _logger.LogWarning("Role not found with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Rol no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar el rol" });
        }
    }

    // Handler to get all privilegios
    public async Task<IActionResult> OnGetPrivilegiosAsync()
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

            var response = await client.GetAsync("/api/privilegios");

            if (response.IsSuccessStatusCode)
            {
                var privilegios = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(privilegios ?? new List<object>());
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading privilegios");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to save (create or update) a role
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement rolData)
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

            var json = JsonSerializer.Serialize(rolData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = !rolData.TryGetProperty("id", out var idProp) ||
                         string.IsNullOrEmpty(idProp.GetString());

            if (isNew)
            {
                response = await client.PostAsync("/api/roles", content);
            }
            else
            {
                var id = idProp.GetString();
                response = await client.PutAsync($"/api/roles/{id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Role {Action} successfully", isNew ? "created" : "updated");
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Rol creado exitosamente" : "Rol actualizado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save role. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving role");
            return new JsonResult(new { success = false, message = "Error al guardar el rol" });
        }
    }

    // Handler to assign privilegios to role
    public async Task<IActionResult> OnPostAsignarPrivilegiosAsync(string roleId, [FromBody] JsonElement privilegiosData)
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

            var json = JsonSerializer.Serialize(privilegiosData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/roles/{roleId}/privilegios", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Privilegios asignados exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning privilegios to role");
            return new JsonResult(new { success = false, message = "Error al asignar privilegios" });
        }
    }

    // Handler to delete a role
    public async Task<IActionResult> OnPostDeleteAsync(string id)
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

            var response = await client.DeleteAsync($"/api/roles/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Role deleted successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Rol eliminado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete role with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el rol" });
        }
    }
}
