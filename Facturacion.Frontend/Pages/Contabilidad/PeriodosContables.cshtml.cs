using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Contador")]
public class PeriodosContablesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PeriodosContablesModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PeriodosContablesModel(IHttpClientFactory httpClientFactory, ILogger<PeriodosContablesModel> logger)
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

    // Handler for DataTable - Load all periodos contables
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new ContentResult { Content = "{\"data\":[]}", ContentType = "application/json", StatusCode = 200 };
            }

            var response = await client.GetAsync($"/api/PeriodosContables/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Unwrap ActionResponse if needed
                using var doc = JsonDocument.Parse(content);
                string dataJson;
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    dataJson = content;
                }
                else if (doc.RootElement.TryGetProperty("result", out var resultProp))
                {
                    dataJson = resultProp.GetRawText();
                }
                else
                {
                    dataJson = "[]";
                }

                return new ContentResult
                {
                    Content = $"{{\"data\":{dataJson}}}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogWarning("Failed to load periodos contables. Status: {StatusCode}", response.StatusCode);
            return new ContentResult { Content = "{\"data\":[]}", ContentType = "application/json", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading periodos contables");
            return new ContentResult { Content = "{\"data\":[]}", ContentType = "application/json", StatusCode = 200 };
        }
    }

    // Handler to get currently open period
    public async Task<IActionResult> OnGetPeriodoAbiertoAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida" });
            }

            var response = await client.GetAsync($"/api/PeriodosContables/periodo-abierto/{empresaId}");

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
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new JsonResult(new { success = false, message = "No hay período abierto" });
            }

            _logger.LogWarning("Failed to load periodo abierto. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, message = "Error al obtener período abierto" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading periodo abierto");
            return new JsonResult(new { success = false, message = "Error al obtener período abierto" });
        }
    }

    // Handler to get a single periodo by ID
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

            var response = await client.GetAsync($"/api/PeriodosContables/{id}");

            if (response.IsSuccessStatusCode)
            {
                var periodo = await response.Content.ReadFromJsonAsync<PeriodoContableDTO>(_jsonOptions);
                return new JsonResult(periodo);
            }

            _logger.LogWarning("Periodo contable not found with ID: {Id}", id);
            return new JsonResult(new { error = "Período contable no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading periodo contable with ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar el período contable" });
        }
    }

    // Handler to save (create) a periodo manually
    public async Task<IActionResult> OnPostSaveAsync([FromBody] PeriodoContableDTO periodoData)
    {
        try
        {
            // No validar ModelState aquí: la validación real la hace el API backend.

            // Validate date range
            if (periodoData.FechaInicio >= periodoData.FechaFin)
            {
                return new JsonResult(new { success = false, message = "La fecha de inicio debe ser anterior a la fecha fin" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId) || !Guid.TryParse(empresaId, out var empresaGuid))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
            }

            // Ensure estado is PND for new periods (user opens them later)
            periodoData.Estado = "PND";

            var json = JsonSerializer.Serialize(periodoData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/PeriodosContables", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Periodo contable created successfully");
                return new JsonResult(new
                {
                    success = true,
                    message = "Período contable creado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save periodo contable. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving periodo contable");
            return new JsonResult(new { success = false, message = "Error al guardar el período contable" });
        }
    }

    // Handler to open a period (auto-closes any currently open period)
    public async Task<IActionResult> OnPostAbrirAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/PeriodosContables/{id}/abrir", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Periodo contable opened successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Período abierto exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to open periodo contable with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening periodo contable with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al abrir el período contable" });
        }
    }

    // Handler to close a period
    public async Task<IActionResult> OnPostCerrarAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email);

            // Send request to close period
            var requestData = new
            {
                CerradoPor = userName
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/PeriodosContables/{id}/cerrar", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Periodo contable closed successfully. ID: {Id}, User: {User}", id, userName);
                return new JsonResult(new { success = true, message = "Período cerrado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to close periodo contable with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing periodo contable with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cerrar el período contable" });
        }
    }

    // Handler to generate 12 periods for a year
    public async Task<IActionResult> OnPostGenerarAnioAsync([FromBody] GenerarAnioRequest request)
    {
        try
        {
            if (request.Anio < 2020 || request.Anio > 2050)
            {
                return new JsonResult(new { success = false, message = "Año inválido. Debe estar entre 2020 y 2050" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
            }

            var json = JsonSerializer.Serialize(new { Anio = request.Anio });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/PeriodosContables/generar-anio/{empresaId}", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GenerarAnioResponse>(_jsonOptions);
                _logger.LogInformation("Generated {Count} periods for year {Year}", result?.PeriodosCreados ?? 0, request.Anio);

                return new JsonResult(new
                {
                    success = true,
                    message = $"Se generaron {result?.PeriodosCreados ?? 12} períodos para el año {request.Anio}",
                    periodosCreados = result?.PeriodosCreados ?? 0
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to generate periods for year {Year}. Error: {Error}", request.Anio, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating periods for year {Year}", request.Anio);
            return new JsonResult(new { success = false, message = "Error al generar los períodos" });
        }
    }

    // Handler to delete a pending period
    public async Task<IActionResult> OnPostEliminarAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/PeriodosContables/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Periodo contable deleted successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Período eliminado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete periodo contable with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting periodo contable with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el período contable" });
        }
    }

    // DTO Classes
    public class PeriodoContableDTO
    {
        public Guid Id { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string? Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } = "PND";
        public DateTime? FechaCierre { get; set; }
        public string? CerradoPor { get; set; }
    }

    public class GenerarAnioRequest
    {
        public int Anio { get; set; }
    }

    public class GenerarAnioResponse
    {
        public int PeriodosCreados { get; set; }
    }
}
