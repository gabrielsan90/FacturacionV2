using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Configuracion;

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
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    }

    public async Task OnGetAsync()
    {
        // No initialization needed - DataTable loads data via AJAX
        await Task.CompletedTask;
    }

    /// <summary>
    /// Load data for DataTable via AJAX
    /// </summary>
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Include JWT from User claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("No token found in user claims for roles data load");
                return new JsonResult(new { data = new List<object>() });
            }

            var response = await client.GetAsync("/api/roles");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = $"{{\"data\":{content}}}", ContentType = "application/json", StatusCode = 200 };
            }

            _logger.LogWarning("Failed to load roles. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading roles data");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    /// <summary>
    /// Load privilegios grouped by módulo
    /// </summary>
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

            _logger.LogWarning("Failed to load privilegios. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, message = "Error al cargar privilegios" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading privilegios");
            return new JsonResult(new { success = false, message = "Error al cargar privilegios" });
        }
    }

    /// <summary>
    /// Get single rol details
    /// </summary>
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

            _logger.LogWarning("Failed to load rol {Id}. Status: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { success = false, message = "Rol no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading rol {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar rol" });
        }
    }

    /// <summary>
    /// Get privilegios for specific rol
    /// </summary>
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
                var privilegioIds = await response.Content.ReadFromJsonAsync<List<int>>(_jsonOptions);
                return new JsonResult(privilegioIds);
            }

            _logger.LogWarning("Failed to load privilegios for rol {Id}. Status: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new List<int>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading privilegios for rol {Id}", id);
            return new JsonResult(new List<int>());
        }
    }

    /// <summary>
    /// Create or update rol
    /// </summary>
    public async Task<IActionResult> OnPostSaveAsync([FromBody] dynamic rolData)
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
            var rolId = rolData.GetProperty("id").GetString();
            var isUpdate = !string.IsNullOrEmpty(rolId);

            if (isUpdate)
            {
                response = await client.PutAsync($"/api/roles/{rolId}", content);
            }
            else
            {
                response = await client.PostAsync("/api/roles", content);
            }

            if (response.IsSuccessStatusCode)
            {
                var resultContent = await response.Content.ReadAsStringAsync();
                var message = isUpdate ? "Rol actualizado exitosamente" : "Rol creado exitosamente";
                return new ContentResult
                {
                    Content = $"{{\"success\":true,\"message\":\"{message}\",\"data\":{resultContent}}}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save rol. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving rol");
            return new JsonResult(new { success = false, message = "Error al guardar rol" });
        }
    }

    /// <summary>
    /// Delete rol
    /// </summary>
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
                return new JsonResult(new { success = true, message = "Rol eliminado exitosamente" });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete rol {Id}. Status: {StatusCode}, Error: {Error}",
                id, response.StatusCode, errorContent);

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rol {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar rol" });
        }
    }

    /// <summary>
    /// Save rol privilegios
    /// </summary>
    public async Task<IActionResult> OnPostSavePrivilegiosAsync(string id, [FromBody] dynamic privilegiosData)
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

            var response = await client.PutAsync($"/api/roles/{id}/privilegios", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Privilegios actualizados exitosamente" });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save privilegios for rol {Id}. Status: {StatusCode}, Error: {Error}",
                id, response.StatusCode, errorContent);

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving privilegios for rol {Id}", id);
            return new JsonResult(new { success = false, message = "Error al guardar privilegios" });
        }
    }
}
