using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Contador,Empleado")]
public class AsientosContablesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AsientosContablesModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public IEnumerable<SelectListItem> Periodos { get; set; } = new List<SelectListItem>();

    [BindProperty]
    public AsientoDTO Asiento { get; set; } = new();

    public AsientosContablesModel(IHttpClientFactory httpClientFactory, ILogger<AsientosContablesModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public async Task OnGetAsync()
    {
        await LoadPeriodosAsync();
    }

    // Handler for DataTable - Load journal entries filtered
    public async Task<IActionResult> OnGetDataAsync(string? periodoId, string? estado)
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
                return new JsonResult(new { data = new List<AsientoListDTO>() });
            }

            // Build query string
            var queryParams = new List<string> { $"empresaId={empresaId}" };
            if (!string.IsNullOrEmpty(periodoId) && periodoId != "todos")
            {
                queryParams.Add($"periodoId={periodoId}");
            }
            if (!string.IsNullOrEmpty(estado) && estado != "todos")
            {
                queryParams.Add($"estado={estado}");
            }

            var queryString = string.Join("&", queryParams);
            var response = await client.GetAsync($"/api/asientoscontables?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                // API returns ActionResponse<T> with { wasSuccess, result }
                if (doc.RootElement.TryGetProperty("result", out var resultElement))
                {
                    var asientos = JsonSerializer.Deserialize<List<AsientoListDTO>>(resultElement.GetRawText(), _jsonOptions);
                    return new ContentResult
                    {
                        Content = JsonSerializer.Serialize(new { data = asientos ?? new List<AsientoListDTO>() }, _jsonOptions),
                        ContentType = "application/json",
                        StatusCode = 200
                    };
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var asientos = JsonSerializer.Deserialize<List<AsientoListDTO>>(content, _jsonOptions);
                    return new ContentResult
                    {
                        Content = JsonSerializer.Serialize(new { data = asientos ?? new List<AsientoListDTO>() }, _jsonOptions),
                        ContentType = "application/json",
                        StatusCode = 200
                    };
                }

                return new ContentResult
                {
                    Content = JsonSerializer.Serialize(new { data = new List<AsientoListDTO>() }, _jsonOptions),
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogWarning("Failed to load asientos contables. Status: {StatusCode}", response.StatusCode);
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(new { data = new List<AsientoListDTO>() }, _jsonOptions),
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading asientos contables");
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(new { data = new List<AsientoListDTO>() }, _jsonOptions),
                ContentType = "application/json",
                StatusCode = 200
            };
        }
    }

    // Handler to get a single asiento by ID with its movements
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

            var response = await client.GetAsync($"/api/asientoscontables/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                // API returns ActionResponse<T> with { wasSuccess, result }
                if (doc.RootElement.TryGetProperty("result", out var resultElement))
                {
                    return new ContentResult
                    {
                        Content = $"{{\"success\":true,\"data\":{resultElement.GetRawText()}}}",
                        ContentType = "application/json",
                        StatusCode = 200
                    };
                }

                return new JsonResult(new { success = false, message = "Respuesta inesperada del API" });
            }

            _logger.LogWarning("Asiento contable not found with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Asiento contable no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading asiento contable with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar el asiento contable" });
        }
    }

    // Handler to get periods for dropdown filter
    public async Task<IActionResult> OnGetPeriodosAsync()
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
                return new JsonResult(new List<PeriodoDTO>());
            }

            var response = await client.GetAsync($"/api/periodoscontables/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var periodos = await response.Content.ReadFromJsonAsync<List<PeriodoDTO>>(_jsonOptions);
                return new JsonResult(periodos ?? new List<PeriodoDTO>());
            }

            _logger.LogWarning("Failed to load periodos. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<PeriodoDTO>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading periodos");
            return new JsonResult(new List<PeriodoDTO>());
        }
    }

    // Handler to get movement-enabled accounts for Select2
    public async Task<IActionResult> OnGetCuentasAsync(string? q)
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
                return new JsonResult(new { results = new List<object>() });
            }

            var response = await client.GetAsync($"/api/cuentascontables/empresa/{empresaId}/movimiento");

            if (response.IsSuccessStatusCode)
            {
                var cuentas = await response.Content.ReadFromJsonAsync<List<CuentaDTO>>(_jsonOptions);

                var filtered = cuentas;
                if (!string.IsNullOrEmpty(q))
                {
                    var searchLower = q.ToLower();
                    filtered = cuentas?.Where(c =>
                        c.Codigo.ToLower().Contains(searchLower) ||
                        c.Nombre.ToLower().Contains(searchLower)
                    ).ToList();
                }

                var results = filtered?.Select(c => new
                {
                    id = c.Id.ToString(),
                    text = $"{c.Codigo} - {c.Nombre}",
                    codigo = c.Codigo,
                    nombre = c.Nombre
                }).ToList();

                if (results == null)
                    return new JsonResult(new { results = new List<object>() });
                return new JsonResult(new { results });
            }

            _logger.LogWarning("Failed to load cuentas. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { results = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cuentas");
            return new JsonResult(new { results = new List<object>() });
        }
    }

    // Handler to save (create or update) an asiento contable
    public async Task<IActionResult> OnPostSaveAsync([FromBody] AsientoDTO asientoData)
    {
        try
        {
            // No validar ModelState aquí: la validación real la hace el API backend.

            // Validate that movements balance (Debe = Haber)
            if (asientoData.Movimientos != null && asientoData.Movimientos.Any())
            {
                var totalDebe = asientoData.Movimientos.Sum(m => m.Debe);
                var totalHaber = asientoData.Movimientos.Sum(m => m.Haber);

                if (totalDebe != totalHaber)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = $"El asiento no está cuadrado. Debe: {totalDebe:N2}, Haber: {totalHaber:N2}, Diferencia: {Math.Abs(totalDebe - totalHaber):N2}"
                    });
                }

                if (totalDebe == 0)
                {
                    return new JsonResult(new { success = false, message = "El asiento debe tener al menos un movimiento con monto mayor a cero" });
                }
            }
            else
            {
                return new JsonResult(new { success = false, message = "El asiento debe tener al menos un movimiento" });
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

            // Serialize movements to JSON for sending to API
            asientoData.MovimientosJson = JsonSerializer.Serialize(asientoData.Movimientos);

            var json = JsonSerializer.Serialize(asientoData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = asientoData.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/asientoscontables", content);
            }
            else
            {
                response = await client.PutAsync($"/api/asientoscontables/{asientoData.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Asiento contable {Action} successfully. ID: {Id}", isNew ? "created" : "updated", asientoData.Id);
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Asiento contable creado exitosamente" : "Asiento contable actualizado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save asiento contable. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);

            // Parse ActionResponse message if API returned JSON
            var errorMessage = error;
            try
            {
                using var doc = JsonDocument.Parse(error);
                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                    errorMessage = msgProp.GetString() ?? error;
            }
            catch { /* Not JSON, use raw string */ }

            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving asiento contable");
            return new JsonResult(new { success = false, message = "Error al guardar el asiento contable: " + ex.Message });
        }
    }

    // Handler to approve an asiento contable
    public async Task<IActionResult> OnPostAprobarAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/asientoscontables/{id}/aprobar", null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Asiento contable approved successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Asiento contable aprobado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to approve asiento contable with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving asiento contable with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al aprobar el asiento contable" });
        }
    }

    // Handler to void an asiento contable
    public async Task<IActionResult> OnPostAnularAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/asientoscontables/{id}/anular", null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Asiento contable voided successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Asiento contable anulado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to void asiento contable with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding asiento contable with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al anular el asiento contable" });
        }
    }

    // Handler to approve multiple asientos contables at once
    public async Task<IActionResult> OnPostAprobarMasivoAsync([FromBody] List<string> ids)
    {
        try
        {
            if (ids == null || ids.Count == 0)
                return new JsonResult(new { success = false, message = "No se seleccionaron asientos para aprobar" });

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            int aprobados = 0;
            var errores = new List<string>();

            foreach (var id in ids)
            {
                var response = await client.PostAsync($"/api/asientoscontables/{id}/aprobar", null);
                if (response.IsSuccessStatusCode)
                {
                    aprobados++;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    errores.Add($"ID {id}: {error}");
                }
            }

            var message = $"{aprobados} de {ids.Count} asientos aprobados exitosamente.";
            if (errores.Count > 0)
                message += $" {errores.Count} con errores.";

            return new JsonResult(new { success = aprobados > 0, message, aprobados, erroresCount = errores.Count, errores });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mass approval of asientos contables");
            return new JsonResult(new { success = false, message = "Error al aprobar masivamente: " + ex.Message });
        }
    }

    // Handler to delete an asiento contable (only if in Borrador state)
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

            var response = await client.DeleteAsync($"/api/asientoscontables/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Asiento contable deleted successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Asiento contable eliminado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete asiento contable with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asiento contable with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el asiento contable" });
        }
    }

    private async Task LoadPeriodosAsync()
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
                Periodos = new List<SelectListItem>();
                return;
            }

            var response = await client.GetAsync($"/api/periodoscontables/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var periodos = await response.Content.ReadFromJsonAsync<List<PeriodoDTO>>(_jsonOptions);

                Periodos = periodos?.Where(p => p.EstaAbierto).Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.PeriodoNombre ?? p.Nombre
                }).ToList() ?? new List<SelectListItem>();
            }
            else
            {
                Periodos = new List<SelectListItem>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading periodos for dropdown");
            Periodos = new List<SelectListItem>();
        }
    }
}

