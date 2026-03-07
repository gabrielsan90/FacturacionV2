using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Facturacion.Frontend.Pages.Maestros;

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
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
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

        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrWhiteSpace(empresaId))
        {
            return new JsonResult(new { data = new List<Proveedor>() });
        }

        var response = await client.GetAsync($"/api/Proveedores/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var proveedores = await response.Content.ReadFromJsonAsync<List<Proveedor>>(_jsonOptions);
            return new JsonResult(new { data = proveedores ?? new List<Proveedor>() });
        }

        return new JsonResult(new { data = new List<Proveedor>() });
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
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement proveedorElement)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrWhiteSpace(empresaId) || !Guid.TryParse(empresaId, out var empresaGuid))
        {
            return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
        }

        // Use JsonNode to manipulate the JSON: inject empresaId, normalize id
        var node = JsonNode.Parse(proveedorElement.GetRawText())?.AsObject();
        if (node == null)
        {
            return new JsonResult(new { success = false, message = "No se recibieron los datos del proveedor." });
        }

        // Determine if new or update
        var idStr = node["id"]?.GetValue<string>() ?? "";
        bool isNew = !Guid.TryParse(idStr, out var idGuid) || idGuid == Guid.Empty;

        if (isNew)
        {
            node["id"] = Guid.Empty.ToString();
        }

        // Inject empresaId if missing
        var existingEmpresa = node["empresaId"]?.GetValue<string>() ?? "";
        if (!Guid.TryParse(existingEmpresa, out var existingEmpresaGuid) || existingEmpresaGuid == Guid.Empty)
        {
            node["empresaId"] = empresaGuid.ToString();
        }

        var json = node.ToJsonString();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;

        if (isNew)
        {
            response = await client.PostAsync("/api/proveedores", content);
        }
        else
        {
            response = await client.PutAsync($"/api/proveedores/{idGuid}", content);
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
        var mensajeError = ExtraerMensajeError(error, (int)response.StatusCode);
        return new JsonResult(new { success = false, message = mensajeError });
    }

    /// <summary>
    /// Extrae un mensaje de error legible de la respuesta del API
    /// </summary>
    private static string ExtraerMensajeError(string responseBody, int statusCode)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return statusCode switch
            {
                400 => "Datos inválidos. Verifique los campos e intente de nuevo.",
                403 => "No tiene permisos para realizar esta acción.",
                404 => "Recurso no encontrado.",
                _ => $"Error del servidor (código {statusCode})."
            };
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // ValidationProblem format
            if (root.TryGetProperty("errors", out var errors))
            {
                var mensajes = new List<string>();
                foreach (var field in errors.EnumerateObject())
                {
                    foreach (var msg in field.Value.EnumerateArray())
                    {
                        mensajes.Add(msg.GetString() ?? field.Name);
                    }
                }
                if (mensajes.Count > 0) return string.Join(" | ", mensajes);
            }

            if (root.TryGetProperty("message", out var message))
                return message.GetString() ?? responseBody;

            if (root.ValueKind == JsonValueKind.String)
                return root.GetString() ?? responseBody;
        }
        catch { /* Not JSON */ }

        if (responseBody.Contains("An error occurred while saving"))
            return "Error al guardar los datos. Verifique que todos los campos requeridos estén completos y no existan duplicados.";

        return responseBody;
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

    // Handler to download import template
    public async Task<IActionResult> OnGetPlantillaAsync()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync("/api/proveedores/plantilla");

        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_Proveedores.xlsx");
        }

        return BadRequest("Error al descargar la plantilla");
    }

    // Handler to import from Excel
    public async Task<IActionResult> OnPostImportarAsync(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return new JsonResult(new { wasSuccess = false, message = "Debe seleccionar un archivo" });
        }

        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { wasSuccess = false, message = "No se encontró la empresa" });
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        using var content = new MultipartFormDataContent();
        using var fileStream = archivo.OpenReadStream();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(archivo.ContentType);
        content.Add(streamContent, "archivo", archivo.FileName);

        var response = await client.PostAsync($"/api/proveedores/empresa/{empresaId}/importar", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var importResult = JsonSerializer.Deserialize<object>(result, _jsonOptions);
            return new JsonResult(importResult);
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { wasSuccess = false, message = error });
    }
}
