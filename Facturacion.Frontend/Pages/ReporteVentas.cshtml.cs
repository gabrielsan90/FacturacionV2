using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.DTOs;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ReporteVentasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReporteVentasModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteVentasModel(IHttpClientFactory httpClientFactory, ILogger<ReporteVentasModel> logger)
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

    public async Task<IActionResult> OnGetResumenAsync(string? fechaInicio, string? fechaFin)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                _logger.LogWarning("EmpresaId not found in user claims for ReporteVentas resumen");
                return new JsonResult(new { totalVentas = 0m, totalDocumentos = 0, promedioVenta = 0m, clientesUnicos = 0 });
            }

            var (inicio, fin) = ResolveFechaRange(fechaInicio, fechaFin);
            _logger.LogInformation("ReporteVentas Resumen: EmpresaId={EmpresaId}, Inicio={Inicio}, Fin={Fin}", empresaId, inicio, fin);

            var client = CreateClient();

            var reporte = await GetReporteVentasAsync(client, empresaId, inicio, fin);
            if (reporte == null)
            {
                _logger.LogWarning("ReporteVentas returned null");
                return new JsonResult(new { totalVentas = 0m, totalDocumentos = 0, promedioVenta = 0m, clientesUnicos = 0 });
            }

            _logger.LogInformation("ReporteVentas data: TotalVentas={TotalVentas}, CantidadDocumentos={CantidadDocumentos}, Detalles={DetallesCount}",
                reporte.TotalVentas, reporte.CantidadDocumentos, reporte.Detalles?.Count ?? 0);

            var promedio = reporte.CantidadDocumentos > 0
                ? reporte.TotalVentas / reporte.CantidadDocumentos
                : 0m;
            var clientesUnicos = reporte.Detalles
                .Select(d => d.Cliente ?? string.Empty)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return new JsonResult(new
            {
                totalVentas = reporte.TotalVentas,
                totalDocumentos = reporte.CantidadDocumentos,
                promedioVenta = promedio,
                clientesUnicos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resumen ventas");
            return new JsonResult(new { totalVentas = 0m, totalDocumentos = 0, promedioVenta = 0m, clientesUnicos = 0 });
        }
    }

    public async Task<IActionResult> OnGetGraficoVentasAsync(string? fechaInicio, string? fechaFin, string? agruparPor)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { labels = new List<string>(), valores = new List<decimal>(), topProductos = new List<object>() });
            }

            var (inicio, fin) = ResolveFechaRange(fechaInicio, fechaFin);
            var client = CreateClient();

            var reporteVentas = await GetReporteVentasAsync(client, empresaId, inicio, fin);
            if (reporteVentas == null)
            {
                return new JsonResult(new { labels = new List<string>(), valores = new List<decimal>(), topProductos = new List<object>() });
            }

            var grouped = reporteVentas.Detalles
                .GroupBy(d => GetGroupingKey(d.FechaEmision, agruparPor))
                .OrderBy(g => g.Key)
                .Select(g => new { Label = g.Key, Total = g.Sum(d => d.Total) })
                .ToList();

            var reporteProductos = await GetReporteProductosAsync(client, empresaId, inicio, fin);
            var topProductos = (reporteProductos?.Detalles ?? new List<ReporteProductosDetalleDTO>())
                .OrderByDescending(d => d.TotalVentas)
                .Take(5)
                .Select(d => new { nombre = d.NombreProducto, monto = d.TotalVentas })
                .ToList();

            return new JsonResult(new
            {
                labels = grouped.Select(g => g.Label).ToList(),
                valores = grouped.Select(g => g.Total).ToList(),
                topProductos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading grafico ventas");
            return new JsonResult(new { labels = new List<string>(), valores = new List<decimal>(), topProductos = new List<object>() });
        }
    }

    public async Task<IActionResult> OnGetTopClientesAsync(string? fechaInicio, string? fechaFin)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new List<object>());
            }

            var (inicio, fin) = ResolveFechaRange(fechaInicio, fechaFin);
            var client = CreateClient();

            var reporteClientes = await GetReporteClientesAsync(client, empresaId, inicio, fin);
            var data = (reporteClientes?.Detalles ?? new List<ReporteClientesDetalleDTO>())
                .OrderByDescending(d => d.TotalCompras)
                .Take(10)
                .Select(d => new
                {
                    nombre = d.NombreCliente,
                    cantidadVentas = d.CantidadCompras,
                    montoTotal = d.TotalCompras
                })
                .ToList();

            return new JsonResult(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading top clientes");
            return new JsonResult(new List<object>());
        }
    }

    public async Task<IActionResult> OnGetTopProductosAsync(string? fechaInicio, string? fechaFin)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new List<object>());
            }

            var (inicio, fin) = ResolveFechaRange(fechaInicio, fechaFin);
            var client = CreateClient();

            var reporteProductos = await GetReporteProductosAsync(client, empresaId, inicio, fin);
            var data = (reporteProductos?.Detalles ?? new List<ReporteProductosDetalleDTO>())
                .OrderByDescending(d => d.TotalVentas)
                .Take(10)
                .Select(d => new
                {
                    nombre = d.NombreProducto,
                    cantidad = d.CantidadVendida,
                    montoTotal = d.TotalVentas
                })
                .ToList();

            return new JsonResult(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading top productos");
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

    private async Task<ReporteVentasDTO?> GetReporteVentasAsync(HttpClient client, Guid empresaId, DateTime inicio, DateTime fin)
    {
        var url = $"/api/Reportes/ventas?empresaId={empresaId}&fechaInicio={FormatDate(inicio)}&fechaFin={FormatDate(fin)}";
        _logger.LogInformation("Calling API: {Url}", url);

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to load reporte ventas. Status code: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("API response length: {Length} chars", content.Length);

        return JsonSerializer.Deserialize<ReporteVentasDTO>(content, _jsonOptions);
    }

    private async Task<ReporteProductosDTO?> GetReporteProductosAsync(HttpClient client, Guid empresaId, DateTime inicio, DateTime fin)
    {
        var url = $"/api/Reportes/productos?empresaId={empresaId}&fechaInicio={FormatDate(inicio)}&fechaFin={FormatDate(fin)}";
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to load reporte productos. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ReporteProductosDTO>(_jsonOptions);
    }

    private async Task<ReporteClientesDTO?> GetReporteClientesAsync(HttpClient client, Guid empresaId, DateTime inicio, DateTime fin)
    {
        var url = $"/api/Reportes/clientes?empresaId={empresaId}&fechaInicio={FormatDate(inicio)}&fechaFin={FormatDate(fin)}";
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to load reporte clientes. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ReporteClientesDTO>(_jsonOptions);
    }

    private static string GetGroupingKey(DateTime fecha, string? agruparPor)
    {
        switch ((agruparPor ?? string.Empty).ToLowerInvariant())
        {
            case "dia":
                return fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            case "semana":
                var week = ISOWeek.GetWeekOfYear(fecha);
                return $"{fecha.Year}-W{week:D2}";
            case "mes":
            default:
                return fecha.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }
    }

    private static (DateTime Inicio, DateTime Fin) ResolveFechaRange(string? fechaInicio, string? fechaFin)
    {
        if (!DateTime.TryParse(fechaInicio, out var inicio) || !DateTime.TryParse(fechaFin, out var fin))
        {
            fin = DateTime.Today;
            inicio = fin.AddDays(-30);
        }

        // Add end of day time to fin to include all records from that day
        fin = fin.Date.AddDays(1).AddSeconds(-1); // 23:59:59

        return (inicio, fin);
    }

    private static string FormatDate(DateTime fecha)
    {
        // Include time component for proper date filtering
        return fecha.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    }

    private bool TryGetEmpresaId(out Guid empresaId)
    {
        empresaId = Guid.Empty;
        var empresaIdValue = User.FindFirstValue("EmpresaId");
        return Guid.TryParse(empresaIdValue, out empresaId);
    }
}
