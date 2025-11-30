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
public class ClientesController : ControllerBase
{
    private readonly IClienteUnitOfWork _unitOfWork;
    private readonly DataContext _context;

    public ClientesController(IClienteUnitOfWork unitOfWork, DataContext context)
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

        var clientes = await _unitOfWork.ClienteRepository.GetByEmpresaAsync(empresaId);
        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var cliente = await _unitOfWork.ClienteRepository.GetAsync(id);

        if (cliente == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a la empresa del cliente
        if (!await TieneAccesoEmpresaAsync(cliente.EmpresaId))
        {
            return Forbid();
        }

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Cliente cliente)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(cliente.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no exista otro cliente con la misma identificación en esta empresa
        var existente = await _unitOfWork.ClienteRepository.GetByIdentificationAsync(
            cliente.EmpresaId,
            cliente.NumeroIdentificacion);

        if (existente != null)
        {
            return BadRequest("Ya existe un cliente con esta identificación en la empresa.");
        }

        // Establecer usuario de creación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        cliente.UsuarioCreacionId = userId;

        var nuevoCliente = await _unitOfWork.ClienteRepository.AddAsync(cliente);

        return Ok(nuevoCliente);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Cliente cliente)
    {
        if (id != cliente.Id)
        {
            return BadRequest("El ID de la URL no coincide con el ID del cliente.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que el cliente existe
        var clienteExistente = await _unitOfWork.ClienteRepository.GetAsync(id);
        if (clienteExistente == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(clienteExistente.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no exista otro cliente con la misma identificación en esta empresa
        var duplicado = await _unitOfWork.ClienteRepository.GetByIdentificationAsync(
            cliente.EmpresaId,
            cliente.NumeroIdentificacion);

        if (duplicado != null && duplicado.Id != id)
        {
            return BadRequest("Ya existe otro cliente con esta identificación en la empresa.");
        }

        // Preservar campos de auditoría
        cliente.FechaCreacion = clienteExistente.FechaCreacion;
        cliente.UsuarioCreacionId = clienteExistente.UsuarioCreacionId;

        // Establecer usuario de modificación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        cliente.UsuarioModificacionId = userId;

        await _unitOfWork.ClienteRepository.UpdateAsync(cliente);

        return Ok(cliente);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var cliente = await _unitOfWork.ClienteRepository.GetAsync(id);

        if (cliente == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(cliente.EmpresaId))
        {
            return Forbid();
        }

        // TODO: Verificar que el cliente no tenga documentos asociados
        // Por ahora solo hacemos soft delete

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _unitOfWork.ClienteRepository.DeleteAsync(id, userId!);

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
