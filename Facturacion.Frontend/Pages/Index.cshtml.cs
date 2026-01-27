using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

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

    /// <summary>
    /// Handler for dashboard summary data (metrics)
    /// </summary>
    public async Task<IActionResult> OnGetResumenAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT from claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/dashboard/resumen");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<object>();
                return new JsonResult(data);
            }

            _logger.LogWarning("Dashboard resumen API returned status code: {StatusCode}", response.StatusCode);
            return new JsonResult(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard resumen");
            return new JsonResult(null);
        }
    }

    /// <summary>
    /// Handler for ventas por dia chart data (last 7 days)
    /// </summary>
    public async Task<IActionResult> OnGetVentasPorDiaAsync(string fechaInicio, string fechaFin)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/dashboard/ventas-dia?fechaInicio={fechaInicio}&fechaFin={fechaFin}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<object>();
                return new JsonResult(data);
            }

            _logger.LogWarning("Dashboard ventas-dia API returned status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading ventas por dia");
            return new JsonResult(new List<object>());
        }
    }

    /// <summary>
    /// Handler for documentos por tipo chart data
    /// </summary>
    public async Task<IActionResult> OnGetDocumentosPorTipoAsync(string fechaInicio, string fechaFin)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/dashboard/ventas-tipo?fechaInicio={fechaInicio}&fechaFin={fechaFin}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<object>();
                return new JsonResult(data);
            }

            _logger.LogWarning("Dashboard ventas-tipo API returned status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading documentos por tipo");
            return new JsonResult(new List<object>());
        }
    }

    /// <summary>
    /// Handler for recent documents data (DataTable)
    /// </summary>
    public async Task<IActionResult> OnGetDocumentosRecientesAsync(int pageSize = 10, int pageNumber = 1)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT from claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Build query string with pagination parameters
            var queryString = $"?pageSize={pageSize}&pageNumber={pageNumber}";
            var response = await client.GetAsync($"/api/Documentos{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<object>();
                return new JsonResult(new { data });
            }

            _logger.LogWarning("Documentos API returned status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recent documents");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    /// <summary>
    /// Handler for PDF download
    /// </summary>
    public async Task<IActionResult> OnGetDocumentoPdfAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT from claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

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
