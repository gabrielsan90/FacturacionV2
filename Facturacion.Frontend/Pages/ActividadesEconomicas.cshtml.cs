using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador")]
public class ActividadesEconomicasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ActividadesEconomicasModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ActividadesEconomicasModel(
        IHttpClientFactory httpClientFactory,
        ILogger<ActividadesEconomicasModel> logger)
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
    public async Task<IActionResult> OnGetDataAsync([FromQuery] string? activa)
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

            var url = "/api/actividadeseconomicas?pageSize=1000";
            if (!string.IsNullOrEmpty(activa))
            {
                url += $"&activa={activa}";
            }

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                if (result.TryGetProperty("data", out var dataProperty))
                {
                    var actividades = JsonSerializer.Deserialize<List<ActividadEconomica>>(dataProperty.GetRawText(), _jsonOptions);
                    return new JsonResult(actividades ?? new List<ActividadEconomica>());
                }

                return new JsonResult(new List<ActividadEconomica>());
            }

            _logger.LogWarning("Failed to load actividades económicas: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<ActividadEconomica>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading actividades económicas data");
            return new JsonResult(new List<ActividadEconomica>());
        }
    }

    // Get single actividad económica for edit
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

            var response = await client.GetAsync($"/api/actividadeseconomicas/id/{id}");

            if (response.IsSuccessStatusCode)
            {
                var actividad = await response.Content.ReadFromJsonAsync<ActividadEconomica>(_jsonOptions);
                return new JsonResult(new { success = true, data = actividad });
            }

            _logger.LogWarning("Actividad económica not found: {Id}", id);
            return new JsonResult(new { success = false, message = "Actividad económica no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading actividad económica details: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar los detalles de la actividad económica" });
        }
    }

    // Save actividad económica (Create or Update)
    public async Task<IActionResult> OnPostSaveAsync([FromBody] ActividadEconomica actividad)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(actividad.CodigoCIIU4))
            {
                return new JsonResult(new { success = false, message = "El código CIIU4 es obligatorio" });
            }

            if (string.IsNullOrWhiteSpace(actividad.Descripcion))
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

            var json = JsonSerializer.Serialize(actividad, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = actividad.Id == 0;

            if (isNew)
            {
                response = await client.PostAsync("/api/actividadeseconomicas", content);
            }
            else
            {
                response = await client.PutAsync($"/api/actividadeseconomicas/{actividad.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Actividad económica {Action}: {Id} - {Codigo}",
                    isNew ? "created" : "updated",
                    actividad.Id,
                    actividad.CodigoCIIU4);

                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Actividad económica creada exitosamente" : "Actividad económica actualizada exitosamente"
                });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save actividad económica. Error: {Error}", errorContent);

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
            _logger.LogError(ex, "Error saving actividad económica");
            return new JsonResult(new { success = false, message = "Error al guardar la actividad económica" });
        }
    }

    // Delete actividad económica
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

            var response = await client.DeleteAsync($"/api/actividadeseconomicas/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Actividad económica deleted successfully: {Id}", id);
                return new JsonResult(new { success = true, message = "Actividad económica eliminada exitosamente" });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete actividad económica {Id}: {Error}", id, errorContent);

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
            _logger.LogError(ex, "Error deleting actividad económica: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar la actividad económica" });
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

            var response = await client.PostAsync("/api/actividadeseconomicas/importar-excel", formData);

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

            var response = await client.GetAsync("/api/actividadeseconomicas/exportar-template");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                _logger.LogInformation("Template downloaded successfully");

                return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Template_Actividades_Economicas.xlsx");
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
