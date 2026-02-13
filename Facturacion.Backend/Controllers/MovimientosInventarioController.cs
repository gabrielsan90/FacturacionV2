using Facturacion.Backend.Helpers;
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
    private readonly ILogger<MovimientosInventarioController> _logger;

    public MovimientosInventarioController(
        IInventarioUnitOfWork unitOfWork,
        ILogger<MovimientosInventarioController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("GetAsync called for MovimientoInventarioId: {MovimientoId}", id);

            var movimiento = await _unitOfWork.MovimientoInventarioRepository.GetAsync(id);
            if (movimiento == null)
            {
                _logger.LogWarning("Inventory movement not found: {MovimientoId}", id);
                return NotFound();
            }

            _logger.LogInformation("Retrieved inventory movement: {MovimientoId}, type: {TipoMovimiento}",
                id, movimiento.TipoMovimiento);
            return Ok(movimiento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory movement: {MovimientoId}", id);
            return StatusCode(500, $"Error al obtener movimiento: {ex.Message}");
        }
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
        try
        {
            // Default to last 30 days if no date range specified
            var inicio = fechaInicio ?? FechaCostaRicaHelper.Ahora.AddDays(-30);
            var fin = fechaFin ?? FechaCostaRicaHelper.Ahora;

            _logger.LogInformation("GetByEmpresaAsync called for EmpresaId: {EmpresaId}, date range: {FechaInicio} to {FechaFin}",
                empresaId, inicio, fin);

            // Get all movements for the date range
            var movimientos = await _unitOfWork.MovimientoInventarioRepository.GetByFechaAsync(empresaId, inicio, fin);

            // Apply additional filters if specified
            if (tipoMovimiento.HasValue)
            {
                movimientos = movimientos.Where(m => m.TipoMovimiento == tipoMovimiento.Value);
                _logger.LogInformation("Filtering by TipoMovimiento: {TipoMovimiento}", tipoMovimiento.Value);
            }

            if (productoId.HasValue)
            {
                movimientos = movimientos.Where(m => m.Inventario.ProductoId == productoId.Value);
                _logger.LogInformation("Filtering by ProductoId: {ProductoId}", productoId.Value);
            }

            if (sucursalId.HasValue)
            {
                movimientos = movimientos.Where(m => m.Inventario.SucursalId == sucursalId.Value);
                _logger.LogInformation("Filtering by SucursalId: {SucursalId}", sucursalId.Value);
            }

            var movimientosList = movimientos.ToList();
            _logger.LogInformation("Retrieved {Count} inventory movements for EmpresaId: {EmpresaId}",
                movimientosList.Count, empresaId);
            return Ok(movimientosList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory movements for EmpresaId: {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener movimientos: {ex.Message}");
        }
    }

    [HttpGet("inventario/{inventarioId:guid}")]
    public async Task<IActionResult> GetByInventarioAsync(Guid inventarioId)
    {
        try
        {
            _logger.LogInformation("GetByInventarioAsync called for InventarioId: {InventarioId}", inventarioId);

            var movimientos = await _unitOfWork.MovimientoInventarioRepository.GetByInventarioAsync(inventarioId);
            _logger.LogInformation("Retrieved {Count} movements for InventarioId: {InventarioId}",
                movimientos?.Count() ?? 0, inventarioId);
            return Ok(movimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movements for InventarioId: {InventarioId}", inventarioId);
            return StatusCode(500, $"Error al obtener movimientos: {ex.Message}");
        }
    }

    [HttpGet("empresa/{empresaId:guid}/fecha")]
    public async Task<IActionResult> GetByFechaAsync(Guid empresaId, [FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        try
        {
            _logger.LogInformation("GetByFechaAsync called for EmpresaId: {EmpresaId}, range: {FechaInicio} to {FechaFin}",
                empresaId, fechaInicio, fechaFin);

            var movimientos = await _unitOfWork.MovimientoInventarioRepository.GetByFechaAsync(empresaId, fechaInicio, fechaFin);
            _logger.LogInformation("Retrieved {Count} movements for date range", movimientos?.Count() ?? 0);
            return Ok(movimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movements by date for EmpresaId: {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener movimientos: {ex.Message}");
        }
    }

    [HttpGet("empresa/{empresaId:guid}/tipo/{tipo}")]
    public async Task<IActionResult> GetByTipoAsync(Guid empresaId, TipoMovimientoInventario tipo)
    {
        try
        {
            _logger.LogInformation("GetByTipoAsync called for EmpresaId: {EmpresaId}, type: {TipoMovimiento}",
                empresaId, tipo);

            var movimientos = await _unitOfWork.MovimientoInventarioRepository.GetByTipoAsync(empresaId, tipo);
            _logger.LogInformation("Retrieved {Count} movements of type {TipoMovimiento}",
                movimientos?.Count() ?? 0, tipo);
            return Ok(movimientos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movements by type for EmpresaId: {EmpresaId}, type: {TipoMovimiento}",
                empresaId, tipo);
            return StatusCode(500, $"Error al obtener movimientos: {ex.Message}");
        }
    }
}
