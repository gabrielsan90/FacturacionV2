using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Gastos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class CategoriasGastoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CategoriasGastoModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public CategoriaGasto CategoriaGasto { get; set; } = new();

    public CategoriasGastoModel(IHttpClientFactory httpClientFactory, ILogger<CategoriasGastoModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
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

    /// <summary>
    /// Handler for DataTable - Load all categorias de gasto
    /// </summary>
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

            // Call API with includeInactive=true to show all categories
            var response = await client.GetAsync("/api/CategoriasGasto?includeInactive=true");

            if (response.IsSuccessStatusCode)
            {
                var categorias = await response.Content.ReadFromJsonAsync<List<CategoriaGasto>>(_jsonOptions);
                return new JsonResult(new { data = categorias ?? new List<CategoriaGasto>() });
            }

            _logger.LogWarning("Failed to load categorías de gasto. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<CategoriaGasto>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorías de gasto");
            return new JsonResult(new { data = new List<CategoriaGasto>() });
        }
    }

    /// <summary>
    /// Handler to get a single categoria by ID
    /// </summary>
    public async Task<IActionResult> OnGetDetailsAsync(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/CategoriasGasto/{id}");

            if (response.IsSuccessStatusCode)
            {
                var categoria = await response.Content.ReadFromJsonAsync<CategoriaGasto>(_jsonOptions);
                return new JsonResult(new { success = true, data = categoria });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Categoría de gasto not found with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categoría de gasto with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar la categoría" });
        }
    }

    /// <summary>
    /// Handler to save (create or update) a categoria
    /// </summary>
    public async Task<IActionResult> OnPostSaveAsync([FromBody] CategoriaGasto categoriaData)
    {
        try
        {
            // Validate model
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

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(categoriaData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = categoriaData.Id == 0;

            if (isNew)
            {
                response = await client.PostAsync("/api/CategoriasGasto", content);
            }
            else
            {
                response = await client.PutAsync($"/api/CategoriasGasto/{categoriaData.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Categoría de gasto {Action} successfully. ID: {Id}",
                    isNew ? "created" : "updated", categoriaData.Id);

                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Categoría de gasto creada exitosamente" : "Categoría de gasto actualizada exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save categoría de gasto. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, error);

            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving categoría de gasto");
            return new JsonResult(new { success = false, message = "Error al guardar la categoría de gasto" });
        }
    }

    /// <summary>
    /// Handler to delete/deactivate a categoria
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/CategoriasGasto/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Categoría de gasto deleted successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Categoría de gasto desactivada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete categoría de gasto with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting categoría de gasto with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al desactivar la categoría de gasto" });
        }
    }

    /// <summary>
    /// Handler to download Excel template for expense categories
    /// </summary>
    public async Task<IActionResult> OnGetPlantillaAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/categoriasgasto/plantilla");

            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PlantillaCategoriasGasto.xlsx");
            }

            _logger.LogWarning("Failed to download template. Status: {StatusCode}", response.StatusCode);
            return BadRequest("Error al descargar la plantilla");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading template");
            return BadRequest("Error al descargar la plantilla");
        }
    }

    /// <summary>
    /// Handler to import expense categories from Excel
    /// </summary>
    public async Task<IActionResult> OnPostImportarAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new JsonResult(new { success = false, message = "No se seleccionó ningún archivo" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var response = await client.PostAsync("/api/categoriasgasto/importar", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
                return new JsonResult(result);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to import. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing categorias de gasto");
            return new JsonResult(new { success = false, message = "Error al importar las categorías de gasto" });
        }
    }
}
