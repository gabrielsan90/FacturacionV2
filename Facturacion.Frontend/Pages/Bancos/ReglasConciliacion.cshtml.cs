using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Bancos;

[Authorize]
public class ReglasConciliacionModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReglasConciliacionModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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

            var response = await client.GetAsync($"api/reglasconciliacion/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var actionResponse = JsonSerializer.Deserialize<ActionResponse<List<ReglaConciliacion>>>(content, _jsonOptions);

                if (actionResponse?.WasSuccess == true && actionResponse.Result != null)
                {
                    return new JsonResult(new { data = actionResponse.Result });
                }
            }

            return new JsonResult(new { data = new List<ReglaConciliacion>() });
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

            var response = await client.GetAsync($"api/reglasconciliacion/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var actionResponse = JsonSerializer.Deserialize<ActionResponse<ReglaConciliacion>>(content, _jsonOptions);

                if (actionResponse?.WasSuccess == true && actionResponse.Result != null)
                {
                    return new JsonResult(new { success = true, data = actionResponse.Result });
                }
            }

            return new JsonResult(new { success = false, message = "No se encontró la regla" });
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
                var cuentas = JsonSerializer.Deserialize<List<CuentaBancaria>>(content, _jsonOptions);

                // Filtrar solo cuentas activas
                var cuentasActivas = cuentas?.Where(c => c.Activo).ToList() ?? new List<CuentaBancaria>();
                return new JsonResult(cuentasActivas);
            }

            return new JsonResult(new List<CuentaBancaria>());
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

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/cuentascontables/empresa/{empresaId}/movimiento");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return new JsonResult(new List<CuentaContable>());
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] ReglaConciliacionDto regla)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var userId = GetUserId();

            var reglaEntity = new ReglaConciliacion
            {
                Id = regla.Id == Guid.Empty ? Guid.NewGuid() : regla.Id,
                EmpresaId = empresaId.Value,
                Nombre = regla.Nombre,
                Descripcion = regla.Descripcion,
                Prioridad = regla.Prioridad,
                Activa = regla.Activa,
                CuentaBancariaId = string.IsNullOrEmpty(regla.CuentaBancariaId) ? null : Guid.Parse(regla.CuentaBancariaId),

                // Criterios de matching
                CompararMonto = regla.CompararMonto,
                ToleranciaMonto = regla.ToleranciaMonto,
                ToleranciaPorcentaje = regla.ToleranciaPorcentaje,

                CompararFecha = regla.CompararFecha,
                ToleranciaFechaDias = regla.ToleranciaFechaDias,

                CompararReferencia = regla.CompararReferencia,
                PatronReferencia = regla.PatronReferencia,

                CompararDescripcion = regla.CompararDescripcion,
                PalabrasClaveDescripcion = regla.PalabrasClaveDescripcion,

                // Acciones automáticas
                AutoConciliar = regla.AutoConciliar,
                CrearMovimientoFaltante = regla.CrearMovimientoFaltante,
                CuentaContableDefaultId = string.IsNullOrEmpty(regla.CuentaContableDefaultId) ? null : Guid.Parse(regla.CuentaContableDefaultId),
                TipoMovimientoDefault = regla.TipoMovimientoDefault,
                ConfianzaMinima = regla.ConfianzaMinima,

                // Audit trail
                FechaCreacion = regla.Id == Guid.Empty ? DateTime.Now : DateTime.MinValue,
                CreadoPorId = regla.Id == Guid.Empty ? userId : null,
                FechaModificacion = regla.Id != Guid.Empty ? DateTime.Now : null,
                ModificadoPorId = regla.Id != Guid.Empty ? userId : null
            };

            var json = JsonSerializer.Serialize(reglaEntity, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (regla.Id == Guid.Empty)
            {
                response = await client.PostAsync("api/reglasconciliacion", content);
            }
            else
            {
                response = await client.PutAsync($"api/reglasconciliacion/{regla.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = regla.Id == Guid.Empty ? "Regla creada exitosamente" : "Regla actualizada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostToggleAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.PostAsync($"api/reglasconciliacion/{id}/toggle", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Estado de la regla actualizado" });
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

            var response = await client.DeleteAsync($"api/reglasconciliacion/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Regla eliminada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostAplicarAsync()
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var requestData = new { empresaId = empresaId.Value };
            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/reglasconciliacion/aplicar", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var actionResponse = JsonSerializer.Deserialize<ActionResponse<dynamic>>(responseContent, _jsonOptions);

                if (actionResponse?.WasSuccess == true)
                {
                    return new JsonResult(new { success = true, message = actionResponse.Message ?? "Reglas aplicadas exitosamente" });
                }

                return new JsonResult(new { success = false, message = actionResponse?.Message ?? "Error al aplicar las reglas" });
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

    private string? GetUserId()
    {
        return User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    }

    public class ReglaConciliacionDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Prioridad { get; set; } = 100;
        public bool Activa { get; set; } = true;
        public string? CuentaBancariaId { get; set; }

        // Criterios de matching
        public bool CompararMonto { get; set; } = true;
        public decimal ToleranciaMonto { get; set; }
        public decimal? ToleranciaPorcentaje { get; set; }

        public bool CompararFecha { get; set; } = true;
        public int ToleranciaFechaDias { get; set; } = 3;

        public bool CompararReferencia { get; set; }
        public string? PatronReferencia { get; set; }

        public bool CompararDescripcion { get; set; }
        public string? PalabrasClaveDescripcion { get; set; }

        // Acciones automáticas
        public bool AutoConciliar { get; set; }
        public bool CrearMovimientoFaltante { get; set; }
        public string? CuentaContableDefaultId { get; set; }
        public string? TipoMovimientoDefault { get; set; }
        public decimal ConfianzaMinima { get; set; } = 95;
    }
}
