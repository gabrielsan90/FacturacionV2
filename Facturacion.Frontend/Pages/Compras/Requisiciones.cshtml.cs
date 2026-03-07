using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Compras;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class RequisicionesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Requisicion Requisicion { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public RequisicionesModel(IHttpClientFactory httpClientFactory)
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
    public async Task<IActionResult> OnGetDataAsync(string? estado = null, string? prioridad = null)
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { data = new List<Requisicion>() });
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var url = $"/api/requisiciones/empresa/{empresaId}";
        var parameters = new List<string>();
        if (!string.IsNullOrEmpty(estado))
        {
            parameters.Add($"estado={estado}");
        }
        if (!string.IsNullOrEmpty(prioridad))
        {
            parameters.Add($"prioridad={prioridad}");
        }
        if (parameters.Count > 0)
        {
            url += "?" + string.Join("&", parameters);
        }

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var requisiciones = await response.Content.ReadFromJsonAsync<List<Requisicion>>(_jsonOptions);
            return new JsonResult(new { data = requisiciones ?? new List<Requisicion>() });
        }

        return new JsonResult(new { data = new List<Requisicion>() });
    }

    // Handler to get a single requisicion by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
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

    // Handler to get productos for Select
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

    // Handler to get departamentos by empresa
    public async Task<IActionResult> OnGetDepartamentosAsync()
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

        var response = await client.GetAsync($"/api/departamentos/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var departamentos = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(departamentos ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to save (create or update) a requisicion
    public async Task<IActionResult> OnPostSaveAsync([FromBody] Requisicion requisicionData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        bool isNew = requisicionData.Id == Guid.Empty;

        if (isNew)
        {
            // Set placeholders for server-generated fields to pass API model validation
            // The backend controller will overwrite these with real values
            if (string.IsNullOrWhiteSpace(requisicionData.Numero))
                requisicionData.Numero = "AUTO";
            if (string.IsNullOrWhiteSpace(requisicionData.SolicitanteId))
                requisicionData.SolicitanteId = "PLACEHOLDER";
        }

        var json = JsonSerializer.Serialize(requisicionData, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;

        if (isNew)
        {
            response = await client.PostAsync("/api/requisiciones", content);
        }
        else
        {
            response = await client.PutAsync($"/api/requisiciones/{requisicionData.Id}", content);
        }

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = isNew ? "Requisición creada exitosamente" : "Requisición actualizada exitosamente"
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to send requisicion for approval
    public async Task<IActionResult> OnPostEnviarAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.PostAsync($"/api/requisiciones/{id}/enviar", null);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Requisición enviada para aprobación exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to approve requisicion
    public async Task<IActionResult> OnPostAprobarAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.PostAsync($"/api/requisiciones/{id}/aprobar", null);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Requisición aprobada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to reject requisicion
    public async Task<IActionResult> OnPostRechazarAsync(string id, [FromBody] string motivo)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(motivo);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"/api/requisiciones/{id}/rechazar", content);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Requisición rechazada" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to delete a requisicion
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.DeleteAsync($"/api/requisiciones/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Requisición eliminada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }
}
