using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class CentrosCostoController : ControllerBase
{
    private readonly IContabilidadUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<CentrosCostoController> _logger;

    public CentrosCostoController(
        IContabilidadUnitOfWork unitOfWork,
        DataContext context,
        ILogger<CentrosCostoController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var centros = await _unitOfWork.CentroCostoRepository.GetByEmpresaAsync(empresaId);

            return Ok(centros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving centros costo for empresa {EmpresaId}", empresaId);
            return BadRequest($"Error al obtener los centros de costo: {ex.Message}");
        }
    }

    [HttpGet("empresa/{empresaId:guid}/arbol")]
    public async Task<IActionResult> GetArbolAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            // Use DataContext for complex query with nested Include
            var centros = await _context.CentrosCosto
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.PadreId == null)
                .Include(c => c.Hijos!.Where(h => !h.IsDeleted))
                    .ThenInclude(c => c.Hijos!.Where(h => !h.IsDeleted))
                        .ThenInclude(c => c.Hijos!.Where(h => !h.IsDeleted))
                .OrderBy(c => c.Codigo)
                .ToListAsync();

            return Ok(centros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving arbol centros costo for empresa {EmpresaId}", empresaId);
            return BadRequest($"Error al obtener el árbol de centros de costo: {ex.Message}");
        }
    }

    [HttpGet("empresa/{empresaId:guid}/movimiento")]
    public async Task<IActionResult> GetCentrosMovimientoAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            // Use DataContext for complex projection query
            var centros = await _context.CentrosCosto
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.AceptaMovimientos && c.Activo)
                .OrderBy(c => c.Codigo)
                .Select(c => new { c.Id, c.Codigo, c.Nombre, c.CodigoNombre, c.Tipo })
                .ToListAsync();

            return Ok(centros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving centros movimiento for empresa {EmpresaId}", empresaId);
            return BadRequest($"Error al obtener los centros de costo de movimiento: {ex.Message}");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var centro = await _unitOfWork.CentroCostoRepository.GetAsync(id);

            if (centro == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(centro.EmpresaId))
            {
                return Forbid();
            }

            return Ok(centro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving centro costo {Id}", id);
            return BadRequest($"Error al obtener el centro de costo: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CentroCosto centro)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(centro.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único (using DataContext for query)
            var existente = await _context.CentrosCosto
                .FirstOrDefaultAsync(c => c.EmpresaId == centro.EmpresaId &&
                                         c.Codigo == centro.Codigo &&
                                         !c.IsDeleted);

            if (existente != null)
            {
                return BadRequest("Ya existe un centro de costo con este código.");
            }

            // Si tiene padre, validar que exista y establecer nivel
            if (centro.PadreId.HasValue)
            {
                var padre = await _context.CentrosCosto
                    .FirstOrDefaultAsync(c => c.Id == centro.PadreId.Value && !c.IsDeleted);

                if (padre == null)
                {
                    return BadRequest("El centro padre no existe.");
                }

                centro.Nivel = padre.Nivel + 1;

                // Padre no puede aceptar movimientos si tiene hijos
                if (padre.AceptaMovimientos)
                {
                    padre.AceptaMovimientos = false;
                    _context.CentrosCosto.Update(padre);
                }
            }
            else
            {
                centro.Nivel = 1;
            }

            centro.Id = Guid.NewGuid();
            centro.CreadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _unitOfWork.CentroCostoRepository.AddAsync(centro);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Centro costo {Codigo} created by user {UserId}", centro.Codigo, centro.CreadoPorId);

            return Ok(centro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating centro costo");
            return BadRequest($"Error al crear el centro de costo: {ex.Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] CentroCosto centro)
    {
        try
        {
            if (id != centro.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _unitOfWork.CentroCostoRepository.GetAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único (using DataContext for query)
            var duplicado = await _context.CentrosCosto
                .FirstOrDefaultAsync(c => c.EmpresaId == centro.EmpresaId &&
                                         c.Codigo == centro.Codigo &&
                                         c.Id != id &&
                                         !c.IsDeleted);

            if (duplicado != null)
            {
                return BadRequest("Ya existe otro centro de costo con este código.");
            }

            // Preservar campos de auditoría
            centro.FechaCreacion = existente.FechaCreacion;
            centro.CreadoPorId = existente.CreadoPorId;
            centro.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _unitOfWork.CentroCostoRepository.UpdateAsync(centro);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Centro costo {Codigo} updated by user {UserId}", centro.Codigo, centro.ModificadoPorId);

            return Ok(centro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating centro costo {Id}", id);
            return BadRequest($"Error al actualizar el centro de costo: {ex.Message}");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            // Use DataContext for complex query with Include
            var centro = await _context.CentrosCosto
                .Include(c => c.Hijos)
                .Include(c => c.MovimientosContables)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (centro == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(centro.EmpresaId))
            {
                return Forbid();
            }

            // No permitir eliminar si tiene centros hijos activos
            if (centro.Hijos?.Any(h => !h.IsDeleted) == true)
            {
                return BadRequest("No se puede eliminar un centro de costo que tiene sub-centros.");
            }

            // No permitir eliminar si tiene movimientos contables
            if (centro.MovimientosContables?.Any() == true)
            {
                return BadRequest("No se puede eliminar un centro de costo que tiene movimientos contables.");
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Delete using repository
            await _unitOfWork.CentroCostoRepository.DeleteAsync(id, userId!);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Centro costo {Id} deleted by user {UserId}", id, userId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting centro costo {Id}", id);
            return BadRequest($"Error al eliminar el centro de costo: {ex.Message}");
        }
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // Verificar si es SuperUser
        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a la empresa
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
