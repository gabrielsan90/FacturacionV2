using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Facturacion.Frontend.Pages.Contabilidad;

[Authorize]
public class CuentasIntegracionModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public CuentasIntegracionModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public void OnGet()
    {
    }

    // Get all mappings, optionally filtered by module
    public async Task<IActionResult> OnGetListaAsync(string? modulo)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var url = $"api/cuentasintegracion/empresa/{empresaId}";
            if (!string.IsNullOrEmpty(modulo))
            {
                url += $"?modulo={modulo}";
            }

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var mappings = JsonSerializer.Deserialize<List<MapeoCuentaDTO>>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = mappings });
            }

            return new JsonResult(new { success = false, message = "Error al obtener los mapeos." });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // Get single mapping details
    public async Task<IActionResult> OnGetDetalleAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.GetAsync($"api/cuentasintegracion/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var mapping = JsonSerializer.Deserialize<MapeoCuentaDTO>(content, _jsonOptions);
                return new JsonResult(new { success = true, data = mapping });
            }

            return new JsonResult(new { success = false, message = "Mapeo no encontrado." });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // Get accounting accounts for Select2
    public async Task<IActionResult> OnGetCuentasContablesAsync()
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.GetAsync($"api/cuentascontables/empresa/{empresaId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cuentas = JsonSerializer.Deserialize<List<CuentaContable>>(content, _jsonOptions);

                // Filter only accounts that accept movements
                var cuentasActivas = cuentas?
                    .Where(c => c.Activo && c.AceptaMovimientos)
                    .Select(c => new
                    {
                        id = c.Id,
                        text = $"{c.Codigo} - {c.Nombre}",
                        codigo = c.Codigo,
                        nombre = c.Nombre
                    })
                    .ToList();

                return new JsonResult(new { success = true, data = cuentasActivas });
            }

            return new JsonResult(new { success = false, message = "Error al obtener las cuentas." });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // Get operation types for a specific module
    public async Task<IActionResult> OnGetTiposOperacionAsync(string modulo)
    {
        try
        {
            var tiposOperacion = GetTiposOperacionPorModulo(modulo);
            return new JsonResult(new { success = true, data = tiposOperacion });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // Create new mapping
    public async Task<IActionResult> OnPostCrearAsync([FromBody] MapeoCuentaDTO mapeo)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            // Set EmpresaId
            mapeo.EmpresaId = empresaId.Value;

            var json = JsonSerializer.Serialize(mapeo, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/cuentasintegracion", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Mapeo creado exitosamente." });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // Update existing mapping
    public async Task<IActionResult> OnPostActualizarAsync([FromBody] MapeoCuentaDTO mapeo)
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            mapeo.EmpresaId = empresaId.Value;

            var json = JsonSerializer.Serialize(mapeo, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/cuentasintegracion/{mapeo.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Mapeo actualizado exitosamente." });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // Delete mapping
    public async Task<IActionResult> OnPostEliminarAsync(Guid id)
    {
        try
        {
            var client = CreateApiClient();

            var response = await client.DeleteAsync($"api/cuentasintegracion/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new JsonResult(new { success = true, message = "Mapeo eliminado exitosamente." });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    // Initialize default mappings (seed)
    public async Task<IActionResult> OnPostSeedAsync()
    {
        try
        {
            var client = CreateApiClient();

            var empresaId = GetEmpresaId();
            if (empresaId == null) return BadRequest("No se encontró la empresa.");

            var response = await client.PostAsync($"api/cuentasintegracion/seed/{empresaId}", null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

                return new JsonResult(new
                {
                    success = true,
                    message = "Mapeos inicializados exitosamente. Debe asignar las cuentas contables manualmente.",
                    data = result
                });
            }

            var error = await response.Content.ReadAsStringAsync();
            return new JsonResult(new { success = false, message = error });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
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
        var empresaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (string.IsNullOrEmpty(empresaIdClaim)) return null;
        return Guid.Parse(empresaIdClaim);
    }

    // Helper method to get operation types by module
    private List<TipoOperacionInfo> GetTiposOperacionPorModulo(string modulo)
    {
        return modulo.ToUpper() switch
        {
            "VEN" => new List<TipoOperacionInfo>
            {
                new() { Codigo = "VENTA_GRAVADA", Nombre = "Venta Gravada con IVA" },
                new() { Codigo = "VENTA_EXENTA", Nombre = "Venta Exenta" },
                new() { Codigo = "IVA_DEBITO", Nombre = "IVA Débito Fiscal (IVA por Pagar)" },
                new() { Codigo = "DESCUENTO", Nombre = "Descuentos en Ventas" },
                new() { Codigo = "DEVOLUCION", Nombre = "Devoluciones sobre Ventas" },
            },
            "COM" => new List<TipoOperacionInfo>
            {
                new() { Codigo = "COMPRA_GRAVADA", Nombre = "Compra Gravada con IVA" },
                new() { Codigo = "COMPRA_EXENTA", Nombre = "Compra Exenta" },
                new() { Codigo = "IVA_CREDITO", Nombre = "IVA Crédito Fiscal" },
                new() { Codigo = "DESCUENTO_RECIBIDO", Nombre = "Descuentos Recibidos" },
                new() { Codigo = "DEVOLUCION_COMPRA", Nombre = "Devoluciones sobre Compras" },
            },
            "INV" => new List<TipoOperacionInfo>
            {
                new() { Codigo = "ENTRADA", Nombre = "Entrada a Inventario" },
                new() { Codigo = "SALIDA", Nombre = "Salida de Inventario / Costo de Ventas" },
                new() { Codigo = "AJUSTE_POSITIVO", Nombre = "Ajuste Positivo de Inventario" },
                new() { Codigo = "AJUSTE_NEGATIVO", Nombre = "Ajuste Negativo / Pérdida Inventario" },
                new() { Codigo = "TRANSFERENCIA", Nombre = "Transferencia entre Bodegas" },
            },
            "CXC" => new List<TipoOperacionInfo>
            {
                new() { Codigo = "CARGO_CLIENTE", Nombre = "Cargo a Cuenta por Cobrar (Clientes)" },
                new() { Codigo = "ABONO_CLIENTE", Nombre = "Abono de Cliente (Pago)" },
                new() { Codigo = "ANTICIPO_CLIENTE", Nombre = "Anticipo de Cliente" },
                new() { Codigo = "DESCUENTO_OTORGADO", Nombre = "Descuento Otorgado (Condonación)" },
                new() { Codigo = "NOTA_CREDITO", Nombre = "Nota de Crédito" },
            },
            "CXP" => new List<TipoOperacionInfo>
            {
                new() { Codigo = "CARGO_PROVEEDOR", Nombre = "Cargo a Cuenta por Pagar (Proveedores)" },
                new() { Codigo = "ABONO_PROVEEDOR", Nombre = "Abono a Proveedor (Pago)" },
                new() { Codigo = "ANTICIPO_PROVEEDOR", Nombre = "Anticipo a Proveedor" },
                new() { Codigo = "DESCUENTO_RECIBIDO", Nombre = "Descuento Recibido" },
                new() { Codigo = "NOTA_DEBITO", Nombre = "Nota de Débito" },
            },
            "BAN" => new List<TipoOperacionInfo>
            {
                new() { Codigo = "DEPOSITO", Nombre = "Depósito Bancario" },
                new() { Codigo = "RETIRO", Nombre = "Retiro o Cheque" },
                new() { Codigo = "TRANSFERENCIA_SALIDA", Nombre = "Transferencia Salida" },
                new() { Codigo = "TRANSFERENCIA_ENTRADA", Nombre = "Transferencia Entrada" },
                new() { Codigo = "COMISION_BANCARIA", Nombre = "Comisión Bancaria" },
                new() { Codigo = "INTERES_GANADO", Nombre = "Interés Ganado" },
                new() { Codigo = "INTERES_PAGADO", Nombre = "Interés Pagado" },
            },
            "NOM" => new List<TipoOperacionInfo>
            {
                new() { Codigo = "SALARIO_BRUTO", Nombre = "Salario Bruto" },
                new() { Codigo = "SALARIO_NETO", Nombre = "Salario Neto por Pagar" },
                new() { Codigo = "CCSS_OBRERO", Nombre = "CCSS Obrero (Retención)" },
                new() { Codigo = "CCSS_PATRONAL", Nombre = "CCSS Patronal" },
                new() { Codigo = "AGUINALDO", Nombre = "Provisión Aguinaldo" },
                new() { Codigo = "VACACIONES", Nombre = "Provisión Vacaciones" },
                new() { Codigo = "CESANTIA", Nombre = "Provisión Cesantía" },
            },
            "ACT" => new List<TipoOperacionInfo>
            {
                new() { Codigo = "COMPRA_ACTIVO", Nombre = "Compra de Activo Fijo" },
                new() { Codigo = "DEPRECIACION", Nombre = "Gasto por Depreciación" },
                new() { Codigo = "DEPRECIACION_ACUMULADA", Nombre = "Depreciación Acumulada" },
                new() { Codigo = "VENTA_ACTIVO", Nombre = "Venta de Activo Fijo" },
                new() { Codigo = "UTILIDAD_VENTA", Nombre = "Utilidad en Venta de Activo" },
                new() { Codigo = "PERDIDA_VENTA", Nombre = "Pérdida en Venta de Activo" },
            },
            _ => new List<TipoOperacionInfo>()
        };
    }

    // Get all modules info
    public IActionResult OnGetModulosAsync()
    {
        var modulos = new List<ModuloInfo>
        {
            new() { Codigo = "VEN", Nombre = "Ventas", Icono = "fa-shopping-cart" },
            new() { Codigo = "COM", Nombre = "Compras", Icono = "fa-shopping-bag" },
            new() { Codigo = "INV", Nombre = "Inventario", Icono = "fa-boxes" },
            new() { Codigo = "CXC", Nombre = "Cuentas por Cobrar", Icono = "fa-file-invoice-dollar" },
            new() { Codigo = "CXP", Nombre = "Cuentas por Pagar", Icono = "fa-file-invoice" },
            new() { Codigo = "BAN", Nombre = "Bancos", Icono = "fa-university" },
            new() { Codigo = "NOM", Nombre = "Nómina", Icono = "fa-users" },
            new() { Codigo = "ACT", Nombre = "Activos Fijos", Icono = "fa-building" },
        };

        return new JsonResult(new { success = true, data = modulos });
    }
}

// DTOs
public class MapeoCuentaDTO
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Modulo { get; set; } = "";
    public string TipoOperacion { get; set; } = "";
    public string ConceptoContable { get; set; } = "";
    public string TipoMovimiento { get; set; } = "D";
    public string? Descripcion { get; set; }
    public Guid CuentaContableId { get; set; }
    public string? CuentaCodigo { get; set; }
    public string? CuentaNombre { get; set; }
    public bool Activo { get; set; } = true;
    public CuentaContableRef? CuentaContable { get; set; }
}

public class CuentaContableRef
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
}

public class ModuloInfo
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Icono { get; set; } = "";
    public List<TipoOperacionInfo>? TiposOperacion { get; set; }
}

public class TipoOperacionInfo
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
}
