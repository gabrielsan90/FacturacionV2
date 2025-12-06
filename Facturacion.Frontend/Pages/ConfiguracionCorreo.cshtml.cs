using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class ConfiguracionCorreoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ConfiguracionCorreoModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConfiguracionCorreoModel(IHttpClientFactory httpClientFactory, ILogger<ConfiguracionCorreoModel> logger)
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

    public async Task<IActionResult> OnGetConfiguracionAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/correo/configuracion");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data ?? new { });
            }

            return new JsonResult(new { });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading email config");
            return new JsonResult(new { });
        }
    }

    public async Task<IActionResult> OnPostGuardarConfiguracionAsync([FromBody] dynamic config)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(config);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/correo/configuracion", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving email config");
            return new JsonResult(new { success = false, message = "Error al guardar la configuración" });
        }
    }

    public async Task<IActionResult> OnPostProbarConexionAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            client.Timeout = TimeSpan.FromSeconds(30);

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync("/api/correo/probar-conexion", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection");
            return new JsonResult(new { success = false, message = "Error al probar la conexión: " + ex.Message });
        }
    }

    public async Task<IActionResult> OnPostEnviarPruebaAsync([FromBody] dynamic testData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            client.Timeout = TimeSpan.FromSeconds(30);

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(testData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/correo/enviar-prueba", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email");
            return new JsonResult(new { success = false, message = "Error al enviar el correo: " + ex.Message });
        }
    }
}
