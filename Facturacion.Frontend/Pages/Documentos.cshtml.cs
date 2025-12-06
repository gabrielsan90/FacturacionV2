using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class DocumentosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DocumentosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DocumentosModel(IHttpClientFactory httpClientFactory, ILogger<DocumentosModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public string EmpresaId { get; set; } = "";

    public void OnGet()
    {
        // Get the current user's empresa (first one for now - TODO: implement empresa selector)
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    // Handler for DataTable - Load documents with filters
    public async Task<IActionResult> OnGetDataAsync(
        string? empresaId,
        string? fechaInicio,
        string? fechaFin,
        int? tipoDocumento,
        int? estado,
        string? sucursalId,
        string? terminalId,
        int? ambiente)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            //// Get JWT token from cookie (as per CLAUDE.md instructions)
            //if (!Request.Cookies.TryGetValue("jwtAdmin", out var jwt))
            //{
            //    _logger.LogWarning("JWT token 'jwtAdmin' not found in cookies for OnGetDataAsync");
            //    return StatusCode(401, new { error = "No se encontró el token de autenticación. Por favor, inicie sesión nuevamente." });
            //}

            //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            // Build query string with filters
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(empresaId))
                queryParams.Add($"empresaId={empresaId}");
            if (!string.IsNullOrEmpty(fechaInicio))
                queryParams.Add($"fechaInicio={fechaInicio}");
            if (!string.IsNullOrEmpty(fechaFin))
                queryParams.Add($"fechaFin={fechaFin}");
            if (tipoDocumento.HasValue)
                queryParams.Add($"tipoDocumento={tipoDocumento}");
            if (estado.HasValue)
                queryParams.Add($"estado={estado}");
            if (!string.IsNullOrEmpty(sucursalId))
                queryParams.Add($"sucursalId={sucursalId}");
            if (!string.IsNullOrEmpty(terminalId))
                queryParams.Add($"terminalId={terminalId}");
            if (ambiente.HasValue)
                queryParams.Add($"ambiente={ambiente}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var apiUrl = $"/api/Documentos{queryString}";

            _logger.LogInformation("Calling API: {ApiUrl}", apiUrl);

            var response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var documentos = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                _logger.LogInformation("Successfully retrieved {Count} documentos from API", documentos?.Count ?? 0);
                return new JsonResult(new { data = documentos ?? new List<object>() });
            }

            // Handle error responses
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "API call failed with status {StatusCode}. URL: {ApiUrl}. Response: {ErrorContent}",
                response.StatusCode, apiUrl, errorContent);

            return StatusCode(
                (int)response.StatusCode,
                new { error = $"Error al obtener documentos: {response.ReasonPhrase}", details = errorContent });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception in OnGetDataAsync");
            return StatusCode(500, new { error = "Error de conexión con el API", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in OnGetDataAsync");
            return StatusCode(500, new { error = "Error inesperado al obtener documentos", details = ex.Message });
        }
    }

    // Handler to get a single document by ID (for viewing)
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/Documentos/{id}");

        if (response.IsSuccessStatusCode)
        {
            var documento = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
            return new JsonResult(new { success = true, data = documento });
        }

        return new JsonResult(new { success = false, message = "Documento no encontrado" });
    }

    // Handler to get clientes for Select2
    public async Task<IActionResult> OnGetClientesAsync(string empresaId)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/Clientes/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var clientes = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(clientes ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to get sucursales by empresa
    public async Task<IActionResult> OnGetSucursalesAsync(string empresaId)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/Sucursales/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var sucursales = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(sucursales ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to get terminales by empresa
    public async Task<IActionResult> OnGetTerminalesAsync(string empresaId)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/Terminales/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var terminales = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(terminales ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to create a new document
    public async Task<IActionResult> OnPostCreateAsync([FromBody] JsonElement documentoData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = documentoData.GetRawText();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/Documentos", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
            return new JsonResult(new
            {
                success = true,
                message = "Documento creado en estado Borrador",
                data = result
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to delete a document
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.DeleteAsync($"/api/Documentos/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Documento eliminado exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to download PDF
    public async Task<IActionResult> OnGetPdfAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/Documentos/{id}/pdf");

        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/pdf";
            var fileName = $"documento_{id}.pdf";

            // Try to get filename from Content-Disposition header
            if (response.Content.Headers.ContentDisposition?.FileName != null)
            {
                fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
            }

            return File(fileBytes, contentType, fileName);
        }

        // If PDF generation is not yet implemented, return a message
        return new JsonResult(new { success = false, message = "La generación de PDF se implementará en un próximo sprint." });
    }

    // Handler to download XML
    public async Task<IActionResult> OnGetXmlAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/Documentos/{id}/xml");

        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/xml";
            var fileName = $"documento_{id}.xml";

            // Try to get filename from Content-Disposition header
            if (response.Content.Headers.ContentDisposition?.FileName != null)
            {
                fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
            }

            return File(fileBytes, contentType, fileName);
        }

        return new JsonResult(new { success = false, message = "XML no disponible." });
    }

    // Handler to send document to Hacienda
    public async Task<IActionResult> OnPostSendToHaciendaAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.PostAsync($"/api/Documentos/{id}/procesar", null);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Documento enviado a Hacienda exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to get Hacienda response XML
    public async Task<IActionResult> OnGetRespuestaHaciendaAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/Documentos/{id}/respuesta-hacienda");

        if (response.IsSuccessStatusCode)
        {
            var xml = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = true, data = xml });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to download ZIP file (XML firmado, respuesta Hacienda, PDF)
    public async Task<IActionResult> OnGetDownloadZipAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/Documentos/{id}/download-zip");

        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/zip";
            var fileName = $"documento_{id}.zip";

            // Try to get filename from Content-Disposition header
            if (response.Content.Headers.ContentDisposition?.FileName != null)
            {
                fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
            }

            return File(fileBytes, contentType, fileName);
        }

        return new JsonResult(new { success = false, message = "No se pudo generar el archivo ZIP." });
    }

    // Handler to resend document to Hacienda
    public async Task<IActionResult> OnPostReenviarHaciendaAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.PostAsync($"/api/Documentos/{id}/reenviar", null);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Documento reenviado a Hacienda exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to send document by email
    public async Task<IActionResult> OnPostEnviarCorreoAsync([FromBody] JsonElement emailData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = emailData.GetRawText();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/Documentos/enviar-correo", content);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Documento enviado por correo exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to get client emails
    public async Task<IActionResult> OnGetClienteEmailsAsync(string clienteId)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/Clientes/{clienteId}");

        if (response.IsSuccessStatusCode)
        {
            var clienteData = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
            return new JsonResult(new { success = true, data = clienteData });
        }

        return new JsonResult(new { success = false, message = "No se pudo obtener la información del cliente" });
    }

    // Handler to retry sending a document that had a technical error
    public async Task<IActionResult> OnPostReintentarEnvioAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Call the API endpoint to retry the document
        var response = await client.PostAsync($"/api/Documentos/{id}/reintentar", null);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Documento marcado para reintento. Se procesará en breve." });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }
}
