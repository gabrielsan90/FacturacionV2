using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Contador,Empleado")]
public class PlantillasAsientoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PlantillasAsientoModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public IEnumerable<SelectListItem> Periodos { get; set; } = new List<SelectListItem>();

    [BindProperty]
    public PlantillaAsientoDTO Plantilla { get; set; } = new();

    public PlantillasAsientoModel(IHttpClientFactory httpClientFactory, ILogger<PlantillasAsientoModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public async Task OnGetAsync()
    {
        await LoadPeriodosAsync();
    }

    // Handler for DataTable - Load all templates
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
                return new JsonResult(new { data = new List<PlantillaListDTO>() });
            }

            var response = await client.GetAsync($"/api/plantillasasiento?empresaId={empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var plantillas = await response.Content.ReadFromJsonAsync<List<PlantillaListDTO>>(_jsonOptions);
                return new JsonResult(new { data = plantillas ?? new List<PlantillaListDTO>() });
            }

            _logger.LogWarning("Failed to load plantillas asiento. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<PlantillaListDTO>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plantillas asiento");
            return new JsonResult(new { data = new List<PlantillaListDTO>() });
        }
    }

    // Handler to get a single template by ID with its lines
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

            var response = await client.GetAsync($"/api/plantillasasiento/{id}");

            if (response.IsSuccessStatusCode)
            {
                var plantilla = await response.Content.ReadFromJsonAsync<PlantillaAsientoDTO>(_jsonOptions);
                return new JsonResult(new { success = true, data = plantilla });
            }

            _logger.LogWarning("Plantilla asiento not found with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Plantilla no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plantilla asiento with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cargar la plantilla" });
        }
    }

    // Handler to get periods for dropdown
    public async Task<IActionResult> OnGetPeriodosAsync()
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
                return new JsonResult(new List<PlantillaPeriodoDTO>());
            }

            var response = await client.GetAsync($"/api/periodoscontables/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var periodos = await response.Content.ReadFromJsonAsync<List<PlantillaPeriodoDTO>>(_jsonOptions);
                return new JsonResult(periodos ?? new List<PlantillaPeriodoDTO>());
            }

            _logger.LogWarning("Failed to load periodos. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<PlantillaPeriodoDTO>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading periodos");
            return new JsonResult(new List<PlantillaPeriodoDTO>());
        }
    }

    // Handler to get movement-enabled accounts for Select2
    public async Task<IActionResult> OnGetCuentasAsync(string? q)
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
                return new JsonResult(new { results = new List<object>() });
            }

            var queryString = $"empresaId={empresaId}&aceptaMovimientos=true";
            if (!string.IsNullOrEmpty(q))
            {
                queryString += $"&search={Uri.EscapeDataString(q)}";
            }

            var response = await client.GetAsync($"/api/cuentascontables?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var cuentas = await response.Content.ReadFromJsonAsync<List<PlantillaCuentaDTO>>(_jsonOptions);

                var results = cuentas?.Select(c => new
                {
                    id = c.Id.ToString(),
                    text = $"{c.Codigo} - {c.Nombre}",
                    codigo = c.Codigo,
                    nombre = c.Nombre
                }).ToList();

                if (results == null)
                    return new JsonResult(new { results = new List<object>() });
                return new JsonResult(new { results });
            }

            _logger.LogWarning("Failed to load cuentas. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { results = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cuentas");
            return new JsonResult(new { results = new List<object>() });
        }
    }

    // Handler to save (create or update) a template
    public async Task<IActionResult> OnPostSaveAsync([FromBody] PlantillaAsientoDTO plantillaData)
    {
        try
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

            // Validate template has lines
            if (plantillaData.Lineas == null || !plantillaData.Lineas.Any())
            {
                return new JsonResult(new { success = false, message = "La plantilla debe tener al menos una línea" });
            }

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId) || !Guid.TryParse(empresaId, out var empresaGuid))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
            }

            var json = JsonSerializer.Serialize(plantillaData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = plantillaData.Id == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/plantillasasiento", content);
            }
            else
            {
                response = await client.PutAsync($"/api/plantillasasiento/{plantillaData.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Plantilla asiento {Action} successfully. ID: {Id}", isNew ? "created" : "updated", plantillaData.Id);
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Plantilla creada exitosamente" : "Plantilla actualizada exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save plantilla asiento. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving plantilla asiento");
            return new JsonResult(new { success = false, message = "Error al guardar la plantilla: " + ex.Message });
        }
    }

    // Handler to delete a template
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

            var response = await client.DeleteAsync($"/api/plantillasasiento/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Plantilla asiento deleted successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Plantilla eliminada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete plantilla asiento with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plantilla asiento with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar la plantilla" });
        }
    }

    // Handler to toggle active status
    public async Task<IActionResult> OnPostToggleActivoAsync(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsync($"/api/plantillasasiento/{id}/toggle-activo", null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Plantilla asiento toggled successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Estado actualizado exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to toggle plantilla asiento with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling plantilla asiento with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al cambiar el estado" });
        }
    }

    // Handler to use a template - creates a new asiento from template
    public async Task<IActionResult> OnPostUsarAsync([FromBody] UsarPlantillaDTO usarData)
    {
        try
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

            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(usarData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/plantillasasiento/{usarData.PlantillaId}/usar", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AsientoCreatedDTO>(_jsonOptions);
                _logger.LogInformation("Asiento created from plantilla. Plantilla ID: {PlantillaId}, Asiento ID: {AsientoId}",
                    usarData.PlantillaId, result?.AsientoId);
                return new JsonResult(new
                {
                    success = true,
                    message = "Asiento contable creado exitosamente desde plantilla",
                    asientoId = result?.AsientoId,
                    numeroAsiento = result?.NumeroAsiento
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to use plantilla. Error: {Error}", error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using plantilla asiento");
            return new JsonResult(new { success = false, message = "Error al usar la plantilla: " + ex.Message });
        }
    }

    private async Task LoadPeriodosAsync()
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
                Periodos = new List<SelectListItem>();
                return;
            }

            var response = await client.GetAsync($"/api/periodoscontables/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var periodos = await response.Content.ReadFromJsonAsync<List<PlantillaPeriodoDTO>>(_jsonOptions);

                Periodos = periodos?.Where(p => p.Activo).Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Nombre
                }).ToList() ?? new List<SelectListItem>();
            }
            else
            {
                Periodos = new List<SelectListItem>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading periodos for dropdown");
            Periodos = new List<SelectListItem>();
        }
    }
}

