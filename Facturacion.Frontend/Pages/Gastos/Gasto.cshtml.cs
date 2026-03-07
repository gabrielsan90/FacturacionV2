using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Gastos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class GastoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GastoModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public GastoModel(IHttpClientFactory httpClientFactory, ILogger<GastoModel> logger)
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

    public void OnGet()
    {
        // Page initialization
    }

    public async Task<IActionResult> OnGetDataAsync()
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
                return new JsonResult(new { data = new List<object>() });
            }

            var response = await client.GetAsync($"/api/Gastos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var gastos = await response.Content.ReadFromJsonAsync<List<Gasto>>(_jsonOptions)
                    ?? new List<Gasto>();
                var data = gastos.Select(g => new
                {
                    id = g.Id,
                    fecha = g.FechaGasto,
                    tipoDocumento = string.IsNullOrWhiteSpace(g.Comprobante) ? "Gasto" : g.Comprobante,
                    numeroDocumento = g.NumeroDocumento,
                    proveedorNombre = g.Proveedor?.Nombre ?? "Sin proveedor",
                    categoria = g.CategoriaGasto?.Nombre ?? "",
                    descripcion = g.Descripcion,
                    monto = g.MontoTotal
                }).ToList();
                return new JsonResult(new { data });
            }

            _logger.LogWarning("Failed to load gastos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading gastos");
            return new JsonResult(new { data = new List<object>() });
        }
    }

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

            var response = await client.GetAsync($"/api/Gastos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var gasto = await response.Content.ReadFromJsonAsync<Gasto>(_jsonOptions);
                if (gasto == null)
                {
                    return new JsonResult(new { error = "Gasto no encontrado" });
                }

                return new JsonResult(new
                {
                    id = gasto.Id,
                    fecha = gasto.FechaGasto,
                    proveedorId = gasto.ProveedorId,
                    tipoDocumento = gasto.Comprobante,
                    numeroDocumento = gasto.NumeroDocumento,
                    categoriaGastoId = gasto.CategoriaGastoId,
                    monto = gasto.MontoTotal,
                    descripcion = gasto.Descripcion,
                    metodoPago = (int)gasto.FormaPago,
                    montoRetencionRenta = gasto.MontoRetencionRenta,
                    montoRetencionIVA = gasto.MontoRetencionIVA
                });
            }

            _logger.LogWarning("Gasto not found. ID: {Id}", id);
            return new JsonResult(new { error = "Gasto no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading gasto details. ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar el gasto" });
        }
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] GastoSaveRequest gastoData)
    {
        if (gastoData == null)
        {
            return new JsonResult(new { success = false, message = "Datos del gasto son requeridos" });
        }

        var validationErrors = new List<string>();
        if (gastoData.CategoriaGastoId <= 0) validationErrors.Add("Categoría es requerida");
        if (gastoData.Monto <= 0) validationErrors.Add("Monto debe ser mayor a cero");
        if (string.IsNullOrWhiteSpace(gastoData.NumeroDocumento)) validationErrors.Add("Número de documento es requerido");
        if (string.IsNullOrWhiteSpace(gastoData.Descripcion)) validationErrors.Add("Descripción es requerida");
        if (gastoData.Fecha == default) validationErrors.Add("Fecha es requerida");
        if (gastoData.FormaPago <= 0) validationErrors.Add("Forma de pago es requerida");

        if (validationErrors.Count > 0)
        {
            return new JsonResult(new { success = false, message = string.Join(". ", validationErrors) });
        }

        try
        {
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

            if (!Enum.IsDefined(typeof(FormaPago), gastoData.FormaPago))
            {
                return new JsonResult(new { success = false, message = "Forma de pago invalida" });
            }

            var dto = new GastoDTO
            {
                Id = gastoData.Id.HasValue && gastoData.Id.Value != Guid.Empty ? gastoData.Id : null,
                EmpresaId = empresaGuid,
                ProveedorId = gastoData.ProveedorId,
                CategoriaGastoId = gastoData.CategoriaGastoId,
                NumeroDocumento = gastoData.NumeroDocumento.Trim(),
                FechaGasto = gastoData.Fecha,
                Descripcion = gastoData.Descripcion.Trim(),
                MontoSubtotal = gastoData.Monto,
                MontoImpuesto = 0,
                MontoTotal = gastoData.Monto,
                Moneda = TipoMoneda.CRC,
                FormaPago = (FormaPago)gastoData.FormaPago,
                EstadoPago = EstadoPago.Pendiente,
                MontoPagado = 0,
                Comprobante = string.IsNullOrWhiteSpace(gastoData.TipoDocumento) ? null : gastoData.TipoDocumento,
                MontoRetencionRenta = gastoData.MontoRetencionRenta,
                MontoRetencionIVA = gastoData.MontoRetencionIVA
            };

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = !gastoData.Id.HasValue || gastoData.Id.Value == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/Gastos", content);
            }
            else
            {
                response = await client.PutAsync($"/api/Gastos/{gastoData.Id}", content);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Gasto API response: StatusCode={StatusCode}, Body={Body}", response.StatusCode, responseBody);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Gasto creado exitosamente" : "Gasto actualizado exitosamente"
                });
            }

            _logger.LogWarning("Failed to save gasto. StatusCode: {StatusCode}, Error: {Error}", response.StatusCode, responseBody);

            // Extract a human-readable error message from the API response
            var errorMessage = ExtractErrorMessage(responseBody, response.StatusCode);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving gasto");
            return new JsonResult(new { success = false, message = "Error al guardar el gasto: " + ex.Message });
        }
    }

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

            var response = await client.DeleteAsync($"/api/Gastos/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Gasto eliminado exitosamente" });
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete gasto. ID: {Id}, StatusCode: {StatusCode}, Error: {Error}", id, response.StatusCode, errorBody);
            var errorMessage = ExtractErrorMessage(errorBody, response.StatusCode);
            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting gasto. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el gasto" });
        }
    }

    public async Task<IActionResult> OnGetProveedoresAsync()
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
                return new JsonResult(new List<object>());
            }

            var response = await client.GetAsync($"/api/Proveedores/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<Proveedor>>(_jsonOptions);
                return new JsonResult(data ?? new List<Proveedor>());
            }

            _logger.LogWarning("Failed to load proveedores. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading proveedores");
            return new JsonResult(new List<object>());
        }
    }

    public async Task<IActionResult> OnGetCategoriasGastoAsync()
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
            var response = await client.GetAsync(
                !string.IsNullOrEmpty(empresaId)
                    ? $"/api/CategoriasGasto/activas/empresa/{empresaId}"
                    : "/api/CategoriasGasto/activas");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<CategoriaGasto>>(_jsonOptions);
                return new JsonResult(data ?? new List<CategoriaGasto>());
            }

            _logger.LogWarning("Failed to load categorias gasto. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorias gasto");
            return new JsonResult(new List<object>());
        }
    }

    /// <summary>
    /// Extracts a human-readable error message from an API error response.
    /// Handles plain text, ValidationProblemDetails JSON, and empty bodies.
    /// NEVER returns an empty string — always provides a meaningful message.
    /// </summary>
    private static string ExtractErrorMessage(string errorBody, System.Net.HttpStatusCode statusCode)
    {
        var fallbackMessage = statusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => "Los datos enviados no son válidos",
            System.Net.HttpStatusCode.Forbidden => "No tiene permisos para realizar esta acción",
            System.Net.HttpStatusCode.Unauthorized => "Su sesión ha expirado. Por favor inicie sesión nuevamente",
            System.Net.HttpStatusCode.NotFound => "El recurso solicitado no fue encontrado",
            System.Net.HttpStatusCode.Conflict => "Conflicto: el registro ya existe o fue modificado",
            _ => $"Error del servidor (código {(int)statusCode})"
        };

        if (string.IsNullOrWhiteSpace(errorBody))
        {
            return fallbackMessage;
        }

        // Try to parse as JSON (ValidationProblemDetails or similar)
        try
        {
            using var doc = JsonDocument.Parse(errorBody);
            var root = doc.RootElement;

            // Check for ValidationProblemDetails format: { "title": "...", "errors": { ... } }
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var messages = new List<string>();
                foreach (var field in errors.EnumerateObject())
                {
                    if (field.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var msg in field.Value.EnumerateArray())
                        {
                            var msgStr = msg.GetString();
                            if (!string.IsNullOrWhiteSpace(msgStr))
                            {
                                messages.Add(msgStr);
                            }
                        }
                    }
                }
                if (messages.Count > 0)
                {
                    return string.Join(". ", messages);
                }
            }

            // Check for { "title": "..." } or { "message": "..." }
            if (root.TryGetProperty("title", out var title) && title.GetString() is string t && !string.IsNullOrWhiteSpace(t))
            {
                return t;
            }
            if (root.TryGetProperty("message", out var message) && message.GetString() is string m && !string.IsNullOrWhiteSpace(m))
            {
                return m;
            }

            // If it's a simple JSON string value
            if (root.ValueKind == JsonValueKind.String)
            {
                var str = root.GetString();
                if (!string.IsNullOrWhiteSpace(str))
                {
                    return str;
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON — try as plain text below
        }

        // Return plain text if non-empty, otherwise the fallback
        return !string.IsNullOrWhiteSpace(errorBody) ? errorBody.Trim() : fallbackMessage;
    }

    public sealed class GastoSaveRequest
    {
        public Guid? Id { get; set; }
        public DateTime Fecha { get; set; }
        public Guid? ProveedorId { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public int CategoriaGastoId { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public int FormaPago { get; set; }
        public decimal MontoRetencionRenta { get; set; }
        public decimal MontoRetencionIVA { get; set; }
    }
}

