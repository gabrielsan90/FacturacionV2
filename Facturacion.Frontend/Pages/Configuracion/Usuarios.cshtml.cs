using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Configuracion;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class UsuariosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UsuariosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UsuariosModel(IHttpClientFactory httpClientFactory, ILogger<UsuariosModel> logger)
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
                _logger.LogWarning("No token found in user claims for usuarios data load");
                return new JsonResult(new { data = new List<object>() });
            }

            var response = await client.GetAsync("/api/usuarios");

            if (response.IsSuccessStatusCode)
            {
                var usuarios = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = usuarios });
            }

            _logger.LogWarning("Failed to load usuarios. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading usuarios data");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    /// <summary>
    /// Load roles for dropdown
    /// </summary>
    public async Task<IActionResult> OnGetRolesAsync()
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
                var roles = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(roles);
            }

            _logger.LogWarning("Failed to load roles. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, message = "Error al cargar roles" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading roles");
            return new JsonResult(new { success = false, message = "Error al cargar roles" });
        }
    }

    /// <summary>
    /// Load empresas for dropdown
    /// </summary>
    public async Task<IActionResult> OnGetEmpresasAsync()
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

            var response = await client.GetAsync("/api/empresas/active");

            if (response.IsSuccessStatusCode)
            {
                var empresas = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(empresas);
            }

            _logger.LogWarning("Failed to load empresas. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, message = "Error al cargar empresas" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading empresas");
            return new JsonResult(new { success = false, message = "Error al cargar empresas" });
        }
    }

    /// <summary>
    /// Get single usuario details
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

            var response = await client.GetAsync($"/api/usuarios/{id}");

            if (response.IsSuccessStatusCode)
            {
                var usuario = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(new { success = true, data = usuario });
            }

            _logger.LogWarning("Failed to load usuario {Id}. Status: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { success = false, message = "Usuario no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading usuario {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar usuario" });
        }
    }

    /// <summary>
    /// Create or update usuario
    /// </summary>
    public async Task<IActionResult> OnPostSaveAsync([FromBody] dynamic usuarioData)
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

            var json = JsonSerializer.Serialize(usuarioData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            var usuarioId = usuarioData.GetProperty("id").GetString();
            var isUpdate = !string.IsNullOrEmpty(usuarioId);

            if (isUpdate)
            {
                response = await client.PutAsync($"/api/usuarios/{usuarioId}", content);
            }
            else
            {
                response = await client.PostAsync("/api/usuarios", content);
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(new
                {
                    success = true,
                    message = isUpdate ? "Usuario actualizado exitosamente" : "Usuario creado exitosamente",
                    data = result
                });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save usuario. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving usuario");
            return new JsonResult(new { success = false, message = "Error al guardar usuario" });
        }
    }

    /// <summary>
    /// Delete usuario
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

            var response = await client.DeleteAsync($"/api/usuarios/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Usuario eliminado exitosamente" });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete usuario {Id}. Status: {StatusCode}, Error: {Error}",
                id, response.StatusCode, errorContent);

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting usuario {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar usuario" });
        }
    }
}
