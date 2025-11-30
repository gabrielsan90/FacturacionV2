using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ClientesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Cliente Cliente { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public ClientesModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public void OnGet()
    {
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    // Handler for DataTable - Load all clients for the current empresa
    public async Task<IActionResult> OnGetDataAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new List<Cliente>());
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        // Get JWT token from user claims
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/clientes/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var clientes = await response.Content.ReadFromJsonAsync<List<Cliente>>(_jsonOptions);
            return new JsonResult(clientes ?? new List<Cliente>());
        }

        return new JsonResult(new List<Cliente>());
    }

    // Handler to get a single client by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/clientes/{id}");

        if (response.IsSuccessStatusCode)
        {
            var cliente = await response.Content.ReadFromJsonAsync<Cliente>(_jsonOptions);
            return new JsonResult(cliente);
        }

        return new JsonResult(new { error = "Cliente no encontrado" });
    }

    // Handler to save (create or update) a client
    public async Task<IActionResult> OnPostSaveAsync([FromBody] Cliente clienteData)
    {
        // Note: Using [FromBody] to receive JSON data from JavaScript
        //if (!ModelState.IsValid)
        //{
        //    var errors = ModelState
        //        .Where(x => x.Value!.Errors.Count > 0)
        //        .Select(x => new
        //        {
        //            Field = x.Key,
        //            Message = x.Value!.Errors.First().ErrorMessage
        //        });

        //    return new JsonResult(new { success = false, message = "Datos inválidos", errors });
        //}

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(clienteData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        bool isNew = clienteData.Id == Guid.Empty;

        if (isNew)
        {
            response = await client.PostAsync("/api/clientes", content);
        }
        else
        {
            response = await client.PutAsync($"/api/clientes/{clienteData.Id}", content);
        }

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = isNew ? "Cliente creado exitosamente" : "Cliente actualizado exitosamente"
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to delete a client
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.DeleteAsync($"/api/clientes/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Cliente eliminado exitosamente" });
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
    public async Task<IActionResult> OnGetCantonesAsync(string provinciaId)
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
