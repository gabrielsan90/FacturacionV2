using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize]
public class MayoresContablesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MayoresContablesModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MayoresContablesModel(IHttpClientFactory httpClientFactory, ILogger<MayoresContablesModel> logger)
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
    }

    public async Task<IActionResult> OnGetCuentasAsync()
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/cuentascontables/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cuentas = JsonSerializer.Deserialize<List<CuentaSelectDTO>>(content, _jsonOptions);

                // Filtrar solo cuentas activas que aceptan movimientos
                var cuentasFiltradas = cuentas?
                    .Where(c => c.Activo && c.AceptaMovimientos)
                    .Select(c => new
                    {
                        id = c.Id,
                        text = $"{c.Codigo} - {c.Nombre}"
                    })
                    .OrderBy(c => c.text)
                    .ToList();

                if (cuentasFiltradas == null)
                    return new JsonResult(new List<object>());
                return new JsonResult(cuentasFiltradas);
            }

            return BadRequest("Error al obtener las cuentas.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar cuentas para select");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetPeriodosAsync()
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/periodoscontables/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var periodos = JsonSerializer.Deserialize<List<PeriodoSelectDTO>>(content, _jsonOptions);

                // Filtrar solo periodos activos
                var periodosFiltrados = periodos?
                    .Where(p => p.Activo)
                    .Select(p => new
                    {
                        id = p.Id,
                        nombre = p.Nombre,
                        fechaInicio = p.FechaInicio,
                        fechaFin = p.FechaFin
                    })
                    .OrderByDescending(p => p.fechaInicio)
                    .ToList();

                if (periodosFiltrados == null)
                    return new JsonResult(new List<object>());
                return new JsonResult(periodosFiltrados);
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar períodos contables");
            return new JsonResult(new List<object>());
        }
    }

    public async Task<IActionResult> OnPostGenerarAsync([FromBody] FiltroMayorDTO filtro)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos inválidos");
            }

            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            // Construir query string
            var queryParams = new List<string>
            {
                $"empresaId={empresaId}",
                $"fechaDesde={filtro.FechaDesde:yyyy-MM-dd}",
                $"fechaHasta={filtro.FechaHasta:yyyy-MM-dd}"
            };

            if (filtro.CuentaId.HasValue && filtro.CuentaId != Guid.Empty)
            {
                queryParams.Add($"cuentaId={filtro.CuentaId}");
            }

            var queryString = string.Join("&", queryParams);
            var response = await client.GetAsync($"api/mayorescontables?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<ResumenMayorDTO>(content, _jsonOptions);

                return new JsonResult(new
                {
                    success = true,
                    data = resultado
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de mayores contables");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnGetMovimientosAsync(Guid cuentaId, DateTime fechaDesde, DateTime fechaHasta)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var queryString = $"empresaId={empresaId}&cuentaId={cuentaId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}";
            var response = await client.GetAsync($"api/mayorescontables/movimientos?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var movimientos = JsonSerializer.Deserialize<List<MovimientoMayorDTO>>(content, _jsonOptions);

                return new JsonResult(new
                {
                    success = true,
                    data = movimientos
                });
            }

            return BadRequest("Error al obtener movimientos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar movimientos de cuenta");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostExportarExcelAsync([FromBody] FiltroMayorDTO filtro)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            // Obtener datos del reporte
            var queryParams = new List<string>
            {
                $"empresaId={empresaId}",
                $"fechaDesde={filtro.FechaDesde:yyyy-MM-dd}",
                $"fechaHasta={filtro.FechaHasta:yyyy-MM-dd}"
            };

            if (filtro.CuentaId.HasValue && filtro.CuentaId != Guid.Empty)
            {
                queryParams.Add($"cuentaId={filtro.CuentaId}");
            }

            var queryString = string.Join("&", queryParams);
            var response = await client.GetAsync($"api/mayorescontables?{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Error al obtener datos del reporte");
            }

            var content = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ResumenMayorDTO>(content, _jsonOptions);

            if (resultado == null || resultado.Mayores == null)
            {
                return BadRequest("No hay datos para exportar");
            }

            // Generar archivo Excel
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Mayores Contables");

            // Título del reporte
            worksheet.Cell(1, 1).Value = "LIBRO MAYOR";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 7).Merge();
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Periodo
            worksheet.Cell(2, 1).Value = $"Período: {filtro.FechaDesde:dd/MM/yyyy} al {filtro.FechaHasta:dd/MM/yyyy}";
            worksheet.Range(2, 1, 2, 7).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int currentRow = 4;

            // Resumen general
            worksheet.Cell(currentRow, 1).Value = "RESUMEN GENERAL";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Range(currentRow, 1, currentRow, 7).Merge();
            currentRow++;

            worksheet.Cell(currentRow, 1).Value = "Total Débitos:";
            worksheet.Cell(currentRow, 2).Value = resultado.TotalDebitos;
            worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
            currentRow++;

            worksheet.Cell(currentRow, 1).Value = "Total Créditos:";
            worksheet.Cell(currentRow, 2).Value = resultado.TotalCreditos;
            worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
            currentRow++;

            worksheet.Cell(currentRow, 1).Value = "Saldo Total:";
            worksheet.Cell(currentRow, 2).Value = resultado.SaldoTotal;
            worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
            currentRow += 2;

            // Detalle por cuenta
            foreach (var mayor in resultado.Mayores.OrderBy(m => m.CuentaCodigo))
            {
                // Encabezado de cuenta
                worksheet.Cell(currentRow, 1).Value = $"Cuenta: {mayor.CuentaCodigo} - {mayor.CuentaNombre}";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                worksheet.Range(currentRow, 1, currentRow, 7).Merge();
                currentRow++;

                // Información de la cuenta
                worksheet.Cell(currentRow, 1).Value = "Saldo Anterior";
                worksheet.Cell(currentRow, 2).Value = "Débitos";
                worksheet.Cell(currentRow, 3).Value = "Créditos";
                worksheet.Cell(currentRow, 4).Value = "Saldo Final";
                worksheet.Range(currentRow, 1, currentRow, 4).Style.Font.Bold = true;
                worksheet.Range(currentRow, 1, currentRow, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = mayor.SaldoAnterior;
                worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(currentRow, 2).Value = mayor.TotalDebe;
                worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(currentRow, 3).Value = mayor.TotalHaber;
                worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(currentRow, 4).Value = mayor.SaldoFinal;
                worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(currentRow, 4).Style.Font.Bold = true;
                currentRow += 2;

                // Movimientos de la cuenta (si existen)
                if (mayor.Movimientos != null && mayor.Movimientos.Any())
                {
                    worksheet.Cell(currentRow, 1).Value = "Fecha";
                    worksheet.Cell(currentRow, 2).Value = "Asiento";
                    worksheet.Cell(currentRow, 3).Value = "Concepto";
                    worksheet.Cell(currentRow, 4).Value = "Referencia";
                    worksheet.Cell(currentRow, 5).Value = "Debe";
                    worksheet.Cell(currentRow, 6).Value = "Haber";
                    worksheet.Cell(currentRow, 7).Value = "Saldo";
                    worksheet.Range(currentRow, 1, currentRow, 7).Style.Font.Bold = true;
                    worksheet.Range(currentRow, 1, currentRow, 7).Style.Fill.BackgroundColor = XLColor.AliceBlue;
                    currentRow++;

                    foreach (var mov in mayor.Movimientos)
                    {
                        worksheet.Cell(currentRow, 1).Value = mov.Fecha.ToString("dd/MM/yyyy");
                        worksheet.Cell(currentRow, 2).Value = mov.AsientoNumero;
                        worksheet.Cell(currentRow, 3).Value = mov.Concepto ?? "";
                        worksheet.Cell(currentRow, 4).Value = mov.Referencia ?? "";
                        worksheet.Cell(currentRow, 5).Value = mov.Debe;
                        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(currentRow, 6).Value = mov.Haber;
                        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(currentRow, 7).Value = mov.SaldoAcumulado;
                        worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";
                        currentRow++;
                    }

                    currentRow++;
                }

                currentRow++;
            }

            // Ajustar ancho de columnas
            worksheet.Columns().AdjustToContents();

            // Generar archivo
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Mayores_Contables_{filtro.FechaDesde:yyyyMMdd}_{filtro.FechaHasta:yyyyMMdd}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar a Excel");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private Guid? GetEmpresaId()
    {
        var empresaIdClaim = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaIdClaim)) return null;
        return Guid.Parse(empresaIdClaim);
    }

    // DTOs
    public class FiltroMayorDTO
    {
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public Guid? CuentaId { get; set; }
    }

    public class MayorDTO
    {
        public Guid CuentaId { get; set; }
        public string? CuentaCodigo { get; set; }
        public string? CuentaNombre { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }
        public decimal SaldoFinal { get; set; }
        public List<MovimientoMayorDTO>? Movimientos { get; set; }
    }

    public class MovimientoMayorDTO
    {
        public Guid Id { get; set; }
        public DateTime Fecha { get; set; }
        public int AsientoNumero { get; set; }
        public string? Concepto { get; set; }
        public string? Referencia { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        public decimal SaldoAcumulado { get; set; }
    }

    public class ResumenMayorDTO
    {
        public decimal TotalDebitos { get; set; }
        public decimal TotalCreditos { get; set; }
        public decimal SaldoTotal { get; set; }
        public List<MayorDTO>? Mayores { get; set; }
    }

    public class CuentaSelectDTO
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public bool AceptaMovimientos { get; set; }
    }

    public class PeriodoSelectDTO
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; }
    }
}
