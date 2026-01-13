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
        _logger.LogWarning("Correo: configuracion no disponible en backend");
        return new JsonResult(new { unavailable = true, message = "No disponible en backend" });
    }

    public async Task<IActionResult> OnPostGuardarConfiguracionAsync([FromBody] dynamic config)
    {
        _logger.LogWarning("Correo: guardar configuracion no disponible en backend");
        return new JsonResult(new { success = false, message = "No disponible en backend" });
    }

    public async Task<IActionResult> OnPostProbarConexionAsync()
    {
        _logger.LogWarning("Correo: probar conexion no disponible en backend");
        return new JsonResult(new { success = false, message = "No disponible en backend" });
    }

    public async Task<IActionResult> OnPostEnviarPruebaAsync([FromBody] dynamic testData)
    {
        _logger.LogWarning("Correo: enviar prueba no disponible en backend");
        return new JsonResult(new { success = false, message = "No disponible en backend" });
    }
}

