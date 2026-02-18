using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Facturacion.Frontend.Pages.Seguridad;

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

    // Handler for DataTable - Load all users
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

            var response = await client.GetAsync("/api/usuarios");

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

            _logger.LogWarning("Failed to load usuarios. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading usuarios");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    // Handler to get a single user by ID
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
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult
                {
                    Content = $"{{\"success\":true,\"data\":{content}}}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogWarning("Usuario not found with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Usuario no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading usuario with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar el usuario" });
        }
    }

    // Handler to get roles (system + custom for current empresa)
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

            var empresaId = User.FindFirst("EmpresaId")?.Value;
            var url = string.IsNullOrEmpty(empresaId) ? "/api/roles" : $"/api/roles?empresaId={empresaId}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var roles = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(roles ?? new List<object>());
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading roles");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to get all empresas
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

            var response = await client.GetAsync("/api/empresas");

            if (response.IsSuccessStatusCode)
            {
                var empresas = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(empresas ?? new List<object>());
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading empresas");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to save (create or update) a user
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement usuarioData)
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

            bool isNew = !usuarioData.TryGetProperty("id", out var idProp) ||
                         string.IsNullOrEmpty(idProp.GetString());

            // Generar contraseña aleatoria en el PageModel si se solicitó
            string? generatedPassword = null;
            bool generateRandomPwd = usuarioData.TryGetProperty("generateRandomPassword", out var grpProp)
                                     && grpProp.ValueKind == JsonValueKind.True;

            if (isNew && generateRandomPwd)
            {
                generatedPassword = GenerateRandomPassword();
                var jsonObj = JsonNode.Parse(json)!.AsObject();
                jsonObj["password"] = generatedPassword;
                jsonObj["confirmPassword"] = generatedPassword;
                jsonObj["generateRandomPassword"] = false;
                json = jsonObj.ToJsonString();
                _logger.LogInformation("Generated random password for new user");
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (isNew)
            {
                response = await client.PostAsync("/api/usuarios", content);
            }
            else
            {
                var id = idProp.GetString();
                response = await client.PutAsync($"/api/usuarios/{id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Usuario {Action} successfully", isNew ? "created" : "updated");

                // Si se generó contraseña aleatoria, verificar si el email fue enviado
                if (generatedPassword != null)
                {
                    bool emailSent = false;
                    try
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        using var responseDoc = JsonDocument.Parse(responseContent);
                        var root = responseDoc.RootElement;
                        emailSent = root.TryGetProperty("emailSent", out var esProp) && esProp.GetBoolean();
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogWarning(parseEx, "Could not parse API response for email info");
                    }

                    return new JsonResult(new
                    {
                        success = true,
                        message = emailSent
                            ? "Usuario creado exitosamente. La contraseña fue enviada por correo."
                            : "Usuario creado exitosamente. No se pudo enviar el correo.",
                        passwordGenerated = true,
                        generatedPassword = emailSent ? null : generatedPassword,
                        emailSent
                    });
                }

                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Usuario creado exitosamente" : "Usuario actualizado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save usuario. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving usuario");
            return new JsonResult(new { success = false, message = "Error al guardar el usuario" });
        }
    }

    private static string GenerateRandomPassword(int length = 12)
    {
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%&*";

        var random = new Random();
        var password = new List<char>
        {
            upper[random.Next(upper.Length)],
            lower[random.Next(lower.Length)],
            digits[random.Next(digits.Length)],
            special[random.Next(special.Length)]
        };

        var allChars = upper + lower + digits + special;
        for (int i = password.Count; i < length; i++)
            password.Add(allChars[random.Next(allChars.Length)]);

        return new string(password.OrderBy(_ => random.Next()).ToArray());
    }

    // Handler to assign roles to user
    public async Task<IActionResult> OnPostAsignarRolesAsync(string userId, [FromBody] JsonElement rolesData)
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

            // Backend expects UpdateUserRolesDto: { roles: [...] }
            var dto = new { roles = rolesData };
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Backend uses HttpPut, not HttpPost
            var response = await client.PutAsync($"/api/usuarios/{userId}/roles", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Roles asignados exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning roles to user");
            return new JsonResult(new { success = false, message = "Error al asignar roles" });
        }
    }

    // Handler to assign empresas to user
    public async Task<IActionResult> OnPostAsignarEmpresasAsync(string userId, [FromBody] JsonElement empresasData)
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

            var json = JsonSerializer.Serialize(empresasData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/usuarios/{userId}/empresas", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Empresas asignadas exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning empresas to user");
            return new JsonResult(new { success = false, message = "Error al asignar empresas" });
        }
    }

    // Handler to reset password
    public async Task<IActionResult> OnPostResetPasswordAsync(string userId)
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

            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"/api/usuarios/{userId}/reset-password", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var message = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Contraseña restablecida";
                var emailSent = root.TryGetProperty("emailSent", out var emailProp) && emailProp.GetBoolean();
                string? generatedPassword = root.TryGetProperty("generatedPassword", out var pwdProp) && pwdProp.ValueKind == JsonValueKind.String
                    ? pwdProp.GetString() : null;

                return new JsonResult(new { success = true, message, emailSent, generatedPassword });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return new JsonResult(new { success = false, message = "Error al restablecer contraseña" });
        }
    }

    // Handler to toggle user active status
    public async Task<IActionResult> OnPostToggleStatusAsync(string userId)
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

            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"/api/usuarios/{userId}/toggle-status", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Estado actualizado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status");
            return new JsonResult(new { success = false, message = "Error al cambiar estado del usuario" });
        }
    }

    // Handler to delete a user
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
                _logger.LogInformation("Usuario deleted successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Usuario eliminado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete usuario with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting usuario with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el usuario" });
        }
    }

    // Handler to unlock a locked user
    public async Task<IActionResult> OnPostUnlockAsync(string userId)
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

            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"/api/usuarios/{userId}/unlock", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Usuario desbloqueado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user {UserId}", userId);
            return new JsonResult(new { success = false, message = "Error al desbloquear el usuario" });
        }
    }
}
