using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.DocumentosElectronicos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class CrearDocumentoModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CrearDocumentoModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CrearDocumentoModel(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<CrearDocumentoModel> logger)
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

    // Handler to get consecutivos configured for the empresa
    public async Task<IActionResult> OnGetConsecutivosAsync(string empresaId)
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

            var response = await client.GetAsync($"/api/Consecutivos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
                return new JsonResult(data ?? new List<object>());
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consecutivos for empresa {EmpresaId}", empresaId);
            return new JsonResult(new List<object>());
        }
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

            // Obtener todos los documentos sin paginación para el modal de referencia
            var response = await client.GetAsync($"/api/Documentos?empresaId={empresaId}&take=500");

            if (response.IsSuccessStatusCode)
            {
                // El backend devuelve { data: [...], recordsTotal, recordsFiltered }
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                if (jsonResponse.TryGetProperty("data", out var dataElement))
                {
                    return new JsonResult(dataElement);
                }

                // Fallback si no tiene propiedad data
                return new JsonResult(jsonResponse);
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

    // Handler to get documento completo con detalles (para NC/ND)
    public async Task<IActionResult> OnGetDocumentoCompletoAsync(string id)
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

            // Obtener documento con todos los detalles
            var response = await client.GetAsync($"/api/Documentos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
                return new JsonResult(jsonResponse);
            }

            _logger.LogWarning("Failed to load documento completo. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { error = "No se pudo cargar el documento" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading documento completo");
            return new JsonResult(new { error = ex.Message });
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

                    if (data.ValueKind == JsonValueKind.Array)
                    {
                        return new JsonResult(new { success = true, data });
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
            var response = await client.PostAsync($"/api/Documentos/{documentoId}/procesar", content);

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

    // Handler to search CABYS by code
    public async Task<IActionResult> OnGetBuscarCabysAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new JsonResult(new { success = false, message = "Código requerido" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // If code is exactly 13 characters, search exact match
            var url = codigo.Length == 13
                ? $"/api/cabys/local/buscar?q={Uri.EscapeDataString(codigo)}&top=1"
                : $"/api/cabys/local/buscar?q={Uri.EscapeDataString(codigo)}&top=5";

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var cabys = await response.Content.ReadFromJsonAsync<List<JsonElement>>(_jsonOptions);
                if (cabys != null && cabys.Count > 0)
                {
                    var first = cabys[0];
                    return new JsonResult(new
                    {
                        success = true,
                        data = new
                        {
                            id = first.GetProperty("id").GetInt32(),
                            codigo = first.GetProperty("codigo").GetString(),
                            descripcion = first.GetProperty("descripcion").GetString()
                        }
                    });
                }
                return new JsonResult(new { success = false, message = "Código no encontrado" });
            }

            _logger.LogWarning("Failed to search CABYS. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, message = "Error al buscar CABYS" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching CABYS by code: {Codigo}", codigo);
            return new JsonResult(new { success = false, message = "Error al buscar CABYS" });
        }
    }

    /// <summary>
    /// Handler to get exchange rate from BCCR via Hacienda API
    /// GET: ?handler=TipoCambio&moneda=USD
    /// </summary>
    public async Task<IActionResult> OnGetTipoCambioAsync(string moneda)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(moneda))
            {
                return new JsonResult(new { success = false, message = "Debe especificar la moneda" });
            }

            // For CRC, exchange rate is always 1
            if (moneda.Equals("CRC", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new { success = true, valor = 1.00m, moneda = "CRC", fecha = DateTime.Now.ToString("yyyy-MM-dd") });
            }

            // Only USD is supported via Hacienda API
            if (!moneda.Equals("USD", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new { success = false, message = $"La moneda {moneda} no está soportada. Ingrese el tipo de cambio manualmente." });
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            decimal tasaVenta = 0;
            string? fecha = null;
            string? errorDetalle = null;

            // Intento 1: API oficial de Hacienda Costa Rica (obtiene datos del BCCR)
            try
            {
                _logger.LogInformation("Fetching exchange rate from Hacienda API");
                var response = await client.GetAsync("https://api.hacienda.go.cr/indicadores/tc/dolar");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Hacienda API response: {Response}", json);

                    // La API retorna: {"venta":{"fecha":"2025-11-30","valor":496.39},"compra":{"fecha":"2025-11-30","valor":488.55}}
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("venta", out var ventaObj))
                    {
                        if (ventaObj.TryGetProperty("valor", out var ventaVal))
                        {
                            tasaVenta = ventaVal.GetDecimal();
                        }
                        if (ventaObj.TryGetProperty("fecha", out var fechaVal))
                        {
                            fecha = fechaVal.GetString();
                        }
                    }
                }
                else
                {
                    errorDetalle = $"API Hacienda: HTTP {(int)response.StatusCode}";
                }
            }
            catch (Exception ex1)
            {
                _logger.LogWarning(ex1, "Error calling Hacienda API");
                errorDetalle = $"API Hacienda: {ex1.Message}";
            }

            // Intento 2: API alternativa si Hacienda falla
            if (tasaVenta == 0)
            {
                try
                {
                    _logger.LogInformation("Trying alternative API for exchange rate");
                    var responseVenta = await client.GetAsync("https://tipodecambio.paginasweb.cr/api/venta");

                    if (responseVenta.IsSuccessStatusCode)
                    {
                        var ventaStr = await responseVenta.Content.ReadAsStringAsync();
                        _logger.LogInformation("Alternative API response: {Response}", ventaStr);

                        if (decimal.TryParse(ventaStr.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var venta))
                        {
                            tasaVenta = venta;
                            fecha = DateTime.Now.ToString("yyyy-MM-dd");
                        }
                    }
                }
                catch (Exception ex2)
                {
                    _logger.LogWarning(ex2, "Error calling alternative API");
                    errorDetalle = (errorDetalle ?? "") + $" | API alternativa: {ex2.Message}";
                }
            }

            // Si se obtuvo el valor, retornar éxito
            if (tasaVenta > 0)
            {
                _logger.LogInformation("Exchange rate for USD: {Valor}", tasaVenta);
                return new JsonResult(new
                {
                    success = true,
                    valor = tasaVenta,
                    moneda = "USD",
                    fecha = fecha ?? DateTime.Now.ToString("yyyy-MM-dd")
                });
            }

            // Si no se pudo obtener, retornar error
            var mensaje = "No se pudo obtener el tipo de cambio del BCCR.";
            if (!string.IsNullOrEmpty(errorDetalle))
                mensaje += $" Detalle: {errorDetalle}";

            _logger.LogWarning("Failed to get exchange rate: {Message}", mensaje);
            return new JsonResult(new
            {
                success = false,
                message = mensaje
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exchange rate for currency: {Moneda}", moneda);
            return new JsonResult(new
            {
                success = false,
                message = "Error al consultar el tipo de cambio: " + ex.Message
            });
        }
    }
}
