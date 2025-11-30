using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MovimientosInventarioController : ControllerBase
{
    private readonly IInventarioUnitOfWork _unitOfWork;

    public MovimientosInventarioController(IInventarioUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var movimiento = await _unitOfWork.MovimientoInventarioRepository.GetAsync(id);
        if (movimiento == null)
        {
            return NotFound();
        }
        return Ok(movimiento);
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(
        Guid empresaId,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] TipoMovimientoInventario? tipoMovimiento,
        [FromQuery] Guid? productoId,
        [FromQuery] Guid? sucursalId)
    {
        // Default to last 30 days if no date range specified
        var inicio = fechaInicio ?? DateTime.UtcNow.AddDays(-30);
        var fin = fechaFin ?? DateTime.UtcNow;

        // Get all movements for the date range
        var movimientos = await _unitOfWork.MovimientoInventarioRepository.GetByFechaAsync(empresaId, inicio, fin);

        // Apply additional filters if specified
        if (tipoMovimiento.HasValue)
        {
            movimientos = movimientos.Where(m => m.TipoMovimiento == tipoMovimiento.Value);
        }

        if (productoId.HasValue)
        {
            movimientos = movimientos.Where(m => m.Inventario.ProductoId == productoId.Value);
        }

        if (sucursalId.HasValue)
        {
            movimientos = movimientos.Where(m => m.Inventario.SucursalId == sucursalId.Value);
        }

        return Ok(movimientos);
    }

    [HttpGet("inventario/{inventarioId:guid}")]
    public async Task<IActionResult> GetByInventarioAsync(Guid inventarioId)
    {
        var movimientos = await _unitOfWork.MovimientoInventarioRepository.GetByInventarioAsync(inventarioId);
        return Ok(movimientos);
    }

    [HttpGet("empresa/{empresaId:guid}/fecha")]
    public async Task<IActionResult> GetByFechaAsync(Guid empresaId, [FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        var movimientos = await _unitOfWork.MovimientoInventarioRepository.GetByFechaAsync(empresaId, fechaInicio, fechaFin);
        return Ok(movimientos);
    }

    [HttpGet("empresa/{empresaId:guid}/tipo/{tipo}")]
    public async Task<IActionResult> GetByTipoAsync(Guid empresaId, TipoMovimientoInventario tipo)
    {
        var movimientos = await _unitOfWork.MovimientoInventarioRepository.GetByTipoAsync(empresaId, tipo);
        return Ok(movimientos);
    }
}
