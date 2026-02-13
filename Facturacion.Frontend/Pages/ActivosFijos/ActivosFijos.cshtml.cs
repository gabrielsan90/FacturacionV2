using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.ActivosFijos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class ActivosFijosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ActivosFijosModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public ActivoFijo ActivoFijo { get; set; } = new();

    public string EmpresaId { get; set; } = "";

    public ActivosFijosModel(IHttpClientFactory httpClientFactory, ILogger<ActivosFijosModel> logger)
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
        EmpresaId = User.FindFirstValue("EmpresaId") ?? "";
    }

    // Handler for DataTable - Load all activos fijos for the current empresa
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { data = new List<ActivoFijo>() });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/activosfijos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var activos = await response.Content.ReadFromJsonAsync<List<ActivoFijo>>(_jsonOptions);
                return new JsonResult(new { data = activos ?? new List<ActivoFijo>() });
            }

            _logger.LogWarning("Failed to load activos fijos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<ActivoFijo>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading activos fijos");
            return new JsonResult(new { data = new List<ActivoFijo>() });
        }
    }

    // Handler to get a single activo fijo by ID
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

            var response = await client.GetAsync($"/api/activosfijos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var activo = await response.Content.ReadFromJsonAsync<ActivoFijo>(_jsonOptions);
                return new JsonResult(activo);
            }

            _logger.LogWarning("Activo fijo not found. ID: {Id}, Status code: {StatusCode}", id, response.StatusCode);
            return new JsonResult(new { error = "Activo fijo no encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading activo fijo details. ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar el activo fijo" });
        }
    }

    // Handler to save (create or update) an activo fijo
    public async Task<IActionResult> OnPostSaveAsync([FromBody] ActivoFijo activoData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(activoData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = activoData.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/activosfijos", content);
            }
            else
            {
                response = await client.PutAsync($"/api/activosfijos/{activoData.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Activo fijo creado exitosamente" : "Activo fijo actualizado exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save activo fijo. Status code: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving activo fijo");
            return new JsonResult(new { success = false, message = "Error al guardar el activo fijo" });
        }
    }

    // Handler to delete an activo fijo
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

            var response = await client.DeleteAsync($"/api/activosfijos/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Activo fijo eliminado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete activo fijo. ID: {Id}, Status code: {StatusCode}, Error: {Error}", id, response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activo fijo. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar el activo fijo" });
        }
    }

    // Handler to depreciate an activo fijo
    public async Task<IActionResult> OnPostDepreciarAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/activosfijos/{id}/depreciar", null);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Depreciación aplicada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to depreciate activo fijo. ID: {Id}, Status code: {StatusCode}, Error: {Error}", id, response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error depreciating activo fijo. ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al aplicar depreciación" });
        }
    }

    // Handler to dar de baja an activo fijo
    public async Task<IActionResult> OnPostDarDeBajaAsync([FromBody] DarDeBajaRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/activosfijos/{request.Id}/dar-de-baja", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Activo dado de baja exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to dar de baja activo fijo. ID: {Id}, Status code: {StatusCode}, Error: {Error}", request.Id, response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error giving baja to activo fijo. ID: {Id}", request?.Id);
            return new JsonResult(new { success = false, message = "Error al dar de baja el activo" });
        }
    }

    // Handler to get categorias activo (for dropdown)
    public async Task<IActionResult> OnGetCategoriasAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new List<CategoriaActivo>());
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/categoriasactivo/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var categorias = await response.Content.ReadFromJsonAsync<List<CategoriaActivo>>(_jsonOptions);
                return new JsonResult(categorias ?? new List<CategoriaActivo>());
            }

            _logger.LogWarning("Failed to load categorias activo. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<CategoriaActivo>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorias activo");
            return new JsonResult(new List<CategoriaActivo>());
        }
    }

    // Handler to get sucursales (for dropdown)
    public async Task<IActionResult> OnGetSucursalesAsync()
    {
        try
        {
            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new List<Sucursal>());
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/sucursales/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var sucursales = await response.Content.ReadFromJsonAsync<List<Sucursal>>(_jsonOptions);
                return new JsonResult(sucursales ?? new List<Sucursal>());
            }

            _logger.LogWarning("Failed to load sucursales. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<Sucursal>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sucursales");
            return new JsonResult(new List<Sucursal>());
        }
    }

    // DTO for dar de baja request
    public class DarDeBajaRequest
    {
        public string Id { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string Motivo { get; set; } = null!;
        public decimal? ValorVenta { get; set; }
    }

    // Handler to download Excel template
    public async Task<IActionResult> OnGetPlantillaAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/activosfijos/plantilla");

            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PlantillaActivosFijos.xlsx");
            }

            _logger.LogWarning("Failed to download template. Status: {StatusCode}", response.StatusCode);
            return BadRequest("Error al descargar la plantilla");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading template");
            return BadRequest("Error al descargar la plantilla");
        }
    }

    // Handler to import from Excel
    public async Task<IActionResult> OnPostImportarAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new JsonResult(new { success = false, message = "No se seleccionó ningún archivo" });
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrEmpty(empresaId))
            {
                return new JsonResult(new { success = false, message = "Empresa no encontrada" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var response = await client.PostAsync($"/api/activosfijos/importar/{empresaId}", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
                return new JsonResult(result);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to import. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing activos fijos");
            return new JsonResult(new { success = false, message = "Error al importar los activos fijos" });
        }
    }
}
