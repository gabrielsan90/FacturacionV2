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

    public SucursalesController(ISucursalUnitOfWork unitOfWork, DataContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // SuperUser can see all sucursales
        if (userRoles.Contains("SuperUser"))
        {
            var allSucursales = await _context.Sucursales
                .Include(s => s.Empresa)
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Codigo)
                .ToListAsync();
            return Ok(allSucursales);
        }

        // Other users can only see sucursales from their empresa
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var empresaIds = await _context.UsuariosEmpresas
            .Where(ue => ue.UserId == userId)
            .Select(ue => ue.EmpresaId)
            .ToListAsync();

        var sucursales = await _context.Sucursales
            .Include(s => s.Empresa)
            .Where(s => !s.IsDeleted && empresaIds.Contains(s.EmpresaId))
            .OrderBy(s => s.Codigo)
            .ToListAsync();

        return Ok(sucursales);
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var sucursales = await _unitOfWork.SucursalRepository.GetByEmpresaAsync(empresaId);
        return Ok(sucursales);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var sucursal = await _unitOfWork.SucursalRepository.GetAsync(id);

        if (sucursal == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(sucursal.EmpresaId))
        {
            return Forbid();
        }

        return Ok(sucursal);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Sucursal sucursal)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(sucursal.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no exista otra sucursal con el mismo código
        var existente = await _unitOfWork.SucursalRepository.GetByCodigoAsync(
            sucursal.EmpresaId,
            sucursal.Codigo);

        if (existente != null)
        {
            return BadRequest("Ya existe una sucursal con este código en la empresa.");
        }

        // Si se marca como principal, desmarcar la anterior
        if (sucursal.EsPrincipal)
        {
            var principal = await _unitOfWork.SucursalRepository.GetPrincipalAsync(sucursal.EmpresaId);
            if (principal != null)
            {
                principal.EsPrincipal = false;
                await _unitOfWork.SucursalRepository.UpdateAsync(principal);
            }
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        sucursal.UsuarioCreacionId = userId;

        var nuevaSucursal = await _unitOfWork.SucursalRepository.AddAsync(sucursal);

        return Ok(nuevaSucursal);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Sucursal sucursal)
    {
        if (id != sucursal.Id)
        {
            return BadRequest("El ID de la URL no coincide con el ID de la sucursal.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var sucursalExistente = await _unitOfWork.SucursalRepository.GetAsync(id);
        if (sucursalExistente == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(sucursalExistente.EmpresaId))
        {
            return Forbid();
        }

        // Verificar código único
        var duplicado = await _unitOfWork.SucursalRepository.GetByCodigoAsync(
            sucursal.EmpresaId,
            sucursal.Codigo);

        if (duplicado != null && duplicado.Id != id)
        {
            return BadRequest("Ya existe otra sucursal con este código en la empresa.");
        }

        // Si se marca como principal, desmarcar la anterior
        if (sucursal.EsPrincipal && !sucursalExistente.EsPrincipal)
        {
            var principal = await _unitOfWork.SucursalRepository.GetPrincipalAsync(sucursal.EmpresaId);
            if (principal != null && principal.Id != id)
            {
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

        return Ok(sucursal);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var sucursal = await _unitOfWork.SucursalRepository.GetAsync(id);

        if (sucursal == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(sucursal.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no tenga terminales activos
        var terminales = await _unitOfWork.TerminalRepository.GetBySucursalAsync(id);
        if (terminales.Any())
        {
            return BadRequest("No se puede eliminar la sucursal porque tiene terminales asignados.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _unitOfWork.SucursalRepository.DeleteAsync(id, userId!);

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
