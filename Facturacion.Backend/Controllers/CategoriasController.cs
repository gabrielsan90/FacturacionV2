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
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(IProductoUnitOfWork unitOfWork, DataContext context, ILogger<CategoriasController> logger)
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
            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a categorías de empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var categorias = await _unitOfWork.CategoriaRepository.GetByEmpresaAsync(empresaId);
            _logger.LogInformation("Se obtuvieron {Count} categorías para empresa {EmpresaId}", categorias.Count(), empresaId);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al obtener categorías.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var categoria = await _unitOfWork.CategoriaRepository.GetAsync(id);

            if (categoria == null)
            {
                _logger.LogWarning("Categoría {CategoriaId} no encontrada", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a la empresa de la categoría
            if (!await TieneAccesoEmpresaAsync(categoria.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó acceder a categoría {CategoriaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categoría {CategoriaId}", id);
            return StatusCode(500, "Error interno al obtener categoría.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Categoria categoria)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de crear categoría con datos inválidos");
                return BadRequest(ModelState);
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(categoria.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó crear categoría en empresa {EmpresaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), categoria.EmpresaId);
                return Forbid();
            }

            // Verificar que no exista otra categoría con el mismo nombre en esta empresa
            var existente = await _unitOfWork.CategoriaRepository.GetByNombreAsync(
                categoria.EmpresaId,
                categoria.Nombre);

            if (existente != null)
            {
                _logger.LogWarning("Intento de crear categoría duplicada con nombre {Nombre} en empresa {EmpresaId}",
                    categoria.Nombre, categoria.EmpresaId);
                return BadRequest("Ya existe una categoría con este nombre en la empresa.");
            }

            // Establecer usuario de creación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            categoria.UsuarioCreacionId = userId;

            var nuevaCategoria = await _unitOfWork.CategoriaRepository.AddAsync(categoria);

            _logger.LogInformation("Categoría {CategoriaId} creada exitosamente para empresa {EmpresaId} por usuario {UserId}",
                nuevaCategoria.Id, categoria.EmpresaId, userId);

            return Ok(nuevaCategoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear categoría para empresa {EmpresaId}", categoria.EmpresaId);
            var detalle = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, $"Error al crear categoría: {detalle}");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Categoria categoria)
    {
        try
        {
            if (id != categoria.Id)
            {
                _logger.LogWarning("Intento de actualizar categoría con ID inconsistente. URL: {UrlId}, Body: {BodyId}", id, categoria.Id);
                return BadRequest("El ID de la URL no coincide con el ID de la categoría.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de actualizar categoría {CategoriaId} con datos inválidos", id);
                return BadRequest(ModelState);
            }

            // Verificar que la categoría existe
            var categoriaExistente = await _unitOfWork.CategoriaRepository.GetAsync(id);
            if (categoriaExistente == null)
            {
                _logger.LogWarning("Intento de actualizar categoría inexistente {CategoriaId}", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(categoriaExistente.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó actualizar categoría {CategoriaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // Verificar que no exista otra categoría con el mismo nombre en esta empresa
            var duplicada = await _unitOfWork.CategoriaRepository.GetByNombreAsync(
                categoria.EmpresaId,
                categoria.Nombre);

            if (duplicada != null && duplicada.Id != id)
            {
                _logger.LogWarning("Intento de actualizar categoría {CategoriaId} con nombre duplicado {Nombre}",
                    id, categoria.Nombre);
                return BadRequest("Ya existe otra categoría con este nombre en la empresa.");
            }

            // Preservar campos de auditoría
            categoria.FechaCreacion = categoriaExistente.FechaCreacion;
            categoria.UsuarioCreacionId = categoriaExistente.UsuarioCreacionId;

            // Establecer usuario de modificación
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            categoria.UsuarioModificacionId = userId;

            await _unitOfWork.CategoriaRepository.UpdateAsync(categoria);

            _logger.LogInformation("Categoría {CategoriaId} actualizada exitosamente por usuario {UserId}", id, userId);

            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar categoría {CategoriaId}", id);
            var detalle = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, $"Error al actualizar categoría: {detalle}");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var categoria = await _unitOfWork.CategoriaRepository.GetAsync(id);

            if (categoria == null)
            {
                _logger.LogWarning("Intento de eliminar categoría inexistente {CategoriaId}", id);
                return NotFound();
            }

            // Verificar que el usuario tiene acceso a esta empresa
            if (!await TieneAccesoEmpresaAsync(categoria.EmpresaId))
            {
                _logger.LogWarning("Usuario {UserId} intentó eliminar categoría {CategoriaId} sin autorización",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), id);
                return Forbid();
            }

            // Verificar que la categoría no tenga productos asignados
            var productosEnCategoria = await _unitOfWork.ProductoRepository.GetByCategoriaAsync(id);
            if (productosEnCategoria.Any())
            {
                _logger.LogWarning("Intento de eliminar categoría {CategoriaId} con {Count} productos asignados",
                    id, productosEnCategoria.Count());
                return BadRequest("No se puede eliminar la categoría porque tiene productos asignados.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _unitOfWork.CategoriaRepository.DeleteAsync(id, userId!);

            _logger.LogInformation("Categoría {CategoriaId} eliminada exitosamente por usuario {UserId}", id, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar categoría {CategoriaId}", id);
            return StatusCode(500, "Error interno al eliminar categoría.");
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
