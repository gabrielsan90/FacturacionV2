using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.DocumentosElectronicos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Contador")]
public class DocumentosRecibidosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DocumentosRecibidosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DocumentosRecibidosModel(IHttpClientFactory httpClientFactory, ILogger<DocumentosRecibidosModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public string EmpresaId { get; set; } = "";

    public void OnGet()
    {
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    private HttpClient GetAuthenticatedClient()
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

    private string GetEmpresaId()
    {
        return User.FindFirstValue("EmpresaId") ?? "";
    }

    /// <summary>
    /// Lee la respuesta del backend y la retorna como JsonResult.
    /// Maneja respuestas vacías, no-JSON, y errores HTTP de forma segura.
    /// </summary>
    private async Task<IActionResult> ForwardApiResponseAsync(HttpResponseMessage response)
    {
        var responseText = await response.Content.ReadAsStringAsync();

        // Handle non-success status codes with clear messages
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("API retornó HTTP {StatusCode}: {Body}", (int)response.StatusCode, responseText);

            // Try to extract a JSON error message from the body
            if (!string.IsNullOrWhiteSpace(responseText))
            {
                try
                {
                    using var doc = JsonDocument.Parse(responseText);
                    if (doc.RootElement.TryGetProperty("message", out var msg))
                        return new JsonResult(new { success = false, message = msg.GetString() });
                    if (doc.RootElement.TryGetProperty("mensaje", out var msg2))
                        return new JsonResult(new { success = false, message = msg2.GetString() });
                    // The response is JSON but doesn't have a message property — forward it
                    var parsed = JsonSerializer.Deserialize<object>(responseText, _jsonOptions);
                    return new JsonResult(parsed);
                }
                catch { /* Not JSON, fall through */ }
            }

            var errorMsg = response.StatusCode switch
            {
                System.Net.HttpStatusCode.NotFound =>
                    "Endpoint no encontrado (404). El backend no tiene este endpoint. Recompile y publique el backend.",
                System.Net.HttpStatusCode.Unauthorized =>
                    "No autorizado (401). La sesion puede haber expirado.",
                System.Net.HttpStatusCode.Forbidden =>
                    "Acceso denegado (403). No tiene permisos para esta operacion.",
                _ => $"Error del servidor (HTTP {(int)response.StatusCode})"
            };
            return new JsonResult(new { success = false, message = errorMsg });
        }

        // Success response
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return new JsonResult(new { success = true });
        }

        try
        {
            var result = JsonSerializer.Deserialize<object>(responseText, _jsonOptions);
            return new JsonResult(result);
        }
        catch
        {
            return new JsonResult(new { success = false, message = responseText });
        }
    }

    /// <summary>
    /// Carga datos para el DataTable de documentos recibidos
    /// </summary>
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var empresaId = GetEmpresaId();
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"/api/documentosrecibidos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult
                {
                    Content = $"{{\"data\":{content}}}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            _logger.LogError("Error al obtener documentos recibidos: {StatusCode}", response.StatusCode);
            return new ContentResult
            {
                Content = "{\"data\":[]}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnGetDataAsync");
            return new ContentResult
            {
                Content = "{\"data\":[]}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
    }

    /// <summary>
    /// Sube uno o más archivos XML para recepción
    /// </summary>
    public async Task<IActionResult> OnPostSubirXmlAsync(List<IFormFile> archivos)
    {
        try
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró empresa asociada" });
            }

            var client = GetAuthenticatedClient();
            var resultados = new List<object>();
            int exitosos = 0, errores = 0;

            foreach (var archivo in archivos)
            {
                if (archivo.Length == 0)
                    continue;

                using var reader = new StreamReader(archivo.OpenReadStream());
                var xmlContent = await reader.ReadToEndAsync();

                var requestBody = new { xmlContent, empresaId };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/documentosrecibidos/recibir", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    exitosos++;
                    using var doc = JsonDocument.Parse(responseContent);
                    resultados.Add(new
                    {
                        exitoso = true,
                        archivo = archivo.FileName,
                        mensaje = doc.RootElement.TryGetProperty("mensaje", out var msg) ? msg.GetString() : "Procesado exitosamente",
                        emisorNombre = doc.RootElement.TryGetProperty("emisorNombre", out var emisor) ? emisor.GetString() : "",
                        totalVenta = doc.RootElement.TryGetProperty("totalVenta", out var total) ? total.GetDecimal() : 0m,
                        clave = doc.RootElement.TryGetProperty("clave", out var clave) ? clave.GetString() : ""
                    });
                }
                else
                {
                    errores++;
                    var errorMsg = "Error al procesar";
                    try
                    {
                        using var doc = JsonDocument.Parse(responseContent);
                        if (doc.RootElement.TryGetProperty("mensaje", out var msg))
                            errorMsg = msg.GetString() ?? errorMsg;
                        else if (doc.RootElement.TryGetProperty("message", out var msg2))
                            errorMsg = msg2.GetString() ?? errorMsg;
                    }
                    catch { }

                    resultados.Add(new
                    {
                        exitoso = false,
                        archivo = archivo.FileName,
                        mensaje = errorMsg,
                        emisorNombre = "",
                        totalVenta = 0m,
                        clave = ""
                    });
                }
            }

            return new JsonResult(new
            {
                success = true,
                totalProcesados = exitosos,
                totalErrores = errores,
                resultados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnPostSubirXmlAsync");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene la configuración IMAP
    /// </summary>
    public async Task<IActionResult> OnGetConfigImapAsync()
    {
        try
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró empresa asociada" });
            }

            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"/api/configuracion/imap/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseText))
                {
                    return new JsonResult(new { success = true, data = (object?)null });
                }

                try
                {
                    var config = JsonSerializer.Deserialize<object>(responseText, _jsonOptions);
                    return new JsonResult(new { success = true, data = config });
                }
                catch
                {
                    return new JsonResult(new { success = true, data = (object?)null });
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Error al obtener configuración IMAP: HTTP {StatusCode} - {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = $"Error al cargar configuración (HTTP {(int)response.StatusCode})" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnGetConfigImapAsync");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Guarda la configuración IMAP
    /// </summary>
    public async Task<IActionResult> OnPostGuardarImapAsync([FromBody] JsonElement config)
    {
        try
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró empresa asociada" });
            }

            var client = GetAuthenticatedClient();
            var json = config.GetRawText();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/configuracion/imap/{empresaId}", content);
            return await ForwardApiResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnPostGuardarImapAsync");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Prueba la conexión IMAP
    /// </summary>
    public async Task<IActionResult> OnPostProbarImapAsync()
    {
        try
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró empresa asociada" });
            }

            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"/api/configuracion/imap/{empresaId}/probar", null);
            return await ForwardApiResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnPostProbarImapAsync");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene documentos pendientes de MensajeReceptor con alertas de vencimiento
    /// </summary>
    public async Task<IActionResult> OnGetPendientesMRAsync()
    {
        try
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
                return new JsonResult(new { success = false, data = Array.Empty<object>() });

            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"/api/MensajesReceptor/pendientes/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult
                {
                    Content = $"{{\"success\":true,\"data\":{content}}}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }

            return new JsonResult(new { success = false, data = Array.Empty<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnGetPendientesMRAsync");
            return new JsonResult(new { success = false, data = Array.Empty<object>() });
        }
    }

    /// <summary>
    /// Lee correos IMAP y procesa XMLs adjuntos
    /// </summary>
    public async Task<IActionResult> OnPostLeerCorreosAsync()
    {
        try
        {
            var empresaId = GetEmpresaId();
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "No se encontró empresa asociada" });
            }

            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"/api/documentosrecibidos/leer-correos/{empresaId}", null);
            return await ForwardApiResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnPostLeerCorreosAsync");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }
}
