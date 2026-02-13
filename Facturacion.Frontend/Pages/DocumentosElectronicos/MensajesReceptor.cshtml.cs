using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.DocumentosElectronicos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado,Contador")]
public class MensajesReceptorModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MensajesReceptorModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MensajesReceptorModel(IHttpClientFactory httpClientFactory, ILogger<MensajesReceptorModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public string EmpresaId { get; set; } = "";

    public void OnGet()
    {
        // Get the current user's empresa
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    // Handler for DataTable - Load messages with filters
    public async Task<IActionResult> OnGetDataAsync(
        string? empresaId,
        string? fechaInicio,
        string? fechaFin,
        int? tipoMensaje,
        string? estado)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Build query string with filters
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(empresaId))
                queryParams.Add($"empresaId={empresaId}");
            if (!string.IsNullOrEmpty(fechaInicio))
                queryParams.Add($"fechaInicio={fechaInicio}");
            if (!string.IsNullOrEmpty(fechaFin))
                queryParams.Add($"fechaFin={fechaFin}");
            if (tipoMensaje.HasValue)
                queryParams.Add($"tipoMensaje={tipoMensaje}");
            if (!string.IsNullOrEmpty(estado))
                queryParams.Add($"estado={estado}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

            // Note: This endpoint may need to be created in the backend
            // For now, we'll use a generic approach
            var apiUrl = $"/api/MensajesReceptor{queryString}";

            _logger.LogInformation("Calling API: {ApiUrl}", apiUrl);

            var response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);

                _logger.LogInformation("Retrieved {Count} mensajes receptor", data?.Count ?? 0);

                return new JsonResult(new { data = data ?? new List<object>() });
            }

            // Handle error responses
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "API call failed with status {StatusCode}. URL: {ApiUrl}. Response: {ErrorContent}",
                response.StatusCode, apiUrl, errorContent);

            return new JsonResult(new { data = new List<object>() });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception in OnGetDataAsync");
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in OnGetDataAsync");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    // Handler to get a single message by ID (for viewing details)
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

            var response = await client.GetAsync($"/api/MensajesReceptor/{id}");

            if (response.IsSuccessStatusCode)
            {
                var mensaje = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
                return new JsonResult(new { success = true, data = mensaje });
            }

            return new JsonResult(new { success = false, message = "Mensaje no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message details");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    // Handler to get messages for a specific document
    public async Task<IActionResult> OnGetMensajesPorDocumentoAsync(string documentoId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/MensajesReceptor/documento/{documentoId}");

            if (response.IsSuccessStatusCode)
            {
                var mensajes = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(new { success = true, data = mensajes ?? new List<object>() });
            }

            return new JsonResult(new { success = false, message = "No se pudieron obtener los mensajes" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages by document");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    // Handler to resend a message to Hacienda
    public async Task<IActionResult> OnPostReenviarAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/MensajesReceptor/{id}/reenviar", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Mensaje reenviado a Hacienda exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending message");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    // Handler to download XML
    public async Task<IActionResult> OnGetXmlAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/MensajesReceptor/{id}/descargar-xml");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/xml";
                var fileName = $"mensaje_receptor_{id}.xml";

                // Try to get filename from Content-Disposition header
                if (response.Content.Headers.ContentDisposition?.FileName != null)
                {
                    fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
                }

                return File(fileBytes, contentType, fileName);
            }

            return new JsonResult(new { success = false, message = "XML no disponible." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading XML");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    // Handler to get pending documents that need MR response
    public async Task<IActionResult> OnGetDocumentosPendientesAsync(string empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/MensajesReceptor/pendientes/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var documentos = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(new { success = true, data = documentos ?? new List<object>() });
            }

            return new JsonResult(new { success = false, message = "No se pudieron obtener los documentos pendientes" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending documents");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }
}
