using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Facturacion.Shared.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.CxP;

[Authorize(Roles = "SuperUser,Administrador de Empresa,Empleado")]
public class EstadoCuentaProveedorModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public IEnumerable<SelectListItem> Proveedores { get; set; } = new List<SelectListItem>();

    public EstadoCuentaProveedorModel(IHttpClientFactory httpClientFactory)
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
        await LoadProveedoresAsync();
    }

    // Handler to get proveedor information
    public async Task<IActionResult> OnGetProveedorInfoAsync(string proveedorId)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"/api/proveedores/{proveedorId}");

        if (response.IsSuccessStatusCode)
        {
            var proveedor = await response.Content.ReadFromJsonAsync<Proveedor>(_jsonOptions);
            return new JsonResult(proveedor);
        }

        return new JsonResult(new { error = "Proveedor no encontrado" });
    }

    // Handler to get estado de cuenta
    public async Task<IActionResult> OnGetEstadoCuentaAsync(string proveedorId, string? fechaDesde, string? fechaHasta)
    {
        var client = _httpClientFactory.CreateClient("FacturacionApi");

        var token = User.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Build query string
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(fechaDesde))
        {
            queryParams.Add($"fechaDesde={fechaDesde}");
        }
        if (!string.IsNullOrWhiteSpace(fechaHasta))
        {
            queryParams.Add($"fechaHasta={fechaHasta}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        var response = await client.GetAsync($"/api/cuentasporpagar/proveedor/{proveedorId}/estado{queryString}");

        if (response.IsSuccessStatusCode)
        {
            var estadoCuenta = await response.Content.ReadFromJsonAsync<EstadoCuentaResponse>(_jsonOptions);
            return new JsonResult(estadoCuenta);
        }

        return new JsonResult(new
        {
            transacciones = new List<TransaccionEstadoCuenta>(),
            resumen = new
            {
                totalFacturado = 0,
                totalPagado = 0,
                saldoPendiente = 0,
                saldoVencido = 0,
                saldoTotal = 0
            }
        });
    }

    private async Task LoadProveedoresAsync()
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

        var response = await client.GetAsync($"/api/proveedores/empresa/{empresaId}");
        if (response.IsSuccessStatusCode)
        {
            var proveedores = await response.Content.ReadFromJsonAsync<List<Proveedor>>(_jsonOptions);
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
    }
}

// DTOs for Estado de Cuenta response
public class EstadoCuentaResponse
{
    public List<TransaccionEstadoCuenta> Transacciones { get; set; } = new();
    public ResumenEstadoCuenta Resumen { get; set; } = new();
}

public class TransaccionEstadoCuenta
{
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = null!; // FACTURA, PAGO, NOTA_CREDITO, NOTA_DEBITO
    public string NumeroDocumento { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal Cargo { get; set; }
    public decimal Abono { get; set; }
    public decimal Saldo { get; set; }
    public string Estado { get; set; } = null!;
}

public class ResumenEstadoCuenta
{
    public decimal TotalFacturado { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal SaldoVencido { get; set; }
    public decimal SaldoTotal { get; set; }
}
