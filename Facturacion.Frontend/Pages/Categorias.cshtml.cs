using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class CategoriasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CategoriasModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Categoria Categoria { get; set; } = new();

    public CategoriasModel(IHttpClientFactory httpClientFactory, ILogger<CategoriasModel> logger)
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

    // Handler for DataTable - Load all categorias
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

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new JsonResult(new List<Categoria>());
            }

            var response = await client.GetAsync($"/api/Categorias/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var categorias = await response.Content.ReadFromJsonAsync<List<Categoria>>(_jsonOptions);
                return new JsonResult(categorias ?? new List<Categoria>());
            }

            _logger.LogWarning("Failed to load categorias. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<Categoria>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorias");
            return new JsonResult(new List<Categoria>());
        }
    }

    // Handler to get a single categoria by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/categorias/{id}");

            if (response.IsSuccessStatusCode)
            {
                var categoria = await response.Content.ReadFromJsonAsync<Categoria>(_jsonOptions);
                return new JsonResult(categoria);
            }

            _logger.LogWarning("Categoria not found with ID: {Id}", id);
            return new JsonResult(new { error = "Categoría no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categoria with ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar la categoría" });
        }
    }

    // Handler to save (create or update) a categoria
    public async Task<IActionResult> OnPostSaveAsync([FromBody] Categoria categoriaData)
    {
        try
        {
            // Note: Using [FromBody] to receive JSON data from JavaScript
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

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId) || !Guid.TryParse(empresaId, out var empresaGuid))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
            }

            if (categoriaData.EmpresaId == Guid.Empty)
            {
                categoriaData.EmpresaId = empresaGuid;
            }

            var json = JsonSerializer.Serialize(categoriaData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = categoriaData.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/categorias", content);
            }
            else
            {
                response = await client.PutAsync($"/api/categorias/{categoriaData.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Categoria {Action} successfully. ID: {Id}", isNew ? "created" : "updated", categoriaData.Id);
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Categoría creada exitosamente" : "Categoría actualizada exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save categoria. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving categoria");
            return new JsonResult(new { success = false, message = "Error al guardar la categoría" });
        }
    }

    // Handler to delete a categoria
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/categorias/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Categoria deleted successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Categoría eliminada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete categoria with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting categoria with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar la categoría" });
        }
    }
}
