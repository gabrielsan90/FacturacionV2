using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize]
public class BalanzaComprobacionModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BalanzaComprobacionModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public BalanzaComprobacionModel(IHttpClientFactory httpClientFactory, ILogger<BalanzaComprobacionModel> logger)
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

            var response = await client.GetAsync($"/api/reportescontables/balanza-comprobacion?{query}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return BadRequest("Error al generar el reporte.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar balanza de comprobación");
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

            var response = await client.GetAsync($"/api/reportescontables/balanza-comprobacion?{query}");
            if (!response.IsSuccessStatusCode) return BadRequest("Error al obtener datos.");

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<BalanzaData>(content, _jsonOptions);
            if (data == null) return BadRequest("No hay datos.");

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Balanza");

            ws.Cell(1, 1).Value = "BALANZA DE COMPROBACIÓN";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 8).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(2, 1).Value = $"{data.Empresa} | Período: {data.Periodo}";
            ws.Range(2, 1, 2, 8).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 4;
            var headers = new[] { "Código", "Cuenta", "Saldo Ant. Debe", "Saldo Ant. Haber", "Mov. Debe", "Mov. Haber", "Saldo Final Debe", "Saldo Final Haber" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
                ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }
            row++;

            if (data.Cuentas != null)
            {
                foreach (var c in data.Cuentas)
                {
                    ws.Cell(row, 1).Value = c.Codigo;
                    ws.Cell(row, 2).Value = c.Nombre;
                    ws.Cell(row, 3).Value = c.SaldoAnteriorDebe;
                    ws.Cell(row, 4).Value = c.SaldoAnteriorHaber;
                    ws.Cell(row, 5).Value = c.MovimientosDebe;
                    ws.Cell(row, 6).Value = c.MovimientosHaber;
                    ws.Cell(row, 7).Value = c.SaldoFinalDebe;
                    ws.Cell(row, 8).Value = c.SaldoFinalHaber;
                    for (int i = 3; i <= 8; i++)
                        ws.Cell(row, i).Style.NumberFormat.Format = "#,##0.00";
                    if (c.Nivel <= 2)
                        ws.Range(row, 1, row, 8).Style.Font.Bold = true;
                    row++;
                }
            }

            // Totals
            ws.Cell(row, 2).Value = "TOTALES";
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = data.TotalSaldoAnteriorDebe;
            ws.Cell(row, 4).Value = data.TotalSaldoAnteriorHaber;
            ws.Cell(row, 5).Value = data.TotalMovimientosDebe;
            ws.Cell(row, 6).Value = data.TotalMovimientosHaber;
            ws.Cell(row, 7).Value = data.TotalSaldoFinalDebe;
            ws.Cell(row, 8).Value = data.TotalSaldoFinalHaber;
            for (int i = 3; i <= 8; i++)
                ws.Cell(row, i).Style.NumberFormat.Format = "#,##0.00";
            ws.Range(row, 1, row, 8).Style.Font.Bold = true;
            ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.LightYellow;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Balanza_Comprobacion_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar balanza");
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
        var empresaIdClaim = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaIdClaim)) return null;
        return Guid.Parse(empresaIdClaim);
    }

    // Internal DTOs for deserialization
    private class BalanzaData
    {
        public string Empresa { get; set; } = "";
        public string Periodo { get; set; } = "";
        public decimal TotalSaldoAnteriorDebe { get; set; }
        public decimal TotalSaldoAnteriorHaber { get; set; }
        public decimal TotalMovimientosDebe { get; set; }
        public decimal TotalMovimientosHaber { get; set; }
        public decimal TotalSaldoFinalDebe { get; set; }
        public decimal TotalSaldoFinalHaber { get; set; }
        public List<CuentaData>? Cuentas { get; set; }
    }

    private class CuentaData
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public int Nivel { get; set; }
        public decimal SaldoAnteriorDebe { get; set; }
        public decimal SaldoAnteriorHaber { get; set; }
        public decimal MovimientosDebe { get; set; }
        public decimal MovimientosHaber { get; set; }
        public decimal SaldoFinalDebe { get; set; }
        public decimal SaldoFinalHaber { get; set; }
    }
}
