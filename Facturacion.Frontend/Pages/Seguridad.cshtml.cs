using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class SeguridadModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SeguridadModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SeguridadModel(IHttpClientFactory httpClientFactory, ILogger<SeguridadModel> logger)
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
        _logger.LogWarning("Seguridad: configuracion no disponible en backend");
        return new JsonResult(new
        {
            minPasswordLength = 8,
            requireUppercase = true,
            requireNumbers = true,
            requireSpecialChars = false,
            sessionTimeout = 60,
            maxLoginAttempts = 5,
            enableTwoFactor = false,
            unavailable = true,
            message = "No disponible en backend"
        });
    }

    public async Task<IActionResult> OnPostGuardarConfiguracionAsync([FromBody] dynamic config)
    {
        _logger.LogWarning("Seguridad: guardar configuracion no disponible en backend");
        return new JsonResult(new { success = false, message = "No disponible en backend" });
    }

    public async Task<IActionResult> OnGetAuditLogAsync(string? fecha)
    {
        _logger.LogWarning("Seguridad: audit log no disponible en backend");
        return new JsonResult(new List<object>());
    }

    public async Task<IActionResult> OnGetSesionesActivasAsync()
    {
        _logger.LogWarning("Seguridad: sesiones activas no disponible en backend");
        return new JsonResult(new { data = new List<object>() });
    }

    public async Task<IActionResult> OnPostCerrarSesionAsync(string sesionId)
    {
        _logger.LogWarning("Seguridad: cerrar sesion no disponible en backend");
        return new JsonResult(new { success = false, message = "No disponible en backend" });
    }

    public async Task<IActionResult> OnPostCerrarTodasSesionesAsync()
    {
        _logger.LogWarning("Seguridad: cerrar todas las sesiones no disponible en backend");
        return new JsonResult(new { success = false, message = "No disponible en backend" });
    }
}

