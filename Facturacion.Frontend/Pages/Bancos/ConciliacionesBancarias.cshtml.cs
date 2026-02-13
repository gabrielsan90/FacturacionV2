using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Bancos;

[Authorize]
public class ConciliacionesBancariasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConciliacionesBancariasModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public void OnGet()
    {
    }

    /// <summary>
    /// Obtiene todas las cuentas bancarias activas de la empresa
    /// </summary>
    public async Task<IActionResult> OnGetCuentasBancariasAsync()
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/cuentasbancarias/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cuentas = JsonSerializer.Deserialize<List<CuentaBancaria>>(content, _jsonOptions);

                // Filtrar solo cuentas activas
                var cuentasActivas = cuentas?.Where(c => c.Activo).ToList() ?? new List<CuentaBancaria>();

                return new JsonResult(cuentasActivas);
            }

            return BadRequest("Error al obtener las cuentas bancarias.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene las conciliaciones de una cuenta bancaria específica
    /// </summary>
    public async Task<IActionResult> OnGetDataAsync(Guid? cuentaBancariaId)
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            if (cuentaBancariaId == null || cuentaBancariaId == Guid.Empty)
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var response = await client.GetAsync($"api/conciliacionesbancarias/cuenta/{cuentaBancariaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // El API retorna directamente una lista de objetos anónimos, no ActionResponse
                var conciliaciones = JsonSerializer.Deserialize<List<JsonElement>>(content, _jsonOptions);
                return new JsonResult(new { data = conciliaciones });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { data = new List<object>(), error = errorContent });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { data = new List<object>(), error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una conciliación específica por ID
    /// </summary>
    public async Task<IActionResult> OnGetByIdAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var response = await client.GetAsync($"api/conciliacionesbancarias/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var conciliacion = JsonSerializer.Deserialize<ConciliacionBancaria>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = conciliacion });
            }

            return new JsonResult(new { success = false, message = "No se encontró la conciliación." });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Crea una nueva conciliación bancaria
    /// </summary>
    public async Task<IActionResult> OnPostSaveAsync([FromBody] ConciliacionBancariaDto? conciliacion)
    {
        try
        {
            if (conciliacion == null)
            {
                return new JsonResult(new { success = false, message = "No se recibieron datos de la conciliación." });
            }

            var client = CreateApiClient();
            if (client == null) return new JsonResult(new { success = false, message = "No autorizado." });

            var empresaId = GetEmpresaId();
            if (empresaId == null) return new JsonResult(new { success = false, message = "No se encontró la empresa." });

            // Validar que CuentaBancariaId sea un Guid válido
            if (string.IsNullOrEmpty(conciliacion.CuentaBancariaId) || !Guid.TryParse(conciliacion.CuentaBancariaId, out var cuentaId))
            {
                return new JsonResult(new { success = false, message = "La cuenta bancaria no es válida." });
            }

            var conciliacionEntity = new ConciliacionBancaria
            {
                Id = conciliacion.Id == Guid.Empty ? Guid.NewGuid() : conciliacion.Id,
                EmpresaId = empresaId.Value,
                CuentaBancariaId = cuentaId,
                Numero = conciliacion.Numero ?? "",
                Mes = conciliacion.Mes,
                Anio = conciliacion.Anio,
                FechaInicio = conciliacion.FechaInicio,
                FechaFin = conciliacion.FechaFin,
                SaldoInicialLibros = conciliacion.SaldoInicialLibros,
                SaldoFinalLibros = conciliacion.SaldoFinalLibros,
                SaldoEstadoCuenta = conciliacion.SaldoEstadoCuenta,
                DepositosEnTransito = conciliacion.DepositosEnTransito,
                ChequesEnTransito = conciliacion.ChequesEnTransito,
                NotasCredito = conciliacion.NotasCredito,
                NotasDebito = conciliacion.NotasDebito,
                Diferencia = conciliacion.Diferencia,
                Estado = conciliacion.Estado ?? "PEN",
                Observaciones = conciliacion.Observaciones,
                FechaCreacion = DateTime.Now
            };

            var json = JsonSerializer.Serialize(conciliacionEntity, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (conciliacion.Id == Guid.Empty)
            {
                response = await client.PostAsync("api/conciliacionesbancarias", content);
            }
            else
            {
                response = await client.PutAsync($"api/conciliacionesbancarias/{conciliacion.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Conciliación guardada correctamente." });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Cierra una conciliación bancaria (solo si está conciliada)
    /// </summary>
    public async Task<IActionResult> OnPostCerrarAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return new JsonResult(new { success = false, message = "No autorizado." });

            var response = await client.PostAsync($"api/conciliacionesbancarias/{id}/cerrar", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Conciliación cerrada exitosamente." });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Elimina una conciliación bancaria (solo si está en estado PEN)
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return new JsonResult(new { success = false, message = "No autorizado." });

            var response = await client.DeleteAsync($"api/conciliacionesbancarias/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Conciliación eliminada exitosamente." });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    private HttpClient? CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        if (Request.Cookies.TryGetValue("authToken", out var jwt))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            return client;
        }

        return null;
    }

    private Guid? GetEmpresaId()
    {
        var empresaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (string.IsNullOrEmpty(empresaIdClaim)) return null;
        return Guid.Parse(empresaIdClaim);
    }

    public class ConciliacionBancariaDto
    {
        public Guid Id { get; set; }
        public string CuentaBancariaId { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public int Mes { get; set; }
        public int Anio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal SaldoInicialLibros { get; set; }
        public decimal SaldoFinalLibros { get; set; }
        public decimal SaldoEstadoCuenta { get; set; }
        public decimal DepositosEnTransito { get; set; }
        public decimal ChequesEnTransito { get; set; }
        public decimal NotasCredito { get; set; }
        public decimal NotasDebito { get; set; }
        public decimal Diferencia { get; set; }
        public string? Estado { get; set; }
        public string? Observaciones { get; set; }
    }
}
