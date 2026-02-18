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

    // Handler for DataTable - Load roles (system + custom for current empresa)
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

            var empresaId = User.FindFirst("EmpresaId")?.Value;
            var url = string.IsNullOrEmpty(empresaId) ? "/api/roles" : $"/api/roles?empresaId={empresaId}";
            var response = await client.GetAsync(url);

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
                    var resultJson = resultElement.GetRawText();
                    return new ContentResult { Content = $"{{\"data\":{resultJson}}}", ContentType = "application/json", StatusCode = 200 };
                }

                return new ContentResult { Content = $"{{\"data\":{content}}}", ContentType = "application/json", StatusCode = 200 };
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
                return new ContentResult
                {
                    Content = $"{{\"success\":true,\"data\":{content}}}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
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
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading privilegios");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to get privilegio IDs assigned to a specific role
    public async Task<IActionResult> OnGetRolPrivilegiosAsync(string id)
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

            var response = await client.GetAsync($"/api/roles/{id}/privilegios");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return new JsonResult(new List<int>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading privilegios for role {Id}", id);
            return new JsonResult(new List<int>());
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

            HttpResponseMessage response;
            bool isNew = !rolData.TryGetProperty("id", out var idProp) ||
                         string.IsNullOrEmpty(idProp.GetString());

            if (isNew)
            {
                // Inyectar empresaId del usuario actual para roles personalizados
                var empresaId = User.FindFirst("EmpresaId")?.Value;
                if (!string.IsNullOrEmpty(empresaId))
                {
                    var jsonObj = System.Text.Json.Nodes.JsonNode.Parse(json)!.AsObject();
                    jsonObj["empresaId"] = empresaId;
                    json = jsonObj.ToJsonString();
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                response = await client.PostAsync("/api/roles", content);
            }
            else
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
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

            // Backend expects RolPrivilegiosDto: { rolId, privilegioIds }
            var dto = new { rolId = roleId, privilegioIds = privilegiosData };
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Backend uses HttpPut, not HttpPost
            var response = await client.PutAsync($"/api/roles/{roleId}/privilegios", content);

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
