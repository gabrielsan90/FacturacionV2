using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Ventas;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class PedidosVentaModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public string EmpresaId { get; set; } = "";

    public PedidosVentaModel(IHttpClientFactory httpClientFactory)
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

    // Handler for DataTable - Load all sales orders for the current empresa
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

        var url = $"/api/pedidosventa/empresa/{empresaId}";
        if (!string.IsNullOrEmpty(estado) && estado != "TODAS")
        {
            url += $"?estado={estado}";
        }

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            // API returns ActionResponse<T> with result property
            if (doc.RootElement.TryGetProperty("result", out var resultElement))
            {
                return new JsonResult(new { data = resultElement });
            }

            // Fallback: try to use the whole response as data
            return new JsonResult(new { data = doc.RootElement });
        }

        return new JsonResult(new { data = new List<object>() });
    }

    // Handler to get a single sales order by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/pedidosventa/{id}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            if (doc.RootElement.TryGetProperty("result", out var resultElement))
            {
                return new JsonResult(new { success = true, data = resultElement });
            }
            return new JsonResult(new { success = true, data = doc.RootElement });
        }

        return new JsonResult(new { success = false, message = "Pedido de venta no encontrado" });
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

    // Handler to save (create or update) a sales order
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement pedidoData)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var json = JsonSerializer.Serialize(pedidoData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;

        // Check if it's a new order or an update
        bool isNew = !pedidoData.TryGetProperty("id", out var idProp) ||
                     idProp.GetString() == "00000000-0000-0000-0000-000000000000" ||
                     string.IsNullOrEmpty(idProp.GetString());

        if (isNew)
        {
            response = await client.PostAsync("/api/pedidosventa", content);
        }
        else
        {
            var id = idProp.GetString();
            response = await client.PutAsync($"/api/pedidosventa/{id}", content);
        }

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
            return new JsonResult(new
            {
                success = true,
                message = isNew ? "Pedido de venta creado exitosamente" : "Pedido de venta actualizado exitosamente",
                data = result
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to approve sales order
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
        var response = await client.PostAsync($"/api/pedidosventa/{id}/aprobar", content);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Pedido de venta aprobado exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to convert sales order to invoice
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
        var response = await client.PostAsync($"/api/pedidosventa/{id}/convertir", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
            return new JsonResult(new
            {
                success = true,
                message = "Pedido convertido a factura exitosamente",
                data = result
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to delete a sales order
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.DeleteAsync($"/api/pedidosventa/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Pedido de venta eliminado exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }
}
