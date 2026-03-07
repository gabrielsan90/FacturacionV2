using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.CxC;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado,Contador")]
public class CuentasPorCobrarModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CuentasPorCobrarModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public string EmpresaId { get; set; } = "";

    public CuentasPorCobrarModel(IHttpClientFactory httpClientFactory, ILogger<CuentasPorCobrarModel> logger)
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

    // Handler for DataTable - Load all cuentas por cobrar
    public async Task<IActionResult> OnGetDataAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { data = new List<object>() });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/cuentasporcobrar/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cuentas = JsonSerializer.Deserialize<List<CuentaPorCobrarDto>>(content, _jsonOptions);

                return new JsonResult(new { data = cuentas ?? new List<CuentaPorCobrarDto>() });
            }

            _logger.LogWarning("Failed to load cuentas por cobrar. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cuentas por cobrar");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    // Handler to get summary statistics
    public async Task<IActionResult> OnGetSummaryAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { totalPendiente = 0, totalVencido = 0, totalCobradoMes = 0 });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Get pendientes
            var pendientesResponse = await client.GetAsync($"/api/cuentasporcobrar/empresa/{empresaId}/pendientes");
            var pendientes = new List<CuentaPorCobrarDto>();
            if (pendientesResponse.IsSuccessStatusCode)
            {
                var pendientesContent = await pendientesResponse.Content.ReadAsStringAsync();
                pendientes = JsonSerializer.Deserialize<List<CuentaPorCobrarDto>>(pendientesContent, _jsonOptions) ?? new();
            }

            // Get vencidas
            var vencidasResponse = await client.GetAsync($"/api/cuentasporcobrar/empresa/{empresaId}/vencidas");
            var vencidas = new List<CuentaPorCobrarDto>();
            if (vencidasResponse.IsSuccessStatusCode)
            {
                var vencidasContent = await vencidasResponse.Content.ReadAsStringAsync();
                vencidas = JsonSerializer.Deserialize<List<CuentaPorCobrarDto>>(vencidasContent, _jsonOptions) ?? new();
            }

            // Calculate total collected this month (from all receivables)
            var allResponse = await client.GetAsync($"/api/cuentasporcobrar/empresa/{empresaId}");
            var all = new List<CuentaPorCobrarDto>();
            if (allResponse.IsSuccessStatusCode)
            {
                var allContent = await allResponse.Content.ReadAsStringAsync();
                all = JsonSerializer.Deserialize<List<CuentaPorCobrarDto>>(allContent, _jsonOptions) ?? new();
            }

            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var cobradosMes = all.Where(c => c.Estado == "Pagado" && c.FechaEmision >= firstDayOfMonth).ToList();

            return new JsonResult(new
            {
                totalPendiente = pendientes.Sum(c => c.MontoSaldo),
                totalVencido = vencidas.Sum(c => c.MontoSaldo),
                totalCobradoMes = cobradosMes.Sum(c => c.MontoOriginal),
                pendientePorMoneda = pendientes.GroupBy(c => c.Moneda ?? "CRC")
                    .Select(g => new { moneda = g.Key, monto = g.Sum(c => c.MontoSaldo) })
                    .OrderByDescending(m => m.monto).ToList(),
                vencidoPorMoneda = vencidas.GroupBy(c => c.Moneda ?? "CRC")
                    .Select(g => new { moneda = g.Key, monto = g.Sum(c => c.MontoSaldo) })
                    .OrderByDescending(m => m.monto).ToList(),
                cobradoMesPorMoneda = cobradosMes.GroupBy(c => c.Moneda ?? "CRC")
                    .Select(g => new { moneda = g.Key, monto = g.Sum(c => c.MontoOriginal) })
                    .OrderByDescending(m => m.monto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading summary");
            return new JsonResult(new { totalPendiente = 0, totalVencido = 0, totalCobradoMes = 0 });
        }
    }

    // Handler to get details of a single cuenta por cobrar
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/cuentasporcobrar/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cuenta = JsonSerializer.Deserialize<CuentaPorCobrarDto>(content, _jsonOptions);

                return new JsonResult(cuenta);
            }

            _logger.LogWarning("Cuenta por cobrar not found. ID: {Id}", id);
            return new JsonResult(new { error = "Cuenta no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cuenta por cobrar details. ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar la cuenta" });
        }
    }

    // Handler to apply payment
    public async Task<IActionResult> OnPostAplicarPagoAsync([FromBody] AplicarPagoRequest pagoData)
    {
        if (pagoData == null || pagoData.Monto <= 0)
        {
            return new JsonResult(new { success = false, message = "Datos inválidos" });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(pagoData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/cuentasporcobrar/{pagoData.CuentaPorCobrarId}/abonar", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = "Pago aplicado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to apply payment. Error: {Error}", error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying payment");
            return new JsonResult(new { success = false, message = "Error al aplicar el pago" });
        }
    }

    // DTOs
    public sealed class CuentaPorCobrarDto
    {
        public string Id { get; set; } = null!;
        public string NumeroConsecutivo { get; set; } = "";
        public string ClienteId { get; set; } = null!;
        public string NombreCliente { get; set; } = "";
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal MontoSaldo { get; set; }
        public string Estado { get; set; } = null!;
        public string Moneda { get; set; } = "CRC";
    }

    public sealed class AplicarPagoRequest
    {
        public string CuentaPorCobrarId { get; set; } = null!;
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public int MetodoPago { get; set; }
        public string? NumeroReferencia { get; set; }
        public string? Notas { get; set; }
    }
}
