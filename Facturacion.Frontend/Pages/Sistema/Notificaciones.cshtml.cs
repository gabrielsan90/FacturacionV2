using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Sistema;

[Authorize]
public class NotificacionesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificacionesModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public NotificacionesModel(IHttpClientFactory httpClientFactory, ILogger<NotificacionesModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public async Task OnGetAsync()
    {
        // No initialization needed - data loads via AJAX
        await Task.CompletedTask;
    }

    /// <summary>
    /// Load notifications for current user and selected empresa
    /// </summary>
    public async Task<IActionResult> OnGetDataAsync(Guid empresaId)
    {
        try
        {
            if (empresaId == Guid.Empty)
            {
                return new JsonResult(new { data = new List<object>() });
            }

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
                _logger.LogWarning("No token found in user claims for notificaciones data load");
                return new JsonResult(new { data = new List<object>() });
            }

            var response = await client.GetAsync($"/api/notificaciones/usuario/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var notificaciones = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = notificaciones });
            }

            _logger.LogWarning("Failed to load notificaciones. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notificaciones data");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    /// <summary>
    /// Get unread notifications count
    /// </summary>
    public async Task<IActionResult> OnGetCountNoLeidasAsync(Guid empresaId)
    {
        try
        {
            if (empresaId == Guid.Empty)
            {
                return new JsonResult(new { count = 0 });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/notificaciones/count-no-leidas/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var count = await response.Content.ReadFromJsonAsync<int>(_jsonOptions);
                return new JsonResult(new { count });
            }

            return new JsonResult(new { count = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return new JsonResult(new { count = 0 });
        }
    }

    /// <summary>
    /// Get notification summary
    /// </summary>
    public async Task<IActionResult> OnGetResumenAsync(Guid empresaId)
    {
        try
        {
            if (empresaId == Guid.Empty)
            {
                return new JsonResult(new { success = false, message = "Empresa no seleccionada" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/notificaciones/resumen/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var resumen = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(new { success = true, data = resumen });
            }

            return new JsonResult(new { success = false, message = "Error al cargar resumen" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notification summary");
            return new JsonResult(new { success = false, message = "Error al cargar resumen" });
        }
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    public async Task<IActionResult> OnPostMarcarLeidaAsync(Guid id)
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

            var response = await client.PutAsync($"/api/notificaciones/marcar-leida/{id}", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Notificación marcada como leída" });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to mark notification as read. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return new JsonResult(new { success = false, message = "Error al marcar como leída" });
        }
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    public async Task<IActionResult> OnPostMarcarTodasLeidasAsync(Guid empresaId)
    {
        try
        {
            if (empresaId == Guid.Empty)
            {
                return new JsonResult(new { success = false, message = "Empresa no seleccionada" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PutAsync($"/api/notificaciones/marcar-todas-leidas/{empresaId}", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Todas las notificaciones marcadas como leídas" });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to mark all notifications as read. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return new JsonResult(new { success = false, message = "Error al marcar todas como leídas" });
        }
    }

    /// <summary>
    /// Delete notification
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
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

            var response = await client.DeleteAsync($"/api/notificaciones/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Notificación eliminada exitosamente" });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete notification {Id}. Status: {StatusCode}, Error: {Error}",
                id, response.StatusCode, errorContent);

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar notificación" });
        }
    }

    /// <summary>
    /// Delete expired notifications
    /// </summary>
    public async Task<IActionResult> OnPostDeleteExpiradasAsync(Guid empresaId)
    {
        try
        {
            if (empresaId == Guid.Empty)
            {
                return new JsonResult(new { success = false, message = "Empresa no seleccionada" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/notificaciones/expiradas/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Notificaciones expiradas eliminadas exitosamente" });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete expired notifications. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expired notifications");
            return new JsonResult(new { success = false, message = "Error al eliminar notificaciones expiradas" });
        }
    }
}
