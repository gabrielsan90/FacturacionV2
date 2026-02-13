using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
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
    private readonly IExcelImportService _excelImportService;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(
        IProductoUnitOfWork unitOfWork,
        DataContext context,
        IExcelImportService excelImportService,
        ILogger<ProductosController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _excelImportService = excelImportService;
        _logger = logger;
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a productos de empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var productos = await _unitOfWork.ProductoRepository.GetByEmpresaAsync(empresaId);
            _logger.LogInformation("Se obtuvieron {Count} productos para empresa {EmpresaId}", productos.Count(), empresaId);
            return Ok(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al obtener productos.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var producto = await _unitOfWork.ProductoRepository.GetAsync(id);

            if (producto == null)
            {
                _logger.LogWarning("Producto {ProductoId} no encontrado", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a la empresa del producto
            if (!await TieneAccesoEmpresaAsync(producto.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a producto {ProductoId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            return Ok(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto {ProductoId}", id);
            return StatusCode(500, "Error interno al obtener producto.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Producto producto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de crear producto con datos inválidos");
                return BadRequest(ModelState);
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(producto.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó crear producto en empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), producto.EmpresaId);
                return Forbid();
            }

            // Verificar que no exista otro producto con el mismo código en esta empresa
            var existente = await _unitOfWork.ProductoRepository.GetByCodigoAsync(
                producto.EmpresaId,
                producto.Codigo);

            if (existente != null)
            {
                _logger.LogWarning("Intento de crear producto duplicado con código {Codigo} en empresa {EmpresaId}",
                    producto.Codigo, producto.EmpresaId);
                return BadRequest(new { success = false, message = "Ya existe un producto con este código en la empresa." });
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
                _logger.LogWarning("Intento de crear producto con unidad de medida inválida {UnidadMedidaId}", producto.UnidadMedidaId);
                return BadRequest(new { success = false, message = "La unidad de medida especificada no existe." });
            }

            if (!await _context.Impuestos.AnyAsync(i => i.Id == producto.ImpuestoId))
            {
                _logger.LogWarning("Intento de crear producto con impuesto inválido {ImpuestoId}", producto.ImpuestoId);
                return BadRequest(new { success = false, message = "El impuesto especificado no existe." });
            }

            if (producto.CategoriaId.HasValue)
            {
                var categoria = await _unitOfWork.CategoriaRepository.GetAsync(producto.CategoriaId.Value);
                if (categoria == null || categoria.EmpresaId != producto.EmpresaId)
                {
                    _logger.LogWarning("Intento de crear producto con categoría inválida {CategoriaId}", producto.CategoriaId.Value);
                    return BadRequest(new { success = false, message = "La categoría especificada no existe o no pertenece a la empresa." });
                }
            }

            // Validar código CABYS si se especifica
            if (producto.CabysId.HasValue)
            {
                if (!await _context.CatalogosCAByS.AnyAsync(c => c.Id == producto.CabysId.Value && c.Activo))
                {
                    _logger.LogWarning("Intento de crear producto con código CABYS inválido {CabysId}", producto.CabysId.Value);
                    return BadRequest(new { success = false, message = "El código CABYS especificado no existe o no está activo." });
                }
            }

            // Establecer usuario de creación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            producto.UsuarioCreacionId = userId;

            var nuevoProducto = await _unitOfWork.ProductoRepository.AddAsync(producto);

            _logger.LogInformation("Producto {ProductoId} creado exitosamente para empresa {EmpresaId} por usuario {UserId}",
                nuevoProducto.Id, producto.EmpresaId, userId);

            return Ok(new { success = true, message = "Producto/Servicio creado exitosamente", data = nuevoProducto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto para empresa {EmpresaId}", producto.EmpresaId);
            return StatusCode(500, "Error interno al crear producto.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Producto producto)
    {
        try
        {
            if (id != producto.Id)
            {
                _logger.LogWarning("Intento de actualizar producto con ID inconsistente. URL: {UrlId}, Body: {BodyId}", id, producto.Id);
                return BadRequest(new { success = false, message = "El ID de la URL no coincide con el ID del producto." });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de actualizar producto {ProductoId} con datos inválidos", id);
                return BadRequest(ModelState);
            }

            // Verificar que el producto existe
            var productoExistente = await _unitOfWork.ProductoRepository.GetAsync(id);
            if (productoExistente == null)
            {
                _logger.LogWarning("Intento de actualizar producto inexistente {ProductoId}", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(productoExistente.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó actualizar producto {ProductoId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // Verificar que no exista otro producto con el mismo código en esta empresa
            var duplicado = await _unitOfWork.ProductoRepository.GetByCodigoAsync(
                producto.EmpresaId,
                producto.Codigo);

            if (duplicado != null && duplicado.Id != id)
            {
                _logger.LogWarning("Intento de actualizar producto {ProductoId} con código duplicado {Codigo}",
                    id, producto.Codigo);
                return BadRequest(new { success = false, message = "Ya existe otro producto con este código en la empresa." });
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
                _logger.LogWarning("Intento de actualizar producto {ProductoId} con unidad de medida inválida {UnidadMedidaId}",
                    id, producto.UnidadMedidaId);
                return BadRequest(new { success = false, message = "La unidad de medida especificada no existe." });
            }

            if (!await _context.Impuestos.AnyAsync(i => i.Id == producto.ImpuestoId))
            {
                _logger.LogWarning("Intento de actualizar producto {ProductoId} con impuesto inválido {ImpuestoId}",
                    id, producto.ImpuestoId);
                return BadRequest(new { success = false, message = "El impuesto especificado no existe." });
            }

            if (producto.CategoriaId.HasValue)
            {
                var categoria = await _unitOfWork.CategoriaRepository.GetAsync(producto.CategoriaId.Value);
                if (categoria == null || categoria.EmpresaId != producto.EmpresaId)
                {
                    _logger.LogWarning("Intento de actualizar producto {ProductoId} con categoría inválida {CategoriaId}",
                        id, producto.CategoriaId.Value);
                    return BadRequest(new { success = false, message = "La categoría especificada no existe o no pertenece a la empresa." });
                }
            }

            // Validar código CABYS si se especifica
            if (producto.CabysId.HasValue)
            {
                if (!await _context.CatalogosCAByS.AnyAsync(c => c.Id == producto.CabysId.Value && c.Activo))
                {
                    _logger.LogWarning("Intento de actualizar producto {ProductoId} con código CABYS inválido {CabysId}",
                        id, producto.CabysId.Value);
                    return BadRequest(new { success = false, message = "El código CABYS especificado no existe o no está activo." });
                }
            }

            // Preservar campos de auditoría
            producto.FechaCreacion = productoExistente.FechaCreacion;
            producto.UsuarioCreacionId = productoExistente.UsuarioCreacionId;

            // Establecer usuario de modificación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            producto.UsuarioModificacionId = userId;

            await _unitOfWork.ProductoRepository.UpdateAsync(producto);

            _logger.LogInformation("Producto {ProductoId} actualizado exitosamente por usuario {UserId}", id, userId);

            return Ok(new { success = true, message = "Producto/Servicio actualizado exitosamente", data = producto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar producto {ProductoId}", id);
            return StatusCode(500, "Error interno al actualizar producto.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var producto = await _unitOfWork.ProductoRepository.GetAsync(id);

            if (producto == null)
            {
                _logger.LogWarning("Intento de eliminar producto inexistente {ProductoId}", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(producto.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó eliminar producto {ProductoId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // TODO: Verificar que el producto no tenga documentos o movimientos asociados
            // Por ahora solo hacemos soft delete

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.ProductoRepository.DeleteAsync(id, userId!);

            _logger.LogInformation("Producto {ProductoId} eliminado exitosamente por usuario {UserId}", id, userId);

            return Ok(new { success = true, message = "Producto/Servicio eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar producto {ProductoId}", id);
            return StatusCode(500, "Error interno al eliminar producto.");
        }
    }

    /// <summary>
    /// Importa productos desde un archivo Excel
    /// </summary>
    [HttpPost("empresa/{empresaId:guid}/importar")]
    public async Task<IActionResult> ImportarAsync(Guid empresaId, IFormFile archivo)
    {
        try
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest("No se proporcionó un archivo válido");
            }

            // Validar extensión
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                return BadRequest("El archivo debe ser un documento Excel (.xlsx o .xls)");
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó importar productos a empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            using var stream = archivo.OpenReadStream();
            var resultado = await _excelImportService.ImportarProductosAsync(stream, empresaId, userId);

            _logger.LogInformation("Importación de productos para empresa {EmpresaId}: {Success} exitosos, {Errors} errores",
                empresaId, resultado.SuccessCount, resultado.ErrorCount);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar productos para empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al importar productos");
        }
    }

    /// <summary>
    /// Descarga la plantilla Excel para importación de productos
    /// </summary>
    [HttpGet("plantilla")]
    public IActionResult DescargarPlantilla()
    {
        try
        {
            var plantilla = _excelImportService.GenerarPlantillaProductos();
            return File(plantilla, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PlantillaProductos.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar plantilla de productos");
            return StatusCode(500, "Error interno al generar plantilla");
        }
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar acceso a empresa {EmpresaId}", empresaId);
            return false;
        }
    }
}
