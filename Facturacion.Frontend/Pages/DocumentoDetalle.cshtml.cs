using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize]
public class DocumentoDetalleModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DocumentoDetalleModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DocumentoDetalleModel(IHttpClientFactory httpClientFactory, ILogger<DocumentoDetalleModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    }

    public string DocumentoId { get; set; } = "";

    public void OnGet(string id)
    {
        DocumentoId = id ?? "";
    }

    /// <summary>
    /// Get documento details
    /// </summary>
    public async Task<IActionResult> OnGetDetailsAsync(string id)
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
            else
            {
                _logger.LogWarning("No token found in user claims for documento details");
                return new JsonResult(new { success = false, message = "No autorizado" });
            }

            var response = await client.GetAsync($"/api/Documentos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var documento = JsonSerializer.Deserialize<object>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = documento });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to load documento {Id}. Status: {StatusCode}, Error: {Error}",
                id, response.StatusCode, errorMessage);

            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading documento {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar documento" });
        }
    }

    /// <summary>
    /// Delete documento
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(string id)
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
            else
            {
                _logger.LogWarning("No token found in user claims for documento delete");
                return new JsonResult(new { success = false, message = "No autorizado" });
            }

            var response = await client.DeleteAsync($"/api/Documentos/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Documento eliminado correctamente" });
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete documento {Id}. Status: {StatusCode}, Error: {Error}",
                id, response.StatusCode, errorMessage);

            return new JsonResult(new { success = false, message = errorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting documento {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar documento" });
        }
    }
}
