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
public class TerminalesController : ControllerBase
{
    private readonly ISucursalUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<TerminalesController> _logger;

    public TerminalesController(
        ISucursalUnitOfWork unitOfWork,
        DataContext context,
        ILogger<TerminalesController> logger)
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
            _logger.LogInformation("GetByEmpresaAsync called for EmpresaId: {EmpresaId}", empresaId);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Access denied to EmpresaId: {EmpresaId} for user: {UserId}",
                    empresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            // Obtener todas las sucursales de la empresa
            var sucursales = await _context.Sucursales
                .Where(s => s.EmpresaId == empresaId && !s.IsDeleted && s.Activo)
                .Select(s => s.Id)
                .ToListAsync();

            // Obtener todas las terminales de esas sucursales
            var terminales = await _context.Terminales
                .Where(t => sucursales.Contains(t.SucursalId) && !t.IsDeleted && t.Activo)
                .Select(t => new
                {
                    t.Id,
                    t.Codigo,
                    t.Nombre,
                    t.SucursalId,
                    Descripcion = t.Nombre // Para compatibilidad con el frontend
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} terminales for EmpresaId: {EmpresaId}",
                terminales.Count, empresaId);
            return Ok(terminales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving terminales for EmpresaId: {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener terminales: {ex.Message}");
        }
    }

