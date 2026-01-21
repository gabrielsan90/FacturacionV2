using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser")]
public class EmpresasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmpresasModel> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly JsonSerializerOptions _jsonOptions;

    // Properties for select lists
    public IEnumerable<SelectListItem> TiposIdentificacion { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Provincias { get; set; } = new List<SelectListItem>();

    [BindProperty]
    public Empresa Empresa { get; set; } = new();

    [BindProperty]
    public IFormFile? LogoFile { get; set; }

    [BindProperty]
    public IFormFile? CertificadoFile { get; set; }

    public EmpresasModel(IHttpClientFactory httpClientFactory, ILogger<EmpresasModel> logger, IWebHostEnvironment environment)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _environment = environment;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public async Task OnGetAsync()
    {
        await LoadSelectListsAsync();
    }

    // Load data for DataTable
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/empresas");

            if (response.IsSuccessStatusCode)
            {
                var empresas = await response.Content.ReadFromJsonAsync<List<Empresa>>(_jsonOptions);

                // Transform data for DataTable
                var data = empresas?.Select(e => new
                {
                    e.Id,
                    e.NumeroIdentificacion,
                    e.NombreComercial,
                    e.RazonSocial,
                    e.Provincia,
                    ProvinciaTexto = ObtenerNombreProvincia(e.Provincia),
                    Ambiente = e.Ambiente.ToString(),
                    AmbienteTexto = e.Ambiente == Ambiente.Produccion ? "Producción" : "Pruebas",
                    Estado = e.Activa ? "Activa" : "Inactiva",
                    e.Activa
                }).ToList();

                return new JsonResult(new { data });
            }

            _logger.LogWarning("Failed to load empresas: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading empresas data");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    // Get single empresa for edit
    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/empresas/{id}");
            if (response.IsSuccessStatusCode)
            {
                var empresa = await response.Content.ReadFromJsonAsync<Empresa>(_jsonOptions);
                return new JsonResult(new { success = true, data = empresa });
            }

            _logger.LogWarning("Empresa not found: {Id}", id);
            return new JsonResult(new { success = false, message = "Empresa no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading empresa details: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar los detalles de la empresa" });
        }
    }

    // Get cantones by provincia
    public async Task<IActionResult> OnGetCantonesByProvinciaAsync(int provinciaId)
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

    // Get distritos by canton
    public async Task<IActionResult> OnGetDistritosByCantonAsync(int provinciaId, int cantonId)
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

    // Save empresa (Create or Update)
    public async Task<IActionResult> OnPostSaveAsync()
    {
        // Remove validation for file uploads as they're optional
        ModelState.Remove("LogoFile");
        ModelState.Remove("CertificadoFile");

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

        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Handle logo file upload
            if (LogoFile != null && LogoFile.Length > 0)
            {
                // Validate file size (max 2MB)
                if (LogoFile.Length > 2 * 1024 * 1024)
                {
                    return new JsonResult(new { success = false, message = "El logo no puede superar los 2MB" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(LogoFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return new JsonResult(new { success = false, message = "Solo se permiten imágenes (jpg, png, gif)" });
                }

                // Convert to base64
                using var ms = new MemoryStream();
                await LogoFile.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                Empresa.Logo = $"data:image/{extension.TrimStart('.')};base64,{Convert.ToBase64String(imageBytes)}";
            }

            // Handle certificate file upload
            if (CertificadoFile != null && CertificadoFile.Length > 0)
            {
                // Validate file size (max 5MB)
                if (CertificadoFile.Length > 5 * 1024 * 1024)
                {
                    return new JsonResult(new { success = false, message = "El certificado no puede superar los 5MB" });
                }

                // Validate file type
                var extension = Path.GetExtension(CertificadoFile.FileName).ToLowerInvariant();
                if (extension != ".p12" && extension != ".pfx")
                {
                    return new JsonResult(new { success = false, message = "Solo se permiten certificados .p12 o .pfx" });
                }

                // Convert to byte array
                using var ms = new MemoryStream();
                await CertificadoFile.CopyToAsync(ms);
                Empresa.CertificadoDigital = ms.ToArray();
            }

            var json = JsonSerializer.Serialize(Empresa);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = Empresa.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/empresas", content);
            }
            else
            {
                response = await client.PutAsync($"/api/empresas/{Empresa.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                var result = new
                {
                    success = true,
                    message = isNew ? "Empresa creada exitosamente" : "Empresa actualizada exitosamente"
                };

                // If it was a new empresa, return the ID so the frontend can enable tabs
                if (isNew)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var empresaCreada = JsonSerializer.Deserialize<Empresa>(responseContent, _jsonOptions);
                        if (empresaCreada != null)
                        {
                            return new JsonResult(new
                            {
                                success = true,
                                message = "Empresa creada exitosamente",
                                empresaId = empresaCreada.Id
                            });
                        }
                    }
                    catch
                    {
                        // If we can't get the ID, just return success
                    }
                }

                return new JsonResult(result);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save empresa: {Error}", error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving empresa");
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // Delete empresa
    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/empresas/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Empresa deleted successfully: {Id}", id);
                return new JsonResult(new { success = true, message = "Empresa eliminada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete empresa {Id}: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting empresa: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar la empresa" });
        }
    }

    // ========================================
    // SUCURSALES HANDLERS
    // ========================================

    public async Task<IActionResult> OnGetSucursalesByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/sucursales/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var sucursales = await response.Content.ReadFromJsonAsync<List<Sucursal>>(_jsonOptions);
                return new JsonResult(new { success = true, data = sucursales ?? new List<Sucursal>() });
            }

            _logger.LogWarning("Failed to load sucursales for empresa {EmpresaId}: {StatusCode}", empresaId, response.StatusCode);
            return new JsonResult(new { success = false, data = new List<Sucursal>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sucursales for empresa {EmpresaId}", empresaId);
            return new JsonResult(new { success = false, data = new List<Sucursal>() });
        }
    }

    public async Task<IActionResult> OnPostSaveSucursalAsync([FromBody] Sucursal sucursal)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(sucursal.Codigo))
            {
                return new JsonResult(new { success = false, message = "El código es obligatorio" });
            }

            if (string.IsNullOrWhiteSpace(sucursal.Nombre))
            {
                return new JsonResult(new { success = false, message = "El nombre es obligatorio" });
            }

            if (sucursal.EmpresaId == Guid.Empty)
            {
                return new JsonResult(new { success = false, message = "Debe seleccionar una empresa" });
            }

            // Validate email format if provided
            if (!string.IsNullOrWhiteSpace(sucursal.Email) && !IsValidEmail(sucursal.Email))
            {
                return new JsonResult(new { success = false, message = "El email no tiene un formato válido" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(sucursal);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = sucursal.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/sucursales", content);
            }
            else
            {
                response = await client.PutAsync($"/api/sucursales/{sucursal.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Sucursal creada exitosamente" : "Sucursal actualizada exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save sucursal. Error: {Error}", error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving sucursal");
            return new JsonResult(new { success = false, message = "Error al guardar la sucursal" });
        }
    }

    public async Task<IActionResult> OnPostDeleteSucursalAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/sucursales/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Sucursal eliminada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete sucursal {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sucursal {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar la sucursal" });
        }
    }

    // ========================================
    // TERMINALES HANDLERS
    // ========================================

    public async Task<IActionResult> OnGetTerminalesByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // First, get all sucursales for this empresa
            var sucursalesResponse = await client.GetAsync($"/api/sucursales/empresa/{empresaId}");
            if (!sucursalesResponse.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = false, data = new List<Terminal>() });
            }

            var sucursales = await sucursalesResponse.Content.ReadFromJsonAsync<List<Sucursal>>(_jsonOptions);
            if (sucursales == null || !sucursales.Any())
            {
                return new JsonResult(new { success = true, data = new List<Terminal>() });
            }

            // Get terminales for all sucursales
            var allTerminales = new List<Terminal>();
            foreach (var sucursal in sucursales)
            {
                var terminalesResponse = await client.GetAsync($"/api/terminales/sucursal/{sucursal.Id}");
                if (terminalesResponse.IsSuccessStatusCode)
                {
                    var terminales = await terminalesResponse.Content.ReadFromJsonAsync<List<Terminal>>(_jsonOptions);
                    if (terminales != null)
                    {
                        allTerminales.AddRange(terminales);
                    }
                }
            }

            return new JsonResult(new { success = true, data = allTerminales });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading terminales for empresa {EmpresaId}", empresaId);
            return new JsonResult(new { success = false, data = new List<Terminal>() });
        }
    }

    public async Task<IActionResult> OnPostSaveTerminalAsync([FromBody] Terminal terminal)
    {
        try
        {
            // Validate ModelState
            if (string.IsNullOrWhiteSpace(terminal.Codigo))
            {
                return new JsonResult(new { success = false, message = "El código es obligatorio" });
            }

            if (string.IsNullOrWhiteSpace(terminal.Nombre))
            {
                return new JsonResult(new { success = false, message = "El nombre es obligatorio" });
            }

            if (terminal.SucursalId == Guid.Empty)
            {
                return new JsonResult(new { success = false, message = "Debe seleccionar una sucursal" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(terminal);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = terminal.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/terminales", content);
            }
            else
            {
                response = await client.PutAsync($"/api/terminales/{terminal.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Terminal creado exitosamente" : "Terminal actualizado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save terminal. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving terminal");
            return new JsonResult(new { success = false, message = "Error al guardar el terminal" });
        }
    }

    public async Task<IActionResult> OnPostDeleteTerminalAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/terminales/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Terminal eliminado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete terminal {Id}. Status: {StatusCode}, Error: {Error}", id, response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting terminal {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el terminal" });
        }
    }

    // ========================================
    // CONSECUTIVOS HANDLERS
    // ========================================

    public async Task<IActionResult> OnGetConsecutivosByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/consecutivos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var consecutivos = await response.Content.ReadFromJsonAsync<List<Consecutivo>>(_jsonOptions);
                return new JsonResult(new { success = true, data = consecutivos ?? new List<Consecutivo>() });
            }

            _logger.LogWarning("Failed to load consecutivos for empresa {EmpresaId}: {StatusCode}", empresaId, response.StatusCode);
            return new JsonResult(new { success = false, data = new List<Consecutivo>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading consecutivos for empresa {EmpresaId}", empresaId);
            return new JsonResult(new { success = false, data = new List<Consecutivo>() });
        }
    }

    public async Task<IActionResult> OnPostSaveConsecutivoAsync([FromBody] Consecutivo consecutivo)
    {
        try
        {
            // Validate required fields
            if (consecutivo.TerminalId == Guid.Empty)
            {
                return new JsonResult(new { success = false, message = "Debe seleccionar un terminal" });
            }

            if (string.IsNullOrWhiteSpace(consecutivo.TipoDocumento))
            {
                return new JsonResult(new { success = false, message = "El tipo de documento es obligatorio" });
            }

            // Validate numero actual is not negative
            if (consecutivo.NumeroActual < 0)
            {
                return new JsonResult(new { success = false, message = "El número actual no puede ser negativo" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(consecutivo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = consecutivo.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/consecutivos", content);
            }
            else
            {
                response = await client.PutAsync($"/api/consecutivos/{consecutivo.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Consecutivo creado exitosamente" : "Consecutivo actualizado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save consecutivo. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving consecutivo");
            return new JsonResult(new { success = false, message = "Error al guardar el consecutivo" });
        }
    }

    public async Task<IActionResult> OnPostDeleteConsecutivoAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/consecutivos/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Consecutivo eliminado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete consecutivo {Id}. Status: {StatusCode}, Error: {Error}", id, response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting consecutivo {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el consecutivo" });
        }
    }

    // ========================================
    // ACTIVIDADES ECONÓMICAS HANDLERS
    // ========================================

    public async Task<IActionResult> OnGetActividadesEconomicasAsync(Guid empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/empresas/{empresaId}/actividades-economicas");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
                return new JsonResult(new { success = true, data });
            }

            return new JsonResult(new { success = false, message = "Error al obtener las actividades económicas" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting actividades economicas for empresa {EmpresaId}", empresaId);
            return new JsonResult(new { success = false, message = "Error al obtener las actividades económicas" });
        }
    }

    public async Task<IActionResult> OnPostAddActividadEconomicaAsync([FromBody] AddActividadEconomicaDTO request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsJsonAsync($"/api/empresas/{request.EmpresaId}/actividades-economicas", new
            {
                codigoActividad = request.CodigoActividad,
                esPrincipal = request.EsPrincipal
            });

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
                return new JsonResult(new { success = true, data, message = "Actividad económica agregada" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding actividad economica");
            return new JsonResult(new { success = false, message = "Error al agregar la actividad económica" });
        }
    }

    public async Task<IActionResult> OnPostDeleteActividadEconomicaAsync(Guid empresaId, int actividadId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/empresas/{empresaId}/actividades-economicas/{actividadId}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Actividad económica eliminada" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting actividad economica {ActividadId} from empresa {EmpresaId}", actividadId, empresaId);
            return new JsonResult(new { success = false, message = "Error al eliminar la actividad económica" });
        }
    }

    public async Task<IActionResult> OnPostSetActividadPrincipalAsync(Guid empresaId, int actividadId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PutAsync($"/api/empresas/{empresaId}/actividades-economicas/{actividadId}/principal", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Actividad económica establecida como principal" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting actividad economica {ActividadId} as principal for empresa {EmpresaId}", actividadId, empresaId);
            return new JsonResult(new { success = false, message = "Error al establecer la actividad como principal" });
        }
    }

    public async Task<IActionResult> OnGetBuscarActividadesAsync(string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return new JsonResult(new { success = false, message = "Ingrese al menos 2 caracteres para buscar" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Buscar en catálogo local por código o descripción
            var response = await client.GetAsync($"/api/actividadeseconomicas/buscar-local?q={Uri.EscapeDataString(q)}&top=20");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
                return new JsonResult(new { success = true, data });
            }

            _logger.LogWarning("Error al buscar actividades: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, message = "Error al buscar actividades económicas" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching actividades economicas");
            return new JsonResult(new { success = false, message = "Error al buscar actividades económicas" });
        }
    }

    // ========================================
    // PRIVATE HELPER METHODS
    // ========================================

    private async Task LoadSelectListsAsync()
    {
        try
        {
            // Load Tipos de Identificación
            TiposIdentificacion = Enum.GetValues(typeof(TipoIdentificacion))
                .Cast<TipoIdentificacion>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = ObtenerNombreTipoIdentificacion(t)
                }).ToList();

            // Load Provincias from catalog API
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/catalogos/provincias");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                if (result.TryGetProperty("data", out var dataProperty))
                {
                    var provincias = dataProperty.EnumerateArray();
                    Provincias = provincias.Select(p => new SelectListItem
                    {
                        Value = p.GetProperty("codigo").GetInt32().ToString(),
                        Text = p.GetProperty("nombre").GetString() ?? ""
                    }).ToList();
                }
            }
            else
            {
                _logger.LogWarning("Failed to load provincias: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading select lists");
        }
    }

    private string ObtenerNombreTipoIdentificacion(TipoIdentificacion tipo)
    {
        return tipo switch
        {
            TipoIdentificacion.Fisica => "Física",
            TipoIdentificacion.Juridica => "Jurídica",
            TipoIdentificacion.DIMEX => "DIMEX",
            TipoIdentificacion.NITE => "NITE",
            TipoIdentificacion.Pasaporte => "Pasaporte",
            TipoIdentificacion.Extranjera => "Extranjera",
            _ => tipo.ToString()
        };
    }

    private string ObtenerNombreProvincia(int codigo)
    {
        return codigo switch
        {
            1 => "San José",
            2 => "Alajuela",
            3 => "Cartago",
            4 => "Heredia",
            5 => "Guanacaste",
            6 => "Puntarenas",
            7 => "Limón",
            _ => "N/A"
        };
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class AddActividadEconomicaDTO
{
    public Guid EmpresaId { get; set; }
    public string CodigoActividad { get; set; } = null!;
    public bool EsPrincipal { get; set; }
}
