using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class SeguridadModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SeguridadModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SeguridadModel(IHttpClientFactory httpClientFactory, ILogger<SeguridadModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public void OnGet()
    {
        // Page initialization
    }

    public async Task<IActionResult> OnGetConfiguracionAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/seguridad/configuracion");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data ?? new { minPasswordLength = 8, requireUppercase = true, requireNumbers = true, requireSpecialChars = false, sessionTimeout = 60, maxLoginAttempts = 5, enableTwoFactor = false });
            }

            return new JsonResult(new { minPasswordLength = 8, requireUppercase = true, requireNumbers = true, requireSpecialChars = false, sessionTimeout = 60, maxLoginAttempts = 5, enableTwoFactor = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading security config");
            return new JsonResult(new { minPasswordLength = 8, requireUppercase = true, requireNumbers = true, requireSpecialChars = false, sessionTimeout = 60, maxLoginAttempts = 5, enableTwoFactor = false });
        }
    }

    public async Task<IActionResult> OnPostGuardarConfiguracionAsync([FromBody] dynamic config)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(config);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/seguridad/configuracion", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving security config");
            return new JsonResult(new { success = false, message = "Error al guardar la configuración" });
        }
    }

    public async Task<IActionResult> OnGetAuditLogAsync(string? fecha)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = string.IsNullOrEmpty(fecha) ? "/api/seguridad/audit-log" : $"/api/seguridad/audit-log?fecha={fecha}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(data ?? new List<dynamic>());
            }

            return new JsonResult(new List<dynamic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit log");
            return new JsonResult(new List<dynamic>());
        }
    }

    public async Task<IActionResult> OnGetSesionesActivasAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/seguridad/sesiones-activas");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = data ?? new List<dynamic>() });
            }

            return new JsonResult(new { data = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading active sessions");
            return new JsonResult(new { data = new List<dynamic>() });
        }
    }

    public async Task<IActionResult> OnPostCerrarSesionAsync(string sesionId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/seguridad/cerrar-sesion/{sesionId}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing session");
            return new JsonResult(new { success = false, message = "Error al cerrar sesión" });
        }
    }

    public async Task<IActionResult> OnPostCerrarTodasSesionesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync("/api/seguridad/cerrar-todas-sesiones", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Todas las sesiones han sido cerradas" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing all sessions");
            return new JsonResult(new { success = false, message = "Error al cerrar las sesiones" });
        }
    }
}
