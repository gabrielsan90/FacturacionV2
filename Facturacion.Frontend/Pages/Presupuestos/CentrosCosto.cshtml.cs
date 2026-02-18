using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Presupuestos;

[Authorize]
public class CentrosCostoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public CentrosCostoModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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

            var response = await client.GetAsync($"api/centroscosto/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var centros = JsonSerializer.Deserialize<List<CentroCosto>>(content, _jsonOptions);
                return new JsonResult(centros);
            }

            return BadRequest("Error al obtener los centros de costo.");
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

    public async Task<IActionResult> OnGetDepartamentosAsync()
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/departamentos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            return BadRequest("Error al obtener los departamentos.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] CentroCostoDto centro)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var centroEntity = new CentroCosto
            {
                Id = centro.Id == Guid.Empty ? Guid.NewGuid() : centro.Id,
                EmpresaId = empresaId.Value,
                Codigo = centro.Codigo,
                Nombre = centro.Nombre,
                Descripcion = centro.Descripcion,
                Tipo = centro.Tipo,
                PadreId = string.IsNullOrEmpty(centro.PadreId) ? null : Guid.Parse(centro.PadreId),
                SucursalId = string.IsNullOrEmpty(centro.SucursalId) ? null : Guid.Parse(centro.SucursalId),
                DepartamentoId = string.IsNullOrEmpty(centro.DepartamentoId) ? null : Guid.Parse(centro.DepartamentoId),
                AceptaMovimientos = centro.AceptaMovimientos,
                Activo = centro.Activo
            };

            var json = JsonSerializer.Serialize(centroEntity, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (centro.Id == Guid.Empty)
            {
                response = await client.PostAsync("api/centroscosto", content);
            }
            else
            {
                response = await client.PutAsync($"api/centroscosto/{centro.Id}", content);
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

            var response = await client.DeleteAsync($"api/centroscosto/{id}");

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

    public class CentroCostoDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Tipo { get; set; } = "COS";
        public string? PadreId { get; set; }
        public string? SucursalId { get; set; }
        public string? DepartamentoId { get; set; }
        public bool AceptaMovimientos { get; set; }
        public bool Activo { get; set; }
    }
}
