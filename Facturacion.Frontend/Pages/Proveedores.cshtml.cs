using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ProveedoresModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Proveedor Proveedor { get; set; } = new();

    public ProveedoresModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public void OnGet()
    {
        // Page initialization
    }

    // Handler for DataTable - Load all proveedores
    public async Task<IActionResult> OnGetDataAsync()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        // Get JWT token from user claims
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync("/api/proveedores");

        if (response.IsSuccessStatusCode)
        {
            var proveedores = await response.Content.ReadFromJsonAsync<List<Proveedor>>(_jsonOptions);
            return new JsonResult(proveedores ?? new List<Proveedor>());
        }

        return new JsonResult(new List<Proveedor>());
    }

    // Handler to get a single proveedor by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/proveedores/{id}");

        if (response.IsSuccessStatusCode)
        {
            var proveedor = await response.Content.ReadFromJsonAsync<Proveedor>(_jsonOptions);
            return new JsonResult(proveedor);
        }

        return new JsonResult(new { error = "Proveedor no encontrado" });
    }

    // Handler to save (create or update) a proveedor
    public async Task<IActionResult> OnPostSaveAsync([FromBody] Proveedor proveedorData)
    {
        // Note: Using [FromBody] to receive JSON data from JavaScript
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value!.Errors.Count > 0)
                .Select(x => new
                {
                    Field = x.Key,
                    Message = x.Value!.Errors.First().ErrorMessage
                });

            return new JsonResult(new { success = false, message = "Datos inválidos", errors });
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(proveedorData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        bool isNew = proveedorData.Id == Guid.Empty;

        if (isNew)
        {
            response = await client.PostAsync("/api/proveedores", content);
        }
        else
        {
            response = await client.PutAsync($"/api/proveedores/{proveedorData.Id}", content);
        }

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = isNew ? "Proveedor creado exitosamente" : "Proveedor actualizado exitosamente"
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to delete a proveedor
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.DeleteAsync($"/api/proveedores/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Proveedor eliminado exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to get provincias (for address dropdown)
    public async Task<IActionResult> OnGetProvinciasAsync()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync("/api/catalogos/provincias");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object>(content, _jsonOptions);
            return new JsonResult(data);
        }

        return new JsonResult(new List<object>());
    }

    // Handler to get cantones by provincia ID
    public async Task<IActionResult> OnGetCantonesAsync(int provinciaId)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/catalogos/cantones/{provinciaId}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object>(content, _jsonOptions);
            return new JsonResult(data);
        }

        return new JsonResult(new List<object>());
    }

    // Handler to get distritos by provincia and canton ID
    public async Task<IActionResult> OnGetDistritosAsync(int provinciaId, int cantonId)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/catalogos/distritos/{provinciaId}/{cantonId}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object>(content, _jsonOptions);
            return new JsonResult(data);
        }

        return new JsonResult(new List<object>());
    }
}
