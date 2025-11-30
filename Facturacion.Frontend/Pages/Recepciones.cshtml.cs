using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            PropertyNameCaseInsensitive = true
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

            var response = await client.GetAsync("/api/recepciones");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = data ?? new List<dynamic>() });
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

            var response = await client.GetAsync($"/api/recepciones/{id}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data);
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

            var response = await client.GetAsync($"/api/recepciones/{id}/xml");

            if (response.IsSuccessStatusCode)
            {
                var xml = await response.Content.ReadAsStringAsync();
                return new JsonResult(new { xml });
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

    public async Task<IActionResult> OnPostResponderAsync([FromBody] dynamic respuestaData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(respuestaData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/recepciones/responder", content);

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

            var response = await client.PostAsync("/api/recepciones/sincronizar", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(new { success = true, message = $"Sincronización completada. {result?.documentosNuevos ?? 0} documentos nuevos" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to sync. Error: {Error}", error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing recepciones");
            return new JsonResult(new { success = false, message = "Error al sincronizar con Hacienda" });
        }
    }
}
