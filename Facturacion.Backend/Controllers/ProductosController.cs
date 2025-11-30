using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoUnitOfWork _unitOfWork;
    private readonly DataContext _context;

    public ProductosController(IProductoUnitOfWork unitOfWork, DataContext context)
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

        var productos = await _unitOfWork.ProductoRepository.GetByEmpresaAsync(empresaId);
        return Ok(productos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var producto = await _unitOfWork.ProductoRepository.GetAsync(id);

        if (producto == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a la empresa del producto
        if (!await TieneAccesoEmpresaAsync(producto.EmpresaId))
        {
            return Forbid();
        }

        return Ok(producto);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Producto producto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(producto.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no exista otro producto con el mismo código en esta empresa
        var existente = await _unitOfWork.ProductoRepository.GetByCodigoAsync(
            producto.EmpresaId,
            producto.Codigo);

        if (existente != null)
        {
            return BadRequest("Ya existe un producto con este código en la empresa.");
        }

        // Validar reglas de negocio
        if (producto.Tipo == TipoProducto.Servicio)
        {
            producto.ControlarInventario = false;
            producto.StockMinimo = null;
        }

        // Verificar que los catálogos existan
        if (!await _context.UnidadesMedida.AnyAsync(u => u.Id == producto.UnidadMedidaId))
        {
            return BadRequest("La unidad de medida especificada no existe.");
        }

        if (!await _context.Impuestos.AnyAsync(i => i.Id == producto.ImpuestoId))
        {
            return BadRequest("El impuesto especificado no existe.");
        }

        if (producto.CategoriaId.HasValue)
        {
            var categoria = await _unitOfWork.CategoriaRepository.GetAsync(producto.CategoriaId.Value);
            if (categoria == null || categoria.EmpresaId != producto.EmpresaId)
            {
                return BadRequest("La categoría especificada no existe o no pertenece a la empresa.");
            }
        }

        // Establecer usuario de creación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        producto.UsuarioCreacionId = userId;

        var nuevoProducto = await _unitOfWork.ProductoRepository.AddAsync(producto);

        return Ok(nuevoProducto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Producto producto)
    {
        if (id != producto.Id)
        {
            return BadRequest("El ID de la URL no coincide con el ID del producto.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que el producto existe
        var productoExistente = await _unitOfWork.ProductoRepository.GetAsync(id);
        if (productoExistente == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(productoExistente.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no exista otro producto con el mismo código en esta empresa
        var duplicado = await _unitOfWork.ProductoRepository.GetByCodigoAsync(
            producto.EmpresaId,
            producto.Codigo);

        if (duplicado != null && duplicado.Id != id)
        {
            return BadRequest("Ya existe otro producto con este código en la empresa.");
        }

        // Validar reglas de negocio
        if (producto.Tipo == TipoProducto.Servicio)
        {
            producto.ControlarInventario = false;
            producto.StockMinimo = null;
        }

        // Verificar que los catálogos existan
        if (!await _context.UnidadesMedida.AnyAsync(u => u.Id == producto.UnidadMedidaId))
        {
            return BadRequest("La unidad de medida especificada no existe.");
        }

        if (!await _context.Impuestos.AnyAsync(i => i.Id == producto.ImpuestoId))
        {
            return BadRequest("El impuesto especificado no existe.");
        }

        if (producto.CategoriaId.HasValue)
        {
            var categoria = await _unitOfWork.CategoriaRepository.GetAsync(producto.CategoriaId.Value);
            if (categoria == null || categoria.EmpresaId != producto.EmpresaId)
            {
                return BadRequest("La categoría especificada no existe o no pertenece a la empresa.");
            }
        }

        // Preservar campos de auditoría
        producto.FechaCreacion = productoExistente.FechaCreacion;
        producto.UsuarioCreacionId = productoExistente.UsuarioCreacionId;

        // Establecer usuario de modificación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        producto.UsuarioModificacionId = userId;

        await _unitOfWork.ProductoRepository.UpdateAsync(producto);

        return Ok(producto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var producto = await _unitOfWork.ProductoRepository.GetAsync(id);

        if (producto == null)
        {
            return NotFound();
        }

        // Verificar que el usuario tiene acceso a esta empresa
        if (!await TieneAccesoEmpresaAsync(producto.EmpresaId))
        {
            return Forbid();
        }

        // TODO: Verificar que el producto no tenga documentos o movimientos asociados
        // Por ahora solo hacemos soft delete

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _unitOfWork.ProductoRepository.DeleteAsync(id, userId!);

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
