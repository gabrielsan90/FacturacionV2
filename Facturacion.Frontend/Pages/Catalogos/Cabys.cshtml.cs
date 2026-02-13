using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities.Catalogos;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Catalogos;

[Authorize(Roles = "SuperUser,Administrador")]
public class CabysModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CabysModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CabysModel(
        IHttpClientFactory httpClientFactory,
        ILogger<CabysModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public void OnGet()
    {
    }

    // Load data for DataTable
    public async Task<IActionResult> OnGetDataAsync([FromQuery] string? activo, [FromQuery] string? categoria)
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

            var url = "/api/cabys/local?pageSize=1000";
            if (!string.IsNullOrEmpty(activo))
            {
                url += $"&activo={activo}";
            }
            if (!string.IsNullOrEmpty(categoria))
            {
                url += $"&categoria={categoria}";
            }

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                if (result.TryGetProperty("data", out var dataProperty))
                {
                    var cabys = JsonSerializer.Deserialize<List<CAByS>>(dataProperty.GetRawText(), _jsonOptions);
                    return new JsonResult(cabys ?? new List<CAByS>());
                }

                return new JsonResult(new List<CAByS>());
            }

            _logger.LogWarning("Failed to load códigos CABYS: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<CAByS>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading códigos CABYS data");
            return new JsonResult(new List<CAByS>());
        }
    }

    // Get single código CABYS for edit
    public async Task<IActionResult> OnGetDetailsAsync(int id)
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

            var response = await client.GetAsync($"/api/cabys/local/{id}");

            if (response.IsSuccessStatusCode)
            {
                var cabys = await response.Content.ReadFromJsonAsync<CAByS>(_jsonOptions);
                return new JsonResult(new { success = true, data = cabys });
            }

            _logger.LogWarning("Código CABYS not found: {Id}", id);
            return new JsonResult(new { success = false, message = "Código CABYS no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading código CABYS details: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar los detalles del código CABYS" });
        }
    }

    // Save código CABYS (Create or Update)
    public async Task<IActionResult> OnPostSaveAsync([FromBody] CAByS cabys)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(cabys.Codigo))
            {
                return new JsonResult(new { success = false, message = "El código CABYS es obligatorio" });
            }

            if (cabys.Codigo.Length != 13)
            {
                return new JsonResult(new { success = false, message = "El código CABYS debe tener exactamente 13 dígitos" });
            }

            if (string.IsNullOrWhiteSpace(cabys.Descripcion))
            {
                return new JsonResult(new { success = false, message = "La descripción es obligatoria" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(cabys, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = cabys.Id == 0;

            if (isNew)
            {
                response = await client.PostAsync("/api/cabys/local", content);
            }
            else
            {
                response = await client.PutAsync($"/api/cabys/local/{cabys.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Código CABYS {Action}: {Id} - {Codigo}",
                    isNew ? "created" : "updated",
                    cabys.Id,
                    cabys.Codigo);

                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Código CABYS creado exitosamente" : "Código CABYS actualizado exitosamente"
                });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save código CABYS. Error: {Error}", errorContent);

            // Try to parse error message
            try
            {
                var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (errorObj.TryGetProperty("message", out var messageProperty))
                {
                    return new JsonResult(new { success = false, message = messageProperty.GetString() });
                }
            }
            catch
            {
                // If can't parse, return raw error
            }

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving código CABYS");
            return new JsonResult(new { success = false, message = "Error al guardar el código CABYS" });
        }
    }

    // Delete código CABYS
    public async Task<IActionResult> OnPostDeleteAsync(int id)
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

            var response = await client.DeleteAsync($"/api/cabys/local/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Código CABYS deleted successfully: {Id}", id);
                return new JsonResult(new { success = true, message = "Código CABYS eliminado exitosamente" });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete código CABYS {Id}: {Error}", id, errorContent);

            // Try to parse error message
            try
            {
                var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (errorObj.TryGetProperty("message", out var messageProperty))
                {
                    return new JsonResult(new { success = false, message = messageProperty.GetString() });
                }
            }
            catch
            {
                // If can't parse, return raw error
            }

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting código CABYS: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el código CABYS" });
        }
    }

    // Importar desde Excel
    public async Task<IActionResult> OnPostImportarExcelAsync(IFormFile archivo)
    {
        try
        {
            if (archivo == null || archivo.Length == 0)
            {
                return new JsonResult(new { success = false, message = "Debe proporcionar un archivo válido" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            client.Timeout = TimeSpan.FromMinutes(10); // Aumentar timeout para importaciones grandes

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Create multipart form data
            using var formData = new MultipartFormDataContent();
            using var fileStream = archivo.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(archivo.ContentType);
            formData.Add(streamContent, "archivo", archivo.FileName);

            var response = await client.PostAsync("/api/cabys/local/importar-excel", formData);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                _logger.LogInformation("Excel import completed successfully");
                return new JsonResult(JsonSerializer.Deserialize<object>(content, _jsonOptions));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to import Excel: {Error}", errorContent);

            // Try to parse error message
            try
            {
                var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (errorObj.TryGetProperty("message", out var messageProperty))
                {
                    return new JsonResult(new { success = false, message = messageProperty.GetString() });
                }
            }
            catch
            {
                // If can't parse, return raw error
            }

            return new JsonResult(new { success = false, message = errorContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing Excel file");
            return new JsonResult(new { success = false, message = $"Error al importar el archivo: {ex.Message}" });
        }
    }

    // Descargar template de Excel
    public async Task<IActionResult> OnGetDescargarTemplateAsync()
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

            var response = await client.GetAsync("/api/cabys/local/exportar-template");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                _logger.LogInformation("Template downloaded successfully");

                return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Template_Codigos_CABYS.xlsx");
            }

            _logger.LogWarning("Failed to download template: {StatusCode}", response.StatusCode);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading template");
            return StatusCode(500);
        }
    }
}
