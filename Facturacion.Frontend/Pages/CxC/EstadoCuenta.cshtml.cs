using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.CxC;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado,Contador")]
public class EstadoCuentaModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EstadoCuentaModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty(SupportsGet = true)]
    public string ClienteId { get; set; } = "";

    public EstadoCuentaModel(IHttpClientFactory httpClientFactory, ILogger<EstadoCuentaModel> logger)
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
        // ClienteId is automatically bound from query string
    }

    // Handler to get client information
    public async Task<IActionResult> OnGetClienteInfoAsync(string clienteId)
    {
        if (string.IsNullOrEmpty(clienteId))
        {
            return new JsonResult(new { error = "Cliente no especificado" });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/clientes/{clienteId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cliente = JsonSerializer.Deserialize<ClienteDto>(content, _jsonOptions);

                return new JsonResult(cliente);
            }

            _logger.LogWarning("Cliente not found. ID: {Id}", clienteId);
            return new JsonResult(new { error = "Cliente no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cliente info. ID: {Id}", clienteId);
            return new JsonResult(new { error = "Error al cargar información del cliente" });
        }
    }

    // Handler for DataTable - Load estado de cuenta
    public async Task<IActionResult> OnGetDataAsync(string clienteId, string? fechaDesde, string? fechaHasta)
    {
        if (string.IsNullOrEmpty(clienteId))
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

            // Build URL with optional date filters
            var url = $"/api/cuentasporcobrar/cliente/{clienteId}/estado";
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(fechaDesde))
            {
                queryParams.Add($"fechaDesde={Uri.EscapeDataString(fechaDesde)}");
            }

            if (!string.IsNullOrEmpty(fechaHasta))
            {
                queryParams.Add($"fechaHasta={Uri.EscapeDataString(fechaHasta)}");
            }

            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var movimientos = JsonSerializer.Deserialize<List<MovimientoCuentaDto>>(content, _jsonOptions);

                return new JsonResult(new { data = movimientos ?? new List<MovimientoCuentaDto>() });
            }

            _logger.LogWarning("Failed to load estado de cuenta. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading estado de cuenta for cliente: {ClienteId}", clienteId);
            return new JsonResult(new { data = new List<object>() });
        }
    }

    // Handler to get summary statistics for client
    public async Task<IActionResult> OnGetSummaryAsync(string clienteId)
    {
        if (string.IsNullOrEmpty(clienteId))
        {
            return new JsonResult(new
            {
                totalFacturado = 0,
                totalPagado = 0,
                saldoPendiente = 0,
                facturasVencidas = 0
            });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/cuentasporcobrar/cliente/{clienteId}/estado");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var movimientos = JsonSerializer.Deserialize<List<MovimientoCuentaDto>>(content, _jsonOptions);

                var totalFacturado = movimientos?
                    .Where(m => m.Tipo == "Factura" || m.Tipo == "Nota de Débito")
                    .Sum(m => m.Debito) ?? 0;

                var totalPagado = movimientos?
                    .Where(m => m.Tipo == "Pago" || m.Tipo == "Nota de Crédito")
                    .Sum(m => m.Credito) ?? 0;

                var saldoPendiente = totalFacturado - totalPagado;

                // Count overdue invoices
                var empresaId = User.FindFirstValue("EmpresaId");
                var vencidasResponse = await client.GetAsync($"/api/cuentasporcobrar/empresa/{empresaId}/vencidas");
                var facturasVencidas = 0;

                if (vencidasResponse.IsSuccessStatusCode)
                {
                    var vencidasContent = await vencidasResponse.Content.ReadAsStringAsync();
                    var vencidas = JsonSerializer.Deserialize<List<CuentasPorCobrarModel.CuentaPorCobrarDto>>(vencidasContent, _jsonOptions);
                    facturasVencidas = vencidas?.Count(c => c.ClienteId == clienteId) ?? 0;
                }

                return new JsonResult(new
                {
                    totalFacturado,
                    totalPagado,
                    saldoPendiente,
                    facturasVencidas
                });
            }

            return new JsonResult(new
            {
                totalFacturado = 0,
                totalPagado = 0,
                saldoPendiente = 0,
                facturasVencidas = 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading summary for cliente: {ClienteId}", clienteId);
            return new JsonResult(new
            {
                totalFacturado = 0,
                totalPagado = 0,
                saldoPendiente = 0,
                facturasVencidas = 0
            });
        }
    }

    // DTOs
    public sealed class ClienteDto
    {
        public string Id { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? NumeroIdentificacion { get; set; }
        public string? EmailPrincipal { get; set; }
        public string? TelefonoPrincipal { get; set; }
        public decimal SaldoActual { get; set; }
        public decimal LimiteCredito { get; set; }
    }

    public sealed class MovimientoCuentaDto
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = null!;
        public string NumeroDocumento { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal Debito { get; set; }
        public decimal Credito { get; set; }
        public decimal SaldoAcumulado { get; set; }
    }
}
