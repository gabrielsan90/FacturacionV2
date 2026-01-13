using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
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
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
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

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new JsonResult(new List<Sucursal>());
            }

            var response = await client.GetAsync($"/api/Sucursales/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<Sucursal>>(_jsonOptions);
                return new JsonResult(data ?? new List<Sucursal>());
            }

            return new JsonResult(new List<Sucursal>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sucursales");
            return new JsonResult(new List<Sucursal>());
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
            if (string.IsNullOrWhiteSpace(sucursalId) || !Guid.TryParse(sucursalId, out var sucursalGuid))
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var productosResponse = await client.GetAsync($"/api/Productos/empresa/{empresaId}");
            var inventariosResponse = await client.GetAsync($"/api/Inventarios/empresa/{empresaId}");

            if (!productosResponse.IsSuccessStatusCode || !inventariosResponse.IsSuccessStatusCode)
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var productos = await productosResponse.Content.ReadFromJsonAsync<List<Producto>>(_jsonOptions)
                ?? new List<Producto>();
            var inventarios = await inventariosResponse.Content.ReadFromJsonAsync<List<Inventario>>(_jsonOptions)
                ?? new List<Inventario>();

            var inventarioSucursal = inventarios
                .Where(i => i.SucursalId == sucursalGuid)
                .ToDictionary(i => i.ProductoId, i => i);

            var data = productos
                .Where(p => p.ControlarInventario)
                .Select(p =>
                {
                    inventarioSucursal.TryGetValue(p.Id, out var inventario);
                    return new
                    {
                        id = p.Id,
                        codigo = p.Codigo,
                        nombre = p.Nombre,
                        categoria = p.Categoria != null ? new { nombre = p.Categoria.Nombre } : null,
                        precioVenta = p.PrecioVenta,
                        asignado = inventario != null,
                        stockActual = inventario?.CantidadActual ?? 0
                    };
                })
                .ToList();

            return new JsonResult(new { data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading productos disponibles");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    public async Task<IActionResult> OnGetProductosAsignadosAsync(string sucursalId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sucursalId) || !Guid.TryParse(sucursalId, out var sucursalGuid))
            {
                return new JsonResult(new List<object>());
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new JsonResult(new List<object>());
            }

            var response = await client.GetAsync($"/api/Inventarios/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var inventarios = await response.Content.ReadFromJsonAsync<List<Inventario>>(_jsonOptions)
                    ?? new List<Inventario>();
                var data = inventarios
                    .Where(i => i.SucursalId == sucursalGuid)
                    .Select(i => new
                    {
                        id = i.Id,
                        producto = new { nombre = i.Producto?.Nombre ?? "" },
                        stock = i.CantidadActual,
                        stockMinimo = i.Producto?.StockMinimo
                    })
                    .ToList();
                return new JsonResult(data);
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading productos asignados");
            return new JsonResult(new List<object>());
        }
    }

    public async Task<IActionResult> OnPostAsignarAsync([FromBody] AsignacionProductosRequest asignacionData)
    {
        try
        {
            if (asignacionData == null || asignacionData.Productos == null || asignacionData.Productos.Count == 0)
            {
                return new JsonResult(new { success = false, message = "No hay productos para asignar" });
            }

            if (asignacionData.SucursalId == Guid.Empty)
            {
                return new JsonResult(new { success = false, message = "Sucursal invalida" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var errores = new List<string>();

            foreach (var producto in asignacionData.Productos)
            {
                if (producto.ProductoId == Guid.Empty)
                {
                    continue;
                }

                var request = new InventarioCreateRequest
                {
                    ProductoId = producto.ProductoId,
                    SucursalId = asignacionData.SucursalId,
                    CantidadActual = producto.CantidadInicial
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/Inventarios", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    errores.Add($"{producto.ProductoId}: {error}");
                }
            }

            if (errores.Count > 0)
            {
                return new JsonResult(new { success = false, message = string.Join(" | ", errores) });
            }

            return new JsonResult(new { success = true, message = "Productos asignados exitosamente" });
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
            if (string.IsNullOrWhiteSpace(asignacionId))
            {
                return new JsonResult(new { success = false, message = "Asignacion invalida" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/Inventarios/{asignacionId}");

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

    public sealed class AsignacionProductosRequest
    {
        public Guid SucursalId { get; set; }
        public List<AsignacionProductoItem> Productos { get; set; } = new();
    }

    public sealed class AsignacionProductoItem
    {
        public Guid ProductoId { get; set; }
        public decimal CantidadInicial { get; set; }
    }

    private sealed class InventarioCreateRequest
    {
        public Guid ProductoId { get; set; }
        public Guid SucursalId { get; set; }
        public decimal CantidadActual { get; set; }
    }
}
