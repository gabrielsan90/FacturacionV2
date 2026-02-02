using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class ConfiguracionCorreoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ConfiguracionCorreoModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public string EmpresaId { get; set; } = "";

    public ConfiguracionCorreoModel(IHttpClientFactory httpClientFactory, ILogger<ConfiguracionCorreoModel> logger)
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
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    private HttpClient GetAuthenticatedClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    private string GetEmpresaId()
    {
        return User.FindFirstValue("EmpresaId") ?? "";
    }

    public async Task<IActionResult> OnGetConfiguracionAsync()
    {
        try
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró empresa asociada" });
            }

            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"/api/Configuracion/correo/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var config = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
                return new JsonResult(config);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Error al obtener configuración: {Error}", error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración de correo");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostGuardarConfiguracionAsync([FromBody] JsonElement config)
    {
        try
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró empresa asociada" });
            }

            var client = GetAuthenticatedClient();
            var json = config.GetRawText();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Configuracion/correo/{empresaId}", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
                return new JsonResult(result);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Error al guardar configuración: {Error}", error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar configuración de correo");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostProbarConexionAsync()
    {
        try
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró empresa asociada" });
            }

            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"/api/Configuracion/correo/{empresaId}/probar", null);

            var result = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al probar conexión SMTP");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostEnviarPruebaAsync([FromBody] JsonElement testData)
    {
        try
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró empresa asociada" });
            }

            var client = GetAuthenticatedClient();
            var json = testData.GetRawText();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/Configuracion/correo/{empresaId}/enviar-prueba", content);

            var result = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo de prueba");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }
}
