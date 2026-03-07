using Facturacion.Backend.Data;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Security.Claims;
using System.Text.Json;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PedidosVentaController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<PedidosVentaController> _logger;

    public PedidosVentaController(DataContext context, ILogger<PedidosVentaController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("empresa/{empresaId}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId, [FromQuery] string? estado)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var query = _context.PedidosVenta
                .Where(p => p.EmpresaId == empresaId)
                .Include(p => p.Cliente)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(estado) && estado != "TODAS")
                query = query.Where(p => p.Estado == estado);

            var pedidos = await query
                .OrderByDescending(p => p.Fecha)
                .Select(p => new
                {
                    p.Id,
                    p.Numero,
                    p.Fecha,
                    p.FechaVencimiento,
                    p.ClienteId,
                    Cliente = new { p.Cliente!.Nombre },
                    p.Moneda,
                    p.Subtotal,
                    p.TotalDescuentos,
                    p.TotalImpuestos,
                    p.Total,
                    p.Estado,
                    p.Observaciones
                })
                .ToListAsync();

            return Ok(pedidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pedidos de venta de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var pedido = await _context.PedidosVenta
                .Include(p => p.Cliente)
                .Include(p => p.Detalles.OrderBy(d => d.NumeroLinea))
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound("Pedido de venta no encontrado");

            if (!await TieneAccesoEmpresaAsync(pedido.EmpresaId))
                return Forbid();

            return Ok(pedido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pedido de venta {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] JsonElement body)
    {
        try
        {
            var empresaId = GetGuid(body, "empresaId");
            if (empresaId == Guid.Empty)
                return BadRequest("EmpresaId es obligatorio");

            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var clienteId = GetGuid(body, "clienteId");
            if (clienteId == Guid.Empty)
                return BadRequest("ClienteId es obligatorio");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var numero = await GenerarNumeroAsync(empresaId);

            var pedido = new PedidoVenta
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                ClienteId = clienteId,
                Numero = numero,
                Fecha = GetDateTime(body, "fecha") ?? DateTime.Now,
                FechaVencimiento = GetDateTime(body, "fechaVencimiento"),
                Estado = GetString(body, "estado") ?? "BORRADOR",
                Moneda = MapMoneda(GetString(body, "monedaId")),
                Observaciones = GetString(body, "observaciones"),
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacionId = userId
            };

            await ProcesarDetalles(body, pedido);
            CalcularTotales(pedido);

            _context.PedidosVenta.Add(pedido);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Pedido de venta creado: {Numero}", pedido.Numero);
            return Ok(pedido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear pedido de venta");
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, $"Error al crear el pedido de venta: {innerMsg}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] JsonElement body)
    {
        try
        {
            var pedido = await _context.PedidosVenta
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound("Pedido de venta no encontrado");

            if (!await TieneAccesoEmpresaAsync(pedido.EmpresaId))
                return Forbid();

            if (pedido.Estado == "FACTURADO" || pedido.Estado == "CANCELADO")
                return BadRequest($"No se puede modificar un pedido en estado {pedido.Estado}");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            pedido.ClienteId = GetGuid(body, "clienteId") is Guid cid && cid != Guid.Empty ? cid : pedido.ClienteId;
            pedido.Fecha = GetDateTime(body, "fecha") ?? pedido.Fecha;
            pedido.FechaVencimiento = GetDateTime(body, "fechaVencimiento");
            pedido.Estado = GetString(body, "estado") ?? pedido.Estado;
            pedido.Moneda = MapMoneda(GetString(body, "monedaId"));
            pedido.Observaciones = GetString(body, "observaciones");
            pedido.FechaModificacion = DateTime.UtcNow;
            pedido.UsuarioModificacionId = userId;

            // Replace all detail lines
            _context.PedidoVentaDetalles.RemoveRange(pedido.Detalles);
            pedido.Detalles.Clear();

            await ProcesarDetalles(body, pedido);
            CalcularTotales(pedido);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pedido de venta actualizado: {Id}", id);
            return Ok(pedido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar pedido de venta {Id}", id);
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, $"Error al actualizar el pedido de venta: {innerMsg}");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var pedido = await _context.PedidosVenta
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound("Pedido de venta no encontrado");

            if (!await TieneAccesoEmpresaAsync(pedido.EmpresaId))
                return Forbid();

            if (pedido.Estado != "BORRADOR" && pedido.Estado != "PENDIENTE")
                return BadRequest("Solo se pueden eliminar pedidos en estado BORRADOR o PENDIENTE");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            pedido.IsDeleted = true;
            pedido.FechaEliminacion = DateTime.UtcNow;
            pedido.UsuarioEliminacionId = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pedido de venta eliminado: {Id}", id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar pedido de venta {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost("{id:guid}/aprobar")]
    public async Task<IActionResult> AprobarAsync(Guid id)
    {
        try
        {
            var pedido = await _context.PedidosVenta
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound("Pedido de venta no encontrado");

            if (!await TieneAccesoEmpresaAsync(pedido.EmpresaId))
                return Forbid();

            if (pedido.Estado != "BORRADOR" && pedido.Estado != "PENDIENTE")
                return BadRequest("Solo se pueden aprobar pedidos en estado BORRADOR o PENDIENTE");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            pedido.Estado = "APROBADO";
            pedido.FechaAprobacion = DateTime.UtcNow;
            pedido.UsuarioAprobacionId = userId;
            pedido.FechaModificacion = DateTime.UtcNow;
            pedido.UsuarioModificacionId = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pedido de venta aprobado: {Id}", id);
            return Ok(new { message = "Pedido aprobado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar pedido de venta {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost("{id:guid}/convertir")]
    public async Task<IActionResult> ConvertirAsync(Guid id)
    {
        try
        {
            var pedido = await _context.PedidosVenta
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound("Pedido de venta no encontrado");

            if (!await TieneAccesoEmpresaAsync(pedido.EmpresaId))
                return Forbid();

            if (pedido.Estado != "APROBADO")
                return BadRequest("Solo se pueden convertir pedidos en estado APROBADO");

            if (pedido.DocumentoGeneradoId.HasValue)
                return BadRequest("Este pedido ya fue convertido a factura");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            pedido.Estado = "FACTURADO";
            pedido.FechaModificacion = DateTime.UtcNow;
            pedido.UsuarioModificacionId = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pedido de venta convertido: {Id}", id);
            return Ok(new { message = "Pedido convertido a factura exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al convertir pedido de venta {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Genera y descarga el PDF de un pedido de venta
    /// </summary>
    [HttpGet("{id:guid}/descargar-pdf")]
    public async Task<IActionResult> DescargarPdfAsync(Guid id)
    {
        try
        {
            var pedido = await _context.PedidosVenta
                .Include(p => p.Empresa)
                    .ThenInclude(e => e!.Emails)
                .Include(p => p.Empresa)
                    .ThenInclude(e => e!.Telefonos)
                .Include(p => p.Cliente)
                .Include(p => p.Detalles.OrderBy(d => d.NumeroLinea))
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (pedido == null)
                return NotFound("Pedido de venta no encontrado.");

            if (!await TieneAccesoEmpresaAsync(pedido.EmpresaId))
                return Forbid();

            var pdfDocument = new Facturacion.Backend.Services.Implementations.PdfDocuments.PedidoVentaPdfDocument(pedido);
            var pdfBytes = pdfDocument.GeneratePdf();

            var fileName = $"PedidoVenta_{pedido.Numero}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF de pedido de venta {Id}", id);
            return StatusCode(500, "Error al generar el PDF del pedido de venta.");
        }
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    private async Task<string> GenerarNumeroAsync(Guid empresaId)
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        var prefix = $"PED-{year:D4}-{month:D2}-";

        var lastNumero = await _context.PedidosVenta
            .IgnoreQueryFilters()
            .Where(p => p.EmpresaId == empresaId && p.Numero.StartsWith(prefix))
            .OrderByDescending(p => p.Numero)
            .Select(p => p.Numero)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastNumero != null)
        {
            var lastPart = lastNumero.Substring(prefix.Length);
            if (int.TryParse(lastPart, out int last))
                nextNumber = last + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task ProcesarDetalles(JsonElement body, PedidoVenta pedido)
    {
        if (!body.TryGetProperty("detalles", out var detallesElement))
            return;

        int lineNumber = 1;
        foreach (var det in detallesElement.EnumerateArray())
        {
            var productoId = GetGuidNullable(det, "productoId");
            var descripcion = "Producto";

            if (productoId.HasValue)
            {
                var producto = await _context.Productos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == productoId.Value);

                if (producto != null)
                    descripcion = producto.Nombre;
            }

            var cantidad = GetDecimal(det, "cantidad");
            var precioUnitario = GetDecimal(det, "precioUnitario");
            var porcentajeDescuento = GetDecimal(det, "porcentajeDescuento");

            var montoTotal = cantidad * precioUnitario;
            var montoDescuento = montoTotal * porcentajeDescuento / 100m;
            var subTotal = montoTotal - montoDescuento;
            var montoIVA = subTotal * 0.13m;
            var totalLinea = subTotal + montoIVA;

            pedido.Detalles.Add(new PedidoVentaDetalle
            {
                Id = Guid.NewGuid(),
                PedidoVentaId = pedido.Id,
                NumeroLinea = lineNumber++,
                ProductoId = productoId,
                Descripcion = descripcion,
                Cantidad = cantidad,
                PrecioUnitario = precioUnitario,
                MontoTotal = montoTotal,
                PorcentajeDescuento = porcentajeDescuento,
                MontoDescuento = montoDescuento,
                SubTotal = subTotal,
                MontoIVA = montoIVA,
                TotalLinea = totalLinea
            });
        }
    }

    private static void CalcularTotales(PedidoVenta pedido)
    {
        pedido.Subtotal = pedido.Detalles.Sum(d => d.MontoTotal);
        pedido.TotalDescuentos = pedido.Detalles.Sum(d => d.MontoDescuento);
        pedido.TotalImpuestos = pedido.Detalles.Sum(d => d.MontoIVA);
        pedido.Total = pedido.Detalles.Sum(d => d.TotalLinea);
    }

    private static string MapMoneda(string? monedaId)
    {
        if (string.IsNullOrEmpty(monedaId))
            return "CRC";
        return monedaId;
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (userRoles.Contains("SuperUser"))
            return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }

    // ========================================
    // JSON EXTRACTION HELPERS
    // ========================================

    private static Guid GetGuid(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
        {
            if (Guid.TryParse(val.GetString(), out var g))
                return g;
        }
        return Guid.Empty;
    }

    private static Guid? GetGuidNullable(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
        {
            if (Guid.TryParse(val.GetString(), out var g) && g != Guid.Empty)
                return g;
        }
        return null;
    }

    private static string? GetString(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
        {
            var s = val.GetString();
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }
        return null;
    }

    private static DateTime? GetDateTime(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
        {
            if (DateTime.TryParse(val.GetString(), out var dt))
                return dt;
        }
        return null;
    }

    private static decimal GetDecimal(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val))
        {
            if (val.ValueKind == JsonValueKind.Number)
                return val.GetDecimal();
            if (val.ValueKind == JsonValueKind.String && decimal.TryParse(val.GetString(), out var d))
                return d;
        }
        return 0;
    }
}
