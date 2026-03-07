using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Presupuestos;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Contador")]
public class PresupuestosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public PresupuestosModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public void OnGet()
    {
    }

    // Lista de presupuestos para DataTable
    public async Task<IActionResult> OnGetDataAsync()
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null)
            {
                return new JsonResult(new { data = new List<object>() });
            }

            var response = await client.GetAsync($"/api/presupuestos/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = $"{{\"data\":{content}}}", ContentType = "application/json", StatusCode = 200 };
            }

            return new JsonResult(new { data = new List<object>() });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { data = new List<object>() });
        }
    }

    // Detalle de presupuesto con líneas
    public async Task<IActionResult> OnGetDetalleAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"/api/presupuestos/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return new JsonResult(new { success = false, message = "Presupuesto no encontrado" });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error al obtener el detalle del presupuesto" });
        }
    }

    // Cuentas contables para Select2
    public async Task<IActionResult> OnGetCuentasContablesAsync()
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null)
            {
                return new JsonResult(new List<object>());
            }

            var response = await client.GetAsync($"/api/cuentascontables/empresa/{empresaId}/movimiento");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            return new JsonResult(new List<object>());
        }
    }

    // Centros de costo para Select2
    public async Task<IActionResult> OnGetCentrosCostoAsync()
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null)
            {
                return new JsonResult(new List<object>());
            }

            var response = await client.GetAsync($"/api/centroscosto/empresa/{empresaId}/movimiento");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = 200 };
            }

            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            return new JsonResult(new List<object>());
        }
    }

    // Guardar presupuesto (crear o actualizar)
    public async Task<IActionResult> OnPostSaveAsync([FromBody] PresupuestoDto dto)
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();
            if (empresaId == null)
            {
                return new JsonResult(new { success = false, message = "Empresa no definida" });
            }

            var presupuesto = new
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                EmpresaId = empresaId.Value,
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                AnioFiscal = dto.AnioFiscal,
                TipoPresupuesto = dto.TipoPresupuesto,
                Observaciones = dto.Observaciones
            };

            var json = JsonSerializer.Serialize(presupuesto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (dto.Id == Guid.Empty)
            {
                response = await client.PostAsync("/api/presupuestos", content);
            }
            else
            {
                response = await client.PutAsync($"/api/presupuestos/{dto.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error al guardar el presupuesto" });
        }
    }

    // Eliminar presupuesto
    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"/api/presupuestos/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error al eliminar el presupuesto" });
        }
    }

    // Cambiar estado del presupuesto
    public async Task<IActionResult> OnPostCambiarEstadoAsync([FromBody] CambiarEstadoDto dto)
    {
        try
        {
            var client = CreateApiClient();
            var json = JsonSerializer.Serialize(new { nuevoEstado = dto.NuevoEstado });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/presupuestos/{dto.PresupuestoId}/cambiar-estado", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error al cambiar el estado" });
        }
    }

    // Guardar línea de presupuesto
    public async Task<IActionResult> OnPostSaveLineaAsync([FromBody] LineaPresupuestoDto dto)
    {
        try
        {
            var client = CreateApiClient();
            var empresaId = GetEmpresaId();

            var linea = new
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                PresupuestoId = dto.PresupuestoId,
                CuentaContableId = dto.CuentaContableId,
                CentroCostoId = string.IsNullOrEmpty(dto.CentroCostoId) ? (Guid?)null : Guid.Parse(dto.CentroCostoId),
                EmpresaId = empresaId ?? Guid.Empty,
                MontoEnero = dto.MontoEnero,
                MontoFebrero = dto.MontoFebrero,
                MontoMarzo = dto.MontoMarzo,
                MontoAbril = dto.MontoAbril,
                MontoMayo = dto.MontoMayo,
                MontoJunio = dto.MontoJunio,
                MontoJulio = dto.MontoJulio,
                MontoAgosto = dto.MontoAgosto,
                MontoSeptiembre = dto.MontoSeptiembre,
                MontoOctubre = dto.MontoOctubre,
                MontoNoviembre = dto.MontoNoviembre,
                MontoDiciembre = dto.MontoDiciembre,
                Notas = dto.Notas
            };

            var json = JsonSerializer.Serialize(linea, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (dto.Id == Guid.Empty)
            {
                response = await client.PostAsync($"/api/presupuestos/{dto.PresupuestoId}/lineas", content);
            }
            else
            {
                response = await client.PutAsync($"/api/presupuestos/{dto.PresupuestoId}/lineas/{dto.Id}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error al guardar la línea" });
        }
    }

    // Eliminar línea de presupuesto
    public async Task<IActionResult> OnPostDeleteLineaAsync(Guid presupuestoId, Guid lineaId)
    {
        try
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"/api/presupuestos/{presupuestoId}/lineas/{lineaId}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = "Error al eliminar la línea" });
        }
    }

    private HttpClient CreateApiClient()
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

    private Guid? GetEmpresaId()
    {
        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrEmpty(empresaId) || !Guid.TryParse(empresaId, out var guid))
            return null;
        return guid;
    }

    // DTOs
    public class PresupuestoDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int AnioFiscal { get; set; }
        public string TipoPresupuesto { get; set; } = "ANU";
        public string? Observaciones { get; set; }
    }

    public class CambiarEstadoDto
    {
        public Guid PresupuestoId { get; set; }
        public string NuevoEstado { get; set; } = string.Empty;
    }

    public class LineaPresupuestoDto
    {
        public Guid Id { get; set; }
        public Guid PresupuestoId { get; set; }
        public Guid CuentaContableId { get; set; }
        public string? CentroCostoId { get; set; }
        public decimal MontoEnero { get; set; }
        public decimal MontoFebrero { get; set; }
        public decimal MontoMarzo { get; set; }
        public decimal MontoAbril { get; set; }
        public decimal MontoMayo { get; set; }
        public decimal MontoJunio { get; set; }
        public decimal MontoJulio { get; set; }
        public decimal MontoAgosto { get; set; }
        public decimal MontoSeptiembre { get; set; }
        public decimal MontoOctubre { get; set; }
        public decimal MontoNoviembre { get; set; }
        public decimal MontoDiciembre { get; set; }
        public string? Notas { get; set; }
    }
}
