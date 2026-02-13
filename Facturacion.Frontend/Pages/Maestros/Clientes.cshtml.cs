using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Maestros;

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
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
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

    // Handler to consult Hacienda API for taxpayer information
    public async Task<IActionResult> OnGetConsultarHaciendaAsync(string identificacion)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
        {
            return new JsonResult(new { success = false, message = "Identificación requerida" });
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var response = await httpClient.GetAsync($"https://api.hacienda.go.cr/fe/ae?identificacion={identificacion}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                // Extract relevant fields
                var result = new
                {
                    success = true,
                    nombre = data.TryGetProperty("nombre", out var nombreProp) ? nombreProp.GetString() : null,
                    tipoIdentificacion = data.TryGetProperty("tipoIdentificacion", out var tipoProp) ? tipoProp.GetString() : null,
                    actividadEconomica = GetPrimaryActivity(data),
                    actividadEconomicaCIIU3 = GetPrimaryActivityCIIU3(data),
                    regimen = data.TryGetProperty("regimen", out var regimenProp) && regimenProp.TryGetProperty("descripcion", out var regDescProp) ? regDescProp.GetString() : null,
                    estado = data.TryGetProperty("situacion", out var sitProp) && sitProp.TryGetProperty("estado", out var estadoProp) ? estadoProp.GetString() : null,
                    moroso = data.TryGetProperty("situacion", out var sit2Prop) && sit2Prop.TryGetProperty("moroso", out var morosoProp) ? morosoProp.GetString() : null,
                    omiso = data.TryGetProperty("situacion", out var sit3Prop) && sit3Prop.TryGetProperty("omiso", out var omisoProp) ? omisoProp.GetString() : null
                };

                return new JsonResult(result);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new JsonResult(new { success = false, message = "Contribuyente no encontrado en Hacienda" });
            }

            return new JsonResult(new { success = false, message = $"Error al consultar Hacienda: {response.StatusCode}" });
        }
        catch (TaskCanceledException)
        {
            return new JsonResult(new { success = false, message = "Tiempo de espera agotado al consultar Hacienda" });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    private static string? GetPrimaryActivity(JsonElement data)
    {
        if (data.TryGetProperty("actividades", out var actividades) && actividades.ValueKind == JsonValueKind.Array)
        {
            foreach (var actividad in actividades.EnumerateArray())
            {
                if (actividad.TryGetProperty("estado", out var estado) && estado.GetString() == "A")
                {
                    if (actividad.TryGetProperty("codigo", out var codigo))
                    {
                        return codigo.GetString();
                    }
                }
            }
            // If no active activity, return first one
            if (actividades.GetArrayLength() > 0)
            {
                var primera = actividades[0];
                if (primera.TryGetProperty("codigo", out var codigo))
                {
                    return codigo.GetString();
                }
            }
        }
        return null;
    }

    private static string? GetPrimaryActivityCIIU3(JsonElement data)
    {
        if (data.TryGetProperty("actividades", out var actividades) && actividades.ValueKind == JsonValueKind.Array)
        {
            foreach (var actividad in actividades.EnumerateArray())
            {
                if (actividad.TryGetProperty("estado", out var estado) && estado.GetString() == "A")
                {
                    if (actividad.TryGetProperty("ciiu3", out var ciiu3) && ciiu3.ValueKind == JsonValueKind.Array && ciiu3.GetArrayLength() > 0)
                    {
                        var primerCIIU3 = ciiu3[0];
                        if (primerCIIU3.TryGetProperty("codigo", out var codigo))
                        {
                            return codigo.GetString();
                        }
                    }
                }
            }
        }
        return null;
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

        var response = await client.GetAsync("/api/clientes/plantilla");

        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_Clientes.xlsx");
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

        var response = await client.PostAsync($"/api/clientes/empresa/{empresaId}/importar", content);

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
