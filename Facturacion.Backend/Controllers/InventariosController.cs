using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
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
    private readonly ILogger<InventariosController> _logger;
    private readonly IExcelImportService _excelImportService;

    public InventariosController(
        IInventarioUnitOfWork unitOfWork,
        DataContext context,
        ILogger<InventariosController> logger,
        IExcelImportService excelImportService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
        _excelImportService = excelImportService;
    }

    /// <summary>
    /// Obtiene todos los registros de inventario de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            _logger.LogInformation("GetByEmpresaAsync called for EmpresaId: {EmpresaId}", empresaId);

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Access denied to EmpresaId: {EmpresaId} for user: {UserId}",
                    empresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var inventarios = await _unitOfWork.InventarioRepository.GetByEmpresaAsync(empresaId);
            _logger.LogInformation("Retrieved {Count} inventory records for EmpresaId: {EmpresaId}",
                inventarios?.Count() ?? 0, empresaId);
            return Ok(inventarios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventories for EmpresaId: {EmpresaId}", empresaId);
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
            _logger.LogInformation("GetAsync called for InventarioId: {InventarioId}", id);

            var inventario = await _unitOfWork.InventarioRepository.GetAsync(id);

            if (inventario == null)
            {
                _logger.LogWarning("Inventory not found: {InventarioId}", id);
                return NotFound("Registro de inventario no encontrado.");
            }

            // Verificar que el usuario tiene acceso a la empresa
            if (inventario.Sucursal?.EmpresaId != null)
            {
                if (!await TieneAccesoEmpresaAsync(inventario.Sucursal.EmpresaId))
                {
                    _logger.LogWarning("Access denied to Inventario: {InventarioId} for user: {UserId}",
                        id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                    return Forbid();
                }
            }

            _logger.LogInformation("Retrieved inventory: {InventarioId} for Producto: {ProductoId}",
                id, inventario.ProductoId);
            return Ok(inventario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory: {InventarioId}", id);
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
            _logger.LogInformation("GetBajoStockAsync called for EmpresaId: {EmpresaId}", empresaId);

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Access denied to EmpresaId: {EmpresaId} for user: {UserId}",
                    empresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var inventarios = await _unitOfWork.InventarioRepository.GetBajoStockAsync(empresaId);
            _logger.LogInformation("Retrieved {Count} low stock items for EmpresaId: {EmpresaId}",
                inventarios?.Count() ?? 0, empresaId);
            return Ok(inventarios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock inventories for EmpresaId: {EmpresaId}", empresaId);
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
            _logger.LogInformation("PostAsync called to create inventory for Producto: {ProductoId}, Sucursal: {SucursalId}",
                inventario.ProductoId, inventario.SucursalId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for inventory creation");
                return BadRequest(ModelState);
            }

            // Verificar que el producto existe
            var producto = await _context.Productos
                .Include(p => p.Empresa)
                .FirstOrDefaultAsync(p => p.Id == inventario.ProductoId && !p.IsDeleted);

            if (producto == null)
            {
                _logger.LogWarning("Product not found: {ProductoId}", inventario.ProductoId);
                return BadRequest("El producto especificado no existe.");
            }

            // Verificar que el producto tiene control de inventario activado
            if (!producto.ControlarInventario)
            {
                _logger.LogWarning("Product {ProductoId} does not have inventory control enabled", inventario.ProductoId);
                return BadRequest("El producto no tiene control de inventario activado.");
            }

            // Verificar que el usuario tiene acceso a la empresa del producto
            if (!await TieneAccesoEmpresaAsync(producto.EmpresaId))
            {
                _logger.LogWarning("Access denied to create inventory for Producto: {ProductoId}, user: {UserId}",
                    inventario.ProductoId, User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            _logger.LogInformation("Created inventory: {InventarioId} for Producto: {ProductoId}, initial quantity: {Cantidad}",
                nuevoInventario.Id, inventario.ProductoId, inventario.CantidadActual);

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
                    Fecha = FechaCostaRicaHelper.Ahora,
                    SucursalOrigenId = inventario.SucursalId,
                    FechaCreacion = FechaCostaRicaHelper.Ahora,
                    UsuarioCreacionId = userId!
                };

                await _unitOfWork.MovimientoInventarioRepository.AddAsync(movimiento);
                _logger.LogInformation("Created initial movement for Inventario: {InventarioId}, cantidad: {Cantidad}",
                    nuevoInventario.Id, inventario.CantidadActual);
            }

            return Ok(nuevoInventario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory for Producto: {ProductoId}, Sucursal: {SucursalId}",
                inventario.ProductoId, inventario.SucursalId);
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
            _logger.LogInformation("PostAjusteAsync called for InventarioId: {InventarioId}, adjustment: {Cantidad}",
                id, ajuste.Cantidad);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for inventory adjustment");
                return BadRequest(ModelState);
            }

            // Validar que la cantidad no sea cero
            if (ajuste.Cantidad == 0)
            {
                _logger.LogWarning("Invalid adjustment quantity (zero) for Inventario: {InventarioId}", id);
                return BadRequest("La cantidad del ajuste no puede ser cero.");
            }

            // Verificar que el inventario existe
            var inventario = await _unitOfWork.InventarioRepository.GetAsync(id);
            if (inventario == null)
            {
                _logger.LogWarning("Inventory not found for adjustment: {InventarioId}", id);
                return NotFound("Registro de inventario no encontrado.");
            }

            // Verificar que el usuario tiene acceso a la empresa
            if (inventario.Sucursal?.EmpresaId != null)
            {
                if (!await TieneAccesoEmpresaAsync(inventario.Sucursal.EmpresaId))
                {
                    _logger.LogWarning("Access denied to adjust Inventario: {InventarioId} for user: {UserId}",
                        id, User.FindFirstValue(ClaimTypes.NameIdentifier));
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
                _logger.LogError("Failed to adjust inventory: {InventarioId}", id);
                return BadRequest("No se pudo realizar el ajuste de inventario.");
            }

            _logger.LogInformation("Successfully adjusted inventory: {InventarioId}, new quantity: {Cantidad}",
                id, cantidadResultante);

            // Obtener el inventario actualizado
            var inventarioActualizado = await _unitOfWork.InventarioRepository.GetAsync(id);
            return Ok(inventarioActualizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting inventory: {InventarioId}, adjustment: {Cantidad}",
                id, ajuste.Cantidad);
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
            _logger.LogInformation("PostTrasladoAsync called for InventarioId: {InventarioId}, to Sucursal: {SucursalDestinoId}, quantity: {Cantidad}",
                id, traslado.SucursalDestinoId, traslado.Cantidad);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for inventory transfer");
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
                _logger.LogError("Failed to transfer inventory: {InventarioId} to Sucursal: {SucursalDestinoId}",
                    id, traslado.SucursalDestinoId);
                return BadRequest("No se pudo realizar el traslado de inventario.");
            }

            _logger.LogInformation("Successfully transferred inventory: {InventarioId}, quantity: {Cantidad} to Sucursal: {SucursalDestinoId}",
                id, traslado.Cantidad, traslado.SucursalDestinoId);

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
            _logger.LogError(ex, "Error transferring inventory: {InventarioId} to Sucursal: {SucursalDestinoId}, quantity: {Cantidad}",
                id, traslado.SucursalDestinoId, traslado.Cantidad);
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
            _logger.LogInformation("DeleteAsync called for InventarioId: {InventarioId}", id);

            var inventario = await _unitOfWork.InventarioRepository.GetAsync(id);

            if (inventario == null)
            {
                _logger.LogWarning("Inventory not found for deletion: {InventarioId}", id);
                return NotFound("Registro de inventario no encontrado.");
            }

            // Verificar que el usuario tiene acceso a la empresa
            if (inventario.Sucursal?.EmpresaId != null)
            {
                if (!await TieneAccesoEmpresaAsync(inventario.Sucursal.EmpresaId))
                {
                    _logger.LogWarning("Access denied to delete Inventario: {InventarioId} for user: {UserId}",
                        id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                    return Forbid();
                }
            }

            // Validar que el inventario no tenga cantidad actual
            if (inventario.CantidadActual > 0)
            {
                _logger.LogWarning("Cannot delete inventory with stock: {InventarioId}, current quantity: {Cantidad}",
                    id, inventario.CantidadActual);
                return BadRequest("No se puede eliminar un registro de inventario con existencias. Primero debe ajustar el inventario a cero.");
            }

            // Validar que no haya cantidad reservada
            if ((inventario.CantidadReservada ?? 0) > 0)
            {
                _logger.LogWarning("Cannot delete inventory with reserved quantity: {InventarioId}, reserved: {Cantidad}",
                    id, inventario.CantidadReservada);
                return BadRequest("No se puede eliminar un registro de inventario con cantidad reservada.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.InventarioRepository.DeleteAsync(id, userId!);
            _logger.LogInformation("Successfully deleted inventory: {InventarioId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory: {InventarioId}", id);
            return StatusCode(500, $"Error al eliminar inventario: {ex.Message}");
        }
    }

    /// <summary>
    /// Importa inventario desde un archivo Excel
    /// </summary>
    [HttpPost("empresa/{empresaId:guid}/importar")]
    public async Task<IActionResult> ImportarAsync(Guid empresaId, [FromQuery] Guid sucursalId, IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe proporcionar un archivo Excel.");

        if (!await TieneAccesoEmpresaAsync(empresaId))
            return Forbid();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        using var stream = archivo.OpenReadStream();
        var result = await _excelImportService.ImportarInventarioAsync(stream, empresaId, sucursalId, userId!);
        return Ok(result);
    }

    /// <summary>
    /// Descarga una plantilla de Excel para importar inventario
    /// </summary>
    [HttpGet("plantilla")]
    public IActionResult DescargarPlantilla()
    {
        var fileBytes = _excelImportService.GenerarPlantillaInventario();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_Inventario.xlsx");
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