// DTOs for PlantillasAsiento
public class PlantillaAsientoDTO
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string TipoAsiento { get; set; } = "DIA"; // DIA, AJU, CIE, APE
    public string? ConceptoTemplate { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? UltimaUtilizacion { get; set; }
    public List<PlantillaLineaDTO>? Lineas { get; set; }
}

public class PlantillaLineaDTO
{
    public Guid CuentaContableId { get; set; }
    public string? CuentaCodigo { get; set; }
    public string? CuentaNombre { get; set; }
    public string? Descripcion { get; set; }
    public string TipoMonto { get; set; } = "FIJO"; // FIJO, VARIABLE, PORCENTAJE
    public decimal MontoDebeFijo { get; set; }
    public decimal MontoHaberFijo { get; set; }
    public string? NaturalezaVariable { get; set; } // DEBE, HABER (for variable amounts)
    public decimal? Porcentaje { get; set; }
    public int Orden { get; set; }
}

public class PlantillaListDTO
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string TipoAsiento { get; set; } = "";
    public int NumeroLineas { get; set; }
    public DateTime? UltimaUtilizacion { get; set; }
    public bool Activo { get; set; }
}

public class UsarPlantillaDTO
{
    public Guid PlantillaId { get; set; }
    public decimal MontoVariable { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Today;
    public Guid PeriodoContableId { get; set; }
    public string? Concepto { get; set; }
}

public class AsientoCreatedDTO
{
    public Guid AsientoId { get; set; }
    public string NumeroAsiento { get; set; } = "";
}

public class PlantillaPeriodoDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
}

public class PlantillaCuentaDTO
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
}
