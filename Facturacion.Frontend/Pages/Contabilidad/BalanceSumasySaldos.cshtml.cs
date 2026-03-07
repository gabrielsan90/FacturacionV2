using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize]
public class BalanceSumasySaldosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BalanceSumasySaldosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public BalanceSumasySaldosModel(IHttpClientFactory httpClientFactory, ILogger<BalanceSumasySaldosModel> logger)
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

    public async Task<IActionResult> OnGetGenerarAsync(DateTime fechaDesde, DateTime fechaHasta, int? nivel, bool incluirSinMovimiento = false)
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var query = $"empresaId={empresaId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}&incluirSinMovimiento={incluirSinMovimiento}";
            if (nivel.HasValue) query += $"&nivel={nivel}";

            var response = await client.GetAsync($"/api/reportescontables/balance-sumas-saldos?{query}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return BadRequest("Error al generar el reporte.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar balance de sumas y saldos");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetExportarExcelAsync(DateTime fechaDesde, DateTime fechaHasta, int? nivel, bool incluirSinMovimiento = false)
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var query = $"empresaId={empresaId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}&incluirSinMovimiento={incluirSinMovimiento}";
            if (nivel.HasValue) query += $"&nivel={nivel}";

            var response = await client.GetAsync($"/api/reportescontables/balance-sumas-saldos?{query}");
            if (!response.IsSuccessStatusCode) return BadRequest("Error al obtener datos.");

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Sumas y Saldos");

            ws.Cell(1, 1).Value = "BALANCE DE SUMAS Y SALDOS";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var empresa = data.TryGetProperty("empresa", out var emp) ? emp.GetString() : "";
            var periodo = data.TryGetProperty("periodo", out var per) ? per.GetString() : "";
            ws.Cell(2, 1).Value = $"{empresa} | {periodo}";
            ws.Range(2, 1, 2, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 4;
            var headers = new[] { "Código", "Cuenta", "Sumas Debe", "Sumas Haber", "Saldos Debe", "Saldos Haber" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
                ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }
            row++;

            if (data.TryGetProperty("cuentas", out var cuentas))
            {
                foreach (var c in cuentas.EnumerateArray())
                {
                    ws.Cell(row, 1).Value = c.GetProperty("codigo").GetString();
                    ws.Cell(row, 2).Value = c.GetProperty("nombre").GetString();
                    ws.Cell(row, 3).Value = c.GetProperty("sumasDebe").GetDecimal();
                    ws.Cell(row, 4).Value = c.GetProperty("sumasHaber").GetDecimal();
                    ws.Cell(row, 5).Value = c.GetProperty("saldoDebe").GetDecimal();
                    ws.Cell(row, 6).Value = c.GetProperty("saldoHaber").GetDecimal();
                    for (int i = 3; i <= 6; i++)
                        ws.Cell(row, i).Style.NumberFormat.Format = "#,##0.00";
                    if (c.GetProperty("nivel").GetInt32() <= 2)
                        ws.Range(row, 1, row, 6).Style.Font.Bold = true;
                    row++;
                }
            }

            ws.Cell(row, 2).Value = "TOTALES";
            ws.Cell(row, 3).Value = data.GetProperty("totalSumasDebe").GetDecimal();
            ws.Cell(row, 4).Value = data.GetProperty("totalSumasHaber").GetDecimal();
            ws.Cell(row, 5).Value = data.GetProperty("totalSaldosDebe").GetDecimal();
            ws.Cell(row, 6).Value = data.GetProperty("totalSaldosHaber").GetDecimal();
            for (int i = 3; i <= 6; i++)
                ws.Cell(row, i).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 1, row, 6).Style.Font.Bold = true;
            ws.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.LightYellow;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Sumas_y_Saldos_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar balance de sumas y saldos");
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
