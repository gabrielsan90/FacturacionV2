using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.DTOs;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Pagos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class RecibosPagoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<RecibosPagoModel> _logger;

    public Guid EmpresaId { get; set; }
    public Guid TerminalId { get; set; }

    public RecibosPagoModel(
        IHttpClientFactory httpClientFactory,
        ILogger<RecibosPagoModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public void OnGet()
    {
        // Get empresa and terminal from user claims or session
        var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
        var terminalIdClaim = User.FindFirst("TerminalId")?.Value;

        if (!string.IsNullOrEmpty(empresaIdClaim) && Guid.TryParse(empresaIdClaim, out var empresaId))
        {
            EmpresaId = empresaId;
        }

        if (!string.IsNullOrEmpty(terminalIdClaim) && Guid.TryParse(terminalIdClaim, out var terminalId))
        {
            TerminalId = terminalId;
        }

        _logger.LogInformation("RecibosPago page loaded for empresa {EmpresaId}", EmpresaId);
    }

    /// <summary>
    /// Handler para obtener documentos pendientes de pago (cuentas por cobrar)
    /// </summary>
    public async Task<IActionResult> OnGetDocumentosPendientesAsync(bool soloVencidos = false)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = GetEmpresaId();
            var url = $"/api/recibospago/pendientes?empresaId={empresaId}&soloVencidos={soloVencidos}";

            _logger.LogInformation("Fetching pending documents from: {Url}", url);

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var documentos = await response.Content.ReadFromJsonAsync<List<DocumentoPendientePagoDTO>>(_jsonOptions);
                return new JsonResult(documentos ?? new List<DocumentoPendientePagoDTO>());
            }

            _logger.LogWarning("Failed to fetch pending documents. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<DocumentoPendientePagoDTO>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending documents");
            return new JsonResult(new List<DocumentoPendientePagoDTO>());
        }
    }

    /// <summary>
    /// Handler para obtener recibos de pago generados
    /// </summary>
    public async Task<IActionResult> OnGetRecibosGeneradosAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = GetEmpresaId();

            // Get all REP documents for the empresa
            var url = $"/api/documentos?empresaId={empresaId}&tipoDocumento=10"; // 10 = REP

            _logger.LogInformation("Fetching REP documents from: {Url}", url);

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var documentos = await response.Content.ReadFromJsonAsync<List<DocumentoElectronicoDTO>>(_jsonOptions);

                // Transform to REP view model
                var recibos = new List<ReciboGeneradoDTO>();

                if (documentos != null)
                {
                    foreach (var doc in documentos)
                    {
                        // Get ReciboPago details for each REP document
                        var reciboResponse = await client.GetAsync($"/api/recibospago/{doc.Id}");

                        if (reciboResponse.IsSuccessStatusCode)
                        {
                            var recibo = await reciboResponse.Content.ReadFromJsonAsync<ReciboPagoDetalleDTO>(_jsonOptions);

                            if (recibo != null)
                            {
                                recibos.Add(new ReciboGeneradoDTO
                                {
                                    DocumentoREPId = doc.Id,
                                    NumeroConsecutivoREP = doc.NumeroConsecutivo,
                                    ClaveREP = doc.Clave,
                                    NumeroConsecutivoOriginal = recibo.NumeroConsecutivoOriginal,
                                    ClienteNombre = doc.ReceptorNombre ?? "",
                                    FechaPago = recibo.FechaPago,
                                    MontoPagado = recibo.MontoPagado,
                                    SaldoPendiente = recibo.SaldoPendiente,
                                    Moneda = doc.Moneda,
                                    Estado = doc.Estado.ToString()
                                });
                            }
                        }
                    }
                }

                return new JsonResult(recibos);
            }

            _logger.LogWarning("Failed to fetch REP documents. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<ReciboGeneradoDTO>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching REP documents");
            return new JsonResult(new List<ReciboGeneradoDTO>());
        }
    }

    /// <summary>
    /// Handler para obtener estadísticas de cobranza
    /// </summary>
    public async Task<IActionResult> OnGetEstadisticasAsync(Guid empresaId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/recibospago/estadisticas/{empresaId}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var stats = await response.Content.ReadFromJsonAsync<EstadisticasCobranzaDTO>(_jsonOptions);
                return new JsonResult(stats ?? new EstadisticasCobranzaDTO());
            }

            // Return empty stats if failed
            return new JsonResult(new EstadisticasCobranzaDTO
            {
                TotalDocumentosPendientes = 0,
                TotalPorCobrar = 0m,
                DocumentosVencidos = 0,
                MontoVencido = 0m,
                DocumentosAlDia = 0,
                MontoAlDia = 0m
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching statistics");
            return new JsonResult(new EstadisticasCobranzaDTO
            {
                TotalDocumentosPendientes = 0,
                TotalPorCobrar = 0m,
                DocumentosVencidos = 0,
                MontoVencido = 0m,
                DocumentosAlDia = 0,
                MontoAlDia = 0m
            });
        }
    }

    /// <summary>
    /// Handler para obtener detalle de un documento para generar REP
    /// </summary>
    public async Task<IActionResult> OnGetDocumentoDetalleAsync(Guid documentoId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Get document details
            var docResponse = await client.GetAsync($"/api/documentos/{documentoId}");
            if (!docResponse.IsSuccessStatusCode)
            {
                return BadRequest(new { error = "Documento no encontrado" });
            }

            var documento = await docResponse.Content.ReadFromJsonAsync<DocumentoElectronicoDTO>(_jsonOptions);

            if (documento == null)
            {
                return BadRequest(new { error = "Documento no encontrado" });
            }

            // Get saldo pendiente
            var saldoResponse = await client.GetAsync($"/api/recibospago/saldo/{documentoId}");
            var saldoData = await saldoResponse.Content.ReadFromJsonAsync<SaldoDocumentoDTO>(_jsonOptions);
            var saldoPendiente = saldoData?.SaldoPendiente ?? 0m;

            // Get total pagado
            var totalPagado = documento.TotalVenta - saldoPendiente;

            var result = new DocumentoDetalleDTO
            {
                DocumentoId = documento.Id,
                NumeroConsecutivo = documento.NumeroConsecutivo,
                ClienteNombre = documento.ReceptorNombre ?? "",
                FechaEmision = documento.FechaEmision,
                TotalVenta = documento.TotalVenta,
                TotalPagado = totalPagado,
                SaldoPendiente = saldoPendiente,
                Moneda = documento.Moneda,
                TipoCambio = documento.TipoCambio
            };

            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document details for documentoId: {DocumentoId}", documentoId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Handler para obtener detalle de pagos de un documento
    /// </summary>
    public async Task<IActionResult> OnGetDetallePagosAsync(Guid documentoId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var url = $"/api/recibospago/documento/{documentoId}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var detalle = await response.Content.ReadFromJsonAsync<DetallePagosDocumentoDTO>(_jsonOptions);
                return new JsonResult(detalle);
            }

            return BadRequest(new { error = "No se pudo obtener el detalle de pagos" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment details for documentoId: {DocumentoId}", documentoId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Handler para generar un REP
    /// </summary>
    public async Task<IActionResult> OnPostGenerarREPAsync(
        [FromBody] ReciboPagoDTO dto,
        Guid empresaId,
        Guid terminalId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return new JsonResult(new ResultadoREP
                {
                    Exitoso = false,
                    Mensaje = "Datos inválidos",
                    Errores = errors
                });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"/api/recibospago/generar?empresaId={empresaId}&terminalId={terminalId}";

            _logger.LogInformation("Generating REP for documento {DocumentoId}", dto.DocumentoOriginalId);

            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var resultado = JsonSerializer.Deserialize<ResultadoREP>(responseContent, _jsonOptions);
                _logger.LogInformation("REP generated successfully: {ClaveREP}", resultado?.ClaveREP);
                return new JsonResult(resultado);
            }
            else
            {
                _logger.LogWarning("Failed to generate REP. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);

                try
                {
                    var errorResult = JsonSerializer.Deserialize<ResultadoREP>(responseContent, _jsonOptions);
                    return new JsonResult(errorResult);
                }
                catch
                {
                    return new JsonResult(new ResultadoREP
                    {
                        Exitoso = false,
                        Mensaje = "Error al generar el REP",
                        Errores = new List<string> { responseContent }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating REP");
            return new JsonResult(new ResultadoREP
            {
                Exitoso = false,
                Mensaje = "Error interno del servidor",
                Errores = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Handler para obtener el XML de un REP
    /// </summary>
    public async Task<IActionResult> OnGetXMLAsync(Guid documentoId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");
            var token = User.FindFirst("Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/documentos/{documentoId}");

            if (response.IsSuccessStatusCode)
            {
                var documento = await response.Content.ReadFromJsonAsync<DocumentoElectronicoDTO>(_jsonOptions);

                if (documento == null)
                {
                    return BadRequest(new { error = "Documento no encontrado" });
                }

                var xml = documento.XmlFirmado
                    ?? documento.XmlGenerado
                    ?? "XML no disponible";

                var result = new DocumentoXMLDTO
                {
                    Xml = xml,
                    Clave = documento.Clave
                };

                return new JsonResult(result);
            }

            return BadRequest(new { error = "Documento no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting XML for documentoId: {DocumentoId}", documentoId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Helper method to get EmpresaId from claims or session
    /// </summary>
    private Guid GetEmpresaId()
    {
        var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;

        if (!string.IsNullOrEmpty(empresaIdClaim) && Guid.TryParse(empresaIdClaim, out var empresaId))
        {
            return empresaId;
        }

        // Fallback: try to get from session or default
        if (HttpContext.Session.TryGetValue("EmpresaId", out var sessionBytes))
        {
            var sessionId = new Guid(sessionBytes);
            return sessionId;
        }

        _logger.LogWarning("EmpresaId not found in claims or session");
        return Guid.Empty;
    }

    /// <summary>
    /// Helper method to get TerminalId from claims or session
    /// </summary>
    private Guid GetTerminalId()
    {
        var terminalIdClaim = User.FindFirst("TerminalId")?.Value;

        if (!string.IsNullOrEmpty(terminalIdClaim) && Guid.TryParse(terminalIdClaim, out var terminalId))
        {
            return terminalId;
        }

        // Fallback: try to get from session or default
        if (HttpContext.Session.TryGetValue("TerminalId", out var sessionBytes))
        {
            var sessionId = new Guid(sessionBytes);
            return sessionId;
        }

        _logger.LogWarning("TerminalId not found in claims or session");
        return Guid.Empty;
    }
}
