using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.ActivosFijos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class DepreciacionesActivoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DepreciacionesActivoModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public string EmpresaId { get; set; } = "";

    public DepreciacionesActivoModel(IHttpClientFactory httpClientFactory, ILogger<DepreciacionesActivoModel> logger)
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

    // Handler for DataTable - Load depreciaciones with filters
    public async Task<IActionResult> OnGetDataAsync(string? activoId, int? anio, int? mes, string? estado)
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { data = new List<DepreciacionActivo>() });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Build query string with filters
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(activoId))
                queryParams.Add($"activoId={activoId}");
            if (anio.HasValue)
                queryParams.Add($"anio={anio}");
            if (mes.HasValue)
                queryParams.Add($"mes={mes}");
            if (!string.IsNullOrEmpty(estado))
                queryParams.Add($"estado={estado}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/activosfijos/empresa/{empresaId}/depreciaciones{queryString}";

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var depreciaciones = await response.Content.ReadFromJsonAsync<List<DepreciacionActivo>>(_jsonOptions);
                return new JsonResult(new { data = depreciaciones ?? new List<DepreciacionActivo>() });
            }

            _logger.LogWarning("Failed to load depreciaciones. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<DepreciacionActivo>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading depreciaciones");
            return new JsonResult(new { data = new List<DepreciacionActivo>() });
        }
    }

    // Handler to get a single depreciacion by ID
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

            var response = await client.GetAsync($"/api/depreciacionesactivo/{id}");

            if (response.IsSuccessStatusCode)
            {
                var depreciacion = await response.Content.ReadFromJsonAsync<DepreciacionActivo>(_jsonOptions);
                return new JsonResult(depreciacion);
            }

            _logger.LogWarning("Depreciacion not found. ID: {Id}, Status code: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { error = "Depreciación no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading depreciacion details. ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar la depreciación" });
        }
    }

    // Handler to delete a depreciacion
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

            var response = await client.DeleteAsync($"/api/depreciacionesactivo/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Depreciación eliminada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete depreciacion. ID: {Id}, Status code: {StatusCode}, Error: {Error}", id, response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting depreciacion. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar la depreciación" });
        }
    }

    // Handler to generate depreciation for a period
    public async Task<IActionResult> OnPostGenerarDepreciacionAsync([FromBody] GenerarDepreciacionRequest request)
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"/api/activosfijos/empresa/{empresaId}/generar-depreciacion";
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GenerarDepreciacionResponse>(_jsonOptions);
                var message = result != null
                    ? $"Depreciación generada exitosamente. {result.ActivosProcesados} activos procesados, {result.DepreciacionesGeneradas} depreciaciones generadas."
                    : "Depreciación generada exitosamente";

                return new JsonResult(new { success = true, message });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to generate depreciacion. Status code: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating depreciacion");
            return new JsonResult(new { success = false, message = "Error al generar la depreciación" });
        }
    }

    // Handler to get activos (for dropdown filter)
    public async Task<IActionResult> OnGetActivosAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new List<ActivoFijo>());
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/activosfijos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var activos = await response.Content.ReadFromJsonAsync<List<ActivoFijo>>(_jsonOptions);
                return new JsonResult(activos ?? new List<ActivoFijo>());
            }

            _logger.LogWarning("Failed to load activos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<ActivoFijo>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading activos");
            return new JsonResult(new List<ActivoFijo>());
        }
    }

    // DTOs for request/response
    public class GenerarDepreciacionRequest
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string? Observaciones { get; set; }
    }

    public class GenerarDepreciacionResponse
    {
        public int ActivosProcesados { get; set; }
        public int DepreciacionesGeneradas { get; set; }
    }
}
