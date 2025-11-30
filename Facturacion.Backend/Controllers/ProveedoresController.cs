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
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorUnitOfWork _unitOfWork;
    private readonly DataContext _context;

    public ProveedoresController(IProveedorUnitOfWork unitOfWork, DataContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var proveedores = await _unitOfWork.ProveedorRepository.GetByEmpresaAsync(empresaId);
        return Ok(proveedores);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var proveedor = await _unitOfWork.ProveedorRepository.GetAsync(id);

        if (proveedor == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a la empresa del proveedor
        if (!await TieneAccesoEmpresaAsync(proveedor.EmpresaId))
        {
            return Forbid();
        }

        return Ok(proveedor);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Proveedor proveedor)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(proveedor.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no exista otro proveedor con la misma identificación en esta empresa
        var existente = await _unitOfWork.ProveedorRepository.GetByIdentificationAsync(
            proveedor.EmpresaId,
            proveedor.NumeroIdentificacion);

        if (existente != null)
        {
            return BadRequest("Ya existe un proveedor con esta identificación en la empresa.");
        }

        // Establecer usuario de creación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        proveedor.UsuarioCreacionId = userId;

        var nuevoProveedor = await _unitOfWork.ProveedorRepository.AddAsync(proveedor);

        return CreatedAtAction(nameof(GetAsync), new { id = nuevoProveedor.Id }, nuevoProveedor);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Proveedor proveedor)
    {
        if (id != proveedor.Id)
        {
            return BadRequest("El ID de la URL no coincide con el ID del proveedor.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que el proveedor existe
        var proveedorExistente = await _unitOfWork.ProveedorRepository.GetAsync(id);
        if (proveedorExistente == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(proveedorExistente.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no exista otro proveedor con la misma identificación en esta empresa
        var duplicado = await _unitOfWork.ProveedorRepository.GetByIdentificationAsync(
            proveedor.EmpresaId,
            proveedor.NumeroIdentificacion);

        if (duplicado != null && duplicado.Id != id)
        {
            return BadRequest("Ya existe otro proveedor con esta identificación en la empresa.");
        }

        // Preservar campos de auditoría
        proveedor.FechaCreacion = proveedorExistente.FechaCreacion;
        proveedor.UsuarioCreacionId = proveedorExistente.UsuarioCreacionId;

        // Establecer usuario de modificación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        proveedor.UsuarioModificacionId = userId;

        await _unitOfWork.ProveedorRepository.UpdateAsync(proveedor);

        return Ok(proveedor);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var proveedor = await _unitOfWork.ProveedorRepository.GetAsync(id);

        if (proveedor == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(proveedor.EmpresaId))
        {
            return Forbid();
        }

        // TODO: Verificar que el proveedor no tenga documentos asociados
        // Por ahora solo hacemos soft delete

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _unitOfWork.ProveedorRepository.DeleteAsync(id, userId!);

        return NoContent();
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // SuperUser tiene acceso a todas las empresas
        if (userRoles.Contains("SuperUser"))
        {
            return true;
        }

        // Otros usuarios solo tienen acceso a sus empresas asignadas
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
