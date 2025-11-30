using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Entities.Catalogos;
using System.Text;
using System.Text.Json;
using System.Security.Claims;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ProductosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProductosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public Producto Producto { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public ProductosModel(IHttpClientFactory httpClientFactory, ILogger<ProductosModel> logger)
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
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    // Handler for DataTable - Load all productos for the current empresa
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { data = new List<Producto>() });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            // Get JWT token from user claims
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/productos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var productos = await response.Content.ReadFromJsonAsync<List<Producto>>(_jsonOptions);
                return new JsonResult(new { data = productos ?? new List<Producto>() });
            }

            _logger.LogWarning("Failed to load productos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<Producto>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading productos");
            return new JsonResult(new { data = new List<Producto>() });
        }
    }

    // Handler to get a single producto by ID
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

            var response = await client.GetAsync($"/api/productos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var producto = await response.Content.ReadFromJsonAsync<Producto>(_jsonOptions);
                return new JsonResult(producto);
            }

            _logger.LogWarning("Producto not found. ID: {Id}, Status code: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { error = "Producto/Servicio no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading producto details. ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar el producto/servicio" });
        }
    }

    // Handler to save (create or update) a producto
    public async Task<IActionResult> OnPostSaveAsync([FromBody] Producto productoData)
    {
        // Nota: No validar ModelState aquí porque incluye errores del BindProperty vacío.
        // La validación real la hace el API backend.

        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(productoData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = productoData.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/productos", content);
            }
            else
            {
                response = await client.PutAsync($"/api/productos/{productoData.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Producto/Servicio creado exitosamente" : "Producto/Servicio actualizado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save producto. Status code: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving producto");
            return new JsonResult(new { success = false, message = "Error al guardar el producto/servicio" });
        }
    }

    // Handler to delete a producto
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

            var response = await client.DeleteAsync($"/api/productos/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Producto/Servicio eliminado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete producto. ID: {Id}, Status code: {StatusCode}, Error: {Error}", id, response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting producto. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el producto/servicio" });
        }
    }

    // Handler to get categorias (for dropdown)
    public async Task<IActionResult> OnGetCategoriasAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new List<Categoria>());
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/categorias/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var categorias = await response.Content.ReadFromJsonAsync<List<Categoria>>(_jsonOptions);
                return new JsonResult(categorias ?? new List<Categoria>());
            }

            _logger.LogWarning("Failed to load categorias. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<Categoria>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorias");
            return new JsonResult(new List<Categoria>());
        }
    }

    // Handler to get unidades de medida (for dropdown)
    public async Task<IActionResult> OnGetUnidadesMedidaAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/catalogos/unidades-medida");

            if (response.IsSuccessStatusCode)
            {
                var unidadesMedida = await response.Content.ReadFromJsonAsync<List<UnidadMedida>>(_jsonOptions);
                return new JsonResult(unidadesMedida ?? new List<UnidadMedida>());
            }

            _logger.LogWarning("Failed to load unidades de medida. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<UnidadMedida>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading unidades de medida");
            return new JsonResult(new List<UnidadMedida>());
        }
    }

    // Handler to get impuestos (for dropdown)
    public async Task<IActionResult> OnGetImpuestosAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/catalogos/impuestos");

            if (response.IsSuccessStatusCode)
            {
                var impuestos = await response.Content.ReadFromJsonAsync<List<Impuesto>>(_jsonOptions);
                return new JsonResult(impuestos ?? new List<Impuesto>());
            }

            _logger.LogWarning("Failed to load impuestos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<Impuesto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading impuestos");
            return new JsonResult(new List<Impuesto>());
        }
    }
}
