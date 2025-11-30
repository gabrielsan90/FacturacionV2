using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class CategoriasGastoController : ControllerBase
{
    private readonly IGastoUnitOfWork _unitOfWork;

    public CategoriasGastoController(IGastoUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Obtiene todas las categorías de gasto
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] bool includeInactive = false)
    {
        var categorias = await _unitOfWork.CategoriaGastoRepository.GetAllAsync(includeInactive);
        return Ok(categorias);
    }

    /// <summary>
    /// Obtiene solo las categorías activas
    /// </summary>
    [HttpGet("activas")]
    public async Task<IActionResult> GetActivasAsync()
    {
        var categorias = await _unitOfWork.CategoriaGastoRepository.GetActivasAsync();
        return Ok(categorias);
    }

    /// <summary>
    /// Obtiene una categoría por ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var categoria = await _unitOfWork.CategoriaGastoRepository.GetAsync(id);

        if (categoria == null)
        {
            return NotFound("La categoría de gasto no existe.");
        }

        return Ok(categoria);
    }

    /// <summary>
    /// Crea una nueva categoría de gasto
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CategoriaGasto categoria)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que no exista otra categoría con el mismo nombre
        var existente = await _unitOfWork.CategoriaGastoRepository.GetByNombreAsync(categoria.Nombre);
        if (existente != null)
        {
            return BadRequest("Ya existe una categoría con este nombre.");
        }

        try
        {
            var nuevaCategoria = await _unitOfWork.CategoriaGastoRepository.AddAsync(categoria);
            return CreatedAtAction(nameof(GetAsync), new { id = nuevaCategoria.Id }, nuevaCategoria);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear la categoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza una categoría de gasto
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutAsync(int id, [FromBody] CategoriaGasto categoria)
    {
        if (id != categoria.Id)
        {
            return BadRequest("El ID de la URL no coincide con el ID de la categoría.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existente = await _unitOfWork.CategoriaGastoRepository.GetAsync(id);
        if (existente == null)
        {
            return NotFound("La categoría de gasto no existe.");
        }

        // Verificar que no exista otra categoría con el mismo nombre
        var duplicado = await _unitOfWork.CategoriaGastoRepository.GetByNombreAsync(categoria.Nombre);
        if (duplicado != null && duplicado.Id != id)
        {
            return BadRequest("Ya existe otra categoría con este nombre.");
        }

        try
        {
            await _unitOfWork.CategoriaGastoRepository.UpdateAsync(categoria);
            return Ok(categoria);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar la categoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Desactiva una categoría de gasto
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var categoria = await _unitOfWork.CategoriaGastoRepository.GetAsync(id);
        if (categoria == null)
        {
            return NotFound("La categoría de gasto no existe.");
        }

        // Verificar si tiene gastos asociados
        if (categoria.Gastos != null && categoria.Gastos.Any(g => !g.IsDeleted))
        {
            return BadRequest("No se puede desactivar una categoría que tiene gastos asociados.");
        }

        try
        {
            await _unitOfWork.CategoriaGastoRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al desactivar la categoría: {ex.Message}");
        }
    }
}
