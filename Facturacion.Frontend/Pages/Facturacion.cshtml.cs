using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class FacturacionModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FacturacionModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public FacturacionModel(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<FacturacionModel> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public string ApiUrl => _configuration["ApiUrl"] ?? "";
    public string EmpresaId { get; set; } = "";
    public string? DocumentoId { get; set; }

    public void OnGet(string? documentoId)
    {
        // Get the current user's empresa
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";

        // Store documentoId if provided (for editing mode)
        DocumentoId = documentoId;
    }

    // Handler to get clientes for Select2
    public async Task<IActionResult> OnGetClientesAsync(string empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Clientes/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(data ?? new List<object>());
            }

            _logger.LogWarning("Failed to load clientes. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading clientes");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to get single cliente details
    public async Task<IActionResult> OnGetClienteDetailsAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Clientes/{id}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
                return new JsonResult(data);
            }

            _logger.LogWarning("Cliente not found. ID: {Id}, Status code: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { error = "Cliente no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cliente details. ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar los datos del cliente" });
        }
    }

    // Handler to get empresa details (including economic activity)
    public async Task<IActionResult> OnGetEmpresaDetailsAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Empresas/{id}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
                return new JsonResult(data);
            }

            _logger.LogWarning("Empresa not found. ID: {Id}, Status code: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { error = "Empresa no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading empresa details. ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar los datos de la empresa" });
        }
    }

    // Handler to get productos for Select2
    public async Task<IActionResult> OnGetProductosAsync(string empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Productos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(data ?? new List<object>());
            }

            _logger.LogWarning("Failed to load productos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading productos");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to get documentos for reference (Select2)
    public async Task<IActionResult> OnGetDocumentosAsync(string empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Documentos?empresaId={empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(data ?? new List<object>());
            }

            _logger.LogWarning("Failed to load documentos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading documentos");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to get sucursales
    public async Task<IActionResult> OnGetSucursalesAsync(string empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Sucursales/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(data ?? new List<object>());
            }

            _logger.LogWarning("Failed to load sucursales. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sucursales");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to get terminales
    public async Task<IActionResult> OnGetTerminalesAsync(string empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Terminales/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(data ?? new List<object>());
            }

            _logger.LogWarning("Failed to load terminales. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading terminales");
            return new JsonResult(new List<object>());
        }
    }

    // Handler to get provincias
    public async Task<IActionResult> OnGetProvinciasAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/Catalogos/provincias");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                // Check if response has success/data structure or is direct array
                if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("success", out var successProp))
                {
                    if (successProp.GetBoolean() && data.TryGetProperty("data", out var dataProp))
                    {
                        return new JsonResult(new { success = true, data = dataProp });
                    }
                }

                // Return as-is if it's already in correct format
                return new JsonResult(JsonSerializer.Deserialize<object>(content, _jsonOptions));
            }

            _logger.LogWarning("Failed to load provincias. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading provincias");
            return new JsonResult(new { success = false, data = new List<object>() });
        }
    }

    // Handler to get cantones by provincia
    public async Task<IActionResult> OnGetCantonesAsync(string provinciaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Catalogos/cantones/{provinciaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                // Check if response has success/data structure or is direct array
                if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("success", out var successProp))
                {
                    if (successProp.GetBoolean() && data.TryGetProperty("data", out var dataProp))
                    {
                        return new JsonResult(new { success = true, data = dataProp });
                    }
                }

                return new JsonResult(JsonSerializer.Deserialize<object>(content, _jsonOptions));
            }

            _logger.LogWarning("Failed to load cantones. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cantones");
            return new JsonResult(new { success = false, data = new List<object>() });
        }
    }

    // Handler to get distritos by provincia and canton
    public async Task<IActionResult> OnGetDistritosAsync(int provinciaId, int cantonId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Catalogos/distritos/{provinciaId}/{cantonId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                // Check if response has success/data structure or is direct array
                if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("success", out var successProp))
                {
                    if (successProp.GetBoolean() && data.TryGetProperty("data", out var dataProp))
                    {
                        return new JsonResult(new { success = true, data = dataProp });
                    }
                }

                return new JsonResult(JsonSerializer.Deserialize<object>(content, _jsonOptions));
            }

            _logger.LogWarning("Failed to load distritos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading distritos");
            return new JsonResult(new { success = false, data = new List<object>() });
        }
    }

    // Handler to create a new document
    public async Task<IActionResult> OnPostCreateDocumentAsync([FromBody] JsonElement documentData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(documentData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/Documentos", content);

            if (response.IsSuccessStatusCode)
            {
                var resultContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(resultContent, _jsonOptions);

                // Extract the id from the response - it could be in different structures
                string? documentoId = null;

                if (result.ValueKind == JsonValueKind.Object)
                {
                    // Try to get id directly
                    if (result.TryGetProperty("id", out var idProp))
                    {
                        documentoId = idProp.GetString();
                    }
                    // Or try to get it from a nested data property
                    else if (result.TryGetProperty("data", out var dataProp) &&
                             dataProp.ValueKind == JsonValueKind.Object &&
                             dataProp.TryGetProperty("id", out var nestedIdProp))
                    {
                        documentoId = nestedIdProp.GetString();
                    }
                }

                // Return a consistent response format with the id
                return new JsonResult(new {
                    success = true,
                    id = documentoId,
                    data = result
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to create document. Status code: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            return new JsonResult(new { success = false, message = "Error al crear el documento" });
        }
    }

    // Handler to send document to Hacienda
    public async Task<IActionResult> OnPostSendToHaciendaAsync(string documentoId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"/api/Documentos/{documentoId}/enviar", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Documento enviado a Hacienda exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to send document to Hacienda. ID: {Id}, Status code: {StatusCode}, Error: {Error}", documentoId, response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending document to Hacienda. ID: {Id}", documentoId);
            return new JsonResult(new { success = false, message = "Error al enviar el documento a Hacienda" });
        }
    }

    // Handler to get document for editing
    public async Task<IActionResult> OnGetDocumentoAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Documentos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var documento = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = documento });
            }

            _logger.LogWarning("Documento not found. ID: {Id}, Status code: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { success = false, message = "Documento no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading documento for editing. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar el documento" });
        }
    }

    // Handler to update existing document
    public async Task<IActionResult> OnPostUpdateDocumentAsync([FromBody] JsonElement documentData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Extract ID from document data
            string? documentoId = null;
            if (documentData.ValueKind == JsonValueKind.Object && documentData.TryGetProperty("id", out var idProp))
            {
                documentoId = idProp.GetString();
            }

            if (string.IsNullOrEmpty(documentoId))
            {
                return new JsonResult(new { success = false, message = "ID del documento es requerido" });
            }

            var json = JsonSerializer.Serialize(documentData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/Documentos/{documentoId}", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Documento actualizado exitosamente", id = documentoId });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to update document. ID: {Id}, Status code: {StatusCode}, Error: {Error}", documentoId, response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document");
            return new JsonResult(new { success = false, message = "Error al actualizar el documento" });
        }
    }

    // Handler to get formas farmacéuticas catalog
    public async Task<IActionResult> OnGetFormasFarmaceuticasAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/catalogos/formas-farmaceuticas");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                // Check if response has success/data structure or is direct array
                if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("success", out var successProp))
                {
                    if (successProp.GetBoolean() && data.TryGetProperty("data", out var dataProp))
                    {
                        return new JsonResult(new { success = true, data = dataProp });
                    }
                }

                return new JsonResult(JsonSerializer.Deserialize<object>(content, _jsonOptions));
            }

            _logger.LogWarning("Failed to load formas farmacéuticas. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading formas farmacéuticas");
            return new JsonResult(new { success = false, data = new List<object>() });
        }
    }
}
