using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Reportes;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ReporteDocumentosRecibidosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReporteDocumentosRecibidosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteDocumentosRecibidosModel(IHttpClientFactory httpClientFactory, ILogger<ReporteDocumentosRecibidosModel> logger)
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

    public async Task<IActionResult> OnGetDataAsync(string? fechaInicio, string? fechaFin, string? estadoRespuesta)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var client = CreateClient();
            var documentos = await GetDocumentosRecibidosAsync(client, empresaId, fechaInicio, fechaFin);
            var tareas = documentos.Select(doc => BuildRecepcionRowAsync(client, doc));
            var data = (await Task.WhenAll(tareas)).ToList();

            var estadoFiltro = MapEstadoFiltro(estadoRespuesta);
            if (!string.IsNullOrWhiteSpace(estadoFiltro))
            {
                data = data.Where(d => string.Equals(d.EstadoRespuesta, estadoFiltro, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return new JsonResult(new { data = data.Select(d => new
            {
                fecha = d.Fecha,
                tipoDocumento = d.TipoDocumento,
                numero = d.Numero,
                emisorNombre = d.EmisorNombre,
                total = d.Total,
                estadoRespuesta = d.EstadoRespuesta,
                fechaRespuesta = d.FechaRespuesta
            }) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reporte documentos recibidos");
            return new JsonResult(new { data = new List<object>() });
        }
    }

    public async Task<IActionResult> OnGetResumenAsync(string? fechaInicio, string? fechaFin, string? estadoRespuesta)
    {
        try
        {
            if (!TryGetEmpresaId(out var empresaId))
            {
                return new JsonResult(new { totalRecibidos = 0, totalRespondidos = 0, totalSinResponder = 0, totalMonto = 0m });
            }

            var client = CreateClient();
            var documentos = await GetDocumentosRecibidosAsync(client, empresaId, fechaInicio, fechaFin);
            var tareas = documentos.Select(doc => BuildRecepcionRowAsync(client, doc));
            var data = (await Task.WhenAll(tareas)).ToList();

            var estadoFiltro = MapEstadoFiltro(estadoRespuesta);
            if (!string.IsNullOrWhiteSpace(estadoFiltro))
            {
                data = data.Where(d => string.Equals(d.EstadoRespuesta, estadoFiltro, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var totalRecibidos = data.Count;
            var totalRespondidos = data.Count(d => !string.Equals(d.EstadoRespuesta, "Sin Responder", StringComparison.OrdinalIgnoreCase));
            var totalSinResponder = data.Count(d => string.Equals(d.EstadoRespuesta, "Sin Responder", StringComparison.OrdinalIgnoreCase));
            var totalMonto = data.Sum(d => d.Total);

            return new JsonResult(new { totalRecibidos, totalRespondidos, totalSinResponder, totalMonto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resumen");
            return new JsonResult(new { totalRecibidos = 0, totalRespondidos = 0, totalSinResponder = 0, totalMonto = 0m });
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

    private async Task<List<DocumentoRecibidoItem>> GetDocumentosRecibidosAsync(
        HttpClient client,
        Guid empresaId,
        string? fechaInicio,
        string? fechaFin)
    {
        var response = await client.GetAsync($"/api/DocumentosRecibidos/empresa/{empresaId}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to load documentos recibidos. Status code: {StatusCode}", response.StatusCode);
            return new List<DocumentoRecibidoItem>();
        }

        var documentos = await response.Content.ReadFromJsonAsync<List<DocumentoRecibidoItem>>(_jsonOptions)
            ?? new List<DocumentoRecibidoItem>();

        if (DateTime.TryParse(fechaInicio, out var inicio))
        {
            documentos = documentos.Where(d => d.FechaEmision.HasValue && d.FechaEmision.Value.Date >= inicio.Date).ToList();
        }

        if (DateTime.TryParse(fechaFin, out var fin))
        {
            documentos = documentos.Where(d => d.FechaEmision.HasValue && d.FechaEmision.Value.Date <= fin.Date).ToList();
        }

        return documentos;
    }

    private async Task<RecepcionRow> BuildRecepcionRowAsync(HttpClient client, DocumentoRecibidoItem doc)
    {
        var estadoRespuesta = "Sin Responder";
        DateTime? fechaRespuesta = null;

        try
        {
            var response = await client.GetAsync($"/api/MensajesReceptor/documento/{doc.Id}");
            if (response.IsSuccessStatusCode)
            {
                var mensajes = await response.Content.ReadFromJsonAsync<List<MensajeReceptorItem>>(_jsonOptions)
                    ?? new List<MensajeReceptorItem>();
                var ultimo = mensajes
                    .Where(m => m.FechaEnvio.HasValue)
                    .OrderByDescending(m => m.FechaEnvio)
                    .FirstOrDefault() ?? mensajes.FirstOrDefault();

                if (ultimo != null)
                {
                    estadoRespuesta = MapEstadoRespuesta(ultimo.TipoMensaje);
                    fechaRespuesta = ultimo.FechaEnvio;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading mensaje receptor for documento {DocumentoId}", doc.Id);
        }

        return new RecepcionRow
        {
            Fecha = doc.FechaEmision ?? doc.FechaCreacion ?? DateTime.MinValue,
            TipoDocumento = doc.TipoDocumentoNombre ?? string.Empty,
            Numero = doc.NumeroConsecutivo ?? string.Empty,
            EmisorNombre = doc.EmisorNombre ?? string.Empty,
            Total = doc.TotalVenta ?? 0m,
            EstadoRespuesta = estadoRespuesta,
            FechaRespuesta = fechaRespuesta
        };
    }

    private static string MapEstadoRespuesta(string? tipoMensaje)
    {
        return tipoMensaje switch
        {
            "Aceptacion" => "Aceptado",
            "AceptacionParcial" => "Parcialmente Aceptado",
            "Rechazo" => "Rechazado",
            _ => "Sin Responder"
        };
    }

    private static string? MapEstadoFiltro(string? estadoRespuesta)
    {
        return estadoRespuesta switch
        {
            "1" => "Aceptado",
            "2" => "Parcialmente Aceptado",
            "3" => "Rechazado",
            _ => null
        };
    }

    private bool TryGetEmpresaId(out Guid empresaId)
    {
        empresaId = Guid.Empty;
        var empresaIdValue = User.FindFirstValue("EmpresaId");
        return Guid.TryParse(empresaIdValue, out empresaId);
    }

    private sealed class DocumentoRecibidoItem
    {
        public Guid Id { get; set; }
        public string? NumeroConsecutivo { get; set; }
        public string? TipoDocumentoNombre { get; set; }
        public DateTime? FechaEmision { get; set; }
        public string? EmisorNombre { get; set; }
        public decimal? TotalVenta { get; set; }
        public DateTime? FechaCreacion { get; set; }
    }

    private sealed class MensajeReceptorItem
    {
        public string? TipoMensaje { get; set; }
        public DateTime? FechaEnvio { get; set; }
    }

    private sealed class RecepcionRow
    {
        public DateTime Fecha { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string EmisorNombre { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string EstadoRespuesta { get; set; } = "Sin Responder";
        public DateTime? FechaRespuesta { get; set; }
    }
}
