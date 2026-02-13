using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Seguridad;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class PrivilegiosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PrivilegiosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PrivilegiosModel(IHttpClientFactory httpClientFactory, ILogger<PrivilegiosModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public void OnGet()
    {
        // Page initialization - read-only view
    }

    // Handler for DataTable - Load all privilegios
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/privilegios");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                // API returns ActionResponse<T> with result property
                if (doc.RootElement.TryGetProperty("result", out var resultElement))
                {
                    return new JsonResult(new { data = resultElement });
                }

                return new JsonResult(new { data = doc.RootElement });
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

    // Handler to get modulos for grouping
    public async Task<IActionResult> OnGetModulosAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/modulos");

            if (response.IsSuccessStatusCode)
            {
                var modulos = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(modulos ?? new List<object>());
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
