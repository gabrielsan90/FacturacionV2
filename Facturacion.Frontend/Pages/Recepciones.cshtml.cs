using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class RecepcionesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RecepcionesModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RecepcionesModel(IHttpClientFactory httpClientFactory, ILogger<RecepcionesModel> logger)
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

    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var response = await client.GetAsync($"/api/DocumentosRecibidos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var documentos = await response.Content.ReadFromJsonAsync<List<DocumentoRecibidoItem>>(_jsonOptions)
                    ?? new List<DocumentoRecibidoItem>();
                var tareas = documentos.Select(doc => BuildDocumentoRowAsync(client, doc));
                var data = await Task.WhenAll(tareas);
                return new JsonResult(new { data });
            }

            _logger.LogWarning("Failed to load recepciones. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recepciones");
            return new JsonResult(new { data = new List<dynamic>() });
        }
    }

    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/DocumentosRecibidos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var documento = await response.Content.ReadFromJsonAsync<DocumentoRecibidoDetalle>(_jsonOptions);
                if (documento == null)
                {
                    return new JsonResult(new { error = "Documento no encontrado" });
                }

                return new JsonResult(new
                {
                    id = documento.Id,
                    tipoDocumento = documento.TipoDocumentoNombre ?? "",
                    numeroDocumento = documento.NumeroConsecutivo ?? "",
                    emisorNombre = documento.Emisor?.Nombre ?? "",
                    monto = documento.Financiero?.TotalVenta ?? 0
                });
            }

            _logger.LogWarning("Recepcion not found. ID: {Id}", id);
            return new JsonResult(new { error = "Documento no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recepcion details. ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar el documento" });
        }
    }

    public async Task<IActionResult> OnGetXMLAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/DocumentosRecibidos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var documento = await response.Content.ReadFromJsonAsync<DocumentoRecibidoDetalle>(_jsonOptions);
                if (documento != null && !string.IsNullOrWhiteSpace(documento.XmlGenerado))
                {
                    return new JsonResult(new { xml = documento.XmlGenerado });
                }

                return new JsonResult(new { error = "XML no encontrado" });
            }

            _logger.LogWarning("XML not found for recepcion. ID: {Id}", id);
            return new JsonResult(new { error = "XML no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading XML. ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar el XML" });
        }
    }

    public async Task<IActionResult> OnPostResponderAsync([FromBody] RespuestaRecepcionRequest respuestaData)
    {
        try
        {
            if (respuestaData == null)
            {
                return new JsonResult(new { success = false, message = "Datos de respuesta invalidos" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var tipoMensaje = respuestaData.TipoRespuesta switch
            {
                2 => 2,
                3 => 3,
                _ => 1
            };

            var codigoMensaje = respuestaData.TipoRespuesta switch
            {
                2 => 2,
                3 => 5,
                _ => 1
            };

            var request = new
            {
                documentoOriginalId = respuestaData.Id,
                tipoMensaje,
                codigoMensaje,
                detalle = respuestaData.DetalleRespuesta,
                montoAceptado = respuestaData.MontoAceptado,
                montoImpuestoAceptado = respuestaData.MontoImpuestoAceptado
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/MensajesReceptor/generar", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Respuesta enviada exitosamente a Hacienda" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to send response. Error: {Error}", error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending response");
            return new JsonResult(new { success = false, message = "Error al enviar la respuesta" });
        }
    }

    public async Task<IActionResult> OnPostSincronizarAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
            }

            var response = await client.GetAsync($"/api/DocumentosRecibidos/empresa/{empresaId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to sync recepciones. Error: {Error}", error);
                return new JsonResult(new { success = false, message = error });
            }

            var documentos = await response.Content.ReadFromJsonAsync<List<DocumentoRecibidoItem>>(_jsonOptions)
                ?? new List<DocumentoRecibidoItem>();
            return new JsonResult(new
            {
                success = true,
                message = $"Sincronizacion completada. {documentos.Count} documentos disponibles"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing recepciones");
            return new JsonResult(new { success = false, message = "Error al sincronizar con Hacienda" });
        }
    }

    private async Task<object> BuildDocumentoRowAsync(HttpClient client, DocumentoRecibidoItem doc)
    {
        var fechaRecepcion = doc.FechaCreacion ?? doc.FechaEmision;
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

        return new
        {
            id = doc.Id,
            fechaRecepcion,
            tipoDocumento = doc.TipoDocumentoNombre ?? doc.TipoDocumento ?? "",
            numeroDocumento = doc.NumeroConsecutivo ?? "",
            emisorNombre = doc.EmisorNombre ?? "",
            monto = doc.TotalVenta ?? 0,
            estadoRespuesta,
            fechaRespuesta
        };
    }

    private static string MapEstadoRespuesta(string? tipoMensaje)
    {
        return tipoMensaje switch
        {
            "Aceptacion" => "Aceptado",
            "AceptacionParcial" => "Aceptado Parcialmente",
            "Rechazo" => "Rechazado",
            _ => "Sin Responder"
        };
    }

    private sealed class DocumentoRecibidoItem
    {
        public Guid Id { get; set; }
        public string? Clave { get; set; }
        public string? NumeroConsecutivo { get; set; }
        public string? TipoDocumento { get; set; }
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
        public string? Estado { get; set; }
    }

    private sealed class DocumentoRecibidoDetalle
    {
        public Guid Id { get; set; }
        public string? NumeroConsecutivo { get; set; }
        public string? TipoDocumentoNombre { get; set; }
        public DocumentoRecibidoEmisor? Emisor { get; set; }
        public DocumentoRecibidoFinanciero? Financiero { get; set; }
        public string? XmlGenerado { get; set; }
    }

    private sealed class DocumentoRecibidoEmisor
    {
        public string? Nombre { get; set; }
    }

    private sealed class DocumentoRecibidoFinanciero
    {
        public decimal? TotalVenta { get; set; }
    }

    public sealed class RespuestaRecepcionRequest
    {
        public string? Id { get; set; }
        public int TipoRespuesta { get; set; }
        public string? DetalleRespuesta { get; set; }
        public decimal? MontoAceptado { get; set; }
        public decimal? MontoImpuestoAceptado { get; set; }
    }
}

