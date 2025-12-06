using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ReporteDocumentosEmitidosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReporteDocumentosEmitidosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteDocumentosEmitidosModel(IHttpClientFactory httpClientFactory, ILogger<ReporteDocumentosEmitidosModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public void OnGet()
    {
        // Page initialization
    }

    public async Task<IActionResult> OnGetDataAsync(string? fechaInicio, string? fechaFin, string? tipoDocumento)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/documentos-emitidos?fechaInicio={fechaInicio}&fechaFin={fechaFin}&tipoDocumento={tipoDocumento}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = data ?? new List<dynamic>() });
            }

            _logger.LogWarning("Failed to load reporte documentos emitidos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reporte documentos emitidos");
            return new JsonResult(new { data = new List<dynamic>() });
        }
    }

    public async Task<IActionResult> OnGetResumenAsync(string? fechaInicio, string? fechaFin, string? tipoDocumento)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/documentos-emitidos/resumen?fechaInicio={fechaInicio}&fechaFin={fechaFin}&tipoDocumento={tipoDocumento}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data ?? new { totalDocumentos = 0, totalAceptados = 0, totalPendientes = 0, totalMonto = 0 });
            }

            _logger.LogWarning("Failed to load resumen. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { totalDocumentos = 0, totalAceptados = 0, totalPendientes = 0, totalMonto = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resumen");
            return new JsonResult(new { totalDocumentos = 0, totalAceptados = 0, totalPendientes = 0, totalMonto = 0 });
        }
    }
}
