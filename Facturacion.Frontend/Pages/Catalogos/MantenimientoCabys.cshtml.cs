using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Catalogos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class MantenimientoCabysModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MantenimientoCabysModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MantenimientoCabysModel(IHttpClientFactory httpClientFactory, ILogger<MantenimientoCabysModel> logger)
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
        return new JsonResult(new { data = new List<object>() });
    }

    public async Task<IActionResult> OnGetBuscarAsync(string termino)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Catalogos/cabys?busqueda={termino}");

            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<CabysResponse<List<CabysItem>>>(_jsonOptions);
                var items = payload?.Data ?? new List<CabysItem>();
                var resultado = items.Select(item => new
                {
                    codigo = item.Codigo ?? "",
                    descripcion = item.Descripcion ?? "",
                    categoria = item.Categoria ?? "",
                    impuestoSugerido = item.TarifaImpuesto ?? 0
                }).ToList();
                return new JsonResult(resultado);
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando CABYS");
            return new JsonResult(new List<object>());
        }
    }

    public async Task<IActionResult> OnGetDetalleAsync(string codigo)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/api/Catalogos/cabys/{codigo}");

            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<CabysResponse<CabysItem>>(_jsonOptions);
                if (payload?.Data == null)
                {
                    return new JsonResult(new { error = "No encontrado" });
                }

                return new JsonResult(new
                {
                    codigo = payload.Data.Codigo ?? "",
                    descripcion = payload.Data.Descripcion ?? "",
                    categoria = payload.Data.Categoria ?? "",
                    impuestoSugerido = payload.Data.TarifaImpuesto ?? 0
                });
            }

            return new JsonResult(new { error = "No encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading CABYS detalle");
            return new JsonResult(new { error = "Error al cargar el detalle" });
        }
    }

    public async Task<IActionResult> OnPostImportarAsync()
    {
        return new JsonResult(new { success = false, message = "No disponible en backend" });
    }

    private sealed class CabysResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    private sealed class CabysItem
    {
        public string? Codigo { get; set; }
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public decimal? TarifaImpuesto { get; set; }
    }
}

