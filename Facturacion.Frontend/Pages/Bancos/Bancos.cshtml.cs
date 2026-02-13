using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Bancos;

[Authorize(Roles = "SuperUser,Administrador de Empresa")]
public class BancosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BancosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Banco Banco { get; set; } = new();

    public BancosModel(IHttpClientFactory httpClientFactory, ILogger<BancosModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public string EmpresaId { get; set; } = "";

    public void OnGet()
    {
        EmpresaId = User.FindFirst("EmpresaId")?.Value ?? "";
    }

    // Handler for DataTable - Load bancos filtered by empresa
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var empresaId = User.FindFirst("EmpresaId")?.Value ?? "";
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/bancos/empresa/{empresaId}/todos");

            if (response.IsSuccessStatusCode)
            {
                var bancos = await response.Content.ReadFromJsonAsync<List<Banco>>(_jsonOptions);
                return new JsonResult(new { data = bancos ?? new List<Banco>() });
            }

            _logger.LogWarning("Failed to load bancos. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<Banco>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bancos");
            return new JsonResult(new { data = new List<Banco>() });
        }
    }

    // Handler to get a single banco by ID
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

            var response = await client.GetAsync($"/api/bancos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var banco = await response.Content.ReadFromJsonAsync<Banco>(_jsonOptions);
                return new JsonResult(new { success = true, data = banco });
            }

            _logger.LogWarning("Banco not found with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Banco no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading banco with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar el banco" });
        }
    }

    // Handler to save (create or update) a banco
    public async Task<IActionResult> OnPostSaveAsync([FromBody] Banco bancoData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(bancoData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = bancoData.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/bancos", content);
            }
            else
            {
                response = await client.PutAsync($"/api/bancos/{bancoData.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Banco {Action} successfully. ID: {Id}", isNew ? "created" : "updated", bancoData.Id);
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Banco creado exitosamente" : "Banco actualizado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save banco. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving banco");
            return new JsonResult(new { success = false, message = "Error al guardar el banco" });
        }
    }

    // Handler to delete a banco
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

            var response = await client.DeleteAsync($"/api/bancos/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Banco deleted successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Banco eliminado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete banco with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting banco with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el banco" });
        }
    }

    // Handler to generate predefined catalog
    public async Task<IActionResult> OnPostGenerarCatalogoAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirst("EmpresaId")?.Value ?? "";
            var response = await client.PostAsync($"/api/bancos/empresa/{empresaId}/generar-catalogo", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<object>(result, _jsonOptions);
                return new JsonResult(data);
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating banco catalog");
            return new JsonResult(new { success = false, message = "Error al generar el catálogo de bancos" });
        }
    }
}
