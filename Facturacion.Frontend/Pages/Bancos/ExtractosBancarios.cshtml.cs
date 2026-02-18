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
public class ExtractosBancariosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExtractosBancariosModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetDataAsync(Guid? cuentaId = null)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            // Si no se especifica cuenta, obtener todos los extractos de la empresa
            string endpoint;
            if (cuentaId.HasValue && cuentaId.Value != Guid.Empty)
            {
                endpoint = $"api/extractosbancarios/cuenta/{cuentaId.Value}";
            }
            else
            {
                // Endpoint para obtener todos los extractos de todas las cuentas de la empresa
                // Por ahora usaremos una cuenta específica o retornaremos vacío
                return new JsonResult(new List<ExtractoBancario>());
            }

            var response = await client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var actionResponse = JsonSerializer.Deserialize<ActionResponse<List<ExtractoBancario>>>(content, _jsonOptions);

                if (actionResponse != null && actionResponse.WasSuccess)
                {
                    return new JsonResult(actionResponse.Result ?? new List<ExtractoBancario>());
                }
            }

            return new JsonResult(new List<ExtractoBancario>());
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
                return new JsonResult(cuentas);
            }

            return BadRequest("Error al obtener las cuentas bancarias.");
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

            var response = await client.GetAsync($"api/extractosbancarios/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var actionResponse = JsonSerializer.Deserialize<ActionResponse<ExtractoBancario>>(content, _jsonOptions);

                if (actionResponse != null && actionResponse.WasSuccess)
                {
                    return new JsonResult(actionResponse.Result);
                }
            }

            return BadRequest("Error al obtener el extracto bancario.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] ExtractoBancarioDto extracto)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var extractoEntity = new ExtractoBancario
            {
                Id = extracto.Id == Guid.Empty ? Guid.NewGuid() : extracto.Id,
                EmpresaId = empresaId.Value,
                CuentaBancariaId = extracto.CuentaBancariaId,
                Numero = extracto.Numero,
                FechaInicio = extracto.FechaInicio,
                FechaFin = extracto.FechaFin,
                SaldoInicial = extracto.SaldoInicial,
                SaldoFinal = extracto.SaldoFinal,
                TotalCreditos = extracto.TotalCreditos,
                TotalDebitos = extracto.TotalDebitos,
                CantidadTransacciones = extracto.CantidadTransacciones,
                FechaImportacion = DateTime.UtcNow,
                ArchivoOrigen = extracto.ArchivoOrigen,
                FormatoArchivo = extracto.FormatoArchivo,
                Estado = extracto.Estado ?? "PEN"
            };

            var json = JsonSerializer.Serialize(extractoEntity, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (extracto.Id == Guid.Empty)
            {
                response = await client.PostAsync("api/extractosbancarios", content);
            }
            else
            {
                response = await client.PutAsync($"api/extractosbancarios/{extracto.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var actionResponse = JsonSerializer.Deserialize<ActionResponse<ExtractoBancario>>(responseContent, _jsonOptions);

                if (actionResponse != null && actionResponse.WasSuccess)
                {
                    return new JsonResult(new { success = true, message = actionResponse.Message });
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostProcesarAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.PostAsync($"api/extractosbancarios/{id}/procesar", null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var actionResponse = JsonSerializer.Deserialize<ActionResponse<object>>(content, _jsonOptions);

                if (actionResponse != null && actionResponse.WasSuccess)
                {
                    return new JsonResult(new { success = true, message = actionResponse.Message });
                }
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

            var response = await client.DeleteAsync($"api/extractosbancarios/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Extracto eliminado correctamente." });
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

    public class ExtractoBancarioDto
    {
        public Guid Id { get; set; }
        public Guid CuentaBancariaId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoFinal { get; set; }
        public decimal TotalCreditos { get; set; }
        public decimal TotalDebitos { get; set; }
        public int CantidadTransacciones { get; set; }
        public string? ArchivoOrigen { get; set; }
        public string? FormatoArchivo { get; set; }
        public string? Estado { get; set; }
    }
}
