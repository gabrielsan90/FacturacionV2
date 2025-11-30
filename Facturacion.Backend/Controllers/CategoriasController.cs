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
public class CategoriasController : ControllerBase
{
    private readonly IProductoUnitOfWork _unitOfWork;
    private readonly DataContext _context;

    public CategoriasController(IProductoUnitOfWork unitOfWork, DataContext context)
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

        var categorias = await _unitOfWork.CategoriaRepository.GetByEmpresaAsync(empresaId);
        return Ok(categorias);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var categoria = await _unitOfWork.CategoriaRepository.GetAsync(id);

        if (categoria == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a la empresa de la categoría
        if (!await TieneAccesoEmpresaAsync(categoria.EmpresaId))
        {
            return Forbid();
        }

        return Ok(categoria);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Categoria categoria)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(categoria.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no exista otra categoría con el mismo nombre en esta empresa
        var existente = await _unitOfWork.CategoriaRepository.GetByNombreAsync(
            categoria.EmpresaId,
            categoria.Nombre);

        if (existente != null)
        {
            return BadRequest("Ya existe una categoría con este nombre en la empresa.");
        }

        // Establecer usuario de creación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        categoria.UsuarioCreacionId = userId;

        var nuevaCategoria = await _unitOfWork.CategoriaRepository.AddAsync(categoria);

        return CreatedAtAction(nameof(GetAsync), new { id = nuevaCategoria.Id }, nuevaCategoria);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Categoria categoria)
    {
        if (id != categoria.Id)
        {
            return BadRequest("El ID de la URL no coincide con el ID de la categoría.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que la categoría existe
        var categoriaExistente = await _unitOfWork.CategoriaRepository.GetAsync(id);
        if (categoriaExistente == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(categoriaExistente.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no exista otra categoría con el mismo nombre en esta empresa
        var duplicada = await _unitOfWork.CategoriaRepository.GetByNombreAsync(
            categoria.EmpresaId,
            categoria.Nombre);

        if (duplicada != null && duplicada.Id != id)
        {
            return BadRequest("Ya existe otra categoría con este nombre en la empresa.");
        }

        // Preservar campos de auditoría
        categoria.FechaCreacion = categoriaExistente.FechaCreacion;
        categoria.UsuarioCreacionId = categoriaExistente.UsuarioCreacionId;

        // Establecer usuario de modificación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        categoria.UsuarioModificacionId = userId;

        await _unitOfWork.CategoriaRepository.UpdateAsync(categoria);

        return Ok(categoria);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var categoria = await _unitOfWork.CategoriaRepository.GetAsync(id);

        if (categoria == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(categoria.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que la categoría no tenga productos asignados
        var productosEnCategoria = await _unitOfWork.ProductoRepository.GetByCategoriaAsync(id);
        if (productosEnCategoria.Any())
        {
            return BadRequest("No se puede eliminar la categoría porque tiene productos asignados.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _unitOfWork.CategoriaRepository.DeleteAsync(id, userId!);

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
