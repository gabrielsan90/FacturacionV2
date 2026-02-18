using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Seguridad;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class AuditoriaAccesosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuditoriaAccesosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public string EmpresaId { get; set; } = "";

    public AuditoriaAccesosModel(IHttpClientFactory httpClientFactory, ILogger<AuditoriaAccesosModel> logger)
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
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    // Handler for DataTable - Load audit logs with filters
    public async Task<IActionResult> OnGetDataAsync(
        string? fechaInicio,
        string? fechaFin,
        string? usuarioId,
        string? accion,
        string? exitoso)
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Build query parameters
            var queryParams = new List<string>();
            queryParams.Add($"empresaId={empresaId}");

            if (!string.IsNullOrEmpty(fechaInicio))
                queryParams.Add($"fechaInicio={fechaInicio}");
            if (!string.IsNullOrEmpty(fechaFin))
                queryParams.Add($"fechaFin={fechaFin}");
            if (!string.IsNullOrEmpty(usuarioId) && usuarioId != "TODOS")
                queryParams.Add($"usuarioId={usuarioId}");
            if (!string.IsNullOrEmpty(accion) && accion != "TODAS")
                queryParams.Add($"accion={accion}");
            if (!string.IsNullOrEmpty(exitoso) && exitoso != "TODOS")
                queryParams.Add($"exitoso={exitoso}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/auditoria/accesos{queryString}";

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                // API returns ActionResponse<T> with result property
                if (doc.RootElement.TryGetProperty("result", out var resultElement))
                {
                    var resultJson = resultElement.GetRawText();
                    return new ContentResult { Content = $"{{\"data\":{resultJson}}}", ContentType = "application/json", StatusCode = 200 };
                }

                return new ContentResult { Content = $"{{\"data\":{content}}}", ContentType = "application/json", StatusCode = 200 };
            }

            _logger.LogWarning("Failed to load audit logs. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit logs");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    // Handler to get audit log details
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

            var response = await client.GetAsync($"/api/auditoria/{id}");

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

            _logger.LogWarning("Audit log not found with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Registro de auditoría no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit log with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar el registro de auditoría" });
        }
    }

    // Handler to get usuarios for filter
    public async Task<IActionResult> OnGetUsuariosAsync()
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
                var usuarios = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(usuarios ?? new List<object>());
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading usuarios");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to get statistics
    public async Task<IActionResult> OnGetEstadisticasAsync(string? fechaInicio, string? fechaFin)
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var queryParams = new List<string>();
            queryParams.Add($"empresaId={empresaId}");
            if (!string.IsNullOrEmpty(fechaInicio))
                queryParams.Add($"fechaInicio={fechaInicio}");
            if (!string.IsNullOrEmpty(fechaFin))
                queryParams.Add($"fechaFin={fechaFin}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/auditoria/estadisticas{queryString}";

            var response = await client.GetAsync(url);

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

            return new JsonResult(new { success = false, message = "Error al obtener estadísticas" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading estadisticas");
            return new JsonResult(new { success = false, message = "Error al cargar estadísticas" });
        }
    }

    // Handler to export audit logs to Excel
    public async Task<IActionResult> OnGetExportarAsync(
        string? fechaInicio,
        string? fechaFin,
        string? usuarioId,
        string? accion)
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return BadRequest("Empresa no definida");
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Build query parameters
            var queryParams = new List<string>();
            queryParams.Add($"empresaId={empresaId}");
            if (!string.IsNullOrEmpty(fechaInicio))
                queryParams.Add($"fechaInicio={fechaInicio}");
            if (!string.IsNullOrEmpty(fechaFin))
                queryParams.Add($"fechaFin={fechaFin}");
            if (!string.IsNullOrEmpty(usuarioId) && usuarioId != "TODOS")
                queryParams.Add($"usuarioId={usuarioId}");
            if (!string.IsNullOrEmpty(accion) && accion != "TODAS")
                queryParams.Add($"accion={accion}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/auditoria/exportar{queryString}";

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var fileName = $"AuditoriaAccesos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            return BadRequest("Error al exportar los datos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs");
            return BadRequest("Error al exportar los datos");
        }
    }
}
