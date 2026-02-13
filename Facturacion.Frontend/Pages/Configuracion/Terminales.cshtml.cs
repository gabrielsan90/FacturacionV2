using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Configuracion;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class TerminalesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TerminalesModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Terminal Terminal { get; set; } = new();

    public TerminalesModel(IHttpClientFactory httpClientFactory, ILogger<TerminalesModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    }

    public void OnGet()
    {
        // Page load
    }

    /// <summary>
    /// Handler to load sucursales for dropdown
    /// </summary>
    public async Task<IActionResult> OnGetSucursalesAsync()
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

            var response = await client.GetAsync("/api/sucursales");

            if (response.IsSuccessStatusCode)
            {
                var sucursales = await response.Content.ReadFromJsonAsync<List<Sucursal>>(_jsonOptions);
                return new JsonResult(new { success = true, data = sucursales });
            }

            _logger.LogWarning("Failed to load sucursales. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { success = false, data = new List<Sucursal>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sucursales");
            return new JsonResult(new { success = false, message = "Error al cargar sucursales", data = new List<Sucursal>() });
        }
    }

    /// <summary>
    /// Handler to load terminales by sucursal for DataTable
    /// </summary>
    public async Task<IActionResult> OnGetDataAsync(Guid sucursalId)
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

            var response = await client.GetAsync($"/api/terminales/sucursal/{sucursalId}");

            if (response.IsSuccessStatusCode)
            {
                var terminales = await response.Content.ReadFromJsonAsync<List<Terminal>>(_jsonOptions);
                return new JsonResult(new { data = terminales });
            }

            _logger.LogWarning("Failed to load terminales for sucursal {SucursalId}. Status: {StatusCode}", sucursalId, response.StatusCode);
            return new JsonResult(new { data = new List<Terminal>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading terminales for sucursal {SucursalId}", sucursalId);
            return new JsonResult(new { data = new List<Terminal>() });
        }
    }

    /// <summary>
    /// Handler to get single terminal for editing
    /// </summary>
    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
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

            var response = await client.GetAsync($"/api/terminales/{id}");

            if (response.IsSuccessStatusCode)
            {
                var terminal = await response.Content.ReadFromJsonAsync<Terminal>(_jsonOptions);
                return new JsonResult(new { success = true, data = terminal });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to load terminal {Id}. Status: {StatusCode}, Error: {Error}", id, response.StatusCode, errorContent);
            return new JsonResult(new { success = false, message = "Terminal no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading terminal {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar el terminal" });
        }
    }

    /// <summary>
    /// Handler to create or update terminal
    /// </summary>
    public async Task<IActionResult> OnPostSaveAsync([FromBody] Terminal terminal)
    {
        // Validate ModelState
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
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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

    /// <summary>
    /// Handler to delete terminal
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
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
}
