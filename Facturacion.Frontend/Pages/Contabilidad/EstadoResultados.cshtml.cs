using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize]
public class EstadoResultadosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EstadoResultadosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public EstadoResultadosModel(IHttpClientFactory httpClientFactory, ILogger<EstadoResultadosModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public void OnGet() { }

    public async Task<IActionResult> OnGetGenerarAsync(DateTime fechaDesde, DateTime fechaHasta)
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"/api/reportescontables/estado-resultados?empresaId={empresaId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return BadRequest("Error al generar el reporte.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar estado de resultados");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetExportarExcelAsync(DateTime fechaDesde, DateTime fechaHasta)
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"/api/reportescontables/estado-resultados?empresaId={empresaId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}");
            if (!response.IsSuccessStatusCode) return BadRequest("Error al obtener datos.");

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Estado de Resultados");

            ws.Cell(1, 1).Value = "ESTADO DE RESULTADOS";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 3).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var empresa = data.TryGetProperty("empresa", out var emp) ? emp.GetString() : "";
            var periodo = data.TryGetProperty("periodo", out var per) ? per.GetString() : "";
            ws.Cell(2, 1).Value = $"{empresa} | {periodo}";
            ws.Range(2, 1, 2, 3).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 4;

            void WriteSection(string titulo, string propName, XLColor color)
            {
                if (!data.TryGetProperty(propName, out var seccion)) return;
                ws.Cell(row, 1).Value = titulo;
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Fill.BackgroundColor = color;
                ws.Range(row, 1, row, 3).Merge();
                row++;

                if (seccion.TryGetProperty("cuentas", out var cuentas))
                {
                    foreach (var c in cuentas.EnumerateArray())
                    {
                        ws.Cell(row, 1).Value = c.GetProperty("codigo").GetString();
                        ws.Cell(row, 2).Value = c.GetProperty("nombre").GetString();
                        ws.Cell(row, 3).Value = c.GetProperty("saldo").GetDecimal();
                        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                        if (c.GetProperty("nivel").GetInt32() <= 2) ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                        row++;
                    }
                }

                ws.Cell(row, 2).Value = $"Total {titulo}";
                ws.Cell(row, 2).Style.Font.Bold = true;
                ws.Cell(row, 3).Value = seccion.GetProperty("total").GetDecimal();
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 3).Style.Font.Bold = true;
                ws.Range(row, 1, row, 3).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                row += 2;
            }

            WriteSection("INGRESOS", "ingresos", XLColor.LightGreen);
            WriteSection("COSTO DE VENTAS", "costosVentas", XLColor.LightSalmon);

            ws.Cell(row, 2).Value = "UTILIDAD BRUTA";
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = data.GetProperty("utilidadBruta").GetDecimal();
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.LightYellow;
            row += 2;

            WriteSection("GASTOS DE OPERACIÓN", "gastos", XLColor.LightCoral);

            ws.Cell(row, 2).Value = "UTILIDAD NETA";
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = data.GetProperty("utilidadNeta").GetDecimal();
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.LightGreen;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Estado_Resultados_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar estado de resultados");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private Guid? GetEmpresaId()
    {
        var claim = User.FindFirstValue("EmpresaId");
        return string.IsNullOrEmpty(claim) ? null : Guid.Parse(claim);
    }
}
