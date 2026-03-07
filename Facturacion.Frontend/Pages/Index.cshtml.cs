using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public void OnGet()
    {
        // Page initialization - no JWT token needed in view anymore
    }

    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    /// <summary>
    /// Extracts the data payload from an API response that may be a direct object/array
    /// or wrapped in ActionResponse {wasSuccess, result}.
    /// For object responses (resumen), returns the raw JSON of the object itself.
    /// </summary>
    private static string ExtractObjectJson(string content)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            // If it has a "result" property (ActionResponse wrapper), extract it
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("result", out var resultProp))
            {
                return resultProp.GetRawText();
            }
            // Otherwise return as-is (direct object)
            return content;
        }
        catch
        {
            return "{}";
        }
    }

    /// <summary>
    /// Extracts an array from an API response that may be a direct array
    /// or wrapped in ActionResponse {wasSuccess, result: [...]}.
    /// </summary>
    private static string ExtractArrayJson(string content)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                return content;
            if (doc.RootElement.TryGetProperty("result", out var resultProp) &&
                resultProp.ValueKind == JsonValueKind.Array)
                return resultProp.GetRawText();
            return "[]";
        }
        catch
        {
            return "[]";
        }
    }

    /// <summary>
    /// Handler for dashboard summary data (metrics)
    /// </summary>
    public async Task<IActionResult> OnGetResumenAsync()
    {
        try
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("/api/dashboard/resumen");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var dataJson = ExtractObjectJson(content);
                return new ContentResult
                {
                    Content = dataJson,
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogWarning("Dashboard resumen API returned status code: {StatusCode}", response.StatusCode);
            return new ContentResult
            {
                Content = "{\"totalVentasHoy\":0,\"documentosPendientesEnvio\":0,\"productosBajoStock\":0,\"pagosPendientes\":0}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard resumen");
            return new ContentResult
            {
                Content = "{\"totalVentasHoy\":0,\"documentosPendientesEnvio\":0,\"productosBajoStock\":0,\"pagosPendientes\":0}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
    }

    /// <summary>
    /// Handler for ventas por dia chart data (last 7 days)
    /// </summary>
    public async Task<IActionResult> OnGetVentasPorDiaAsync(string fechaInicio, string fechaFin)
    {
        try
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"/api/dashboard/ventas-dia?fechaInicio={fechaInicio}&fechaFin={fechaFin}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var dataJson = ExtractArrayJson(content);
                return new ContentResult
                {
                    Content = dataJson,
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogWarning("Dashboard ventas-dia API returned status code: {StatusCode}", response.StatusCode);
            return new ContentResult { Content = "[]", ContentType = "application/json", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading ventas por dia");
            return new ContentResult { Content = "[]", ContentType = "application/json", StatusCode = 200 };
        }
    }

    /// <summary>
    /// Handler for documentos por tipo chart data
    /// </summary>
    public async Task<IActionResult> OnGetDocumentosPorTipoAsync(string fechaInicio, string fechaFin)
    {
        try
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"/api/dashboard/ventas-tipo?fechaInicio={fechaInicio}&fechaFin={fechaFin}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var dataJson = ExtractArrayJson(content);
                return new ContentResult
                {
                    Content = dataJson,
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogWarning("Dashboard ventas-tipo API returned status code: {StatusCode}", response.StatusCode);
            return new ContentResult { Content = "[]", ContentType = "application/json", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading documentos por tipo");
            return new ContentResult { Content = "[]", ContentType = "application/json", StatusCode = 200 };
        }
    }

    /// <summary>
    /// Handler for recent documents data (DataTable)
    /// </summary>
    public async Task<IActionResult> OnGetDocumentosRecientesAsync(int pageSize = 10, int pageNumber = 1)
    {
        try
        {
            var client = CreateApiClient();

            // Build query string with pagination parameters
            var queryString = $"?pageSize={pageSize}&pageNumber={pageNumber}";
            var response = await client.GetAsync($"/api/Documentos{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var dataJson = ExtractArrayJson(content);
                return new ContentResult
                {
                    Content = $"{{\"data\":{dataJson}}}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogWarning("Documentos API returned status code: {StatusCode}", response.StatusCode);
            return new ContentResult
            {
                Content = "{\"data\":[]}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recent documents");
            return new ContentResult
            {
                Content = "{\"data\":[]}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
    }

    /// <summary>
    /// Handler for PDF download
    /// </summary>
    public async Task<IActionResult> OnGetDocumentoPdfAsync(string id)
    {
        try
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"/api/Documentos/{id}/descargar-pdf");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";
                var fileName = $"documento-{id}.pdf";

                // Try to get filename from Content-Disposition header if available
                if (response.Content.Headers.ContentDisposition?.FileName != null)
                {
                    fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
                }

                return File(pdfBytes, contentType, fileName);
            }

            _logger.LogWarning("PDF download API returned status code: {StatusCode}", response.StatusCode);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading PDF for document {DocumentId}", id);
            return StatusCode(500, "Error al descargar el PDF");
        }
    }
}
