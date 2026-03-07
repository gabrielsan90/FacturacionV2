using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize]
public class BalanceGeneralModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BalanceGeneralModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public BalanceGeneralModel(IHttpClientFactory httpClientFactory, ILogger<BalanceGeneralModel> logger)
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

    public async Task<IActionResult> OnGetGenerarAsync(DateTime fechaCorte)
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"/api/reportescontables/balance-general?empresaId={empresaId}&fechaCorte={fechaCorte:yyyy-MM-dd}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API returned {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
            return BadRequest("Error al generar el reporte.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar balance general");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetExportarExcelAsync(DateTime fechaCorte)
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"/api/reportescontables/balance-general?empresaId={empresaId}&fechaCorte={fechaCorte:yyyy-MM-dd}");
            if (!response.IsSuccessStatusCode) return BadRequest("Error al obtener datos.");

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Balance General");

            ws.Cell(1, 1).Value = "BALANCE GENERAL - ESTADO DE SITUACIÓN FINANCIERA";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 3).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var empresa = data.TryGetProperty("empresa", out var emp) ? emp.GetString() : "";
            ws.Cell(2, 1).Value = $"{empresa} | Al {fechaCorte:dd/MM/yyyy}";
            ws.Range(2, 1, 2, 3).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 4;

            void WriteSection(string titulo, JsonElement seccion)
            {
                ws.Cell(row, 1).Value = titulo;
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Font.FontSize = 12;
                ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                ws.Range(row, 1, row, 3).Merge();
                row++;

                if (seccion.TryGetProperty("cuentas", out var cuentas))
                {
                    foreach (var c in cuentas.EnumerateArray())
                    {
                        var nivel = c.GetProperty("nivel").GetInt32();
                        var codigo = c.GetProperty("codigo").GetString() ?? "";
                        var nombre = c.GetProperty("nombre").GetString() ?? "";
                        var saldo = c.GetProperty("saldo").GetDecimal();

                        ws.Cell(row, 1).Value = codigo;
                        ws.Cell(row, 2).Value = nombre;
                        ws.Cell(row, 3).Value = saldo;
                        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                        if (nivel <= 2) ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                        row++;
                    }
                }

                var total = seccion.GetProperty("total").GetDecimal();
                ws.Cell(row, 2).Value = $"TOTAL {titulo}";
                ws.Cell(row, 2).Style.Font.Bold = true;
                ws.Cell(row, 3).Value = total;
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 3).Style.Font.Bold = true;
                ws.Range(row, 1, row, 3).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                row += 2;
            }

            WriteSection("ACTIVOS", data.GetProperty("activos"));
            WriteSection("PASIVOS", data.GetProperty("pasivos"));
            WriteSection("CAPITAL CONTABLE", data.GetProperty("capital"));

            if (data.TryGetProperty("resultadoEjercicio", out var resultado))
            {
                ws.Cell(row, 2).Value = "Resultado del Ejercicio";
                ws.Cell(row, 3).Value = resultado.GetDecimal();
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
                ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                row++;
            }

            ws.Cell(row, 2).Value = "TOTAL PASIVO + CAPITAL";
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = data.GetProperty("totalPasivoMasCapital").GetDecimal();
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.LightYellow;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Balance_General_{fechaCorte:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar balance general");
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
