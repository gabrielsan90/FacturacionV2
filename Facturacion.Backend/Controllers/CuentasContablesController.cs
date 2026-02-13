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
public class CuentasContablesController : ControllerBase
{
    private readonly IContabilidadUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<CuentasContablesController> _logger;

    public CuentasContablesController(
        IContabilidadUnitOfWork unitOfWork,
        DataContext context,
        ILogger<CuentasContablesController> logger)
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

            var cuentas = await _unitOfWork.CuentaContableRepository.GetByEmpresaAsync(empresaId);

            return Ok(cuentas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cuentas contables for empresa {EmpresaId}", empresaId);
            return BadRequest($"Error al obtener las cuentas contables: {ex.Message}");
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

            var cuentas = await _unitOfWork.CuentaContableRepository.GetArbolAsync(empresaId);

            return Ok(cuentas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving arbol cuentas for empresa {EmpresaId}", empresaId);
            return BadRequest($"Error al obtener el árbol de cuentas: {ex.Message}");
        }
    }

    [HttpGet("empresa/{empresaId:guid}/movimiento")]
    public async Task<IActionResult> GetCuentasMovimientoAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            // Use DataContext for complex projection query
            var cuentas = await _context.CuentasContables
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.AceptaMovimientos && c.Activo)
                .OrderBy(c => c.Codigo)
                .Select(c => new { c.Id, c.Codigo, c.Nombre, c.CodigoNombre, c.TipoCuenta, c.Naturaleza })
                .ToListAsync();

            return Ok(cuentas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cuentas movimiento for empresa {EmpresaId}", empresaId);
            return BadRequest($"Error al obtener las cuentas de movimiento: {ex.Message}");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var cuenta = await _unitOfWork.CuentaContableRepository.GetAsync(id);

            if (cuenta == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
            {
                return Forbid();
            }

            return Ok(cuenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cuenta contable {Id}", id);
            return BadRequest($"Error al obtener la cuenta contable: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CuentaContable cuenta)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único (using DataContext for query)
            var existente = await _context.CuentasContables
                .FirstOrDefaultAsync(c => c.EmpresaId == cuenta.EmpresaId &&
                                         c.Codigo == cuenta.Codigo &&
                                         !c.IsDeleted);

            if (existente != null)
            {
                return BadRequest("Ya existe una cuenta con este código.");
            }

            // Si tiene padre, validar que exista y establecer nivel
            if (cuenta.CuentaPadreId.HasValue)
            {
                var padre = await _context.CuentasContables
                    .FirstOrDefaultAsync(c => c.Id == cuenta.CuentaPadreId.Value && !c.IsDeleted);

                if (padre == null)
                {
                    return BadRequest("La cuenta padre no existe.");
                }

                cuenta.Nivel = padre.Nivel + 1;

                // Padre no puede aceptar movimientos si tiene hijas
                if (padre.AceptaMovimientos)
                {
                    padre.AceptaMovimientos = false;
                    _context.CuentasContables.Update(padre);
                }
            }
            else
            {
                cuenta.Nivel = 1;
            }

            cuenta.Id = Guid.NewGuid();
            cuenta.CreadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _unitOfWork.CuentaContableRepository.AddAsync(cuenta);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Cuenta contable {Codigo} created by user {UserId}", cuenta.Codigo, cuenta.CreadoPorId);

            return Ok(cuenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cuenta contable");
            return BadRequest($"Error al crear la cuenta contable: {ex.Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] CuentaContable cuenta)
    {
        try
        {
            if (id != cuenta.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _unitOfWork.CuentaContableRepository.GetAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único (using DataContext for query)
            var duplicado = await _context.CuentasContables
                .FirstOrDefaultAsync(c => c.EmpresaId == cuenta.EmpresaId &&
                                         c.Codigo == cuenta.Codigo &&
                                         c.Id != id &&
                                         !c.IsDeleted);

            if (duplicado != null)
            {
                return BadRequest("Ya existe otra cuenta con este código.");
            }

            // Preservar campos de auditoría
            cuenta.FechaCreacion = existente.FechaCreacion;
            cuenta.CreadoPorId = existente.CreadoPorId;
            cuenta.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _unitOfWork.CuentaContableRepository.UpdateAsync(cuenta);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Cuenta contable {Codigo} updated by user {UserId}", cuenta.Codigo, cuenta.ModificadoPorId);

            return Ok(cuenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cuenta contable {Id}", id);
            return BadRequest($"Error al actualizar la cuenta contable: {ex.Message}");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            // Use DataContext for complex query with Include
            var cuenta = await _context.CuentasContables
                .Include(c => c.CuentasHijas)
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (cuenta == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
            {
                return Forbid();
            }

            // No permitir eliminar si tiene cuentas hijas activas
            if (cuenta.CuentasHijas?.Any(h => !h.IsDeleted) == true)
            {
                return BadRequest("No se puede eliminar una cuenta que tiene subcuentas.");
            }

            // No permitir eliminar si tiene movimientos
            if (cuenta.Movimientos?.Any() == true)
            {
                return BadRequest("No se puede eliminar una cuenta que tiene movimientos contables.");
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Delete using repository
            await _unitOfWork.CuentaContableRepository.DeleteAsync(id, userId!);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Cuenta contable {Id} deleted by user {UserId}", id, userId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cuenta contable {Id}", id);
            return BadRequest($"Error al eliminar la cuenta contable: {ex.Message}");
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
