using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Seguridad;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class PrivilegiosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PrivilegiosModel> _logger;

    public PrivilegiosModel(IHttpClientFactory httpClientFactory, ILogger<PrivilegiosModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void OnGet()
    {
        // Page initialization - read-only view
    }

    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    // Handler for DataTable - Load all privilegios (flattened from module-grouped API)
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("/api/privilegios");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                // API returns modules with nested privilegios — flatten into individual privilege rows
                var sb = new StringBuilder("[");
                bool first = true;
                foreach (var modulo in doc.RootElement.EnumerateArray())
                {
                    if (modulo.TryGetProperty("privilegios", out var privilegios))
                    {
                        foreach (var priv in privilegios.EnumerateArray())
                        {
                            if (!first) sb.Append(',');
                            sb.Append(priv.GetRawText());
                            first = false;
                        }
                    }
                }
                sb.Append(']');

                return new ContentResult
                {
                    Content = $"{{\"data\":{sb}}}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogWarning("Failed to load privilegios. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading privilegios");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    // Handler to get modulos for filter dropdown (extracted from privilegios API)
    public async Task<IActionResult> OnGetModulosAsync()
    {
        try
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("/api/privilegios");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                // Extract unique modules from the grouped response
                var sb = new StringBuilder("[");
                bool first = true;
                foreach (var modulo in doc.RootElement.EnumerateArray())
                {
                    if (!first) sb.Append(',');
                    var id = modulo.GetProperty("moduloId").GetRawText();
                    var nombre = modulo.GetProperty("nombreModulo").GetString();
                    var icono = modulo.TryGetProperty("icono", out var iconoEl) ? iconoEl.GetString() : null;
                    sb.Append($"{{\"id\":{id},\"nombre\":{JsonSerializer.Serialize(nombre)},\"icono\":{JsonSerializer.Serialize(icono)}}}");
                    first = false;
                }
                sb.Append(']');

                return new ContentResult { Content = sb.ToString(), ContentType = "application/json", StatusCode = 200 };
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading modulos");
            return new JsonResult(new List<object>());
        }
    }
}
