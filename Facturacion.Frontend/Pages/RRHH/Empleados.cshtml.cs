using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.RRHH;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class EmpleadosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Empleado Empleado { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public EmpleadosModel(IHttpClientFactory httpClientFactory)
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
            return new ContentResult { Content = "{\"data\":[]}", ContentType = "application/json", StatusCode = 200 };
        }

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

            // Unwrap ActionResponse if needed
            using var doc = JsonDocument.Parse(content);
            string dataJson;
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                dataJson = content;
            }
            else if (doc.RootElement.TryGetProperty("result", out var resultProp))
            {
                dataJson = resultProp.GetRawText();
            }
            else
            {
                dataJson = "[]";
            }

            return new ContentResult
            {
                Content = $"{{\"data\":{dataJson}}}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        return new ContentResult { Content = "{\"data\":[]}", ContentType = "application/json", StatusCode = 200 };
    }

    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/empleados/{id}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return new ContentResult
            {
                Content = $"{{\"success\":true,\"data\":{content}}}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        return new JsonResult(new { success = false, message = "Empleado no encontrado" });
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] Empleado empleadoData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(empleadoData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        bool isNew = empleadoData.Id == Guid.Empty;

        if (isNew)
        {
            response = await client.PostAsync("/api/empleados", content);
        }
        else
        {
            response = await client.PutAsync($"/api/empleados/{empleadoData.Id}", content);
        }

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = isNew ? "Empleado creado exitosamente" : "Empleado actualizado exitosamente"
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

        var response = await client.DeleteAsync($"/api/empleados/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Empleado eliminado exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    public async Task<IActionResult> OnGetDepartamentosAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/departamentos/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            // Unwrap ActionResponse if needed
            using var doc = JsonDocument.Parse(content);
            string dataJson;
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                dataJson = content;
            }
            else if (doc.RootElement.TryGetProperty("result", out var resultProp))
            {
                dataJson = resultProp.GetRawText();
            }
            else
            {
                dataJson = "[]";
            }

            return new ContentResult
            {
                Content = $"{{\"data\":{dataJson}}}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        return new ContentResult { Content = "{\"data\":[]}", ContentType = "application/json", StatusCode = 200 };
    }

    public async Task<IActionResult> OnGetPuestosAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/puestos/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            // Unwrap ActionResponse if needed
            using var doc = JsonDocument.Parse(content);
            string dataJson;
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                dataJson = content;
            }
            else if (doc.RootElement.TryGetProperty("result", out var resultProp))
            {
                dataJson = resultProp.GetRawText();
            }
            else
            {
                dataJson = "[]";
            }

            return new ContentResult
            {
                Content = $"{{\"data\":{dataJson}}}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        return new ContentResult { Content = "{\"data\":[]}", ContentType = "application/json", StatusCode = 200 };
    }
}
