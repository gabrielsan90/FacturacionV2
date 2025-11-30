using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ReporteDocumentosRecibidosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReporteDocumentosRecibidosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteDocumentosRecibidosModel(IHttpClientFactory httpClientFactory, ILogger<ReporteDocumentosRecibidosModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public void OnGet()
    {
        // Page initialization
    }

    public async Task<IActionResult> OnGetDataAsync(string? fechaInicio, string? fechaFin, string? estadoRespuesta)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/documentos-recibidos?fechaInicio={fechaInicio}&fechaFin={fechaFin}&estadoRespuesta={estadoRespuesta}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = data ?? new List<dynamic>() });
            }

            _logger.LogWarning("Failed to load reporte documentos recibidos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reporte documentos recibidos");
            return new JsonResult(new { data = new List<dynamic>() });
        }
    }

    public async Task<IActionResult> OnGetResumenAsync(string? fechaInicio, string? fechaFin, string? estadoRespuesta)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/reportes/documentos-recibidos/resumen?fechaInicio={fechaInicio}&fechaFin={fechaFin}&estadoRespuesta={estadoRespuesta}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data ?? new { totalRecibidos = 0, totalRespondidos = 0, totalSinResponder = 0, totalMonto = 0 });
            }

            _logger.LogWarning("Failed to load resumen. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { totalRecibidos = 0, totalRespondidos = 0, totalSinResponder = 0, totalMonto = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resumen");
            return new JsonResult(new { totalRecibidos = 0, totalRespondidos = 0, totalSinResponder = 0, totalMonto = 0 });
        }
    }
}
