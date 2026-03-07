using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Contador")]
public class MayorizacionModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MayorizacionModel> _logger;

    public string? EmpresaId { get; set; }

    public MayorizacionModel(IHttpClientFactory httpClientFactory, ILogger<MayorizacionModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void OnGet()
    {
        EmpresaId = User.FindFirstValue("EmpresaId");
    }

    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<IActionResult> OnGetPendientesAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
                return new JsonResult(new { documentos = Array.Empty<object>(), gastos = Array.Empty<object>(), totalDocumentos = 0, totalGastos = 0 });

            var client = CreateApiClient();
            var response = await client.GetAsync($"/api/mayorizacion/pendientes/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult
                {
                    Content = content,
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogWarning("Error al obtener pendientes. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { documentos = Array.Empty<object>(), gastos = Array.Empty<object>(), totalDocumentos = 0, totalGastos = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pendientes de mayorización");
            return StatusCode(500, "Error al obtener documentos pendientes.");
        }
    }

    public async Task<IActionResult> OnPostGenerarAsync([FromBody] JsonElement request)
    {
        try
        {
            var client = CreateApiClient();
            var json = request.GetRawText();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/mayorizacion/generar", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return new ContentResult
                {
                    Content = responseContent,
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Error al generar asientos. Status: {StatusCode}, Body: {Body}",
                response.StatusCode, errorContent);
            return StatusCode((int)response.StatusCode, errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar asientos de mayorización");
            return StatusCode(500, "Error al generar asientos contables.");
        }
    }
}
