using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class DepreciacionesActivoController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IActivoFijoUnitOfWork _unitOfWork;
    private readonly ILogger<DepreciacionesActivoController> _logger;

    public DepreciacionesActivoController(
        DataContext context,
        IActivoFijoUnitOfWork unitOfWork,
        ILogger<DepreciacionesActivoController> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una depreciación por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var depreciacion = await _context.DepreciacionesActivo
                .Include(d => d.ActivoFijo)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (depreciacion == null)
                return NotFound("Depreciación no encontrada.");

            // Verify access through the activo's empresa
            if (depreciacion.ActivoFijo != null && !await TieneAccesoEmpresaAsync(depreciacion.ActivoFijo.EmpresaId))
                return Forbid();

            return Ok(depreciacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo depreciación {Id}", id);
            return StatusCode(500, $"Error al obtener depreciación: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina (reversa) una depreciación. Solo depreciaciones en estado Calculada (CAL) pueden eliminarse.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var depreciacion = await _context.DepreciacionesActivo
                .Include(d => d.ActivoFijo)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (depreciacion == null)
                return NotFound("Depreciación no encontrada.");

            if (depreciacion.ActivoFijo != null && !await TieneAccesoEmpresaAsync(depreciacion.ActivoFijo.EmpresaId))
                return Forbid();

            // Only allow deleting CAL (calculated, not applied) depreciations
            if (depreciacion.Estado == "APL")
            {
                // Use repository to properly reverse the depreciation
                var result = await _unitOfWork.DepreciacionActivoRepository.ReversarDepreciacionAsync(id, userId);
                if (!result.WasSuccess)
                    return BadRequest(result.Message);

                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Depreciación reversada: {Id}", id);
                return Ok(new ActionResponse<bool> { WasSuccess = true, Message = "Depreciación reversada exitosamente." });
            }

            // For CAL state, just delete
            _context.DepreciacionesActivo.Remove(depreciacion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Depreciación eliminada: {Id}", id);
            return Ok(new ActionResponse<bool> { WasSuccess = true, Message = "Depreciación eliminada exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando depreciación {Id}", id);
            return StatusCode(500, $"Error al eliminar depreciación: {ex.Message}");
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
