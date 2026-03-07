using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Reportes;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Contador")]
public class DeclaracionIVAModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DeclaracionIVAModel> _logger;

    public DeclaracionIVAModel(IHttpClientFactory httpClientFactory, ILogger<DeclaracionIVAModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetReporteAsync(string? fechaInicio, string? fechaFin)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { success = false, message = "EmpresaId no encontrado" });
            }

            var (inicio, fin) = ResolveFechaRange(fechaInicio, fechaFin);
            var client = CreateClient();

            var url = $"/api/Reportes/impuestos?empresaId={empresaId}&fechaInicio={FormatDate(inicio)}&fechaFin={FormatDate(fin)}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Unwrap ActionResponse if needed
                using var doc = JsonDocument.Parse(content);
                string resultJson;
                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("result", out var resultProp))
                {
                    resultJson = resultProp.GetRawText();
                }
                else
                {
                    resultJson = content;
                }

                return new ContentResult
                {
                    Content = $"{{\"success\":true,\"data\":{resultJson}}}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogWarning("Failed to load reporte impuestos. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, message = "Error al cargar datos del reporte" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reporte impuestos");
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static (DateTime Inicio, DateTime Fin) ResolveFechaRange(string? fechaInicio, string? fechaFin)
    {
        if (!DateTime.TryParse(fechaInicio, out var inicio) || !DateTime.TryParse(fechaFin, out var fin))
        {
            // Default: current month
            var now = DateTime.Today;
            inicio = new DateTime(now.Year, now.Month, 1);
            fin = inicio.AddMonths(1).AddDays(-1);
        }
        fin = fin.Date.AddDays(1).AddSeconds(-1);
        return (inicio, fin);
    }

    private static string FormatDate(DateTime fecha)
    {
        return fecha.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    }

    private bool TryGetEmpresaId(out Guid empresaId)
    {
        empresaId = Guid.Empty;
        var val = User.FindFirst("EmpresaId")?.Value;
        return Guid.TryParse(val, out empresaId);
    }
}
