using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Ventas;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado,Consultor")]
public class HistorialVentasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public string EmpresaId { get; set; } = "";

    public HistorialVentasModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
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

    // Handler for DataTable - Load sales history
    public async Task<IActionResult> OnGetDataAsync(string? fechaInicio, string? fechaFin, string? clienteId, string? sucursalId)
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { data = new List<object>() });
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        // Get JWT token from user claims
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Build query parameters
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(fechaInicio))
            queryParams.Add($"fechaInicio={fechaInicio}");
        if (!string.IsNullOrEmpty(fechaFin))
            queryParams.Add($"fechaFin={fechaFin}");
        if (!string.IsNullOrEmpty(clienteId) && clienteId != "TODOS")
            queryParams.Add($"clienteId={clienteId}");
        if (!string.IsNullOrEmpty(sucursalId) && sucursalId != "TODAS")
            queryParams.Add($"sucursalId={sucursalId}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var url = $"/api/ventas/historial/empresa/{empresaId}{queryString}";

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            // API may return a plain array or an ActionResponse<T> wrapper
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return new ContentResult { Content = $"{{\"data\":{content}}}", ContentType = "application/json", StatusCode = 200 };
            }

            // If it's an object, check for ActionResponse<T> with result property
            if (doc.RootElement.TryGetProperty("result", out var resultElement))
            {
                var resultJson = resultElement.GetRawText();
                return new ContentResult { Content = $"{{\"data\":{resultJson}}}", ContentType = "application/json", StatusCode = 200 };
            }

            return new ContentResult { Content = $"{{\"data\":[]}}", ContentType = "application/json", StatusCode = 200 };
        }

        return new JsonResult(new { data = new List<object>() });
    }

    // Handler to get summary statistics
    public async Task<IActionResult> OnGetResumenAsync(string? fechaInicio, string? fechaFin)
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
        if (!string.IsNullOrEmpty(fechaInicio))
            queryParams.Add($"fechaInicio={fechaInicio}");
        if (!string.IsNullOrEmpty(fechaFin))
            queryParams.Add($"fechaFin={fechaFin}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var url = $"/api/ventas/resumen/empresa/{empresaId}{queryString}";

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

        return new JsonResult(new { success = false, message = "Error al obtener resumen" });
    }

    // Handler to get clientes for filter
    public async Task<IActionResult> OnGetClientesAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new List<object>());
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/clientes/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var clientes = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(clientes ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to get sucursales for filter
    public async Task<IActionResult> OnGetSucursalesAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new List<object>());
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/sucursales/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var sucursales = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(sucursales ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to export to Excel
    public async Task<IActionResult> OnGetExportarAsync(string? fechaInicio, string? fechaFin, string? clienteId, string? sucursalId)
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
        if (!string.IsNullOrEmpty(fechaInicio))
            queryParams.Add($"fechaInicio={fechaInicio}");
        if (!string.IsNullOrEmpty(fechaFin))
            queryParams.Add($"fechaFin={fechaFin}");
        if (!string.IsNullOrEmpty(clienteId) && clienteId != "TODOS")
            queryParams.Add($"clienteId={clienteId}");
        if (!string.IsNullOrEmpty(sucursalId) && sucursalId != "TODAS")
            queryParams.Add($"sucursalId={sucursalId}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var url = $"/api/ventas/exportar/empresa/{empresaId}{queryString}";

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            var fileName = $"HistorialVentas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        return BadRequest("Error al exportar los datos");
    }
}
