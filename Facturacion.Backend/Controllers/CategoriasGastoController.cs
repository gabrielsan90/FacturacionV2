using Facturacion.Backend.Services.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class CategoriasGastoController : ControllerBase
{
    private readonly IGastoUnitOfWork _unitOfWork;
    private readonly ILogger<CategoriasGastoController> _logger;
    private readonly IExcelImportService _excelImportService;

    public CategoriasGastoController(
        IGastoUnitOfWork unitOfWork,
        ILogger<CategoriasGastoController> logger,
        IExcelImportService excelImportService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _excelImportService = excelImportService;
    }

    /// <summary>
    /// Obtiene todas las categorías de gasto
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] bool includeInactive = false)
    {
        try
        {
            var categorias = await _unitOfWork.CategoriaGastoRepository.GetAllAsync(includeInactive);
            _logger.LogInformation("Se obtuvieron {Count} categorías de gasto (incluir inactivas: {IncludeInactive})",
                categorias.Count(), includeInactive);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías de gasto");
            return StatusCode(500, "Error interno al obtener categorías de gasto.");
        }
    }

    /// <summary>
    /// Obtiene solo las categorías activas
    /// </summary>
    [HttpGet("activas")]
    public async Task<IActionResult> GetActivasAsync()
    {
        try
        {
            var categorias = await _unitOfWork.CategoriaGastoRepository.GetActivasAsync();
            _logger.LogInformation("Se obtuvieron {Count} categorías de gasto activas", categorias.Count());
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías de gasto activas");
            return StatusCode(500, "Error interno al obtener categorías de gasto activas.");
        }
    }

    /// <summary>
    /// Obtiene una categoría por ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        try
        {
            var categoria = await _unitOfWork.CategoriaGastoRepository.GetAsync(id);

            if (categoria == null)
            {
                _logger.LogWarning("Categoría de gasto {CategoriaGastoId} no encontrada", id);
                return NotFound("La categoría de gasto no existe.");
            }

            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categoría de gasto {CategoriaGastoId}", id);
            return StatusCode(500, "Error interno al obtener categoría de gasto.");
        }
    }

    /// <summary>
    /// Crea una nueva categoría de gasto
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CategoriaGasto categoria)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de crear categoría de gasto con datos inválidos");
                return BadRequest(ModelState);
            }

            // Verificar que no exista otra categoría con el mismo nombre
            var existente = await _unitOfWork.CategoriaGastoRepository.GetByNombreAsync(categoria.Nombre);
            if (existente != null)
            {
                _logger.LogWarning("Intento de crear categoría de gasto duplicada con nombre {Nombre}", categoria.Nombre);
                return BadRequest("Ya existe una categoría con este nombre.");
            }

            var nuevaCategoria = await _unitOfWork.CategoriaGastoRepository.AddAsync(categoria);

            _logger.LogInformation("Categoría de gasto {CategoriaGastoId} creada exitosamente con nombre {Nombre}",
                nuevaCategoria.Id, categoria.Nombre);

            return Ok(nuevaCategoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear categoría de gasto");
            return StatusCode(500, "Error interno al crear la categoría.");
        }
    }

    /// <summary>
    /// Actualiza una categoría de gasto
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutAsync(int id, [FromBody] CategoriaGasto categoria)
    {
        try
        {
            if (id != categoria.Id)
            {
                _logger.LogWarning("Intento de actualizar categoría de gasto con ID inconsistente. URL: {UrlId}, Body: {BodyId}",
                    id, categoria.Id);
                return BadRequest("El ID de la URL no coincide con el ID de la categoría.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de actualizar categoría de gasto {CategoriaGastoId} con datos inválidos", id);
                return BadRequest(ModelState);
            }

            var existente = await _unitOfWork.CategoriaGastoRepository.GetAsync(id);
            if (existente == null)
            {
                _logger.LogWarning("Intento de actualizar categoría de gasto inexistente {CategoriaGastoId}", id);
                return NotFound("La categoría de gasto no existe.");
            }

            // Verificar que no exista otra categoría con el mismo nombre
            var duplicado = await _unitOfWork.CategoriaGastoRepository.GetByNombreAsync(categoria.Nombre);
            if (duplicado != null && duplicado.Id != id)
            {
                _logger.LogWarning("Intento de actualizar categoría de gasto {CategoriaGastoId} con nombre duplicado {Nombre}",
                    id, categoria.Nombre);
                return BadRequest("Ya existe otra categoría con este nombre.");
            }

            await _unitOfWork.CategoriaGastoRepository.UpdateAsync(categoria);

            _logger.LogInformation("Categoría de gasto {CategoriaGastoId} actualizada exitosamente", id);

            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar categoría de gasto {CategoriaGastoId}", id);
            return StatusCode(500, "Error interno al actualizar la categoría.");
        }
    }

    /// <summary>
    /// Desactiva una categoría de gasto
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            var categoria = await _unitOfWork.CategoriaGastoRepository.GetAsync(id);
            if (categoria == null)
            {
                _logger.LogWarning("Intento de eliminar categoría de gasto inexistente {CategoriaGastoId}", id);
                return NotFound("La categoría de gasto no existe.");
            }

            // Verificar si tiene gastos asociados
            if (categoria.Gastos != null && categoria.Gastos.Any(g => !g.IsDeleted))
            {
                _logger.LogWarning("Intento de eliminar categoría de gasto {CategoriaGastoId} con {Count} gastos asociados",
                    id, categoria.Gastos.Count(g => !g.IsDeleted));
                return BadRequest("No se puede desactivar una categoría que tiene gastos asociados.");
            }

            await _unitOfWork.CategoriaGastoRepository.DeleteAsync(id);

            _logger.LogInformation("Categoría de gasto {CategoriaGastoId} eliminada exitosamente", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar categoría de gasto {CategoriaGastoId}", id);
            return StatusCode(500, "Error interno al desactivar la categoría.");
        }
    }

    /// <summary>
    /// Importa categorías de gasto desde un archivo Excel
    /// </summary>
    [HttpPost("importar")]
    public async Task<IActionResult> ImportarAsync(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe proporcionar un archivo Excel.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        using var stream = archivo.OpenReadStream();
        var result = await _excelImportService.ImportarCategoriasGastoAsync(stream, userId!);
        return Ok(result);
    }

    /// <summary>
    /// Descarga una plantilla de Excel para importar categorías de gasto
    /// </summary>
    [HttpGet("plantilla")]
    public IActionResult DescargarPlantilla()
    {
        var fileBytes = _excelImportService.GenerarPlantillaCategoriasGasto();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_CategoriasGasto.xlsx");
    }
}
