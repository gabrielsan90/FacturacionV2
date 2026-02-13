using ClosedXML.Excel;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize]
public class PlanCuentasModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public PlanCuentasModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/cuentascontables/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cuentas = JsonSerializer.Deserialize<List<CuentaContable>>(content, _jsonOptions);
                return new JsonResult(cuentas);
            }

            return BadRequest("Error al obtener las cuentas.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] CuentaContableDto cuenta)
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var cuentaEntity = new CuentaContable
            {
                Id = cuenta.Id == Guid.Empty ? Guid.NewGuid() : cuenta.Id,
                EmpresaId = empresaId.Value,
                Codigo = cuenta.Codigo,
                Nombre = cuenta.Nombre,
                TipoCuenta = cuenta.TipoCuenta,
                Naturaleza = cuenta.Naturaleza,
                CuentaPadreId = string.IsNullOrEmpty(cuenta.CuentaPadreId) ? null : Guid.Parse(cuenta.CuentaPadreId),
                Descripcion = cuenta.Descripcion,
                AceptaMovimientos = cuenta.AceptaMovimientos,
                RequiereCentroCosto = cuenta.RequiereCentroCosto,
                Activo = cuenta.Activo,
                SaldoInicial = cuenta.SaldoInicial
            };

            var json = JsonSerializer.Serialize(cuentaEntity, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (cuenta.Id == Guid.Empty)
            {
                response = await client.PostAsync("api/cuentascontables", content);
            }
            else
            {
                response = await client.PutAsync($"api/cuentascontables/{cuenta.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var response = await client.DeleteAsync($"api/cuentascontables/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    private HttpClient? CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        // Try to get token from claims first (standard approach)
        var token = User.FindFirst("Token")?.Value;

        // Fallback to cookie if not in claims
        if (string.IsNullOrEmpty(token))
        {
            Request.Cookies.TryGetValue("authToken", out token);
        }

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        return null;
    }

    private Guid? GetEmpresaId()
    {
        var empresaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (string.IsNullOrEmpty(empresaIdClaim)) return null;
        return Guid.Parse(empresaIdClaim);
    }

    // Handler para descargar plantilla Excel
    public IActionResult OnGetDescargarPlantilla()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Plan de Cuentas");

        // Encabezados
        var headers = new[] { "Codigo", "Nombre", "TipoCuenta", "Naturaleza", "CodigoPadre", "AceptaMovimientos", "RequiereCentroCosto", "SaldoInicial", "Descripcion" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        // Datos de ejemplo
        var ejemplos = new[]
        {
            new { Codigo = "1", Nombre = "ACTIVO", TipoCuenta = "ACT", Naturaleza = "D", CodigoPadre = "", AceptaMov = "No", ReqCC = "No", Saldo = "0", Desc = "Cuentas de activo" },
            new { Codigo = "1.1", Nombre = "Activo Circulante", TipoCuenta = "ACT", Naturaleza = "D", CodigoPadre = "1", AceptaMov = "No", ReqCC = "No", Saldo = "0", Desc = "" },
            new { Codigo = "1.1.01", Nombre = "Caja y Bancos", TipoCuenta = "ACT", Naturaleza = "D", CodigoPadre = "1.1", AceptaMov = "No", ReqCC = "No", Saldo = "0", Desc = "" },
            new { Codigo = "1.1.01.001", Nombre = "Caja General", TipoCuenta = "ACT", Naturaleza = "D", CodigoPadre = "1.1.01", AceptaMov = "Si", ReqCC = "No", Saldo = "0", Desc = "Caja principal" },
            new { Codigo = "2", Nombre = "PASIVO", TipoCuenta = "PAS", Naturaleza = "A", CodigoPadre = "", AceptaMov = "No", ReqCC = "No", Saldo = "0", Desc = "Cuentas de pasivo" },
            new { Codigo = "3", Nombre = "CAPITAL", TipoCuenta = "CAP", Naturaleza = "A", CodigoPadre = "", AceptaMov = "No", ReqCC = "No", Saldo = "0", Desc = "Cuentas de capital" },
            new { Codigo = "4", Nombre = "INGRESOS", TipoCuenta = "ING", Naturaleza = "A", CodigoPadre = "", AceptaMov = "No", ReqCC = "No", Saldo = "0", Desc = "Cuentas de ingresos" },
            new { Codigo = "5", Nombre = "GASTOS", TipoCuenta = "GAS", Naturaleza = "D", CodigoPadre = "", AceptaMov = "No", ReqCC = "No", Saldo = "0", Desc = "Cuentas de gastos" },
            new { Codigo = "6", Nombre = "COSTOS", TipoCuenta = "COS", Naturaleza = "D", CodigoPadre = "", AceptaMov = "No", ReqCC = "No", Saldo = "0", Desc = "Cuentas de costos" },
        };

        int row = 2;
        foreach (var ej in ejemplos)
        {
            worksheet.Cell(row, 1).Value = ej.Codigo;
            worksheet.Cell(row, 2).Value = ej.Nombre;
            worksheet.Cell(row, 3).Value = ej.TipoCuenta;
            worksheet.Cell(row, 4).Value = ej.Naturaleza;
            worksheet.Cell(row, 5).Value = ej.CodigoPadre;
            worksheet.Cell(row, 6).Value = ej.AceptaMov;
            worksheet.Cell(row, 7).Value = ej.ReqCC;
            worksheet.Cell(row, 8).Value = ej.Saldo;
            worksheet.Cell(row, 9).Value = ej.Desc;
            row++;
        }

        // Hoja de instrucciones
        var instrucciones = workbook.Worksheets.Add("Instrucciones");
        instrucciones.Cell(1, 1).Value = "INSTRUCCIONES DE USO";
        instrucciones.Cell(1, 1).Style.Font.Bold = true;
        instrucciones.Cell(1, 1).Style.Font.FontSize = 14;

        var instr = new[]
        {
            "",
            "1. Complete los datos en la hoja 'Plan de Cuentas'",
            "2. El campo Codigo es obligatorio y debe ser único",
            "3. El campo Nombre es obligatorio",
            "4. TipoCuenta: ACT (Activo), PAS (Pasivo), CAP (Capital), ING (Ingreso), GAS (Gasto), COS (Costo)",
            "5. Naturaleza: D (Deudora) o A (Acreedora)",
            "6. CodigoPadre: Código de la cuenta padre (opcional, dejar vacío para cuentas raíz)",
            "7. AceptaMovimientos: Si o No (las cuentas de detalle deben aceptar movimientos)",
            "8. RequiereCentroCosto: Si o No",
            "9. SaldoInicial: Saldo inicial de la cuenta (número decimal)",
            "10. Descripcion: Descripción opcional de la cuenta",
            "",
            "IMPORTANTE:",
            "- Las cuentas padre deben existir antes de crear las cuentas hijas",
            "- Los códigos existentes serán actualizados",
            "- Los códigos nuevos serán insertados"
        };

        for (int i = 0; i < instr.Length; i++)
        {
            instrucciones.Cell(i + 2, 1).Value = instr[i];
        }

        worksheet.Columns().AdjustToContents();
        instrucciones.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_Plan_Cuentas.xlsx");
    }

    // Handler para importar Excel
    public async Task<IActionResult> OnPostImportarExcelAsync(IFormFile file)
    {
        var errores = new List<object>();
        int insertados = 0, actualizados = 0, erroresCount = 0;

        try
        {
            if (file == null || file.Length == 0)
                return new JsonResult(new { success = false, message = "No se seleccionó ningún archivo" });

            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            // Obtener cuentas existentes para validar padres y detectar actualizaciones
            var existingResponse = await client.GetAsync($"api/cuentascontables/empresa/{empresaId}");
            var cuentasExistentes = new Dictionary<string, Guid>();
            if (existingResponse.IsSuccessStatusCode)
            {
                var content = await existingResponse.Content.ReadAsStringAsync();
                var existingCuentas = JsonSerializer.Deserialize<List<CuentaContable>>(content, _jsonOptions);
                if (existingCuentas != null)
                {
                    foreach (var c in existingCuentas)
                        cuentasExistentes[c.Codigo] = c.Id;
                }
            }

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RowsUsed().Skip(1); // Skip header

            foreach (var row in rows)
            {
                int rowNumber = row.RowNumber();
                try
                {
                    var codigo = row.Cell(1).GetString().Trim();
                    var nombre = row.Cell(2).GetString().Trim();

                    if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(nombre))
                    {
                        errores.Add(new { Fila = rowNumber, Codigo = codigo, Mensaje = "Código y Nombre son obligatorios" });
                        erroresCount++;
                        continue;
                    }

                    var tipoCuenta = row.Cell(3).GetString().Trim().ToUpper();
                    if (!new[] { "ACT", "PAS", "CAP", "ING", "GAS", "COS" }.Contains(tipoCuenta))
                        tipoCuenta = "ACT";

                    var naturaleza = row.Cell(4).GetString().Trim().ToUpper();
                    if (naturaleza != "D" && naturaleza != "A") naturaleza = "D";

                    var codigoPadre = row.Cell(5).GetString().Trim();
                    Guid? cuentaPadreId = null;
                    if (!string.IsNullOrEmpty(codigoPadre))
                    {
                        if (cuentasExistentes.TryGetValue(codigoPadre, out Guid padreId))
                            cuentaPadreId = padreId;
                    }

                    var aceptaMovStr = row.Cell(6).GetString().Trim().ToLower();
                    bool aceptaMovimientos = aceptaMovStr == "si" || aceptaMovStr == "sí" || aceptaMovStr == "true" || aceptaMovStr == "1";

                    var reqCCStr = row.Cell(7).GetString().Trim().ToLower();
                    bool requiereCentroCosto = reqCCStr == "si" || reqCCStr == "sí" || reqCCStr == "true" || reqCCStr == "1";

                    decimal saldoInicial = 0;
                    var saldoStr = row.Cell(8).GetString().Trim();
                    if (!string.IsNullOrEmpty(saldoStr)) decimal.TryParse(saldoStr, out saldoInicial);

                    var descripcion = row.Cell(9).GetString().Trim();

                    var cuenta = new CuentaContable
                    {
                        Id = Guid.NewGuid(),
                        EmpresaId = empresaId.Value,
                        Codigo = codigo,
                        Nombre = nombre,
                        TipoCuenta = tipoCuenta,
                        Naturaleza = naturaleza,
                        CuentaPadreId = cuentaPadreId,
                        AceptaMovimientos = aceptaMovimientos,
                        RequiereCentroCosto = requiereCentroCosto,
                        SaldoInicial = saldoInicial,
                        Descripcion = descripcion,
                        Activo = true
                    };

                    var json = JsonSerializer.Serialize(cuenta, _jsonOptions);
                    var contentBody = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response;
                    bool esActualizacion = cuentasExistentes.ContainsKey(codigo);

                    if (esActualizacion)
                    {
                        cuenta.Id = cuentasExistentes[codigo];
                        json = JsonSerializer.Serialize(cuenta, _jsonOptions);
                        contentBody = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await client.PutAsync($"api/cuentascontables/{cuenta.Id}", contentBody);
                    }
                    else
                    {
                        response = await client.PostAsync("api/cuentascontables", contentBody);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        if (esActualizacion)
                            actualizados++;
                        else
                        {
                            insertados++;
                            // Agregar a cuentas existentes para que las siguientes filas puedan usarla como padre
                            var createdContent = await response.Content.ReadAsStringAsync();
                            var createdCuenta = JsonSerializer.Deserialize<CuentaContable>(createdContent, _jsonOptions);
                            if (createdCuenta != null)
                                cuentasExistentes[codigo] = createdCuenta.Id;
                        }
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        errores.Add(new { Fila = rowNumber, Codigo = codigo, Mensaje = errorMsg });
                        erroresCount++;
                    }
                }
                catch (Exception ex)
                {
                    errores.Add(new { Fila = rowNumber, Codigo = "", Mensaje = ex.Message });
                    erroresCount++;
                }
            }

            var mensaje = $"Importación completada. Insertados: {insertados}, Actualizados: {actualizados}, Errores: {erroresCount}";
            return new JsonResult(new
            {
                success = erroresCount == 0,
                message = mensaje,
                insertados,
                actualizados,
                errores = erroresCount,
                detalleErrores = errores
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error al procesar el archivo: {ex.Message}" });
        }
    }

    // Handler para generar plan de cuentas estándar de Costa Rica
    public async Task<IActionResult> OnPostGenerarPlanDefectoAsync()
    {
        try
        {
            var client = CreateApiClient();
            if (client == null) return Unauthorized();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            // Verificar si ya existen cuentas
            var existingResponse = await client.GetAsync($"api/cuentascontables/empresa/{empresaId}");
            if (existingResponse.IsSuccessStatusCode)
            {
                var content = await existingResponse.Content.ReadAsStringAsync();
                var cuentas = JsonSerializer.Deserialize<List<CuentaContable>>(content, _jsonOptions);
                if (cuentas != null && cuentas.Count > 0)
                {
                    return new JsonResult(new { success = false, message = "Ya existen cuentas en el plan de cuentas. Solo se puede generar el plan cuando no hay cuentas." });
                }
            }

            // Plan de cuentas estándar para Costa Rica
            var planCuentas = GetPlanCuentasCR(empresaId.Value);
            int total = 0;

            // Crear cuentas en orden (primero los padres)
            var cuentasCreadas = new Dictionary<string, Guid>();

            foreach (var cuenta in planCuentas.OrderBy(c => c.Codigo.Length).ThenBy(c => c.Codigo))
            {
                // Asignar cuenta padre si existe
                if (!string.IsNullOrEmpty(cuenta.CodigoPadreTemp) && cuentasCreadas.TryGetValue(cuenta.CodigoPadreTemp, out var padreId))
                {
                    cuenta.CuentaPadreId = padreId;
                }

                var json = JsonSerializer.Serialize(cuenta, _jsonOptions);
                var contentBody = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/cuentascontables", contentBody);
                if (response.IsSuccessStatusCode)
                {
                    var createdContent = await response.Content.ReadAsStringAsync();
                    var createdCuenta = JsonSerializer.Deserialize<CuentaContable>(createdContent, _jsonOptions);
                    if (createdCuenta != null)
                    {
                        cuentasCreadas[cuenta.Codigo] = createdCuenta.Id;
                        total++;
                    }
                }
            }

            return new JsonResult(new
            {
                success = true,
                message = "Plan de cuentas generado exitosamente",
                total
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error al generar el plan: {ex.Message}" });
        }
    }

    private List<CuentaContableTemp> GetPlanCuentasCR(Guid empresaId)
    {
        return new List<CuentaContableTemp>
        {
            // ACTIVO
            new() { Codigo = "1", Nombre = "ACTIVO", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = false, EmpresaId = empresaId },
            new() { Codigo = "1.1", Nombre = "Activo Circulante", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "1", EmpresaId = empresaId },
            new() { Codigo = "1.1.01", Nombre = "Caja y Bancos", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "1.1", EmpresaId = empresaId },
            new() { Codigo = "1.1.01.001", Nombre = "Caja General", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.1.01", EmpresaId = empresaId },
            new() { Codigo = "1.1.01.002", Nombre = "Caja Chica", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.1.01", EmpresaId = empresaId },
            new() { Codigo = "1.1.01.003", Nombre = "Bancos Nacionales", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.1.01", EmpresaId = empresaId },
            new() { Codigo = "1.1.02", Nombre = "Cuentas por Cobrar", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "1.1", EmpresaId = empresaId },
            new() { Codigo = "1.1.02.001", Nombre = "Clientes", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.1.02", EmpresaId = empresaId },
            new() { Codigo = "1.1.02.002", Nombre = "Documentos por Cobrar", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.1.02", EmpresaId = empresaId },
            new() { Codigo = "1.1.02.003", Nombre = "Anticipos a Proveedores", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.1.02", EmpresaId = empresaId },
            new() { Codigo = "1.1.03", Nombre = "Inventarios", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "1.1", EmpresaId = empresaId },
            new() { Codigo = "1.1.03.001", Nombre = "Inventario de Mercaderías", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.1.03", EmpresaId = empresaId },
            new() { Codigo = "1.1.03.002", Nombre = "Inventario de Materias Primas", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.1.03", EmpresaId = empresaId },
            new() { Codigo = "1.1.04", Nombre = "Impuestos por Cobrar", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "1.1", EmpresaId = empresaId },
            new() { Codigo = "1.1.04.001", Nombre = "IVA Crédito Fiscal", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.1.04", EmpresaId = empresaId },
            new() { Codigo = "1.1.04.002", Nombre = "Retenciones de Renta", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.1.04", EmpresaId = empresaId },
            new() { Codigo = "1.2", Nombre = "Activo No Circulante", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "1", EmpresaId = empresaId },
            new() { Codigo = "1.2.01", Nombre = "Propiedad, Planta y Equipo", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "1.2", EmpresaId = empresaId },
            new() { Codigo = "1.2.01.001", Nombre = "Terrenos", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.2.01", EmpresaId = empresaId },
            new() { Codigo = "1.2.01.002", Nombre = "Edificios", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.2.01", EmpresaId = empresaId },
            new() { Codigo = "1.2.01.003", Nombre = "Mobiliario y Equipo", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.2.01", EmpresaId = empresaId },
            new() { Codigo = "1.2.01.004", Nombre = "Equipo de Cómputo", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.2.01", EmpresaId = empresaId },
            new() { Codigo = "1.2.01.005", Nombre = "Vehículos", TipoCuenta = "ACT", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "1.2.01", EmpresaId = empresaId },
            new() { Codigo = "1.2.02", Nombre = "Depreciación Acumulada", TipoCuenta = "ACT", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "1.2", EmpresaId = empresaId },
            new() { Codigo = "1.2.02.001", Nombre = "Depreciación Edificios", TipoCuenta = "ACT", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "1.2.02", EmpresaId = empresaId },
            new() { Codigo = "1.2.02.002", Nombre = "Depreciación Mobiliario", TipoCuenta = "ACT", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "1.2.02", EmpresaId = empresaId },
            new() { Codigo = "1.2.02.003", Nombre = "Depreciación Equipo Cómputo", TipoCuenta = "ACT", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "1.2.02", EmpresaId = empresaId },
            new() { Codigo = "1.2.02.004", Nombre = "Depreciación Vehículos", TipoCuenta = "ACT", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "1.2.02", EmpresaId = empresaId },

            // PASIVO
            new() { Codigo = "2", Nombre = "PASIVO", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = false, EmpresaId = empresaId },
            new() { Codigo = "2.1", Nombre = "Pasivo Circulante", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "2", EmpresaId = empresaId },
            new() { Codigo = "2.1.01", Nombre = "Cuentas por Pagar", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "2.1", EmpresaId = empresaId },
            new() { Codigo = "2.1.01.001", Nombre = "Proveedores", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.01", EmpresaId = empresaId },
            new() { Codigo = "2.1.01.002", Nombre = "Documentos por Pagar", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.01", EmpresaId = empresaId },
            new() { Codigo = "2.1.01.003", Nombre = "Anticipos de Clientes", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.01", EmpresaId = empresaId },
            new() { Codigo = "2.1.02", Nombre = "Impuestos por Pagar", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "2.1", EmpresaId = empresaId },
            new() { Codigo = "2.1.02.001", Nombre = "IVA por Pagar", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.02", EmpresaId = empresaId },
            new() { Codigo = "2.1.02.002", Nombre = "Retención en la Fuente", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.02", EmpresaId = empresaId },
            new() { Codigo = "2.1.02.003", Nombre = "Impuesto sobre la Renta", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.02", EmpresaId = empresaId },
            new() { Codigo = "2.1.03", Nombre = "Obligaciones Laborales", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "2.1", EmpresaId = empresaId },
            new() { Codigo = "2.1.03.001", Nombre = "Salarios por Pagar", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.03", EmpresaId = empresaId },
            new() { Codigo = "2.1.03.002", Nombre = "CCSS Patronal por Pagar", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.03", EmpresaId = empresaId },
            new() { Codigo = "2.1.03.003", Nombre = "Aguinaldo por Pagar", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.03", EmpresaId = empresaId },
            new() { Codigo = "2.1.03.004", Nombre = "Vacaciones por Pagar", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.03", EmpresaId = empresaId },
            new() { Codigo = "2.1.03.005", Nombre = "Cesantía por Pagar", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.1.03", EmpresaId = empresaId },
            new() { Codigo = "2.2", Nombre = "Pasivo No Circulante", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "2", EmpresaId = empresaId },
            new() { Codigo = "2.2.01", Nombre = "Préstamos a Largo Plazo", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "2.2", EmpresaId = empresaId },
            new() { Codigo = "2.2.01.001", Nombre = "Préstamos Bancarios LP", TipoCuenta = "PAS", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "2.2.01", EmpresaId = empresaId },

            // CAPITAL
            new() { Codigo = "3", Nombre = "CAPITAL", TipoCuenta = "CAP", Naturaleza = "A", AceptaMovimientos = false, EmpresaId = empresaId },
            new() { Codigo = "3.1", Nombre = "Capital Social", TipoCuenta = "CAP", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "3", EmpresaId = empresaId },
            new() { Codigo = "3.1.01", Nombre = "Capital Suscrito y Pagado", TipoCuenta = "CAP", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "3.1", EmpresaId = empresaId },
            new() { Codigo = "3.2", Nombre = "Reservas", TipoCuenta = "CAP", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "3", EmpresaId = empresaId },
            new() { Codigo = "3.2.01", Nombre = "Reserva Legal", TipoCuenta = "CAP", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "3.2", EmpresaId = empresaId },
            new() { Codigo = "3.3", Nombre = "Resultados", TipoCuenta = "CAP", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "3", EmpresaId = empresaId },
            new() { Codigo = "3.3.01", Nombre = "Utilidades Retenidas", TipoCuenta = "CAP", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "3.3", EmpresaId = empresaId },
            new() { Codigo = "3.3.02", Nombre = "Utilidad del Ejercicio", TipoCuenta = "CAP", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "3.3", EmpresaId = empresaId },
            new() { Codigo = "3.3.03", Nombre = "Pérdidas Acumuladas", TipoCuenta = "CAP", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "3.3", EmpresaId = empresaId },

            // INGRESOS
            new() { Codigo = "4", Nombre = "INGRESOS", TipoCuenta = "ING", Naturaleza = "A", AceptaMovimientos = false, EmpresaId = empresaId },
            new() { Codigo = "4.1", Nombre = "Ingresos Operativos", TipoCuenta = "ING", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "4", EmpresaId = empresaId },
            new() { Codigo = "4.1.01", Nombre = "Ventas", TipoCuenta = "ING", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "4.1", EmpresaId = empresaId },
            new() { Codigo = "4.1.01.001", Nombre = "Ventas de Mercaderías", TipoCuenta = "ING", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "4.1.01", EmpresaId = empresaId },
            new() { Codigo = "4.1.01.002", Nombre = "Ventas de Servicios", TipoCuenta = "ING", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "4.1.01", EmpresaId = empresaId },
            new() { Codigo = "4.1.02", Nombre = "Devoluciones y Descuentos", TipoCuenta = "ING", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "4.1", EmpresaId = empresaId },
            new() { Codigo = "4.1.02.001", Nombre = "Devoluciones sobre Ventas", TipoCuenta = "ING", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "4.1.02", EmpresaId = empresaId },
            new() { Codigo = "4.1.02.002", Nombre = "Descuentos sobre Ventas", TipoCuenta = "ING", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "4.1.02", EmpresaId = empresaId },
            new() { Codigo = "4.2", Nombre = "Otros Ingresos", TipoCuenta = "ING", Naturaleza = "A", AceptaMovimientos = false, CodigoPadreTemp = "4", EmpresaId = empresaId },
            new() { Codigo = "4.2.01", Nombre = "Ingresos Financieros", TipoCuenta = "ING", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "4.2", EmpresaId = empresaId },
            new() { Codigo = "4.2.02", Nombre = "Diferencial Cambiario Positivo", TipoCuenta = "ING", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "4.2", EmpresaId = empresaId },
            new() { Codigo = "4.2.03", Nombre = "Otros Ingresos no Operativos", TipoCuenta = "ING", Naturaleza = "A", AceptaMovimientos = true, CodigoPadreTemp = "4.2", EmpresaId = empresaId },

            // GASTOS
            new() { Codigo = "5", Nombre = "GASTOS", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = false, EmpresaId = empresaId },
            new() { Codigo = "5.1", Nombre = "Gastos de Administración", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "5", EmpresaId = empresaId },
            new() { Codigo = "5.1.01", Nombre = "Salarios Administrativos", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.1", EmpresaId = empresaId },
            new() { Codigo = "5.1.02", Nombre = "Cargas Sociales", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.1", EmpresaId = empresaId },
            new() { Codigo = "5.1.03", Nombre = "Alquileres", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.1", EmpresaId = empresaId },
            new() { Codigo = "5.1.04", Nombre = "Servicios Públicos", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.1", EmpresaId = empresaId },
            new() { Codigo = "5.1.05", Nombre = "Depreciaciones", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.1", EmpresaId = empresaId },
            new() { Codigo = "5.1.06", Nombre = "Papelería y Útiles", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.1", EmpresaId = empresaId },
            new() { Codigo = "5.1.07", Nombre = "Servicios Profesionales", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.1", EmpresaId = empresaId },
            new() { Codigo = "5.1.08", Nombre = "Seguros", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.1", EmpresaId = empresaId },
            new() { Codigo = "5.2", Nombre = "Gastos de Ventas", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "5", EmpresaId = empresaId },
            new() { Codigo = "5.2.01", Nombre = "Salarios de Vendedores", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.2", EmpresaId = empresaId },
            new() { Codigo = "5.2.02", Nombre = "Comisiones", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.2", EmpresaId = empresaId },
            new() { Codigo = "5.2.03", Nombre = "Publicidad y Mercadeo", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.2", EmpresaId = empresaId },
            new() { Codigo = "5.2.04", Nombre = "Fletes y Acarreos", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.2", EmpresaId = empresaId },
            new() { Codigo = "5.3", Nombre = "Gastos Financieros", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "5", EmpresaId = empresaId },
            new() { Codigo = "5.3.01", Nombre = "Intereses Bancarios", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.3", EmpresaId = empresaId },
            new() { Codigo = "5.3.02", Nombre = "Comisiones Bancarias", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.3", EmpresaId = empresaId },
            new() { Codigo = "5.3.03", Nombre = "Diferencial Cambiario Negativo", TipoCuenta = "GAS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "5.3", EmpresaId = empresaId },

            // COSTOS
            new() { Codigo = "6", Nombre = "COSTOS", TipoCuenta = "COS", Naturaleza = "D", AceptaMovimientos = false, EmpresaId = empresaId },
            new() { Codigo = "6.1", Nombre = "Costo de Ventas", TipoCuenta = "COS", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "6", EmpresaId = empresaId },
            new() { Codigo = "6.1.01", Nombre = "Costo de Mercaderías Vendidas", TipoCuenta = "COS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "6.1", EmpresaId = empresaId },
            new() { Codigo = "6.1.02", Nombre = "Costo de Servicios Prestados", TipoCuenta = "COS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "6.1", EmpresaId = empresaId },
            new() { Codigo = "6.2", Nombre = "Costos de Producción", TipoCuenta = "COS", Naturaleza = "D", AceptaMovimientos = false, CodigoPadreTemp = "6", EmpresaId = empresaId },
            new() { Codigo = "6.2.01", Nombre = "Materia Prima", TipoCuenta = "COS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "6.2", EmpresaId = empresaId },
            new() { Codigo = "6.2.02", Nombre = "Mano de Obra Directa", TipoCuenta = "COS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "6.2", EmpresaId = empresaId },
            new() { Codigo = "6.2.03", Nombre = "Costos Indirectos de Fabricación", TipoCuenta = "COS", Naturaleza = "D", AceptaMovimientos = true, CodigoPadreTemp = "6.2", EmpresaId = empresaId },
        };
    }

    // Clase temporal para generar plan de cuentas
    private class CuentaContableTemp
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EmpresaId { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string TipoCuenta { get; set; } = "ACT";
        public string Naturaleza { get; set; } = "D";
        public Guid? CuentaPadreId { get; set; }
        public string? CodigoPadreTemp { get; set; }
        public string? Descripcion { get; set; }
        public bool AceptaMovimientos { get; set; }
        public bool RequiereCentroCosto { get; set; }
        public decimal SaldoInicial { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class CuentaContableDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string TipoCuenta { get; set; } = string.Empty;
        public string Naturaleza { get; set; } = string.Empty;
        public string? CuentaPadreId { get; set; }
        public string? Descripcion { get; set; }
        public bool AceptaMovimientos { get; set; }
        public bool RequiereCentroCosto { get; set; }
        public bool Activo { get; set; }
        public decimal SaldoInicial { get; set; }
    }
}
