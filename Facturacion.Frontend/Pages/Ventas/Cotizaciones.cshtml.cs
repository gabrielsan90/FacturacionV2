using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Facturacion.Frontend.Pages.Ventas;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class CotizacionesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public string EmpresaId { get; set; } = "";

    public CotizacionesModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public void OnGet()
    {
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    // Handler for DataTable - Load all quotations for the current empresa
    public async Task<IActionResult> OnGetDataAsync(string? estado)
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new { data = new List<object>() });
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        // Get JWT token from user claims
        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var url = $"/api/cotizaciones/empresa/{empresaId}";
        if (!string.IsNullOrEmpty(estado) && estado != "TODAS")
        {
            url += $"?estado={estado}";
        }

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            // API may return array directly or wrapped in ActionResponse<T>
            string dataJson;
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                dataJson = content;
            }
            else if (doc.RootElement.TryGetProperty("result", out var resultElement))
            {
                dataJson = resultElement.GetRawText();
            }
            else
            {
                dataJson = content;
            }

            return new ContentResult
            {
                Content = $"{{\"data\":{dataJson}}}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        return new JsonResult(new { data = new List<object>() });
    }

    // Handler to get a single quotation by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/cotizaciones/{id}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            string dataJson;
            if (doc.RootElement.ValueKind == JsonValueKind.Array || doc.RootElement.ValueKind == JsonValueKind.Object && !doc.RootElement.TryGetProperty("result", out _))
            {
                dataJson = content;
            }
            else if (doc.RootElement.TryGetProperty("result", out var resultElement))
            {
                dataJson = resultElement.GetRawText();
            }
            else
            {
                dataJson = content;
            }

            return new ContentResult
            {
                Content = $"{{\"success\":true,\"data\":{dataJson}}}",
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        return new JsonResult(new { success = false, message = "Cotización no encontrada" });
    }

    // Handler to get clientes for Select2
    public async Task<IActionResult> OnGetClientesAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new List<object>());
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/clientes/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var clientes = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(clientes ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to get productos for line items
    public async Task<IActionResult> OnGetProductosAsync()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId))
        {
            return new JsonResult(new List<object>());
        }

        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/productos/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var productos = await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
            return new JsonResult(productos ?? new List<object>());
        }

        return new JsonResult(new List<object>());
    }

    // Handler to save (create or update) a quotation
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement cotizacionData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var empresaId = User.FindFirstValue("EmpresaId") ?? "";

        // Transform JS payload to match Cotizacion entity structure
        var obj = JsonNode.Parse(JsonSerializer.Serialize(cotizacionData))!.AsObject();

        // Fix estado: JS sends 'BOR' but backend expects EstadoCotizacion enum name
        obj.Remove("estado");
        obj["estado"] = "Borrador";

        // Map 'fecha' → 'fechaEmision' (entity field name)
        if (obj.ContainsKey("fecha"))
        {
            obj["fechaEmision"] = obj["fecha"]!.DeepClone();
            obj.Remove("fecha");
        }

        // Map 'descuentoTotal' → 'totalDescuentos'
        if (obj.ContainsKey("descuentoTotal"))
        {
            obj["totalDescuentos"] = obj["descuentoTotal"]!.DeepClone();
            obj.Remove("descuentoTotal");
        }

        // Map 'impuestos' → 'totalImpuestos'
        if (obj.ContainsKey("impuestos"))
        {
            obj["totalImpuestos"] = obj["impuestos"]!.DeepClone();
            obj.Remove("impuestos");
        }

        // Set required defaults if missing
        if (!obj.ContainsKey("numero") || string.IsNullOrEmpty(obj["numero"]?.ToString()))
            obj["numero"] = "PENDIENTE"; // Controller generates the real number

        if (!obj.ContainsKey("condicionVenta") || string.IsNullOrEmpty(obj["condicionVenta"]?.ToString()))
            obj["condicionVenta"] = "01"; // Default: Contado

        if (!obj.ContainsKey("medioPago") || string.IsNullOrEmpty(obj["medioPago"]?.ToString()))
            obj["medioPago"] = "01"; // Default: Efectivo

        if (!obj.ContainsKey("moneda") || string.IsNullOrEmpty(obj["moneda"]?.ToString()))
            obj["moneda"] = "CRC";

        // Ensure empresaId is set
        if (string.IsNullOrWhiteSpace(empresaId))
        {
            return new JsonResult(new { success = false, message = "No se encontró la empresa del usuario. Cierre sesión e inicie de nuevo." });
        }
        obj["empresaId"] = empresaId;

        // Fetch default sucursal/terminal if missing
        if (!obj.ContainsKey("sucursalId") || obj["sucursalId"]?.ToString() == "00000000-0000-0000-0000-000000000000")
        {
            var sucTerminal = await ObtenerSucursalTerminalDefaultAsync(client, empresaId);

            if (sucTerminal.sucursalId == Guid.Empty.ToString())
            {
                return new JsonResult(new { success = false, message = "No hay sucursales configuradas para esta empresa. Configure al menos una sucursal en Configuración → Sucursales antes de crear cotizaciones." });
            }
            if (sucTerminal.terminalId == Guid.Empty.ToString())
            {
                return new JsonResult(new { success = false, message = "No hay terminales configuradas para la sucursal. Configure al menos una terminal en Configuración → Terminales antes de crear cotizaciones." });
            }

            obj["sucursalId"] = sucTerminal.sucursalId;
            obj["terminalId"] = sucTerminal.terminalId;
        }

        var json = obj.ToJsonString();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;

        // Check if it's a new quotation or an update
        bool isNew = !cotizacionData.TryGetProperty("id", out var idProp) ||
                     idProp.GetString() == "00000000-0000-0000-0000-000000000000" ||
                     string.IsNullOrEmpty(idProp.GetString());

        if (isNew)
        {
            response = await client.PostAsync("/api/cotizaciones", content);
        }
        else
        {
            var id = idProp.GetString();
            response = await client.PutAsync($"/api/cotizaciones/{id}", content);
        }

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
            return new JsonResult(new
            {
                success = true,
                message = isNew ? "Cotización creada exitosamente" : "Cotización actualizada exitosamente",
                data = result
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        var mensajeError = ExtraerMensajeError(error, (int)response.StatusCode);
        return new JsonResult(new { success = false, message = mensajeError });
    }

    // Handler to send quotation to customer
    public async Task<IActionResult> OnPostEnviarAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var content = new StringContent("", Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/cotizaciones/{id}/enviar", content);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Cotización enviada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to approve quotation
    public async Task<IActionResult> OnPostAprobarAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var content = new StringContent("", Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/cotizaciones/{id}/aprobar", content);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Cotización aprobada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to reject quotation
    public async Task<IActionResult> OnPostRechazarAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var content = new StringContent("", Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/cotizaciones/{id}/rechazar", content);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Cotización rechazada" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to convert quotation to invoice
    public async Task<IActionResult> OnPostConvertirAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var content = new StringContent("", Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/cotizaciones/{id}/convertir", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
            return new JsonResult(new
            {
                success = true,
                message = "Cotización convertida a factura exitosamente",
                data = result
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to delete a quotation
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.DeleteAsync($"/api/cotizaciones/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Cotización eliminada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to view or download PDF
    // download parameter: true = download (attachment), false = view inline
    public async Task<IActionResult> OnGetPdfAsync(string id, bool download = false)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/cotizaciones/{id}/descargar-pdf");

        if (response.IsSuccessStatusCode)
        {
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"Cotizacion_{id}.pdf";

            if (response.Content.Headers.ContentDisposition?.FileName != null)
            {
                fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
            }

            if (download)
            {
                return File(fileBytes, "application/pdf", fileName);
            }
            else
            {
                // Return without fileName to use Content-Disposition: inline
                return File(fileBytes, "application/pdf");
            }
        }

        return new JsonResult(new { success = false, message = "Error al generar el PDF de la cotización." });
    }

    /// <summary>
    /// Extrae un mensaje de error legible de la respuesta del API
    /// </summary>
    private static string ExtraerMensajeError(string responseBody, int statusCode)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return statusCode switch
            {
                400 => "Datos inválidos. Verifique los campos e intente de nuevo.",
                401 => "No autorizado. Inicie sesión nuevamente.",
                403 => "No tiene permisos para realizar esta acción.",
                404 => "Recurso no encontrado.",
                _ => $"Error del servidor (código {statusCode})."
            };
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // ValidationProblem format: { "errors": { "field": ["msg"] }, "title": "..." }
            if (root.TryGetProperty("errors", out var errors))
            {
                var mensajes = new List<string>();
                foreach (var field in errors.EnumerateObject())
                {
                    foreach (var msg in field.Value.EnumerateArray())
                    {
                        mensajes.Add(msg.GetString() ?? field.Name);
                    }
                }
                return mensajes.Count > 0
                    ? string.Join(" | ", mensajes)
                    : root.TryGetProperty("title", out var title) ? title.GetString() ?? responseBody : responseBody;
            }

            // ActionResponse format: { "message": "..." }
            if (root.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? responseBody;
            }

            // Plain string
            if (root.ValueKind == JsonValueKind.String)
            {
                return root.GetString() ?? responseBody;
            }
        }
        catch
        {
            // Not JSON, return as-is
        }

        // If the raw message contains known English error patterns, translate them
        if (responseBody.Contains("An error occurred while saving the entity changes"))
        {
            return "Error al guardar los datos. Verifique que todos los campos requeridos estén completos y que las referencias (cliente, sucursal, terminal) sean válidas.";
        }

        return responseBody;
    }

    /// <summary>
    /// Obtiene la sucursal y terminal por defecto de la empresa
    /// </summary>
    private async Task<(string sucursalId, string terminalId)> ObtenerSucursalTerminalDefaultAsync(HttpClient client, string empresaId)
    {
        try
        {
            var response = await client.GetAsync($"/api/sucursales/empresa/{empresaId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var sucursales = doc.RootElement;

                // If wrapped in ActionResponse
                if (sucursales.ValueKind == JsonValueKind.Object && sucursales.TryGetProperty("result", out var result))
                    sucursales = result;

                if (sucursales.ValueKind == JsonValueKind.Array && sucursales.GetArrayLength() > 0)
                {
                    var primera = sucursales[0];
                    var sucId = primera.GetProperty("id").GetString() ?? "";

                    // Get first terminal of this sucursal
                    var termResponse = await client.GetAsync($"/api/terminales/sucursal/{sucId}");
                    if (termResponse.IsSuccessStatusCode)
                    {
                        var termContent = await termResponse.Content.ReadAsStringAsync();
                        using var termDoc = JsonDocument.Parse(termContent);
                        var terminales = termDoc.RootElement;

                        if (terminales.ValueKind == JsonValueKind.Object && terminales.TryGetProperty("result", out var termResult))
                            terminales = termResult;

                        if (terminales.ValueKind == JsonValueKind.Array && terminales.GetArrayLength() > 0)
                        {
                            var termId = terminales[0].GetProperty("id").GetString() ?? "";
                            return (sucId, termId);
                        }
                    }

                    return (sucId, Guid.Empty.ToString());
                }
            }
        }
        catch
        {
            // Fallback to empty GUIDs
        }

        return (Guid.Empty.ToString(), Guid.Empty.ToString());
    }
}
