using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Compras;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class OrdenesCompraModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public OrdenCompra OrdenCompra { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public OrdenesCompraModel(IHttpClientFactory httpClientFactory)
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

    // Handler for DataTable - Load all purchase orders for the current empresa
    public async Task<IActionResult> OnGetDataAsync(string? estado = null)
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { data = new List<OrdenCompra>() });
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        // Get JWT token from user claims
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Build query string with optional estado filter
        var url = $"/api/ordenescompra/empresa/{empresaId}";
        if (!string.IsNullOrEmpty(estado))
        {
            url += $"?estado={estado}";
        }

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var ordenes = await response.Content.ReadFromJsonAsync<List<OrdenCompra>>(_jsonOptions);
            return new JsonResult(new { data = ordenes ?? new List<OrdenCompra>() });
        }

        return new JsonResult(new { data = new List<OrdenCompra>() });
    }

    // Handler to get a single orden de compra by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
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

    // Handler to get proveedores for Select2
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

    // Handler to get productos for Select2
    public async Task<IActionResult> OnGetProductosAsync()
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

        var response = await client.GetAsync($"/api/productos/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var productos = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(productos ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to get sucursales by empresa
    public async Task<IActionResult> OnGetSucursalesAsync()
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

        var response = await client.GetAsync($"/api/sucursales/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var sucursales = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(sucursales ?? new List<object>());
        }

        return new JsonResult(new List<object>());
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

    // Handler to save (create or update) an orden de compra
    public async Task<IActionResult> OnPostSaveAsync([FromBody] OrdenCompra ordenData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(ordenData, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        bool isNew = ordenData.Id == Guid.Empty;

        if (isNew)
        {
            response = await client.PostAsync("/api/ordenescompra", content);
        }
        else
        {
            response = await client.PutAsync($"/api/ordenescompra/{ordenData.Id}", content);
        }

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = isNew ? "Orden de compra creada exitosamente" : "Orden de compra actualizada exitosamente"
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to send orden de compra for approval
    public async Task<IActionResult> OnPostEnviarAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.PostAsync($"/api/ordenescompra/{id}/enviar", null);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Orden enviada para aprobación exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to approve orden de compra
    public async Task<IActionResult> OnPostAprobarAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.PostAsync($"/api/ordenescompra/{id}/aprobar", null);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Orden aprobada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to delete an orden de compra
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.DeleteAsync($"/api/ordenescompra/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Orden de compra eliminada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }
}
