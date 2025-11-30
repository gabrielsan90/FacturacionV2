using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class GastosController : ControllerBase
{
    private readonly IGastoUnitOfWork _unitOfWork;
    private readonly DataContext _context;

    public GastosController(IGastoUnitOfWork unitOfWork, DataContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los gastos de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var gastos = await _unitOfWork.GastoRepository.GetByEmpresaAsync(empresaId);
        return Ok(gastos);
    }

    /// <summary>
    /// Obtiene el resumen de gastos de una empresa con filtros opcionales
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/resumen")]
    public async Task<IActionResult> GetResumenAsync(
        Guid empresaId,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var resumen = await _unitOfWork.GastoService.GetResumenGastosAsync(empresaId, fechaInicio, fechaFin);
        return Ok(resumen);
    }

    /// <summary>
    /// Obtiene un gasto por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var gasto = await _unitOfWork.GastoRepository.GetAsync(id);

        if (gasto == null)
        {
            return NotFound("El gasto no existe.");
        }

        if (!await TieneAccesoEmpresaAsync(gasto.EmpresaId))
        {
            return Forbid();
        }

        return Ok(gasto);
    }

    /// <summary>
    /// Obtiene los gastos con pagos pendientes
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/pendientes")]
    public async Task<IActionResult> GetPendientesAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var pendientes = await _unitOfWork.GastoRepository.GetPendientesPagoAsync(empresaId);
        return Ok(pendientes);
    }

    /// <summary>
    /// Obtiene gastos por proveedor
    /// </summary>
    [HttpGet("proveedor/{proveedorId:guid}")]
    public async Task<IActionResult> GetByProveedorAsync(Guid proveedorId)
    {
        var gastos = await _unitOfWork.GastoRepository.GetByProveedorAsync(proveedorId);

        // Verificar acceso a la empresa del primer gasto
        if (gastos.Any() && !await TieneAccesoEmpresaAsync(gastos.First().EmpresaId))
        {
            return Forbid();
        }

        return Ok(gastos);
    }

    /// <summary>
    /// Obtiene gastos por categoría
    /// </summary>
    [HttpGet("categoria/{categoriaId:int}")]
    public async Task<IActionResult> GetByCategoriaAsync(int categoriaId)
    {
        var gastos = await _unitOfWork.GastoRepository.GetByCategoriaAsync(categoriaId);

        // Verificar acceso a la empresa del primer gasto
        if (gastos.Any() && !await TieneAccesoEmpresaAsync(gastos.First().EmpresaId))
        {
            return Forbid();
        }

        return Ok(gastos);
    }

    /// <summary>
    /// Obtiene estadísticas de gastos por periodo
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/estadisticas")]
    public async Task<IActionResult> GetEstadisticasAsync(
        Guid empresaId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var estadisticas = await _unitOfWork.GastoService.GetEstadisticasAsync(empresaId, fechaInicio, fechaFin);
        return Ok(estadisticas);
    }

    /// <summary>
    /// Crea un nuevo gasto
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] GastoDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(dto.EmpresaId))
        {
            return Forbid();
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gasto = await _unitOfWork.GastoService.CrearGastoAsync(dto, dto.EmpresaId, userId!);

            return CreatedAtAction(nameof(GetAsync), new { id = gasto.Id }, gasto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el gasto: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza un gasto existente
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] GastoDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var gastoExistente = await _unitOfWork.GastoRepository.GetAsync(id);
        if (gastoExistente == null)
        {
            return NotFound("El gasto no existe.");
        }

        if (!await TieneAccesoEmpresaAsync(gastoExistente.EmpresaId))
        {
            return Forbid();
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gasto = await _unitOfWork.GastoService.ActualizarGastoAsync(id, dto, userId!);

            return Ok(gasto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el gasto: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina (soft delete) un gasto
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var gasto = await _unitOfWork.GastoRepository.GetAsync(id);
        if (gasto == null)
        {
            return NotFound("El gasto no existe.");
        }

        if (!await TieneAccesoEmpresaAsync(gasto.EmpresaId))
        {
            return Forbid();
        }

        // No permitir eliminar gastos aprobados
        if (gasto.Aprobado)
        {
            return BadRequest("No se puede eliminar un gasto aprobado.");
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.GastoRepository.DeleteAsync(id, userId!);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al eliminar el gasto: {ex.Message}");
        }
    }

    /// <summary>
    /// Registra un pago parcial o total
    /// </summary>
    [HttpPost("pagar")]
    public async Task<IActionResult> RegistrarPagoAsync([FromBody] RegistrarPagoDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var gasto = await _unitOfWork.GastoRepository.GetAsync(dto.GastoId);
        if (gasto == null)
        {
            return NotFound("El gasto no existe.");
        }

        if (!await TieneAccesoEmpresaAsync(gasto.EmpresaId))
        {
            return Forbid();
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gastoActualizado = await _unitOfWork.GastoService.RegistrarPagoAsync(dto, userId!);

            return Ok(gastoActualizado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al registrar el pago: {ex.Message}");
        }
    }

    /// <summary>
    /// Aprueba o rechaza un gasto
    /// </summary>
    [HttpPost("aprobar")]
    public async Task<IActionResult> AprobarAsync([FromBody] AprobarGastoDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var gasto = await _unitOfWork.GastoRepository.GetAsync(dto.GastoId);
        if (gasto == null)
        {
            return NotFound("El gasto no existe.");
        }

        if (!await TieneAccesoEmpresaAsync(gasto.EmpresaId))
        {
            return Forbid();
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gastoActualizado = await _unitOfWork.GastoService.AprobarGastoAsync(dto, userId!);

            return Ok(gastoActualizado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al aprobar/rechazar el gasto: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si el usuario tiene acceso a la empresa
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // Verificar si el usuario es admin (tiene acceso a todo)
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a esta empresa
        var tieneAcceso = await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);

        return tieneAcceso;
    }
}
