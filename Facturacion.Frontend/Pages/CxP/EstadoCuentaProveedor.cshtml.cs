using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Facturacion.Shared.DTOs;
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
            var dto = await response.Content.ReadFromJsonAsync<EstadoCuentaProveedorDTO>(_jsonOptions);
            if (dto != null)
            {
                // Transform CuentasPorPagar into Transacciones for the frontend
                var transacciones = new List<TransaccionEstadoCuenta>();
                foreach (var c in dto.CuentasPorPagar)
                {
                    // Add the invoice as a FACTURA transaction
                    transacciones.Add(new TransaccionEstadoCuenta
                    {
                        Fecha = c.FechaFactura,
                        Tipo = "FACTURA",
                        NumeroDocumento = c.NumeroFactura,
                        Descripcion = c.Observaciones,
                        Cargo = c.MontoOriginal,
                        Abono = 0,
                        Saldo = c.MontoSaldo,
                        Estado = c.Estado,
                        Moneda = c.Moneda ?? "CRC"
                    });

                    // Add each payment as a PAGO transaction
                    foreach (var a in c.Abonos)
                    {
                        transacciones.Add(new TransaccionEstadoCuenta
                        {
                            Fecha = a.FechaPago,
                            Tipo = "PAGO",
                            NumeroDocumento = a.NumeroReferencia ?? "-",
                            Descripcion = a.Notas ?? $"Pago {a.MetodoPagoDescripcion}",
                            Cargo = 0,
                            Abono = a.Monto,
                            Saldo = 0,
                            Estado = "PAG",
                            Moneda = c.Moneda ?? "CRC"
                        });
                    }
                }

                // Per-currency breakdowns
                var facturadoPorMoneda = dto.CuentasPorPagar
                    .GroupBy(c => c.Moneda ?? "CRC")
                    .Select(g => new { moneda = g.Key, monto = g.Sum(c => c.MontoOriginal) })
                    .OrderByDescending(m => m.monto).ToList();
                var pagadoPorMoneda = dto.CuentasPorPagar
                    .GroupBy(c => c.Moneda ?? "CRC")
                    .Select(g => new { moneda = g.Key, monto = g.Sum(c => c.MontoAbonado) })
                    .OrderByDescending(m => m.monto).ToList();
                var pendientePorMoneda = dto.CuentasPorPagar
                    .GroupBy(c => c.Moneda ?? "CRC")
                    .Select(g => new { moneda = g.Key, monto = g.Sum(c => c.MontoSaldo) })
                    .Where(m => m.monto > 0)
                    .OrderByDescending(m => m.monto).ToList();
                var vencidoPorMoneda = dto.CuentasPorPagar
                    .Where(c => c.EstaVencida)
                    .GroupBy(c => c.Moneda ?? "CRC")
                    .Select(g => new { moneda = g.Key, monto = g.Sum(c => c.MontoSaldo) })
                    .Where(m => m.monto > 0)
                    .OrderByDescending(m => m.monto).ToList();

                return new JsonResult(new
                {
                    transacciones = transacciones.OrderBy(t => t.Fecha).ToList(),
                    resumen = new
                    {
                        totalFacturado = dto.TotalFacturas,
                        totalPagado = dto.TotalPagado,
                        saldoPendiente = dto.TotalPendiente,
                        saldoVencido = dto.CuentasPorPagar.Where(c => c.EstaVencida).Sum(c => c.MontoSaldo),
                        saldoTotal = dto.TotalPendiente,
                        facturadoPorMoneda,
                        pagadoPorMoneda,
                        pendientePorMoneda,
                        vencidoPorMoneda
                    }
                });
            }
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
    public string Moneda { get; set; } = "CRC";
}

