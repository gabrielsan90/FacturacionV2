using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.DTOs;
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
public class InventariosController : ControllerBase
{
    private readonly IInventarioUnitOfWork _unitOfWork;
    private readonly DataContext _context;

    public InventariosController(IInventarioUnitOfWork unitOfWork, DataContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los registros de inventario de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var inventarios = await _unitOfWork.InventarioRepository.GetByEmpresaAsync(empresaId);
            return Ok(inventarios);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener inventarios: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene un registro de inventario por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var inventario = await _unitOfWork.InventarioRepository.GetAsync(id);

            if (inventario == null)
            {
                return NotFound("Registro de inventario no encontrado.");
            }

            // Verificar que el usuario tiene acceso a la empresa
            if (inventario.Sucursal?.EmpresaId != null)
            {
                if (!await TieneAccesoEmpresaAsync(inventario.Sucursal.EmpresaId))
                {
                    return Forbid();
                }
            }

            return Ok(inventario);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener inventario: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene los productos con stock bajo (por debajo del mínimo)
    /// </summary>
    [HttpGet("bajostock/{empresaId:guid}")]
    public async Task<IActionResult> GetBajoStockAsync(Guid empresaId)
    {
        try
        {
            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var inventarios = await _unitOfWork.InventarioRepository.GetBajoStockAsync(empresaId);
            return Ok(inventarios);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener inventarios bajo stock: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea un nuevo registro de inventario
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Inventario inventario)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el producto existe
            var producto = await _context.Productos
                .Include(p => p.Empresa)
                .FirstOrDefaultAsync(p => p.Id == inventario.ProductoId && !p.IsDeleted);

            if (producto == null)
            {
                return BadRequest("El producto especificado no existe.");
            }

            // Verificar que el producto tiene control de inventario activado
            if (!producto.ControlarInventario)
            {
                return BadRequest("El producto no tiene control de inventario activado.");
            }

            // Verificar que el usuario tiene acceso a la empresa del producto
            if (!await TieneAccesoEmpresaAsync(producto.EmpresaId))
            {
                return Forbid();
            }

            // Verificar que la sucursal existe
            var sucursal = await _context.Sucursales
                .FirstOrDefaultAsync(s => s.Id == inventario.SucursalId && !s.IsDeleted);

            if (sucursal == null)
            {
                return BadRequest("La sucursal especificada no existe.");
            }

            // Verificar que la sucursal pertenece a la misma empresa del producto
            if (sucursal.EmpresaId != producto.EmpresaId)
            {
                return BadRequest("La sucursal no pertenece a la misma empresa del producto.");
            }

            // Verificar que no exista ya un registro de inventario para este producto en esta sucursal
            var inventarioExistente = await _unitOfWork.InventarioRepository
                .GetByProductoSucursalAsync(inventario.ProductoId, inventario.SucursalId);

            if (inventarioExistente != null)
            {
                return BadRequest("Ya existe un registro de inventario para este producto en esta sucursal.");
            }

            // Validar que la cantidad sea válida
            if (inventario.CantidadActual < 0)
            {
                return BadRequest("La cantidad actual no puede ser negativa.");
            }

            // Establecer usuario de creación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            inventario.UsuarioCreacionId = userId;
            inventario.CantidadReservada = inventario.CantidadReservada ?? 0;

            var nuevoInventario = await _unitOfWork.InventarioRepository.AddAsync(inventario);

            // Si hay cantidad inicial, crear un movimiento de inventario
            if (inventario.CantidadActual > 0)
            {
                var movimiento = new MovimientoInventario
                {
                    Id = Guid.NewGuid(),
                    InventarioId = nuevoInventario.Id,
                    TipoMovimiento = Shared.Enums.TipoMovimientoInventario.AjusteEntrada,
                    Cantidad = inventario.CantidadActual,
                    CantidadAnterior = 0,
                    CantidadNueva = inventario.CantidadActual,
                    Referencia = "Inventario inicial",
                    Observaciones = "Registro inicial de inventario",
                    Fecha = DateTime.UtcNow,
                    SucursalOrigenId = inventario.SucursalId,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacionId = userId!
                };

                await _unitOfWork.MovimientoInventarioRepository.AddAsync(movimiento);
            }

            return Ok(nuevoInventario);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear inventario: {ex.Message}");
        }
    }

    /// <summary>
    /// Ajusta el inventario (aumenta o disminuye la cantidad)
    /// </summary>
    [HttpPost("{id:guid}/ajuste")]
    public async Task<IActionResult> PostAjusteAsync(Guid id, [FromBody] AjusteInventarioDTO ajuste)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar que la cantidad no sea cero
            if (ajuste.Cantidad == 0)
            {
                return BadRequest("La cantidad del ajuste no puede ser cero.");
            }

            // Verificar que el inventario existe
            var inventario = await _unitOfWork.InventarioRepository.GetAsync(id);
            if (inventario == null)
            {
                return NotFound("Registro de inventario no encontrado.");
            }

