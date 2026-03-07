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

            var url = $"/api/cuentasporcobrar/cliente/{clienteId}/estado";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // API returns CuentaPorCobrar[] - transform to movimientos
                var movimientos = new List<MovimientoCuentaDto>();

                // Handle both array and ActionResponse wrapper
                JsonElement cuentasArray;
                if (root.ValueKind == JsonValueKind.Array)
                    cuentasArray = root;
                else if (root.TryGetProperty("result", out var resultProp) && resultProp.ValueKind == JsonValueKind.Array)
                    cuentasArray = resultProp;
                else
                {
                    return new JsonResult(new { data = movimientos });
                }

                foreach (var cuenta in cuentasArray.EnumerateArray())
                {
                    var fechaEmision = cuenta.TryGetProperty("fechaEmision", out var fe) ? fe.GetDateTime() : DateTime.MinValue;
                    var numeroConsecutivo = cuenta.TryGetProperty("numeroConsecutivo", out var nc) ? nc.GetString() ?? "" : "";
                    var montoOriginal = cuenta.TryGetProperty("montoOriginal", out var mo) ? mo.GetDecimal() : 0;
                    var montoSaldo = cuenta.TryGetProperty("montoSaldo", out var ms) ? ms.GetDecimal() : 0;
                    var estado = cuenta.TryGetProperty("estado", out var est) ? est.ToString() : "";

                    // Apply date filters
                    if (!string.IsNullOrEmpty(fechaDesde) && DateTime.TryParse(fechaDesde, out var desde) && fechaEmision < desde)
                        continue;
                    if (!string.IsNullOrEmpty(fechaHasta) && DateTime.TryParse(fechaHasta, out var hasta) && fechaEmision > hasta.AddDays(1))
                        continue;

                    // Add the invoice as a debit entry
                    movimientos.Add(new MovimientoCuentaDto
                    {
                        Fecha = fechaEmision,
                        Tipo = "Factura",
                        NumeroDocumento = numeroConsecutivo,
                        Descripcion = estado == "Pagada" ? "Factura (Pagada)" : estado == "Vencida" ? "Factura (Vencida)" : "Factura a crédito",
                        Debito = montoOriginal,
                        Credito = 0
                    });

                    // If there are payments (montoOriginal - montoSaldo > 0), add as credit
                    var montoPagado = montoOriginal - montoSaldo;
                    if (montoPagado > 0)
                    {
                        var fechaUltimoPago = cuenta.TryGetProperty("fechaUltimoPago", out var fup) && fup.ValueKind != JsonValueKind.Null
                            ? fup.GetDateTime()
                            : fechaEmision;

                        movimientos.Add(new MovimientoCuentaDto
                        {
                            Fecha = fechaUltimoPago,
                            Tipo = "Pago",
                            NumeroDocumento = numeroConsecutivo,
                            Descripcion = "Abono/Pago aplicado",
                            Debito = 0,
                            Credito = montoPagado
                        });
                    }
                }

                // Sort by date
                movimientos = movimientos.OrderBy(m => m.Fecha).ThenBy(m => m.Tipo == "Factura" ? 0 : 1).ToList();

                return new JsonResult(new { data = movimientos });
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
        var emptyResult = new { totalFacturado = 0m, totalPagado = 0m, saldoPendiente = 0m, facturasVencidas = 0,
            facturadoPorMoneda = new List<object>(), pagadoPorMoneda = new List<object>(), pendientePorMoneda = new List<object>() };

        if (string.IsNullOrEmpty(clienteId))
            return new JsonResult(emptyResult);

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
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                JsonElement cuentasArray;
                if (root.ValueKind == JsonValueKind.Array)
                    cuentasArray = root;
                else if (root.TryGetProperty("result", out var resultProp) && resultProp.ValueKind == JsonValueKind.Array)
                    cuentasArray = resultProp;
                else
                    return new JsonResult(emptyResult);

                decimal totalFacturado = 0;
                decimal totalPagado = 0;
                int facturasVencidas = 0;

                // Per-currency accumulators
                var facturadoPorMonedaDict = new Dictionary<string, decimal>();
                var pagadoPorMonedaDict = new Dictionary<string, decimal>();
                var pendientePorMonedaDict = new Dictionary<string, decimal>();

                foreach (var cuenta in cuentasArray.EnumerateArray())
                {
                    var montoOriginal = cuenta.TryGetProperty("montoOriginal", out var mo) ? mo.GetDecimal() : 0;
                    var montoSaldo = cuenta.TryGetProperty("montoSaldo", out var ms) ? ms.GetDecimal() : 0;
                    var estado = cuenta.TryGetProperty("estado", out var est) ? est.ToString() : "";
                    var moneda = cuenta.TryGetProperty("moneda", out var mon) ? mon.ToString() : "CRC";

                    totalFacturado += montoOriginal;
                    var pagado = montoOriginal - montoSaldo;
                    totalPagado += pagado;

                    // Accumulate per currency
                    if (!facturadoPorMonedaDict.ContainsKey(moneda)) facturadoPorMonedaDict[moneda] = 0;
                    if (!pagadoPorMonedaDict.ContainsKey(moneda)) pagadoPorMonedaDict[moneda] = 0;
                    if (!pendientePorMonedaDict.ContainsKey(moneda)) pendientePorMonedaDict[moneda] = 0;

                    facturadoPorMonedaDict[moneda] += montoOriginal;
                    pagadoPorMonedaDict[moneda] += pagado;
                    pendientePorMonedaDict[moneda] += montoSaldo;

                    if (estado == "Vencida" || estado == "1")
                        facturasVencidas++;
                }

                var facturadoPorMoneda = facturadoPorMonedaDict
                    .Select(kv => new { moneda = kv.Key, monto = kv.Value })
                    .OrderByDescending(m => m.monto).ToList();
                var pagadoPorMoneda = pagadoPorMonedaDict
                    .Select(kv => new { moneda = kv.Key, monto = kv.Value })
                    .OrderByDescending(m => m.monto).ToList();
                var pendientePorMoneda = pendientePorMonedaDict
                    .Where(kv => kv.Value > 0)
                    .Select(kv => new { moneda = kv.Key, monto = kv.Value })
                    .OrderByDescending(m => m.monto).ToList();

                return new JsonResult(new
                {
                    totalFacturado,
                    totalPagado,
                    saldoPendiente = totalFacturado - totalPagado,
                    facturasVencidas,
                    facturadoPorMoneda,
                    pagadoPorMoneda,
                    pendientePorMoneda
                });
            }

            return new JsonResult(emptyResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading summary for cliente: {ClienteId}", clienteId);
            return new JsonResult(emptyResult);
        }
    }

    // Handler to get clients list for selection modal
    public async Task<IActionResult> OnGetClientesAsync()
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

            var response = await client.GetAsync($"/api/clientes/empresa/{empresaId}/select");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading clientes list");
            return new JsonResult(new List<object>());
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
