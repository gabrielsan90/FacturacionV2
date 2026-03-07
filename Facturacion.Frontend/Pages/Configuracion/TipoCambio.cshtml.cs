using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Configuracion;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class TipoCambioModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TipoCambioModel> _logger;

    public TipoCambioModel(IHttpClientFactory httpClientFactory, ILogger<TipoCambioModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    /// <summary>
    /// Obtiene el tipo de cambio actual usando la API de Hacienda (misma fuente que CrearDocumento)
    /// </summary>
    public async Task<IActionResult> OnGetCurrentRateAsync()
    {
        try
        {
            var result = await ObtenerTipoCambioHaciendaAsync();
            if (result != null)
            {
                return new JsonResult(new
                {
                    success = true,
                    data = new
                    {
                        fecha = result.Value.Fecha,
                        compra = result.Value.Compra,
                        venta = result.Value.Venta
                    }
                });
            }

            return new JsonResult(new
            {
                success = false,
                message = "No se pudo obtener el tipo de cambio del BCCR. Intente nuevamente."
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
                return new JsonResult(new { success = false, message = "La fecha es requerida" });

            // La API de Hacienda solo devuelve el tipo de cambio del día actual
            // Para fechas específicas se usa la misma fuente
            var result = await ObtenerTipoCambioHaciendaAsync();
            if (result != null)
            {
                return new JsonResult(new
                {
                    success = true,
                    data = new
                    {
                        fecha = fecha,
                        compra = result.Value.Compra,
                        venta = result.Value.Venta
                    }
                });
            }

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
    public async Task<IActionResult> OnGetRateByCurrencyAsync(string moneda, string? fecha)
    {
        try
        {
            if (string.IsNullOrEmpty(moneda))
                return new JsonResult(new { success = false, message = "El código de moneda es requerido" });

            if (moneda.Equals("CRC", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new
                {
                    success = true,
                    data = new { fecha = DateTime.Now.ToString("yyyy-MM-dd"), compra = 1.00m, venta = 1.00m }
                });
            }

            var result = await ObtenerTipoCambioHaciendaAsync();
            if (result != null)
            {
                return new JsonResult(new
                {
                    success = true,
                    data = new
                    {
                        fecha = result.Value.Fecha,
                        compra = result.Value.Compra,
                        venta = result.Value.Venta
                    }
                });
            }

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
    /// Obtiene tipos de cambio en un rango de fechas (solo devuelve el actual, la API no soporta históricos)
    /// </summary>
    public async Task<IActionResult> OnPostRangeAsync([FromBody] RangeRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.FechaInicio) || string.IsNullOrEmpty(request.FechaFin))
                return new JsonResult(new { success = false, message = "Las fechas de inicio y fin son requeridas" });

            var result = await ObtenerTipoCambioHaciendaAsync();
            if (result != null)
            {
                // La API de Hacienda solo retorna el tipo de cambio vigente
                var data = new[]
                {
                    new
                    {
                        fecha = result.Value.Fecha,
                        moneda = "USD",
                        compra = result.Value.Compra,
                        venta = result.Value.Venta
                    }
                };
                return new JsonResult(new { success = true, data });
            }

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

    /// <summary>
    /// Obtiene tipo de cambio USD desde la API oficial de Hacienda (misma lógica que CrearDocumento)
    /// con fallback a API alternativa
    /// </summary>
    private async Task<TipoCambioResult?> ObtenerTipoCambioHaciendaAsync()
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(15);

        decimal compra = 0, venta = 0;
        string? fecha = null;

        // Intento 1: API oficial de Hacienda Costa Rica
        try
        {
            _logger.LogInformation("Fetching exchange rate from Hacienda API");
            var response = await client.GetAsync("https://api.hacienda.go.cr/indicadores/tc/dolar");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("venta", out var ventaObj))
                {
                    if (ventaObj.TryGetProperty("valor", out var ventaVal))
                        venta = ventaVal.GetDecimal();
                    if (ventaObj.TryGetProperty("fecha", out var fechaVal))
                        fecha = fechaVal.GetString();
                }
                if (doc.RootElement.TryGetProperty("compra", out var compraObj))
                {
                    if (compraObj.TryGetProperty("valor", out var compraVal))
                        compra = compraVal.GetDecimal();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calling Hacienda API for exchange rate");
        }

        // Intento 2: API alternativa si Hacienda falla
        if (venta == 0)
        {
            try
            {
                var responseVenta = await client.GetAsync("https://tipodecambio.paginasweb.cr/api/venta");
                if (responseVenta.IsSuccessStatusCode)
                {
                    var ventaStr = await responseVenta.Content.ReadAsStringAsync();
                    if (decimal.TryParse(ventaStr.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                        venta = v;
                }

                var responseCompra = await client.GetAsync("https://tipodecambio.paginasweb.cr/api/compra");
                if (responseCompra.IsSuccessStatusCode)
                {
                    var compraStr = await responseCompra.Content.ReadAsStringAsync();
                    if (decimal.TryParse(compraStr.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var c))
                        compra = c;
                }

                fecha = DateTime.Now.ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calling alternative API for exchange rate");
            }
        }

        if (venta > 0)
        {
            return new TipoCambioResult
            {
                Fecha = fecha ?? DateTime.Now.ToString("yyyy-MM-dd"),
                Compra = compra,
                Venta = venta
            };
        }

        return null;
    }

    private struct TipoCambioResult
    {
        public string Fecha;
        public decimal Compra;
        public decimal Venta;
    }

    public class RangeRequest
    {
        public string FechaInicio { get; set; } = string.Empty;
        public string FechaFin { get; set; } = string.Empty;
    }
}