            // Verificar que el usuario tiene acceso a la empresa
            if (inventario.Sucursal?.EmpresaId != null)
            {
                if (!await TieneAccesoEmpresaAsync(inventario.Sucursal.EmpresaId))
                {
                    return Forbid();
                }
            }

            // Validar que el ajuste no resulte en cantidad negativa
            var cantidadResultante = inventario.CantidadActual + ajuste.Cantidad;
            if (cantidadResultante < 0)
            {
                return BadRequest($"El ajuste resultaría en cantidad negativa. Cantidad actual: {inventario.CantidadActual}, Ajuste: {ajuste.Cantidad}");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var resultado = await _unitOfWork.InventarioRepository.AjustarInventarioAsync(
                id,
                ajuste.Cantidad,
                ajuste.Referencia,
                ajuste.Observaciones,
                userId!
            );

            if (!resultado)
            {
                return BadRequest("No se pudo realizar el ajuste de inventario.");
            }

            // Obtener el inventario actualizado
            var inventarioActualizado = await _unitOfWork.InventarioRepository.GetAsync(id);
            return Ok(inventarioActualizado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al ajustar inventario: {ex.Message}");
        }
    }

    /// <summary>
    /// Traslada inventario entre sucursales
    /// </summary>
    [HttpPost("{id:guid}/traslado")]
    public async Task<IActionResult> PostTrasladoAsync(Guid id, [FromBody] TrasladoInventarioDTO traslado)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar que la cantidad sea mayor a cero
            if (traslado.Cantidad <= 0)
            {
                return BadRequest("La cantidad del traslado debe ser mayor a cero.");
            }

            // Verificar que el inventario de origen existe
            var inventarioOrigen = await _unitOfWork.InventarioRepository.GetAsync(id);
            if (inventarioOrigen == null)
            {
                return NotFound("Registro de inventario de origen no encontrado.");
            }

            // Verificar que el usuario tiene acceso a la empresa
            if (inventarioOrigen.Sucursal?.EmpresaId != null)
            {
                if (!await TieneAccesoEmpresaAsync(inventarioOrigen.Sucursal.EmpresaId))
                {
                    return Forbid();
                }
            }

            // Verificar que haya suficiente cantidad disponible
            var cantidadDisponible = inventarioOrigen.CantidadActual - (inventarioOrigen.CantidadReservada ?? 0);
            if (cantidadDisponible < traslado.Cantidad)
            {
                return BadRequest($"No hay suficiente cantidad disponible para el traslado. Disponible: {cantidadDisponible}, Solicitado: {traslado.Cantidad}");
            }

            // Verificar que la sucursal destino existe
            var sucursalDestino = await _context.Sucursales
                .FirstOrDefaultAsync(s => s.Id == traslado.SucursalDestinoId && !s.IsDeleted);

            if (sucursalDestino == null)
            {
                return BadRequest("La sucursal de destino especificada no existe.");
            }

            // Verificar que origen y destino sean diferentes
            if (inventarioOrigen.SucursalId == traslado.SucursalDestinoId)
            {
                return BadRequest("La sucursal de origen y destino no pueden ser la misma.");
            }

            // Verificar que ambas sucursales pertenezcan a la misma empresa
            if (inventarioOrigen.Sucursal!.EmpresaId != sucursalDestino.EmpresaId)
            {
                return BadRequest("Las sucursales deben pertenecer a la misma empresa.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var resultado = await _unitOfWork.InventarioRepository.TrasladarInventarioAsync(
                id,
                traslado.SucursalDestinoId,
                traslado.Cantidad,
                traslado.Referencia,
                traslado.Observaciones,
                userId!
            );

            if (!resultado)
            {
                return BadRequest("No se pudo realizar el traslado de inventario.");
            }

            // Obtener el inventario actualizado de origen
            var inventarioActualizado = await _unitOfWork.InventarioRepository.GetAsync(id);
            return Ok(new
            {
                mensaje = "Traslado realizado exitosamente",
                inventarioOrigen = inventarioActualizado,
                sucursalDestino = sucursalDestino.Nombre
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al trasladar inventario: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina (soft delete) un registro de inventario
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var inventario = await _unitOfWork.InventarioRepository.GetAsync(id);

            if (inventario == null)
            {
                return NotFound("Registro de inventario no encontrado.");
            }

            // Verificar que el usuario tiene acceso a la empresa
            if (inventario.Sucursal?.EmpresaId != null)
            {
                if (!await TieneAccesoEmpresaAsync(inventario.Sucursal.EmpresaId))
                {
                    return Forbid();
                }
            }

            // Validar que el inventario no tenga cantidad actual
            if (inventario.CantidadActual > 0)
            {
                return BadRequest("No se puede eliminar un registro de inventario con existencias. Primero debe ajustar el inventario a cero.");
            }

            // Validar que no haya cantidad reservada
            if ((inventario.CantidadReservada ?? 0) > 0)
            {
                return BadRequest("No se puede eliminar un registro de inventario con cantidad reservada.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.InventarioRepository.DeleteAsync(id, userId!);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al eliminar inventario: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si el usuario actual tiene acceso a una empresa específica
    /// </summary>
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
