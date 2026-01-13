using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ReporteGastosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReporteGastosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteGastosModel(IHttpClientFactory httpClientFactory, ILogger<ReporteGastosModel> logger)
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

    public async Task<IActionResult> OnGetDataAsync(string? fechaInicio, string? fechaFin, string? categoriaGasto)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var (inicio, fin) = ResolveFechaRange(fechaInicio, fechaFin);
            var categoriaId = ParseCategoriaId(categoriaGasto);
            var client = CreateClient();

            var reporte = await GetReporteGastosAsync(client, empresaId, inicio, fin, categoriaId);
            if (reporte == null)
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var data = reporte.Detalles.Select(d => new
            {
                fecha = d.FechaGasto,
                tipoDocumento = "Gasto",
                numero = d.NumeroDocumento,
                proveedorNombre = string.IsNullOrWhiteSpace(d.Proveedor) ? "Sin proveedor" : d.Proveedor,
                categoria = d.Categoria,
                monto = d.Total,
                descripcion = d.Descripcion
            }).ToList();

            return new JsonResult(new { data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reporte gastos");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    public async Task<IActionResult> OnGetResumenAsync(string? fechaInicio, string? fechaFin, string? categoriaGasto)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { totalGastos = 0m, totalDocumentos = 0, promedioGasto = 0m, totalProveedores = 0 });
            }

            var (inicio, fin) = ResolveFechaRange(fechaInicio, fechaFin);
            var categoriaId = ParseCategoriaId(categoriaGasto);
            var client = CreateClient();

            var reporte = await GetReporteGastosAsync(client, empresaId, inicio, fin, categoriaId);
            if (reporte == null)
            {
                return new JsonResult(new { totalGastos = 0m, totalDocumentos = 0, promedioGasto = 0m, totalProveedores = 0 });
            }

            var promedio = reporte.CantidadGastos > 0
                ? reporte.TotalGastos / reporte.CantidadGastos
                : 0m;
            var totalProveedores = reporte.Detalles
                .Select(d => d.Proveedor ?? string.Empty)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return new JsonResult(new
            {
                totalGastos = reporte.TotalGastos,
                totalDocumentos = reporte.CantidadGastos,
                promedioGasto = promedio,
                totalProveedores
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resumen gastos");
            return new JsonResult(new { totalGastos = 0m, totalDocumentos = 0, promedioGasto = 0m, totalProveedores = 0 });
        }
    }

    public async Task<IActionResult> OnGetGraficosAsync(string? fechaInicio, string? fechaFin, string? categoriaGasto)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { meses = new { labels = new List<string>(), valores = new List<decimal>() }, categorias = new List<object>() });
            }

            var (inicio, fin) = ResolveFechaRange(fechaInicio, fechaFin);
            var categoriaId = ParseCategoriaId(categoriaGasto);
            var client = CreateClient();

            var reporte = await GetReporteGastosAsync(client, empresaId, inicio, fin, categoriaId);
            if (reporte == null)
            {
                return new JsonResult(new { meses = new { labels = new List<string>(), valores = new List<decimal>() }, categorias = new List<object>() });
            }

            var meses = reporte.Detalles
                .GroupBy(d => d.FechaGasto.ToString("yyyy-MM", CultureInfo.InvariantCulture))
                .OrderBy(g => g.Key)
                .Select(g => new { Label = g.Key, Total = g.Sum(x => x.Total) })
                .ToList();

            var categorias = reporte.Detalles
                .GroupBy(d => d.Categoria ?? string.Empty)
                .Select(g => new { nombre = g.Key, monto = g.Sum(x => x.Total) })
                .OrderByDescending(g => g.monto)
                .ToList();

            return new JsonResult(new
            {
                meses = new
                {
                    labels = meses.Select(m => m.Label).ToList(),
                    valores = meses.Select(m => m.Total).ToList()
                },
                categorias
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading graficos gastos");
            return new JsonResult(new { meses = new { labels = new List<string>(), valores = new List<decimal>() }, categorias = new List<object>() });
        }
    }

    public async Task<IActionResult> OnGetTopProveedoresAsync(string? fechaInicio, string? fechaFin, string? categoriaGasto)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new List<object>());
            }

            var (inicio, fin) = ResolveFechaRange(fechaInicio, fechaFin);
            var categoriaId = ParseCategoriaId(categoriaGasto);
            var client = CreateClient();

            var reporte = await GetReporteGastosAsync(client, empresaId, inicio, fin, categoriaId);
            if (reporte == null)
            {
                return new JsonResult(new List<object>());
            }

            var data = reporte.Detalles
                .GroupBy(d => string.IsNullOrWhiteSpace(d.Proveedor) ? "Sin proveedor" : d.Proveedor)
                .Select(g => new
                {
                    nombre = g.Key,
                    cantidadDocumentos = g.Count(),
                    montoTotal = g.Sum(x => x.Total),
                    promedio = g.Average(x => x.Total)
                })
                .OrderByDescending(g => g.montoTotal)
                .Take(10)
                .ToList();

            return new JsonResult(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading top proveedores");
            return new JsonResult(new List<object>());
        }
    }

    public async Task<IActionResult> OnGetCategoriasGastoAsync()
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync("/api/CategoriasGasto/activas");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<CategoriaGasto>>(_jsonOptions);
                return new JsonResult(data ?? new List<CategoriaGasto>());
            }

            _logger.LogWarning("Failed to load categorias gasto. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorias gasto");
            return new JsonResult(new List<object>());
        }
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

    private async Task<ReporteGastosDTO?> GetReporteGastosAsync(HttpClient client, Guid empresaId, DateTime inicio, DateTime fin, int? categoriaId)
    {
        var url = $"/api/Reportes/gastos?empresaId={empresaId}&fechaInicio={FormatDate(inicio)}&fechaFin={FormatDate(fin)}";
        if (categoriaId.HasValue)
        {
            url += $"&categoriaId={categoriaId.Value}";
        }

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to load reporte gastos. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ReporteGastosDTO>(_jsonOptions);
    }

    private static int? ParseCategoriaId(string? categoriaGasto)
    {
        return int.TryParse(categoriaGasto, out var id) && id > 0 ? id : null;
    }

    private static (DateTime Inicio, DateTime Fin) ResolveFechaRange(string? fechaInicio, string? fechaFin)
    {
        if (!DateTime.TryParse(fechaInicio, out var inicio) || !DateTime.TryParse(fechaFin, out var fin))
        {
            fin = DateTime.Today;
            inicio = fin.AddDays(-30);
        }

        return (inicio, fin);
    }

    private static string FormatDate(DateTime fecha)
    {
        return fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private bool TryGetEmpresaId(out Guid empresaId)
    {
        empresaId = Guid.Empty;
        var empresaIdValue = User.FindFirstValue("EmpresaId");
        return Guid.TryParse(empresaIdValue, out empresaId);
    }
}
