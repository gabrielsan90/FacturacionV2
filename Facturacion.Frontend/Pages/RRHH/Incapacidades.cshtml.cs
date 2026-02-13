using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.RRHH;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class IncapacidadesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Incapacidad Incapacidad { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public IncapacidadesModel(IHttpClientFactory httpClientFactory)
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
            return new JsonResult(new { data = new List<Incapacidad>() });
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/incapacidades/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var incapacidades = await response.Content.ReadFromJsonAsync<List<Incapacidad>>(_jsonOptions);
            return new JsonResult(new { data = incapacidades ?? new List<Incapacidad>() });
        }

        return new JsonResult(new { data = new List<Incapacidad>() });
    }

    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/incapacidades/{id}");

        if (response.IsSuccessStatusCode)
        {
            var incapacidad = await response.Content.ReadFromJsonAsync<Incapacidad>(_jsonOptions);
            return new JsonResult(new { success = true, data = incapacidad });
        }

        return new JsonResult(new { success = false, message = "Incapacidad no encontrada" });
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] Incapacidad incapacidadData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(incapacidadData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        bool isNew = incapacidadData.Id == Guid.Empty;

        if (isNew)
        {
            response = await client.PostAsync("/api/incapacidades", content);
        }
        else
        {
            response = await client.PutAsync($"/api/incapacidades/{incapacidadData.Id}", content);
        }

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = isNew ? "Incapacidad registrada exitosamente" : "Incapacidad actualizada exitosamente"
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

        var response = await client.DeleteAsync($"/api/incapacidades/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Incapacidad eliminada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    public async Task<IActionResult> OnGetEmpleadosAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/empleados/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object>(content, _jsonOptions);
            return new JsonResult(new { data });
        }

        return new JsonResult(new { data = new List<object>() });
    }
}
