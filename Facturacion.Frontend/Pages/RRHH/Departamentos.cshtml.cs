using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.RRHH;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class DepartamentosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Departamento Departamento { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public DepartamentosModel(IHttpClientFactory httpClientFactory)
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

    public async Task<IActionResult> OnGetDataAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { data = new List<Departamento>() });
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
            var departamentos = await response.Content.ReadFromJsonAsync<List<Departamento>>(_jsonOptions);
            return new JsonResult(new { data = departamentos ?? new List<Departamento>() });
        }

        return new JsonResult(new { data = new List<Departamento>() });
    }

    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/departamentos/{id}");

        if (response.IsSuccessStatusCode)
        {
            var departamento = await response.Content.ReadFromJsonAsync<Departamento>(_jsonOptions);
            return new JsonResult(new { success = true, data = departamento });
        }

        return new JsonResult(new { success = false, message = "Departamento no encontrado" });
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] Departamento departamentoData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(departamentoData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        bool isNew = departamentoData.Id == Guid.Empty;

        if (isNew)
        {
            response = await client.PostAsync("/api/departamentos", content);
        }
        else
        {
            response = await client.PutAsync($"/api/departamentos/{departamentoData.Id}", content);
        }

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = isNew ? "Departamento creado exitosamente" : "Departamento actualizado exitosamente"
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.DeleteAsync($"/api/departamentos/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Departamento eliminado exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to generate predefined catalog
    public async Task<IActionResult> OnPostGenerarCatalogoAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { success = false, message = "No se encontró la empresa" });
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.PostAsync($"/api/departamentos/empresa/{empresaId}/generar-catalogo", null);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object>(result, _jsonOptions);
            return new JsonResult(data);
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }
}
