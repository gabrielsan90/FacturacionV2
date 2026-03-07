using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Bancos;

[Authorize]
public class MovimientosBancariosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public MovimientosBancariosModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/movimientosbancarios/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var movimientos = JsonSerializer.Deserialize<List<MovimientoBancario>>(content, _jsonOptions);
                return new JsonResult(new { data = movimientos });
            }

            return BadRequest("Error al obtener los movimientos bancarios.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetByIdAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.GetAsync($"api/movimientosbancarios/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var movimiento = JsonSerializer.Deserialize<MovimientoBancario>(content, _jsonOptions);
                return new JsonResult(movimiento);
            }

            return BadRequest("Error al obtener el movimiento.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetCuentasBancariasAsync()
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/cuentasbancarias/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return BadRequest("Error al obtener las cuentas bancarias.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetPorCuentaAsync(Guid cuentaBancariaId)
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.GetAsync($"api/movimientosbancarios/cuenta/{cuentaBancariaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var movimientos = JsonSerializer.Deserialize<List<MovimientoBancario>>(content, _jsonOptions);
                return new JsonResult(movimientos);
            }

            return BadRequest("Error al obtener los movimientos de la cuenta.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetSaldoCuentaAsync(Guid cuentaBancariaId)
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.GetAsync($"api/movimientosbancarios/cuenta/{cuentaBancariaId}/saldo");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return BadRequest("Error al obtener el saldo de la cuenta.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] MovimientoBancarioDto movimiento)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var movimientoEntity = new MovimientoBancario
            {
                Id = movimiento.Id == Guid.Empty ? Guid.NewGuid() : movimiento.Id,
                EmpresaId = empresaId.Value,
                CuentaBancariaId = Guid.Parse(movimiento.CuentaBancariaId),
                Numero = movimiento.Numero,
                Fecha = movimiento.Fecha,
                TipoMovimiento = movimiento.TipoMovimiento,
                Naturaleza = movimiento.Naturaleza,
                Monto = movimiento.Monto,
                Beneficiario = movimiento.Beneficiario,
                NumeroReferencia = movimiento.NumeroReferencia,
                NumeroDocumento = movimiento.NumeroDocumento,
                Descripcion = movimiento.Descripcion,
                TipoDocumentoOrigen = movimiento.TipoDocumentoOrigen,
                DocumentoOrigenId = string.IsNullOrEmpty(movimiento.DocumentoOrigenId) ? null : Guid.Parse(movimiento.DocumentoOrigenId),
                Estado = movimiento.Estado,
                Observaciones = movimiento.Observaciones
            };

            var json = JsonSerializer.Serialize(movimientoEntity, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (movimiento.Id == Guid.Empty)
            {
                response = await client.PostAsync("api/movimientosbancarios", content);
            }
            else
            {
                response = await client.PutAsync($"api/movimientosbancarios/{movimiento.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.DeleteAsync($"api/movimientosbancarios/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostConciliarAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.PostAsync($"api/movimientosbancarios/{id}/conciliar", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private Guid? GetEmpresaId()
    {
        var empresaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (string.IsNullOrEmpty(empresaIdClaim)) return null;
        return Guid.Parse(empresaIdClaim);
    }

    public class MovimientoBancarioDto
    {
        public Guid Id { get; set; }
        public string CuentaBancariaId { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string TipoMovimiento { get; set; } = "DEP";
        public string Naturaleza { get; set; } = "CRE";
        public decimal Monto { get; set; }
        public string? Beneficiario { get; set; }
        public string? NumeroReferencia { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? Descripcion { get; set; }
        public string? TipoDocumentoOrigen { get; set; }
        public string? DocumentoOrigenId { get; set; }
        public string Estado { get; set; } = "REG";
        public string? Observaciones { get; set; }
    }
}
