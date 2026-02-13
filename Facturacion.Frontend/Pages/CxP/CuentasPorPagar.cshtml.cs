using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.CxP;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class CuentasPorPagarModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    [BindProperty]
    public CuentaPorPagar CuentaPorPagar { get; set; } = new();

    public IEnumerable<SelectListItem> Proveedores { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> CuentasBancarias { get; set; } = new List<SelectListItem>();

    public CuentasPorPagarModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public async Task OnGetAsync()
    {
        await LoadSelectListsAsync();
    }

    // Handler for DataTable - Load all cuentas por pagar
    public async Task<IActionResult> OnGetDataAsync()
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
            return new JsonResult(new { data = new List<CuentaPorPagar>() });
        }

        var response = await client.GetAsync($"/api/cuentasporpagar/empresa/{empresaId}");

        if (response.IsSuccessStatusCode)
        {
            var cuentas = await response.Content.ReadFromJsonAsync<List<CuentaPorPagar>>(_jsonOptions);
            return new JsonResult(new { data = cuentas ?? new List<CuentaPorPagar>() });
        }

        return new JsonResult(new { data = new List<CuentaPorPagar>() });
    }

    // Handler to get summary statistics
    public async Task<IActionResult> OnGetSummaryAsync()
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
            return new JsonResult(new { totalPendiente = 0, totalVencido = 0, pagosDelMes = 0 });
        }

        decimal totalPendiente = 0;
        decimal totalVencido = 0;
        decimal pagosDelMes = 0;

        // Get pending accounts
        var pendientesResponse = await client.GetAsync($"/api/cuentasporpagar/empresa/{empresaId}/pendientes");
        if (pendientesResponse.IsSuccessStatusCode)
        {
            var pendientes = await pendientesResponse.Content.ReadFromJsonAsync<List<CuentaPorPagar>>(_jsonOptions);
            totalPendiente = pendientes?.Sum(c => c.MontoSaldo) ?? 0;
        }

        // Get overdue accounts
        var vencidasResponse = await client.GetAsync($"/api/cuentasporpagar/empresa/{empresaId}/vencidas");
        if (vencidasResponse.IsSuccessStatusCode)
        {
            var vencidas = await vencidasResponse.Content.ReadFromJsonAsync<List<CuentaPorPagar>>(_jsonOptions);
            totalVencido = vencidas?.Sum(c => c.MontoSaldo) ?? 0;
        }

        // Calculate payments of current month
        var todasResponse = await client.GetAsync($"/api/cuentasporpagar/empresa/{empresaId}");
        if (todasResponse.IsSuccessStatusCode)
        {
            var todas = await todasResponse.Content.ReadFromJsonAsync<List<CuentaPorPagar>>(_jsonOptions);
            if (todas != null)
            {
                var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                pagosDelMes = todas
                    .Where(c => c.FechaModificacion.HasValue && c.FechaModificacion.Value >= primerDiaMes)
                    .Sum(c => c.MontoAbonado);
            }
        }

        return new JsonResult(new
        {
            totalPendiente,
            totalVencido,
            pagosDelMes
        });
    }

    // Handler to get a single cuenta por pagar by ID
    public async Task<IActionResult> OnGetDetailsAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/cuentasporpagar/{id}");

        if (response.IsSuccessStatusCode)
        {
            var cuenta = await response.Content.ReadFromJsonAsync<CuentaPorPagar>(_jsonOptions);
            return new JsonResult(cuenta);
        }

        return new JsonResult(new { error = "Cuenta por pagar no encontrada" });
    }

    // Handler to save (create or update) a cuenta por pagar
    public async Task<IActionResult> OnPostSaveAsync([FromBody] CuentaPorPagar cuentaData)
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

        var empresaId = User.FindFirstValue("EmpresaId");
        if (string.IsNullOrWhiteSpace(empresaId) || !Guid.TryParse(empresaId, out var empresaGuid))
        {
            return new JsonResult(new { success = false, message = "Empresa no definida para el usuario" });
        }

        if (cuentaData.EmpresaId == Guid.Empty)
        {
            cuentaData.EmpresaId = empresaGuid;
        }

        // Initialize saldo to original amount for new records
        if (cuentaData.Id == Guid.Empty)
        {
            cuentaData.MontoSaldo = cuentaData.MontoOriginal;
        }

        var json = JsonSerializer.Serialize(cuentaData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        bool isNew = cuentaData.Id == Guid.Empty;

        if (isNew)
        {
            response = await client.PostAsync("/api/cuentasporpagar", content);
        }
        else
        {
            response = await client.PutAsync($"/api/cuentasporpagar/{cuentaData.Id}", content);
        }

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = isNew ? "Cuenta por pagar creada exitosamente" : "Cuenta por pagar actualizada exitosamente"
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to register a payment
    public async Task<IActionResult> OnPostRegistrarPagoAsync([FromBody] AbonoPago pagoData)
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

        var json = JsonSerializer.Serialize(pagoData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"/api/cuentasporpagar/{pagoData.CuentaPorPagarId}/pagar", content);

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new
            {
                success = true,
                message = "Pago registrado exitosamente"
            });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    // Handler to delete a cuenta por pagar
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.DeleteAsync($"/api/cuentasporpagar/{id}");

        if (response.IsSuccessStatusCode)
        {
            return new JsonResult(new { success = true, message = "Cuenta por pagar eliminada exitosamente" });
        }

        var error = await response.Content.ReadAsStringAsync();
        return new JsonResult(new { success = false, message = error });
    }

    private async Task LoadSelectListsAsync()
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
            return;
        }

        // Load Proveedores
        var proveedoresResponse = await client.GetAsync($"/api/proveedores/empresa/{empresaId}");
        if (proveedoresResponse.IsSuccessStatusCode)
        {
            var proveedores = await proveedoresResponse.Content.ReadFromJsonAsync<List<Proveedor>>(_jsonOptions);
            Proveedores = proveedores?
                .Where(p => p.Activo)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Nombre
                })
                .OrderBy(p => p.Text)
                .ToList() ?? new List<SelectListItem>();
        }

        // Load Cuentas Bancarias
        var cuentasResponse = await client.GetAsync($"/api/cuentasbancarias/empresa/{empresaId}");
        if (cuentasResponse.IsSuccessStatusCode)
        {
            var cuentas = await cuentasResponse.Content.ReadFromJsonAsync<List<CuentaBancaria>>(_jsonOptions);
            CuentasBancarias = cuentas?
                .Where(c => c.Activo)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NumeroCuenta} - {c.Nombre}"
                })
                .OrderBy(c => c.Text)
                .ToList() ?? new List<SelectListItem>();
        }
    }
}
