using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Configuracion;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class TipoCambioModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TipoCambioModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public TipoCambioModel(IHttpClientFactory httpClientFactory, ILogger<TipoCambioModel> logger)
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
        // Page initialization
    }

    /// <summary>
    /// Obtiene el tipo de cambio actual del BCCR (hoy)
    /// </summary>
    public async Task<IActionResult> OnGetCurrentRateAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/TipoCambio");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(new { success = true, data });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to load current exchange rate. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new JsonResult(new
            {
                success = false,
                message = "No se pudo obtener el tipo de cambio actual del BCCR. Intente nuevamente."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading current exchange rate");
            return new JsonResult(new
            {
                success = false,
                message = "Error al consultar el tipo de cambio: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Obtiene el tipo de cambio para una fecha específica
    /// </summary>
    public async Task<IActionResult> OnGetRateByDateAsync(string fecha)
    {
        try
        {
            if (string.IsNullOrEmpty(fecha))
            {
                return new JsonResult(new { success = false, message = "La fecha es requerida" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/TipoCambio?fecha={fecha}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(new { success = true, data });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to load exchange rate for date {Fecha}. Status: {StatusCode}, Error: {Error}",
                fecha, response.StatusCode, errorContent);

            return new JsonResult(new
            {
                success = false,
                message = $"No se pudo obtener el tipo de cambio para la fecha {fecha}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading exchange rate for date {Fecha}", fecha);
            return new JsonResult(new
            {
                success = false,
                message = "Error al consultar el tipo de cambio: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Obtiene el tipo de cambio para una moneda específica
    /// </summary>
    public async Task<IActionResult> OnGetRateByCurrencyAsync(string moneda, string fecha)
    {
        try
        {
            if (string.IsNullOrEmpty(moneda))
            {
                return new JsonResult(new { success = false, message = "El código de moneda es requerido" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/TipoCambio/moneda/{moneda}";
            if (!string.IsNullOrEmpty(fecha))
            {
                url += $"?fecha={fecha}";
            }

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(new { success = true, data });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to load exchange rate for currency {Moneda}. Status: {StatusCode}, Error: {Error}",
                moneda, response.StatusCode, errorContent);

            return new JsonResult(new
            {
                success = false,
                message = $"No se pudo obtener el tipo de cambio para {moneda}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading exchange rate for currency {Moneda}", moneda);
            return new JsonResult(new
            {
                success = false,
                message = "Error al consultar el tipo de cambio: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Obtiene tipos de cambio en un rango de fechas
    /// </summary>
    public async Task<IActionResult> OnPostRangeAsync([FromBody] RangeRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.FechaInicio) || string.IsNullOrEmpty(request.FechaFin))
            {
                return new JsonResult(new { success = false, message = "Las fechas de inicio y fin son requeridas" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var requestData = new
            {
                fechaInicio = request.FechaInicio,
                fechaFin = request.FechaFin
            };

            var response = await client.PostAsJsonAsync("/api/TipoCambio/rango", requestData);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { success = true, data });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to load exchange rate range. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new JsonResult(new
            {
                success = false,
                message = "No se pudo obtener el rango de tipos de cambio solicitado"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading exchange rate range");
            return new JsonResult(new
            {
                success = false,
                message = "Error al consultar los tipos de cambio: " + ex.Message
            });
        }
    }

    public class RangeRequest
    {
        public string FechaInicio { get; set; } = string.Empty;
        public string FechaFin { get; set; } = string.Empty;
    }
}
