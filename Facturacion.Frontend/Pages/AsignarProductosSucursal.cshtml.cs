using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class AsignarProductosSucursalModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AsignarProductosSucursalModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AsignarProductosSucursalModel(IHttpClientFactory httpClientFactory, ILogger<AsignarProductosSucursalModel> logger)
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

    public async Task<IActionResult> OnGetSucursalesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/sucursales");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(data ?? new List<dynamic>());
            }

            return new JsonResult(new List<dynamic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sucursales");
            return new JsonResult(new List<dynamic>());
        }
    }

    public async Task<IActionResult> OnGetCategoriasAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/categorias");

            if (response.IsSuccessStatusCode)
            {
                var categorias = await response.Content.ReadFromJsonAsync<List<Categoria>>(_jsonOptions);
                return new JsonResult(categorias ?? new List<Categoria>());
            }

            return new JsonResult(new List<Categoria>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorias");
            return new JsonResult(new List<Categoria>());
        }
    }

    public async Task<IActionResult> OnGetProductosDisponiblesAsync(string sucursalId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/inventario/productos-disponibles?sucursalId={sucursalId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = data ?? new List<dynamic>() });
            }

            return new JsonResult(new { data = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading productos disponibles");
            return new JsonResult(new { data = new List<dynamic>() });
        }
    }

    public async Task<IActionResult> OnGetProductosAsignadosAsync(string sucursalId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/inventario/productos-asignados?sucursalId={sucursalId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(data ?? new List<dynamic>());
            }

            return new JsonResult(new List<dynamic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading productos asignados");
            return new JsonResult(new List<dynamic>());
        }
    }

    public async Task<IActionResult> OnPostAsignarAsync([FromBody] dynamic asignacionData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(asignacionData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/inventario/asignar-productos", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Productos asignados exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error asignando productos");
            return new JsonResult(new { success = false, message = "Error al asignar productos" });
        }
    }

    public async Task<IActionResult> OnPostDesasignarAsync(string asignacionId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/inventario/desasignar-producto/{asignacionId}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Producto desasignado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error desasignando producto");
            return new JsonResult(new { success = false, message = "Error al desasignar producto" });
        }
    }
}
