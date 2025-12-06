using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class GastoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GastoModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public GastoModel(IHttpClientFactory httpClientFactory, ILogger<GastoModel> logger)
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

            var response = await client.GetAsync("/api/gastos");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(new { data = data ?? new List<dynamic>() });
            }

            _logger.LogWarning("Failed to load gastos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<dynamic>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading gastos");
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

            var response = await client.GetAsync($"/api/gastos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                return new JsonResult(data);
            }

            _logger.LogWarning("Gasto not found. ID: {Id}", id);
            return new JsonResult(new { error = "Gasto no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading gasto details. ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar el gasto" });
        }
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] dynamic gastoData)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value!.Errors.Count > 0)
                .Select(x => new
                {
                    Field = x.Key,
                    Message = x.Value!.Errors.First().ErrorMessage
                });

            return new JsonResult(new { success = false, message = "Datos inválidos", errors });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(gastoData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = gastoData.id.ToString() == "00000000-0000-0000-0000-000000000000";

            if (isNew)
            {
                response = await client.PostAsync("/api/gastos", content);
            }
            else
            {
                response = await client.PutAsync($"/api/gastos/{gastoData.id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Gasto creado exitosamente" : "Gasto actualizado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save gasto. Error: {Error}", error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving gasto");
            return new JsonResult(new { success = false, message = "Error al guardar el gasto" });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync($"/api/gastos/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Gasto eliminado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete gasto. ID: {Id}, Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting gasto. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el gasto" });
        }
    }

    public async Task<IActionResult> OnGetProveedoresAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/proveedores");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<dynamic>>(_jsonOptions);
                return new JsonResult(data ?? new List<dynamic>());
            }

            _logger.LogWarning("Failed to load proveedores. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<dynamic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading proveedores");
            return new JsonResult(new List<dynamic>());
        }
    }
}
