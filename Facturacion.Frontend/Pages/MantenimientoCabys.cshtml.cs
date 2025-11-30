using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class MantenimientoCabysModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MantenimientoCabysModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MantenimientoCabysModel(IHttpClientFactory httpClientFactory, ILogger<MantenimientoCabysModel> logger)
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

            var response = await client.GetAsync("/api/cabys");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = data ?? new List<dynamic>() });
            }

            return new JsonResult(new { data = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading CABYS");
            return new JsonResult(new { data = new List<dynamic>() });
        }
    }

    public async Task<IActionResult> OnGetBuscarAsync(string termino)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/cabys/buscar?termino={termino}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(data ?? new List<dynamic>());
            }

            return new JsonResult(new List<dynamic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando CABYS");
            return new JsonResult(new List<dynamic>());
        }
    }

    public async Task<IActionResult> OnGetDetalleAsync(string codigo)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/cabys/{codigo}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data);
            }

            return new JsonResult(new { error = "No encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading CABYS detalle");
            return new JsonResult(new { error = "Error al cargar el detalle" });
        }
    }

    public async Task<IActionResult> OnPostImportarAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            client.Timeout = TimeSpan.FromMinutes(5);

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync("/api/cabys/importar", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(new { success = true, message = $"Catálogo importado exitosamente. {result?.registrosImportados ?? 0} registros" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importando CABYS");
            return new JsonResult(new { success = false, message = "Error al importar el catálogo" });
        }
    }
}
