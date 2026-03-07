using Facturacion.Backend.Data;
using Facturacion.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class KardexController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<KardexController> _logger;

    // Types that increase inventory
    private static readonly TipoMovimientoInventario[] TiposEntrada =
    {
        TipoMovimientoInventario.Compra,
        TipoMovimientoInventario.AjusteEntrada,
        TipoMovimientoInventario.TrasladoEntrada,
        TipoMovimientoInventario.DevolucionCliente
    };

    public KardexController(DataContext context, ILogger<KardexController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el kardex de un producto, opcionalmente filtrado por bodega y rango de fechas.
    /// Joins through Inventario to access ProductoId and BodegaId.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetKardexAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] Guid productoId,
        [FromQuery] Guid? bodegaId,
        [FromQuery] string? fechaDesde,
        [FromQuery] string? fechaHasta)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            // Filter through Inventario → Producto relationship
            var query = _context.MovimientosInventario
                .Include(m => m.Inventario)
                    .ThenInclude(i => i!.Producto)
                .Include(m => m.Inventario)
                    .ThenInclude(i => i!.Bodega)
                .Where(m => m.Inventario != null
                    && m.Inventario.Producto != null
                    && m.Inventario.Producto.EmpresaId == empresaId
                    && m.Inventario.ProductoId == productoId)
                .AsNoTracking();

            if (bodegaId.HasValue && bodegaId != Guid.Empty)
                query = query.Where(m => m.Inventario!.BodegaId == bodegaId);

            if (DateTime.TryParse(fechaDesde, out var desde))
                query = query.Where(m => m.Fecha >= desde);

            if (DateTime.TryParse(fechaHasta, out var hasta))
                query = query.Where(m => m.Fecha <= hasta.AddDays(1));

            var movimientos = await query
                .OrderBy(m => m.Fecha)
                .ThenBy(m => m.FechaCreacion)
                .ToListAsync();

            // Calculate running balance
            decimal saldoCantidad = 0;
            var kardexItems = movimientos.Select(m =>
            {
                var esEntrada = TiposEntrada.Contains(m.TipoMovimiento);
                decimal entrada = esEntrada ? m.Cantidad : 0;
                decimal salida = esEntrada ? 0 : m.Cantidad;

                if (esEntrada)
                    saldoCantidad += m.Cantidad;
                else
                    saldoCantidad -= m.Cantidad;

                var costoUnitario = m.Inventario?.CostoPromedio ?? 0;

                return new
                {
                    m.Id,
                    m.Fecha,
                    tipoMovimiento = m.TipoMovimiento.ToString(),
                    entrada,
                    salida,
                    costoUnitario,
                    valorEntrada = entrada * costoUnitario,
                    valorSalida = salida * costoUnitario,
                    saldoCantidad,
                    saldoValor = saldoCantidad * costoUnitario,
                    m.Referencia,
                    m.Observaciones,
                    bodegaNombre = m.Inventario?.Bodega?.Nombre ?? "",
                    productoNombre = m.Inventario?.Producto?.Nombre ?? "",
                    productoCodigo = m.Inventario?.Producto?.Codigo ?? "",
                    m.CantidadAnterior,
                    m.CantidadNueva
                };
            }).ToList();

            return Ok(kardexItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo kardex para producto {ProductoId}", productoId);
            return StatusCode(500, $"Error al obtener kardex: {ex.Message}");
        }
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        if (User.IsInRole("SuperUser"))
            return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
