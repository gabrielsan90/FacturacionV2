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
public class SucursalesController : ControllerBase
{
    private readonly ISucursalUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<SucursalesController> _logger;

    public SucursalesController(
        ISucursalUnitOfWork unitOfWork,
        DataContext context,
        ILogger<SucursalesController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("GetAllAsync called by user: {UserId}", userId);

            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // SuperUser can see all sucursales
            if (userRoles.Contains("SuperUser"))
            {
                _logger.LogInformation("SuperUser retrieving all sucursales");
                var allSucursales = await _context.Sucursales
                    .Include(s => s.Empresa)
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Codigo)
                    .ToListAsync();
                _logger.LogInformation("Retrieved {Count} sucursales (all)", allSucursales.Count);
                return Ok(allSucursales);
            }

            // Other users can only see sucursales from their empresa
            var empresaIds = await _context.UsuariosEmpresas
                .Where(ue => ue.UserId == userId)
                .Select(ue => ue.EmpresaId)
                .ToListAsync();

            _logger.LogInformation("User {UserId} has access to {Count} empresas", userId, empresaIds.Count);

            var sucursales = await _context.Sucursales
                .Include(s => s.Empresa)
                .Where(s => !s.IsDeleted && empresaIds.Contains(s.EmpresaId))
                .OrderBy(s => s.Codigo)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} sucursales for user empresas", sucursales.Count);
            return Ok(sucursales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sucursales");
            return StatusCode(500, $"Error al obtener sucursales: {ex.Message}");
        }
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            _logger.LogInformation("GetByEmpresaAsync called for EmpresaId: {EmpresaId}", empresaId);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Access denied to EmpresaId: {EmpresaId} for user: {UserId}",
                    empresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var sucursales = await _unitOfWork.SucursalRepository.GetByEmpresaAsync(empresaId);
            _logger.LogInformation("Retrieved {Count} sucursales for EmpresaId: {EmpresaId}",
                sucursales?.Count() ?? 0, empresaId);
            return Ok(sucursales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sucursales for EmpresaId: {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener sucursales: {ex.Message}");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("GetAsync called for SucursalId: {SucursalId}", id);

            var sucursal = await _unitOfWork.SucursalRepository.GetAsync(id);

            if (sucursal == null)
            {
                _logger.LogWarning("Sucursal not found: {SucursalId}", id);
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(sucursal.EmpresaId))
            {
                _logger.LogWarning("Access denied to Sucursal: {SucursalId} for user: {UserId}",
                    id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            _logger.LogInformation("Retrieved sucursal: {SucursalId}, codigo: {Codigo}", id, sucursal.Codigo);
            return Ok(sucursal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sucursal: {SucursalId}", id);
            return StatusCode(500, $"Error al obtener sucursal: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Sucursal sucursal)
    {
        try
        {
            _logger.LogInformation("PostAsync called to create sucursal with codigo: {Codigo} for EmpresaId: {EmpresaId}",
                sucursal.Codigo, sucursal.EmpresaId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for sucursal creation");
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(sucursal.EmpresaId))
            {
                _logger.LogWarning("Access denied to create sucursal for EmpresaId: {EmpresaId}, user: {UserId}",
                    sucursal.EmpresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            // Verificar que no exista otra sucursal con el mismo código
            var existente = await _unitOfWork.SucursalRepository.GetByCodigoAsync(
                sucursal.EmpresaId,
                sucursal.Codigo);

            if (existente != null)
            {
                _logger.LogWarning("Duplicate sucursal codigo: {Codigo} for EmpresaId: {EmpresaId}",
                    sucursal.Codigo, sucursal.EmpresaId);
                return BadRequest("Ya existe una sucursal con este código en la empresa.");
            }

            // Si se marca como principal, desmarcar la anterior
            if (sucursal.EsPrincipal)
            {
                var principal = await _unitOfWork.SucursalRepository.GetPrincipalAsync(sucursal.EmpresaId);
                if (principal != null)
                {
                    _logger.LogInformation("Unmarking previous principal sucursal: {SucursalId}", principal.Id);
                    principal.EsPrincipal = false;
                    await _unitOfWork.SucursalRepository.UpdateAsync(principal);
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            sucursal.UsuarioCreacionId = userId;

            var nuevaSucursal = await _unitOfWork.SucursalRepository.AddAsync(sucursal);
            _logger.LogInformation("Created sucursal: {SucursalId}, codigo: {Codigo}, principal: {EsPrincipal}",
                nuevaSucursal.Id, nuevaSucursal.Codigo, nuevaSucursal.EsPrincipal);

            return Ok(nuevaSucursal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sucursal with codigo: {Codigo}", sucursal.Codigo);
            return StatusCode(500, $"Error al crear sucursal: {ex.Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Sucursal sucursal)
    {
        try
        {
            _logger.LogInformation("PutAsync called for SucursalId: {SucursalId}", id);

            if (id != sucursal.Id)
            {
                _logger.LogWarning("ID mismatch in sucursal update: URL {UrlId} vs Body {BodyId}", id, sucursal.Id);
                return BadRequest("El ID de la URL no coincide con el ID de la sucursal.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for sucursal update");
                return BadRequest(ModelState);
            }

            var sucursalExistente = await _unitOfWork.SucursalRepository.GetAsync(id);
            if (sucursalExistente == null)
            {
                _logger.LogWarning("Sucursal not found for update: {SucursalId}", id);
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(sucursalExistente.EmpresaId))
            {
                _logger.LogWarning("Access denied to update Sucursal: {SucursalId} for user: {UserId}",
                    id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            // Verificar código único
            var duplicado = await _unitOfWork.SucursalRepository.GetByCodigoAsync(
                sucursal.EmpresaId,
                sucursal.Codigo);

            if (duplicado != null && duplicado.Id != id)
            {
                _logger.LogWarning("Duplicate sucursal codigo on update: {Codigo} for EmpresaId: {EmpresaId}",
                    sucursal.Codigo, sucursal.EmpresaId);
                return BadRequest("Ya existe otra sucursal con este código en la empresa.");
            }

            // Si se marca como principal, desmarcar la anterior
            if (sucursal.EsPrincipal && !sucursalExistente.EsPrincipal)
            {
                var principal = await _unitOfWork.SucursalRepository.GetPrincipalAsync(sucursal.EmpresaId);
                if (principal != null && principal.Id != id)
                {
                    _logger.LogInformation("Unmarking previous principal sucursal: {SucursalId} on update", principal.Id);
                    principal.EsPrincipal = false;
                    await _unitOfWork.SucursalRepository.UpdateAsync(principal);
                }
            }

            // Preservar campos de auditoría
            sucursal.FechaCreacion = sucursalExistente.FechaCreacion;
            sucursal.UsuarioCreacionId = sucursalExistente.UsuarioCreacionId;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            sucursal.UsuarioModificacionId = userId;

            await _unitOfWork.SucursalRepository.UpdateAsync(sucursal);
            _logger.LogInformation("Updated sucursal: {SucursalId}, codigo: {Codigo}", id, sucursal.Codigo);

            return Ok(sucursal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sucursal: {SucursalId}", id);
            return StatusCode(500, $"Error al actualizar sucursal: {ex.Message}");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("DeleteAsync called for SucursalId: {SucursalId}", id);

            var sucursal = await _unitOfWork.SucursalRepository.GetAsync(id);

            if (sucursal == null)
            {
                _logger.LogWarning("Sucursal not found for deletion: {SucursalId}", id);
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(sucursal.EmpresaId))
            {
                _logger.LogWarning("Access denied to delete Sucursal: {SucursalId} for user: {UserId}",
                    id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            // Verificar que no tenga terminales activos
            var terminales = await _unitOfWork.TerminalRepository.GetBySucursalAsync(id);
            if (terminales.Any())
            {
                _logger.LogWarning("Cannot delete sucursal {SucursalId}: has {Count} terminals",
                    id, terminales.Count());
                return BadRequest("No se puede eliminar la sucursal porque tiene terminales asignados.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.SucursalRepository.DeleteAsync(id, userId!);
            _logger.LogInformation("Successfully deleted sucursal: {SucursalId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sucursal: {SucursalId}", id);
            return StatusCode(500, $"Error al eliminar sucursal: {ex.Message}");
        }
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (userRoles.Contains("SuperUser"))
        {
            return true;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
