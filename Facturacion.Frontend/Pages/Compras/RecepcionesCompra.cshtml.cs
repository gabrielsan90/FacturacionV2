using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Compras;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class RecepcionesCompraModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public RecepcionCompra RecepcionCompra { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public RecepcionesCompraModel(IHttpClientFactory httpClientFactory)
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
    public async Task<IActionResult> OnGetDataAsync(string? estado = null, string? fechaDesde = null, string? fechaHasta = null)
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { data = new List<RecepcionCompra>() });
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var url = $"/api/recepcionescompra/empresa/{empresaId}";
        var parameters = new List<string>();
        if (!string.IsNullOrEmpty(estado))
        {
            parameters.Add($"estado={estado}");
        }
        if (!string.IsNullOrEmpty(fechaDesde))
        {
            parameters.Add($"fechaDesde={fechaDesde}");
        }
        if (!string.IsNullOrEmpty(fechaHasta))
        {
            parameters.Add($"fechaHasta={fechaHasta}");
        }
        if (parameters.Count > 0)
        {
            url += "?" + string.Join("&", parameters);
        }

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var recepciones = await response.Content.ReadFromJsonAsync<List<RecepcionCompra>>(_jsonOptions);
            return new JsonResult(new { data = recepciones ?? new List<RecepcionCompra>() });
        }

        return new JsonResult(new { data = new List<RecepcionCompra>() });
    }

    // Handler to get a single recepcion by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/recepcionescompra/{id}");

        if (response.IsSuccessStatusCode)
        {
            var recepcion = await response.Content.ReadFromJsonAsync<RecepcionCompra>(_jsonOptions);
            return new JsonResult(new { success = true, data = recepcion });
        }

        return new JsonResult(new { success = false, message = "Recepción no encontrada" });
    }

    // Handler to get ordenes de compra aprobadas
    public async Task<IActionResult> OnGetOrdenesCompraAprobadasAsync()
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

        // Get approved purchase orders with pending items to receive
        var response = await client.GetAsync($"/api/ordenescompra/empresa/{empresaId}?estado=APR");

        if (response.IsSuccessStatusCode)
        {
            var ordenes = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(ordenes ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to get orden de compra details
    public async Task<IActionResult> OnGetOrdenCompraDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/ordenescompra/{id}");

        if (response.IsSuccessStatusCode)
        {
            var orden = await response.Content.ReadFromJsonAsync<OrdenCompra>(_jsonOptions);
            return new JsonResult(new { success = true, data = orden });
        }

        return new JsonResult(new { success = false, message = "Orden de compra no encontrada" });
    }

    // Handler to get bodegas by empresa
    public async Task<IActionResult> OnGetBodegasAsync()
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

        var response = await client.GetAsync($"/api/bodegas/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var bodegas = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(bodegas ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to save a recepcion
    public async Task<IActionResult> OnPostSaveAsync([FromBody] RecepcionCompra recepcionData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(recepcionData, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/recepcionescompra", content);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = "Recepción registrada exitosamente"
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to anular a recepcion
    public async Task<IActionResult> OnPostAnularAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.PostAsync($"/api/recepcionescompra/{id}/anular", null);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Recepción anulada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }
}
