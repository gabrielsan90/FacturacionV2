using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class SucursalesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SucursalesModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Sucursal Sucursal { get; set; } = new();

    public SucursalesModel(IHttpClientFactory httpClientFactory, ILogger<SucursalesModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public void OnGet()
    {
        // Page initialization
    }

    // Handler for DataTable - Load all sucursales
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

            var response = await client.GetAsync("/api/sucursales");

            if (response.IsSuccessStatusCode)
            {
                var sucursales = await response.Content.ReadFromJsonAsync<List<Sucursal>>(_jsonOptions);
                return new JsonResult(new { data = sucursales ?? new List<Sucursal>() });
            }

            _logger.LogWarning("Failed to load sucursales. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<Sucursal>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sucursales");
            return new JsonResult(new { data = new List<Sucursal>() });
        }
    }

    // Handler to get a single sucursal by ID
    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/sucursales/{id}");

            if (response.IsSuccessStatusCode)
            {
                var sucursal = await response.Content.ReadFromJsonAsync<Sucursal>(_jsonOptions);
                return new JsonResult(new { success = true, data = sucursal });
            }

            _logger.LogWarning("Sucursal not found with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Sucursal no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sucursal {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar la sucursal" });
        }
    }

    // Handler to save (create or update) a sucursal
    public async Task<IActionResult> OnPostSaveAsync([FromBody] Sucursal sucursalData)
    {
        try
        {
            // If not SuperUser, set EmpresaId from user claims
            if (!User.IsInRole("SuperUser"))
            {
                var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
                if (Guid.TryParse(empresaIdClaim, out Guid empresaId))
                {
                    sucursalData.EmpresaId = empresaId;
                }
                else
                {
                    return new JsonResult(new { success = false, message = "No se pudo obtener la empresa del usuario" });
                }
            }

            // Validate required fields manually since ModelState might not be populated correctly with [FromBody]
            if (string.IsNullOrWhiteSpace(sucursalData.Codigo))
            {
                return new JsonResult(new { success = false, message = "El código es obligatorio" });
            }

            if (string.IsNullOrWhiteSpace(sucursalData.Nombre))
            {
                return new JsonResult(new { success = false, message = "El nombre es obligatorio" });
            }

            if (sucursalData.EmpresaId == Guid.Empty)
            {
                return new JsonResult(new { success = false, message = "Debe seleccionar una empresa" });
            }

            // Validate email format if provided
            if (!string.IsNullOrWhiteSpace(sucursalData.Email) && !IsValidEmail(sucursalData.Email))
            {
                return new JsonResult(new { success = false, message = "El email no tiene un formato válido" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(sucursalData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = sucursalData.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/sucursales", content);
            }
            else
            {
                response = await client.PutAsync($"/api/sucursales/{sucursalData.Id}", content);
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

    // Handler to delete a sucursal
    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
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

    // Handler to get empresas (for SuperUser only)
    public async Task<IActionResult> OnGetEmpresasAsync()
    {
        try
        {
            // Only SuperUser can access this
            if (!User.IsInRole("SuperUser"))
            {
                return new JsonResult(new { success = false, message = "No tiene permisos para acceder a esta funcionalidad" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/empresas");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<object>(content, _jsonOptions);
                return new JsonResult(data);
            }

            _logger.LogWarning("Failed to load empresas. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading empresas");
            return new JsonResult(new List<object>());
        }
    }

    // Helper method to validate email format
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
