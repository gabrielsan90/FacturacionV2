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

    public TerminalesController(ISucursalUnitOfWork unitOfWork, DataContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
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

        return Ok(terminales);
    }

    [HttpGet("sucursal/{sucursalId:guid}")]
    public async Task<IActionResult> GetBySucursalAsync(Guid sucursalId)
    {
        var sucursal = await _unitOfWork.SucursalRepository.GetAsync(sucursalId);
        if (sucursal == null)
        {
            return NotFound("Sucursal no encontrada.");
        }

        if (!await TieneAccesoEmpresaAsync(sucursal.EmpresaId))
        {
            return Forbid();
        }

        var terminales = await _unitOfWork.TerminalRepository.GetBySucursalAsync(sucursalId);
        return Ok(terminales);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var terminal = await _unitOfWork.TerminalRepository.GetAsync(id);

        if (terminal == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(terminal.Sucursal.EmpresaId))
        {
            return Forbid();
        }

        return Ok(terminal);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Terminal terminal)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar acceso a la sucursal
        var sucursal = await _unitOfWork.SucursalRepository.GetAsync(terminal.SucursalId);
        if (sucursal == null)
        {
            return NotFound("Sucursal no encontrada.");
        }

        if (!await TieneAccesoEmpresaAsync(sucursal.EmpresaId))
        {
            return Forbid();
        }

        // Verificar código único en sucursal
        var existente = await _unitOfWork.TerminalRepository.GetByCodigoAsync(
            terminal.SucursalId,
            terminal.Codigo);

        if (existente != null)
        {
            return BadRequest("Ya existe un terminal con este código en la sucursal.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        terminal.UsuarioCreacionId = userId;

        var nuevoTerminal = await _unitOfWork.TerminalRepository.AddAsync(terminal);

        return Ok(nuevoTerminal);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Terminal terminal)
    {
        if (id != terminal.Id)
        {
            return BadRequest("El ID de la URL no coincide con el ID del terminal.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var terminalExistente = await _unitOfWork.TerminalRepository.GetAsync(id);
        if (terminalExistente == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(terminalExistente.Sucursal.EmpresaId))
        {
            return Forbid();
        }

        // Verificar código único en sucursal
        var duplicado = await _unitOfWork.TerminalRepository.GetByCodigoAsync(
            terminal.SucursalId,
            terminal.Codigo);

        if (duplicado != null && duplicado.Id != id)
        {
            return BadRequest("Ya existe otro terminal con este código en la sucursal.");
        }

        // Preservar campos de auditoría
        terminal.FechaCreacion = terminalExistente.FechaCreacion;
        terminal.UsuarioCreacionId = terminalExistente.UsuarioCreacionId;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        terminal.UsuarioModificacionId = userId;

        await _unitOfWork.TerminalRepository.UpdateAsync(terminal);

        return Ok(terminal);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var terminal = await _unitOfWork.TerminalRepository.GetAsync(id);

        if (terminal == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(terminal.Sucursal.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no tenga documentos asociados
        var tieneDocumentos = await _context.Documentos
            .AnyAsync(d => d.TerminalId == id && !d.IsDeleted);

        if (tieneDocumentos)
        {
            return BadRequest("No se puede eliminar el terminal porque tiene documentos asociados.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _unitOfWork.TerminalRepository.DeleteAsync(id, userId!);

        return NoContent();
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