    [HttpGet("sucursal/{sucursalId:guid}")]
    public async Task<IActionResult> GetBySucursalAsync(Guid sucursalId)
    {
        try
        {
            _logger.LogInformation("GetBySucursalAsync called for SucursalId: {SucursalId}", sucursalId);

            var sucursal = await _unitOfWork.SucursalRepository.GetAsync(sucursalId);
            if (sucursal == null)
            {
                _logger.LogWarning("Sucursal not found: {SucursalId}", sucursalId);
                return NotFound("Sucursal no encontrada.");
            }

            if (!await TieneAccesoEmpresaAsync(sucursal.EmpresaId))
            {
                _logger.LogWarning("Access denied to Sucursal: {SucursalId} for user: {UserId}",
                    sucursalId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var terminales = await _unitOfWork.TerminalRepository.GetBySucursalAsync(sucursalId);
            _logger.LogInformation("Retrieved {Count} terminales for SucursalId: {SucursalId}",
                terminales?.Count() ?? 0, sucursalId);
            return Ok(terminales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving terminales for SucursalId: {SucursalId}", sucursalId);
            return StatusCode(500, $"Error al obtener terminales: {ex.Message}");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("GetAsync called for TerminalId: {TerminalId}", id);

            var terminal = await _unitOfWork.TerminalRepository.GetAsync(id);

            if (terminal == null)
            {
                _logger.LogWarning("Terminal not found: {TerminalId}", id);
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(terminal.Sucursal.EmpresaId))
            {
                _logger.LogWarning("Access denied to Terminal: {TerminalId} for user: {UserId}",
                    id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            _logger.LogInformation("Retrieved terminal: {TerminalId}, codigo: {Codigo}", id, terminal.Codigo);
            return Ok(terminal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving terminal: {TerminalId}", id);
            return StatusCode(500, $"Error al obtener terminal: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Terminal terminal)
    {
        try
        {
            _logger.LogInformation("PostAsync called to create terminal with codigo: {Codigo} for SucursalId: {SucursalId}",
                terminal.Codigo, terminal.SucursalId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for terminal creation");
                return BadRequest(ModelState);
            }

            // Verificar acceso a la sucursal
            var sucursal = await _unitOfWork.SucursalRepository.GetAsync(terminal.SucursalId);
            if (sucursal == null)
            {
                _logger.LogWarning("Sucursal not found: {SucursalId}", terminal.SucursalId);
                return NotFound("Sucursal no encontrada.");
            }

            if (!await TieneAccesoEmpresaAsync(sucursal.EmpresaId))
            {
                _logger.LogWarning("Access denied to create terminal for Sucursal: {SucursalId}, user: {UserId}",
                    terminal.SucursalId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            // Verificar código único en sucursal
            var existente = await _unitOfWork.TerminalRepository.GetByCodigoAsync(
                terminal.SucursalId,
                terminal.Codigo);

            if (existente != null)
            {
                _logger.LogWarning("Duplicate terminal codigo: {Codigo} for SucursalId: {SucursalId}",
                    terminal.Codigo, terminal.SucursalId);
                return BadRequest("Ya existe un terminal con este código en la sucursal.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            terminal.UsuarioCreacionId = userId;

            var nuevoTerminal = await _unitOfWork.TerminalRepository.AddAsync(terminal);
            _logger.LogInformation("Created terminal: {TerminalId}, codigo: {Codigo}",
                nuevoTerminal.Id, nuevoTerminal.Codigo);

            return Ok(nuevoTerminal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating terminal with codigo: {Codigo}", terminal.Codigo);
            return StatusCode(500, $"Error al crear terminal: {ex.Message}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Terminal terminal)
    {
        try
        {
            _logger.LogInformation("PutAsync called for TerminalId: {TerminalId}", id);

            if (id != terminal.Id)
            {
                _logger.LogWarning("ID mismatch in terminal update: URL {UrlId} vs Body {BodyId}", id, terminal.Id);
                return BadRequest("El ID de la URL no coincide con el ID del terminal.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for terminal update");
                return BadRequest(ModelState);
            }

            var terminalExistente = await _unitOfWork.TerminalRepository.GetAsync(id);
            if (terminalExistente == null)
            {
                _logger.LogWarning("Terminal not found for update: {TerminalId}", id);
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(terminalExistente.Sucursal.EmpresaId))
            {
                _logger.LogWarning("Access denied to update Terminal: {TerminalId} for user: {UserId}",
                    id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            // Verificar código único en sucursal
            var duplicado = await _unitOfWork.TerminalRepository.GetByCodigoAsync(
                terminal.SucursalId,
                terminal.Codigo);

            if (duplicado != null && duplicado.Id != id)
            {
                _logger.LogWarning("Duplicate terminal codigo on update: {Codigo} for SucursalId: {SucursalId}",
                    terminal.Codigo, terminal.SucursalId);
                return BadRequest("Ya existe otro terminal con este código en la sucursal.");
            }

            // Preservar campos de auditoría
            terminal.FechaCreacion = terminalExistente.FechaCreacion;
            terminal.UsuarioCreacionId = terminalExistente.UsuarioCreacionId;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            terminal.UsuarioModificacionId = userId;

            await _unitOfWork.TerminalRepository.UpdateAsync(terminal);
            _logger.LogInformation("Updated terminal: {TerminalId}, codigo: {Codigo}", id, terminal.Codigo);

            return Ok(terminal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating terminal: {TerminalId}", id);
            return StatusCode(500, $"Error al actualizar terminal: {ex.Message}");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("DeleteAsync called for TerminalId: {TerminalId}", id);

            var terminal = await _unitOfWork.TerminalRepository.GetAsync(id);

            if (terminal == null)
            {
                _logger.LogWarning("Terminal not found for deletion: {TerminalId}", id);
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(terminal.Sucursal.EmpresaId))
            {
                _logger.LogWarning("Access denied to delete Terminal: {TerminalId} for user: {UserId}",
                    id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            // Verificar que no tenga documentos asociados
            var tieneDocumentos = await _context.Documentos
                .AnyAsync(d => d.TerminalId == id && !d.IsDeleted);

            if (tieneDocumentos)
            {
                _logger.LogWarning("Cannot delete terminal {TerminalId}: has associated documents", id);
                return BadRequest("No se puede eliminar el terminal porque tiene documentos asociados.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.TerminalRepository.DeleteAsync(id, userId!);
            _logger.LogInformation("Successfully deleted terminal: {TerminalId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting terminal: {TerminalId}", id);
            return StatusCode(500, $"Error al eliminar terminal: {ex.Message}");
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
