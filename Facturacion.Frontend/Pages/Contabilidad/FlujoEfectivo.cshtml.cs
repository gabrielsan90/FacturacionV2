using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize]
public class FlujoEfectivoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FlujoEfectivoModel> _logger;

    public FlujoEfectivoModel(IHttpClientFactory httpClientFactory, ILogger<FlujoEfectivoModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnGetGenerarAsync(DateTime fechaDesde, DateTime fechaHasta)
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"/api/reportescontables/flujo-efectivo?empresaId={empresaId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return BadRequest("Error al generar el reporte.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar flujo de efectivo");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private Guid? GetEmpresaId()
    {
        var claim = User.FindFirstValue("EmpresaId");
        return string.IsNullOrEmpty(claim) ? null : Guid.Parse(claim);
    }
}
