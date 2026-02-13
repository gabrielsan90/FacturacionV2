using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Reportes;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ReporteProductosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReporteProductosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteProductosModel(IHttpClientFactory httpClientFactory, ILogger<ReporteProductosModel> logger)
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

    public async Task<IActionResult> OnGetDataAsync(string? categoriaId, string? estado, string? stockBajo)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var items = await LoadReporteItemsAsync(empresaId);
            var filtered = ApplyFilters(items, categoriaId, estado, stockBajo);

            var data = filtered.Select(p => new
            {
                codigo = p.Codigo,
                nombre = p.Nombre,
                categoria = p.CategoriaNombre != null ? new { nombre = p.CategoriaNombre } : null,
                precioVenta = p.PrecioVenta,
                stockActual = p.StockActual,
                stockMinimo = p.StockMinimo,
                valorStock = p.ValorStock,
                activo = p.Activo
            }).ToList();

            return new JsonResult(new { data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reporte productos");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    public async Task<IActionResult> OnGetResumenAsync(string? categoriaId, string? estado, string? stockBajo)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { totalProductos = 0, totalActivos = 0, totalStockBajo = 0, valorInventario = 0m });
            }

            var items = await LoadReporteItemsAsync(empresaId);
            var filtered = ApplyFilters(items, categoriaId, estado, stockBajo);

            var totalProductos = filtered.Count;
            var totalActivos = filtered.Count(p => p.Activo);
            var totalStockBajo = filtered.Count(p => p.StockMinimo.HasValue && p.StockActual <= p.StockMinimo.Value);
            var valorInventario = filtered.Sum(p => p.ValorStock);

            return new JsonResult(new { totalProductos, totalActivos, totalStockBajo, valorInventario });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resumen");
            return new JsonResult(new { totalProductos = 0, totalActivos = 0, totalStockBajo = 0, valorInventario = 0m });
        }
    }

    public async Task<IActionResult> OnGetCategoriasAsync()
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new List<Categoria>());
            }

            var client = CreateClient();
            var response = await client.GetAsync($"/api/Categorias/empresa/{empresaId}");

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

    private async Task<List<ProductoReporteItem>> LoadReporteItemsAsync(Guid empresaId)
    {
        var client = CreateClient();

        var productosResponse = await client.GetAsync($"/api/Productos/empresa/{empresaId}");
        var inventariosResponse = await client.GetAsync($"/api/Inventarios/empresa/{empresaId}");

        if (!productosResponse.IsSuccessStatusCode || !inventariosResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to load productos/inventarios for reporte productos");
            return new List<ProductoReporteItem>();
        }

        var productos = await productosResponse.Content.ReadFromJsonAsync<List<Producto>>(_jsonOptions)
            ?? new List<Producto>();
        var inventarios = await inventariosResponse.Content.ReadFromJsonAsync<List<Inventario>>(_jsonOptions)
            ?? new List<Inventario>();

        var stockPorProducto = inventarios
            .GroupBy(i => i.ProductoId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.CantidadActual));

        return productos.Select(p =>
        {
            stockPorProducto.TryGetValue(p.Id, out var stockActual);
            return new ProductoReporteItem
            {
                Id = p.Id,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria?.Nombre,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                PrecioVenta = p.PrecioVenta,
                StockActual = stockActual,
                StockMinimo = p.StockMinimo,
                Activo = p.Activo,
                ValorStock = stockActual * p.PrecioVenta
            };
        }).ToList();
    }

    private static List<ProductoReporteItem> ApplyFilters(
        List<ProductoReporteItem> items,
        string? categoriaId,
        string? estado,
        string? stockBajo)
    {
        var result = items.AsEnumerable();

        if (Guid.TryParse(categoriaId, out var categoriaGuid))
        {
            result = result.Where(p => p.CategoriaId == categoriaGuid);
        }

        if (bool.TryParse(estado, out var activo))
        {
            result = result.Where(p => p.Activo == activo);
        }

        if (string.Equals(stockBajo, "true", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Where(p => p.StockMinimo.HasValue && p.StockActual <= p.StockMinimo.Value);
        }

        return result.ToList();
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private bool TryGetEmpresaId(out Guid empresaId)
    {
        empresaId = Guid.Empty;
        var empresaIdValue = User.FindFirstValue("EmpresaId");
        return Guid.TryParse(empresaIdValue, out empresaId);
    }

    private sealed class ProductoReporteItem
    {
        public Guid Id { get; set; }
        public Guid? CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public decimal StockActual { get; set; }
        public decimal? StockMinimo { get; set; }
        public bool Activo { get; set; }
        public decimal ValorStock { get; set; }
    }
}
