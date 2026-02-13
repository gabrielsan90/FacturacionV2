using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Compras;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class CotizacionesProveedorModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public CotizacionProveedor CotizacionProveedor { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public CotizacionesProveedorModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
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

    // Handler for DataTable
    public async Task<IActionResult> OnGetDataAsync(string? estado = null, string? requisicionId = null)
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { data = new List<CotizacionProveedor>() });
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var url = $"/api/cotizacionesproveedor/empresa/{empresaId}";
        var parameters = new List<string>();
        if (!string.IsNullOrEmpty(estado))
        {
            parameters.Add($"estado={estado}");
        }
        if (!string.IsNullOrEmpty(requisicionId))
        {
            parameters.Add($"requisicionId={requisicionId}");
        }
        if (parameters.Count > 0)
        {
            url += "?" + string.Join("&", parameters);
        }

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var cotizaciones = await response.Content.ReadFromJsonAsync<List<CotizacionProveedor>>(_jsonOptions);
            return new JsonResult(new { data = cotizaciones ?? new List<CotizacionProveedor>() });
        }

        return new JsonResult(new { data = new List<CotizacionProveedor>() });
    }

    // Handler to get a single cotizacion by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/cotizacionesproveedor/{id}");

        if (response.IsSuccessStatusCode)
        {
            var cotizacion = await response.Content.ReadFromJsonAsync<CotizacionProveedor>(_jsonOptions);
            return new JsonResult(new { success = true, data = cotizacion });
        }

        return new JsonResult(new { success = false, message = "Cotización no encontrada" });
    }

    // Handler to get requisiciones aprobadas
    public async Task<IActionResult> OnGetRequisicionesAprobadasAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new List<object>());
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/requisiciones/empresa/{empresaId}?estado=APR");

        if (response.IsSuccessStatusCode)
        {
            var requisiciones = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(requisiciones ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to get requisicion details
    public async Task<IActionResult> OnGetRequisicionDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/requisiciones/{id}");

        if (response.IsSuccessStatusCode)
        {
            var requisicion = await response.Content.ReadFromJsonAsync<Requisicion>(_jsonOptions);
            return new JsonResult(new { success = true, data = requisicion });
        }

        return new JsonResult(new { success = false, message = "Requisición no encontrada" });
    }

    // Handler to get proveedores
    public async Task<IActionResult> OnGetProveedoresAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new List<object>());
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/proveedores/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var proveedores = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(proveedores ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to save (create) a cotizacion
    public async Task<IActionResult> OnPostSaveAsync([FromBody] CotizacionProveedor cotizacionData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(cotizacionData, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/cotizacionesproveedor", content);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = "Solicitud de cotización enviada exitosamente"
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to register response from supplier
    public async Task<IActionResult> OnPostRegistrarRespuestaAsync([FromBody] CotizacionProveedor cotizacionData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(cotizacionData, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"/api/cotizacionesproveedor/{cotizacionData.Id}", content);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = "Respuesta de proveedor registrada exitosamente"
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to select a quotation
    public async Task<IActionResult> OnPostSeleccionarAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.PostAsync($"/api/cotizacionesproveedor/{id}/seleccionar", null);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Cotización seleccionada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }
}
