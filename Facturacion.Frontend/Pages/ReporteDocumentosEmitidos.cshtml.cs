using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ReporteDocumentosEmitidosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReporteDocumentosEmitidosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteDocumentosEmitidosModel(IHttpClientFactory httpClientFactory, ILogger<ReporteDocumentosEmitidosModel> logger)
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

    public async Task<IActionResult> OnGetDataAsync(string? fechaInicio, string? fechaFin, string? tipoDocumento)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var client = CreateClient();
            var documentos = await GetDocumentosAsync(client, empresaId, fechaInicio, fechaFin, tipoDocumento);
            var data = documentos
                .Where(d => !d.EsDocumentoRecibido)
                .Select(d => new
                {
                    fecha = d.FechaEmision,
                    tipoDocumento = MapTipoDocumento(d.TipoDocumento),
                    numero = d.NumeroConsecutivo ?? string.Empty,
                    clienteNombre = d.ReceptorNombre ?? d.Cliente?.Nombre ?? "Cliente no especificado",
                    total = d.TotalVenta,
                    estado = MapEstadoDocumento(d.Estado),
                    clave = d.Clave ?? string.Empty
                })
                .ToList();

            return new JsonResult(new { data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reporte documentos emitidos");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    public async Task<IActionResult> OnGetResumenAsync(string? fechaInicio, string? fechaFin, string? tipoDocumento)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { totalDocumentos = 0, totalAceptados = 0, totalPendientes = 0, totalMonto = 0m });
            }

            var client = CreateClient();
            var documentos = await GetDocumentosAsync(client, empresaId, fechaInicio, fechaFin, tipoDocumento);
            var emitidos = documentos.Where(d => !d.EsDocumentoRecibido).ToList();

            var totalAceptados = emitidos.Count(d => IsEstado(d.Estado, "Aceptado"));
            var totalRechazados = emitidos.Count(d => IsEstado(d.Estado, "Rechazado"));
            var totalPendientes = emitidos.Count - totalAceptados - totalRechazados;
            var totalMonto = emitidos.Sum(d => d.TotalVenta);

            return new JsonResult(new
            {
                totalDocumentos = emitidos.Count,
                totalAceptados,
                totalPendientes,
                totalMonto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resumen");
            return new JsonResult(new { totalDocumentos = 0, totalAceptados = 0, totalPendientes = 0, totalMonto = 0m });
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

    private async Task<List<DocumentoItem>> GetDocumentosAsync(
        HttpClient client,
        Guid empresaId,
        string? fechaInicio,
        string? fechaFin,
        string? tipoDocumento)
    {
        var query = new List<string> { $"empresaId={empresaId}" };

        if (DateTime.TryParse(fechaInicio, out var inicio))
        {
            query.Add($"fechaInicio={inicio:yyyy-MM-dd}");
        }

        if (DateTime.TryParse(fechaFin, out var fin))
        {
            query.Add($"fechaFin={fin:yyyy-MM-dd}");
        }

        if (int.TryParse(tipoDocumento, out var tipoId) && tipoId > 0)
        {
            query.Add($"tipoDocumento={tipoId}");
        }

        var url = "/api/Documentos" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to load documentos. Status code: {StatusCode}", response.StatusCode);
            return new List<DocumentoItem>();
        }

        var documentos = await response.Content.ReadFromJsonAsync<List<DocumentoItem>>(_jsonOptions);
        return documentos ?? new List<DocumentoItem>();
    }

    private static string MapTipoDocumento(string? tipoDocumento)
    {
        return tipoDocumento switch
        {
            "FacturaElectronica" => "Factura Electronica",
            "NotaDebitoElectronica" => "Nota de Debito",
            "NotaCreditoElectronica" => "Nota de Credito",
            "TiqueteElectronico" => "Tiquete Electronico",
            "FacturaElectronicaExportacion" => "Factura de Exportacion",
            "FacturaElectronicaCompra" => "Factura Electronica Compra",
            "NotaCreditoElectronicaCompra" => "Nota Credito Compra",
            "NotaDebitoElectronicaCompra" => "Nota Debito Compra",
            "ComprobanteElectronicoCompra" => "Comprobante Electronico Compra",
            "ReciboElectronicoPago" => "Recibo Electronico Pago",
            _ => tipoDocumento ?? string.Empty
        };
    }

    private static string MapEstadoDocumento(string? estado)
    {
        if (IsEstado(estado, "Aceptado"))
        {
            return "Aceptado";
        }

        if (IsEstado(estado, "Rechazado"))
        {
            return "Rechazado";
        }

        return "Pendiente";
    }

    private static bool IsEstado(string? estado, string esperado)
    {
        return string.Equals(estado, esperado, StringComparison.OrdinalIgnoreCase);
    }

    private bool TryGetEmpresaId(out Guid empresaId)
    {
        empresaId = Guid.Empty;
        var empresaIdValue = User.FindFirstValue("EmpresaId");
        return Guid.TryParse(empresaIdValue, out empresaId);
    }

    private sealed class DocumentoItem
    {
        public Guid Id { get; set; }
        public string? Clave { get; set; }
        public string? NumeroConsecutivo { get; set; }
        public string? TipoDocumento { get; set; }
        public string? Estado { get; set; }
        public bool EsDocumentoRecibido { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal TotalVenta { get; set; }
        public string? ReceptorNombre { get; set; }
        public DocumentoCliente? Cliente { get; set; }
    }

    private sealed class DocumentoCliente
    {
        public string? Nombre { get; set; }
    }
}
