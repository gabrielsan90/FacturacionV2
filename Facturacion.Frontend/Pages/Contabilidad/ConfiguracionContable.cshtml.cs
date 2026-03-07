using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class ConfiguracionContableModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ConfiguracionContableModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public ConfiguracionContable Configuracion { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public ConfiguracionContableModel(IHttpClientFactory httpClientFactory, ILogger<ConfiguracionContableModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public void OnGet()
    {
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    // Handler to get current configuration
    public async Task<IActionResult> OnGetConfiguracionAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró la empresa del usuario" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/configuracioncontable/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var config = await response.Content.ReadFromJsonAsync<ConfiguracionContable>(_jsonOptions);
                return new JsonResult(new { success = true, data = config });
            }

            // Si no existe configuración, devolver valores por defecto
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var defaultConfig = new ConfiguracionContable
                {
                    EmpresaId = Guid.Parse(empresaId),
                    MonedaBase = "CRC",
                    MesInicioPeriodoFiscal = 1,
                    DiaInicioPeriodoFiscal = 1,
                    NumeroPeriodosPorAnio = 12,
                    DecimalesMoneda = 2,
                    DecimalesTipoCambio = 4,
                    TipoNumeracion = "PER",
                    LongitudNumero = 6,
                    PeriodosAbiertosSimultaneos = 2,
                    BloquearPeriodosCerrados = true,
                    GenerarAsientosAutomaticos = true,
                    FrecuenciaGeneracion = "INM",
                    RequiereAprobacionCierre = true,
                    ValidarBalanceAntesCierre = true,
                    GenerarAsientoCierreAutomatico = true,
                    ToleranciaDiferencias = 0.01m,
                    RegistrarDiferenciasCambiarias = true,
                    RequiereAprobacionAsientos = true,
                    MontoLimiteSinAprobacion = 100000,
                    NivelesAprobacion = 1
                };
                return new JsonResult(new { success = true, data = defaultConfig, isNew = true });
            }

            _logger.LogWarning("Failed to load configuracion contable. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, message = "Error al cargar la configuración" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuracion contable");
            return new JsonResult(new { success = false, message = "Error al cargar la configuración" });
        }
    }

    // Handler to get available currencies
    public async Task<IActionResult> OnGetMonedasAsync()
    {
        try
        {
            // Monedas comúnmente usadas en Costa Rica
            var monedas = new List<object>
            {
                new { codigo = "CRC", nombre = "Colones Costarricenses (₡)" },
                new { codigo = "USD", nombre = "Dólares Americanos ($)" }
            };

            return new JsonResult(monedas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading monedas");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to get cuentas contables for dropdown (principal accounts mapping)
    public async Task<IActionResult> OnGetCuentasContablesAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new List<CuentaContable>());
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/cuentascontables/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var cuentas = await response.Content.ReadFromJsonAsync<List<CuentaContable>>(_jsonOptions);
                // Solo devolver cuentas que aceptan movimientos (cuentas de detalle)
                var cuentasDetalle = cuentas?.Where(c => c.AceptaMovimientos && c.Activo).ToList() ?? new List<CuentaContable>();
                return new JsonResult(cuentasDetalle);
            }

            _logger.LogWarning("Failed to load cuentas contables. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<CuentaContable>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cuentas contables");
            return new JsonResult(new List<CuentaContable>());
        }
    }

    // Handler to get recent periods
    public async Task<IActionResult> OnGetPeriodosRecientesAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new List<PeriodoContable>());
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/periodoscontables/empresa/{empresaId}?top=5");

            if (response.IsSuccessStatusCode)
            {
                var periodos = await response.Content.ReadFromJsonAsync<List<PeriodoContable>>(_jsonOptions);
                return new JsonResult(periodos ?? new List<PeriodoContable>());
            }

            _logger.LogWarning("Failed to load periodos contables. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<PeriodoContable>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading periodos contables");
            return new JsonResult(new List<PeriodoContable>());
        }
    }

    // Handler to save configuration
    public async Task<IActionResult> OnPostGuardarAsync([FromBody] ConfiguracionContable configData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(configData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = configData.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/configuracioncontable", content);
            }
            else
            {
                response = await client.PutAsync($"/api/configuracioncontable/{configData.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Configuración creada exitosamente" : "Configuración actualizada exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save configuracion contable. Status code: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuracion contable");
            return new JsonResult(new { success = false, message = "Error al guardar la configuración" });
        }
    }

    // Handler to initialize default configuration
    public async Task<IActionResult> OnPostInicializarAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró la empresa del usuario" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/configuracioncontable/inicializar/{empresaId}", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = "Configuración inicializada exitosamente con valores por defecto"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to initialize configuracion contable. Status code: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing configuracion contable");
            return new JsonResult(new { success = false, message = "Error al inicializar la configuración" });
        }
    }
}