// DTOs for AsientosContables
public class AsientoDTO
{
    public Guid Id { get; set; }
    public Guid PeriodoContableId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Today;
    public string TipoAsiento { get; set; } = "DIA"; // DIA, AJU, CIE, APE
    public string Concepto { get; set; } = "";
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
    public string? MovimientosJson { get; set; }
    public List<MovimientoDTO>? Movimientos { get; set; }
}

public class MovimientoDTO
{
    public Guid CuentaContableId { get; set; }
    public string? Descripcion { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string? Tercero { get; set; }
}

public class AsientoListDTO
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public DateTime Fecha { get; set; }
    public string? PeriodoNombre { get; set; }
    public string TipoAsiento { get; set; } = "";
    public string TipoAsientoDescripcion { get; set; } = "";
    public string Concepto { get; set; } = "";
    public string? Referencia { get; set; }
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public string Estado { get; set; } = "";
    public string EstadoDescripcion { get; set; } = "";
    public bool EstaBalanceado { get; set; }
    public string? ModuloOrigen { get; set; }
    public string ModuloOrigenDescripcion { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string? CreadoPorNombre { get; set; }
}

public class PeriodoDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
    public bool EstaAbierto { get; set; }
    public string? PeriodoNombre { get; set; }
}

public class CuentaDTO
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
}
