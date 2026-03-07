using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Facturacion.Frontend.Pages.ActivosFijos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class CategoriasActivoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CategoriasActivoModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public CategoriaActivo CategoriaActivo { get; set; } = new();

    public CategoriasActivoModel(IHttpClientFactory httpClientFactory, ILogger<CategoriasActivoModel> logger)
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

    // Handler for DataTable - Load all categorias activo
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
                return new JsonResult(new { data = new List<CategoriaActivo>() });
            }

            var response = await client.GetAsync($"/api/categoriasactivo/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var categorias = await response.Content.ReadFromJsonAsync<List<CategoriaActivo>>(_jsonOptions);
                return new JsonResult(new { data = categorias ?? new List<CategoriaActivo>() });
            }

            _logger.LogWarning("Failed to load categorias activo. Status: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<CategoriaActivo>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorias activo");
            return new JsonResult(new { data = new List<CategoriaActivo>() });
        }
    }

    // Handler to get a single categoria activo by ID
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

            var response = await client.GetAsync($"/api/categoriasactivo/{id}");

            if (response.IsSuccessStatusCode)
            {
                var categoria = await response.Content.ReadFromJsonAsync<CategoriaActivo>(_jsonOptions);
                return new JsonResult(categoria);
            }

            _logger.LogWarning("Categoria activo not found with ID: {Id}", id);
            return new JsonResult(new { error = "Categoría no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categoria activo with ID: {Id}", id);
            return new JsonResult(new { error = "Error al cargar la categoría" });
        }
    }

    // Handler to save (create or update) a categoria activo
    public async Task<IActionResult> OnPostSaveAsync([FromBody] JsonElement categoriaElement)
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
            if (string.IsNullOrWhiteSpace(empresaId) || !Guid.TryParse(empresaId, out var empresaGuid))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
            }

            var node = JsonNode.Parse(categoriaElement.GetRawText())?.AsObject();
            if (node == null)
            {
                return new JsonResult(new { success = false, message = "No se recibieron los datos de la categoría." });
            }

            // Determine if new or update
            var idStr = node["id"]?.GetValue<string>() ?? "";
            bool isNew = !Guid.TryParse(idStr, out var idGuid) || idGuid == Guid.Empty;

            if (isNew)
            {
                node["id"] = Guid.Empty.ToString();
            }

            // Inject empresaId if missing
            var existingEmpresa = node["empresaId"]?.GetValue<string>() ?? "";
            if (!Guid.TryParse(existingEmpresa, out var existingEmpresaGuid) || existingEmpresaGuid == Guid.Empty)
            {
                node["empresaId"] = empresaGuid.ToString();
            }

            var json = node.ToJsonString();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            if (isNew)
            {
                response = await client.PostAsync("/api/categoriasactivo", content);
            }
            else
            {
                response = await client.PutAsync($"/api/categoriasactivo/{idGuid}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Categoria activo {Action} successfully", isNew ? "created" : "updated");
                return new JsonResult(new
                {
                    success = true,
                    message = isNew ? "Categoría creada exitosamente" : "Categoría actualizada exitosamente"
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to save categoria activo. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving categoria activo");
            return new JsonResult(new { success = false, message = "Error al guardar la categoría" });
        }
    }

    // Handler to delete a categoria activo
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

            var response = await client.DeleteAsync($"/api/categoriasactivo/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Categoria activo deleted successfully. ID: {Id}", id);
                return new JsonResult(new { success = true, message = "Categoría eliminada exitosamente" });
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to delete categoria activo with ID: {Id}. Error: {Error}", id, error);
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting categoria activo with ID: {Id}", id);
            return new JsonResult(new { success = false, message = "Error al eliminar la categoría" });
        }
    }

    // Handler to generate a generic catalog of asset categories
    public async Task<IActionResult> OnPostGenerarCatalogoAsync()
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
            if (string.IsNullOrWhiteSpace(empresaId) || !Guid.TryParse(empresaId, out var empresaGuid))
            {
                return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
            }

            var catalogoGenerico = new[]
            {
                new { Codigo = "EDI", Nombre = "Edificios y Construcciones", Descripcion = "Edificios, locales comerciales y construcciones", VidaUtilAnios = 50, PorcentajeDepreciacionAnual = 2.00m },
                new { Codigo = "MAQ", Nombre = "Maquinaria y Equipo", Descripcion = "Maquinaria industrial y equipo de producción", VidaUtilAnios = 10, PorcentajeDepreciacionAnual = 10.00m },
                new { Codigo = "VEH", Nombre = "Vehículos", Descripcion = "Automóviles, camiones y vehículos de trabajo", VidaUtilAnios = 10, PorcentajeDepreciacionAnual = 10.00m },
                new { Codigo = "MOB", Nombre = "Mobiliario y Equipo de Oficina", Descripcion = "Escritorios, sillas, archivadores y equipo de oficina", VidaUtilAnios = 10, PorcentajeDepreciacionAnual = 10.00m },
                new { Codigo = "EQC", Nombre = "Equipo de Cómputo", Descripcion = "Computadoras, laptops, servidores y periféricos", VidaUtilAnios = 4, PorcentajeDepreciacionAnual = 25.00m },
                new { Codigo = "HER", Nombre = "Herramientas", Descripcion = "Herramientas manuales y eléctricas", VidaUtilAnios = 5, PorcentajeDepreciacionAnual = 20.00m },
                new { Codigo = "EQT", Nombre = "Equipo de Transporte", Descripcion = "Montacargas, carretillas y equipo de transporte interno", VidaUtilAnios = 10, PorcentajeDepreciacionAnual = 10.00m },
                new { Codigo = "EQM", Nombre = "Equipo Médico y Laboratorio", Descripcion = "Instrumentos médicos, de laboratorio y científicos", VidaUtilAnios = 10, PorcentajeDepreciacionAnual = 10.00m },
                new { Codigo = "SOF", Nombre = "Software y Licencias", Descripcion = "Licencias de software, sistemas y aplicaciones", VidaUtilAnios = 3, PorcentajeDepreciacionAnual = 33.33m },
                new { Codigo = "INS", Nombre = "Instalaciones y Mejoras", Descripcion = "Mejoras a propiedades arrendadas e instalaciones", VidaUtilAnios = 20, PorcentajeDepreciacionAnual = 5.00m },
                new { Codigo = "COM", Nombre = "Equipo de Comunicación", Descripcion = "Teléfonos, centrales telefónicas y equipo de redes", VidaUtilAnios = 5, PorcentajeDepreciacionAnual = 20.00m },
            };

            int creados = 0;
            var errores = new List<string>();

            foreach (var cat in catalogoGenerico)
            {
                var categoriaJson = JsonSerializer.Serialize(new
                {
                    id = Guid.Empty,
                    empresaId = empresaGuid,
                    codigo = cat.Codigo,
                    nombre = cat.Nombre,
                    descripcion = cat.Descripcion,
                    vidaUtilAnios = cat.VidaUtilAnios,
                    porcentajeDepreciacionAnual = cat.PorcentajeDepreciacionAnual,
                    activo = true
                });

                var content = new StringContent(categoriaJson, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("/api/categoriasactivo", content);

                if (response.IsSuccessStatusCode)
                {
                    creados++;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    errores.Add($"{cat.Codigo} - {cat.Nombre}: {error}");
                }
            }

            return new JsonResult(new
            {
                success = true,
                message = $"Catálogo generado: {creados} categorías creadas.",
                creados,
                totalErrores = errores.Count,
                errores
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating generic catalog");
            return new JsonResult(new { success = false, message = "Error al generar el catálogo genérico" });
        }
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

            var response = await client.GetAsync("/api/categoriasactivo/plantilla");

            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PlantillaCategoriasActivo.xlsx");
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

            var response = await client.PostAsync($"/api/categoriasactivo/importar/{empresaId}", content);

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
            _logger.LogError(ex, "Error importing categorias activo");
            return new JsonResult(new { success = false, message = "Error al importar las categorías de activo" });
        }
    }
}
