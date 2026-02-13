using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Gastos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class GastoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GastoModel> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public GastoModel(IHttpClientFactory httpClientFactory, ILogger<GastoModel> logger)
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

            var response = await client.GetAsync($"/api/Gastos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var gastos = await response.Content.ReadFromJsonAsync<List<Gasto>>(_jsonOptions)
                    ?? new List<Gasto>();
                var data = gastos.Select(g => new
                {
                    id = g.Id,
                    fecha = g.FechaGasto,
                    tipoDocumento = string.IsNullOrWhiteSpace(g.Comprobante) ? "Gasto" : g.Comprobante,
                    numeroDocumento = g.NumeroDocumento,
                    proveedorNombre = g.Proveedor?.Nombre ?? "Sin proveedor",
                    categoria = g.CategoriaGasto?.Nombre ?? "",
                    descripcion = g.Descripcion,
                    monto = g.MontoTotal
                }).ToList();
                return new JsonResult(new { data });
            }

            _logger.LogWarning("Failed to load gastos. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading gastos");
            return new JsonResult(new { data = new List<object>() });
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

            var response = await client.GetAsync($"/api/Gastos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var gasto = await response.Content.ReadFromJsonAsync<Gasto>(_jsonOptions);
                if (gasto == null)
                {
                    return new JsonResult(new { error = "Gasto no encontrado" });
                }

                return new JsonResult(new
                {
                    id = gasto.Id,
                    fecha = gasto.FechaGasto,
                    proveedorId = gasto.ProveedorId,
                    tipoDocumento = gasto.Comprobante,
                    numeroDocumento = gasto.NumeroDocumento,
                    categoriaGastoId = gasto.CategoriaGastoId,
                    monto = gasto.MontoTotal,
                    descripcion = gasto.Descripcion,
                    metodoPago = (int)gasto.FormaPago
                });
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

    public async Task<IActionResult> OnPostSaveAsync([FromBody] GastoSaveRequest gastoData)
    {
        if (gastoData == null)
        {
            return new JsonResult(new { success = false, message = "Datos invalidos" });
        }

        if (gastoData.CategoriaGastoId <= 0 || gastoData.Monto <= 0 ||
            string.IsNullOrWhiteSpace(gastoData.NumeroDocumento) ||
            string.IsNullOrWhiteSpace(gastoData.Descripcion) ||
            gastoData.Fecha == default ||
            gastoData.FormaPago <= 0)
        {
            return new JsonResult(new { success = false, message = "Datos invalidos" });
        }

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

            if (!Enum.IsDefined(typeof(FormaPago), gastoData.FormaPago))
            {
                return new JsonResult(new { success = false, message = "Forma de pago invalida" });
            }

            var dto = new GastoDTO
            {
                Id = gastoData.Id.HasValue && gastoData.Id.Value != Guid.Empty ? gastoData.Id : null,
                EmpresaId = empresaGuid,
                ProveedorId = gastoData.ProveedorId,
                CategoriaGastoId = gastoData.CategoriaGastoId,
                NumeroDocumento = gastoData.NumeroDocumento.Trim(),
                FechaGasto = gastoData.Fecha,
                Descripcion = gastoData.Descripcion.Trim(),
                MontoSubtotal = gastoData.Monto,
                MontoImpuesto = 0,
                MontoTotal = gastoData.Monto,
                Moneda = TipoMoneda.CRC,
                FormaPago = (FormaPago)gastoData.FormaPago,
                EstadoPago = EstadoPago.Pendiente,
                MontoPagado = 0,
                Comprobante = string.IsNullOrWhiteSpace(gastoData.TipoDocumento) ? null : gastoData.TipoDocumento
            };

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            bool isNew = !gastoData.Id.HasValue || gastoData.Id.Value == Guid.Empty;

            if (isNew)
            {
                response = await client.PostAsync("/api/Gastos", content);
            }
            else
            {
                response = await client.PutAsync($"/api/Gastos/{gastoData.Id}", content);
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

            var response = await client.DeleteAsync($"/api/Gastos/{id}");

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

            var empresaId = User.FindFirstValue("EmpresaId");
            if (string.IsNullOrWhiteSpace(empresaId))
            {
                return new JsonResult(new List<object>());
            }

            var response = await client.GetAsync($"/api/Proveedores/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<Proveedor>>(_jsonOptions);
                return new JsonResult(data ?? new List<Proveedor>());
            }

            _logger.LogWarning("Failed to load proveedores. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading proveedores");
            return new JsonResult(new List<object>());
        }
    }

    public async Task<IActionResult> OnGetCategoriasGastoAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FacturacionApi");

            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/CategoriasGasto/activas");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<CategoriaGasto>>(_jsonOptions);
                return new JsonResult(data ?? new List<CategoriaGasto>());
            }

            _logger.LogWarning("Failed to load categorias gasto. Status code: {StatusCode}", response.StatusCode);
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categorias gasto");
            return new JsonResult(new List<object>());
        }
    }

    public sealed class GastoSaveRequest
    {
        public Guid? Id { get; set; }
        public DateTime Fecha { get; set; }
        public Guid? ProveedorId { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public int CategoriaGastoId { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public int FormaPago { get; set; }
    }
}

