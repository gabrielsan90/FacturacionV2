using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class UnidadesMedidaPageModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UnidadesMedidaPageModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UnidadesMedidaPageModel(IHttpClientFactory httpClientFactory, ILogger<UnidadesMedidaPageModel> logger)
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

    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/catalogos/unidades-medida");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = data ?? new List<dynamic>() });
            }

            _logger.LogWarning("Failed to load unidades de medida. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading unidades de medida");
            return new JsonResult(new { data = new List<dynamic>() });
        }
    }
}
