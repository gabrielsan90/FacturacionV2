using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize]
public class LibroDiarioModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LibroDiarioModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public LibroDiarioModel(IHttpClientFactory httpClientFactory, ILogger<LibroDiarioModel> logger)
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

            var response = await client.GetAsync($"/api/reportescontables/libro-diario?empresaId={empresaId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return BadRequest("Error al generar el reporte.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar libro diario");
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

            var response = await client.GetAsync($"/api/reportescontables/libro-diario?empresaId={empresaId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}");
            if (!response.IsSuccessStatusCode) return BadRequest("Error al obtener datos.");

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Libro Diario");

            ws.Cell(1, 1).Value = "LIBRO DIARIO";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var empresa = data.TryGetProperty("empresa", out var emp) ? emp.GetString() : "";
            var periodo = data.TryGetProperty("periodo", out var per) ? per.GetString() : "";
            ws.Cell(2, 1).Value = $"{empresa} | {periodo}";
            ws.Range(2, 1, 2, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 4;

            if (data.TryGetProperty("asientos", out var asientos))
            {
                foreach (var asiento in asientos.EnumerateArray())
                {
                    var numero = asiento.GetProperty("numero").GetInt32();
                    var fecha = asiento.GetProperty("fecha").GetDateTime();
                    var concepto = asiento.TryGetProperty("concepto", out var con) ? con.GetString() : "";
                    var tipoDesc = asiento.TryGetProperty("tipoAsientoDescripcion", out var td) ? td.GetString() : "";

                    ws.Cell(row, 1).Value = $"Asiento #{numero} | {fecha:dd/MM/yyyy} | {tipoDesc} | {concepto}";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                    ws.Range(row, 1, row, 6).Merge();
                    row++;

                    ws.Cell(row, 1).Value = "Cuenta";
                    ws.Cell(row, 2).Value = "Nombre";
                    ws.Cell(row, 3).Value = "Descripción";
                    ws.Cell(row, 4).Value = "Debe";
                    ws.Cell(row, 5).Value = "Haber";
                    ws.Range(row, 1, row, 5).Style.Font.Bold = true;
                    ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightGray;
                    row++;

                    if (asiento.TryGetProperty("movimientos", out var movimientos))
                    {
                        foreach (var mov in movimientos.EnumerateArray())
                        {
                            ws.Cell(row, 1).Value = mov.TryGetProperty("cuentaCodigo", out var cc) ? cc.GetString() : "";
                            ws.Cell(row, 2).Value = mov.TryGetProperty("cuentaNombre", out var cn) ? cn.GetString() : "";
                            ws.Cell(row, 3).Value = mov.TryGetProperty("descripcion", out var desc) ? desc.GetString() : "";
                            ws.Cell(row, 4).Value = mov.GetProperty("debe").GetDecimal();
                            ws.Cell(row, 5).Value = mov.GetProperty("haber").GetDecimal();
                            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                            row++;
                        }
                    }

                    ws.Cell(row, 3).Value = "Total Asiento:";
                    ws.Cell(row, 3).Style.Font.Bold = true;
                    ws.Cell(row, 4).Value = asiento.GetProperty("totalDebe").GetDecimal();
                    ws.Cell(row, 5).Value = asiento.GetProperty("totalHaber").GetDecimal();
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                    ws.Range(row, 3, row, 5).Style.Font.Bold = true;
                    row += 2;
                }
            }

            ws.Cell(row, 3).Value = "TOTAL GENERAL:";
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 4).Value = data.GetProperty("totalDebe").GetDecimal();
            ws.Cell(row, 5).Value = data.GetProperty("totalHaber").GetDecimal();
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightYellow;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Libro_Diario_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar libro diario");
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
