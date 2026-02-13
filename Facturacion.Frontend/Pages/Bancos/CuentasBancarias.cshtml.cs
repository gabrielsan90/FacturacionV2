using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Bancos;

[Authorize]
public class CuentasBancariasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public CuentasBancariasModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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
            if (client == null) return Unauthorized();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/cuentasbancarias/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cuentas = JsonSerializer.Deserialize<List<CuentaBancaria>>(content, _jsonOptions);
                return new JsonResult(cuentas);
            }

            return BadRequest("Error al obtener las cuentas bancarias.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetBancosAsync()
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var empresaId = User.FindFirst("EmpresaId")?.Value ?? "";
            var response = await client.GetAsync($"api/bancos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return BadRequest("Error al obtener los bancos.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetSucursalesAsync()
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/sucursales/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return BadRequest("Error al obtener las sucursales.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetCuentasContablesAsync()
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/cuentascontables/empresa/{empresaId}/movimiento");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return BadRequest("Error al obtener las cuentas contables.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] CuentaBancariaDto cuenta)
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var cuentaEntity = new CuentaBancaria
            {
                Id = cuenta.Id == Guid.Empty ? Guid.NewGuid() : cuenta.Id,
                EmpresaId = empresaId.Value,
                NumeroCuenta = cuenta.NumeroCuenta,
                Nombre = cuenta.Nombre,
                BancoId = string.IsNullOrEmpty(cuenta.BancoId) ? null : Guid.Parse(cuenta.BancoId),
                NombreBanco = cuenta.NombreBanco,
                TipoCuenta = cuenta.TipoCuenta,
                SucursalId = string.IsNullOrEmpty(cuenta.SucursalId) ? null : Guid.Parse(cuenta.SucursalId),
                Moneda = cuenta.Moneda,
                SaldoInicial = cuenta.SaldoInicial,
                FechaApertura = cuenta.FechaApertura,
                NumeroClabe = cuenta.NumeroClabe,
                SwiftCode = cuenta.SwiftCode,
                Contacto = cuenta.Contacto,
                Telefono = cuenta.Telefono,
                CuentaContableId = string.IsNullOrEmpty(cuenta.CuentaContableId) ? null : Guid.Parse(cuenta.CuentaContableId),
                Activo = cuenta.Activo,
                Observaciones = cuenta.Observaciones
            };

            var json = JsonSerializer.Serialize(cuentaEntity, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (cuenta.Id == Guid.Empty)
            {
                response = await client.PostAsync("api/cuentasbancarias", content);
            }
            else
            {
                response = await client.PutAsync($"api/cuentasbancarias/{cuenta.Id}", content);
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
            if (client == null) return Unauthorized();

            var response = await client.DeleteAsync($"api/cuentasbancarias/{id}");

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

    public class CuentaBancariaDto
    {
        public Guid Id { get; set; }
        public string NumeroCuenta { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? BancoId { get; set; }
        public string? NombreBanco { get; set; }
        public string TipoCuenta { get; set; } = "CTE";
        public string? SucursalId { get; set; }
        public string Moneda { get; set; } = "CRC";
        public decimal SaldoInicial { get; set; }
        public DateTime? FechaApertura { get; set; }
        public string? NumeroClabe { get; set; }
        public string? SwiftCode { get; set; }
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? CuentaContableId { get; set; }
        public bool Activo { get; set; }
        public string? Observaciones { get; set; }
    }
}
