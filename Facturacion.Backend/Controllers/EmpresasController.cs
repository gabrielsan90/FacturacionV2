using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperUser")]
public class EmpresasController : ControllerBase
{
    private readonly IEmpresaUnitOfWork _unitOfWork;
    private readonly ILogger<EmpresasController> _logger;

    public EmpresasController(
        IEmpresaUnitOfWork unitOfWork,
        ILogger<EmpresasController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las empresas (no eliminadas)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        try
        {
            var response = await _unitOfWork.EmpresaRepository.GetAsync();
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }

            return Ok(response.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las empresas");
            return StatusCode(500, "Error interno del servidor al obtener las empresas.");
        }
    }

    /// <summary>
    /// Obtiene todas las empresas activas (no eliminadas y activas)
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAsync()
    {
        try
        {
            var response = await _unitOfWork.EmpresaRepository.GetAllActiveAsync();
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }

            return Ok(response.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las empresas activas");
            return StatusCode(500, "Error interno del servidor al obtener las empresas activas.");
        }
    }

    /// <summary>
    /// Obtiene una empresa por su ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var response = await _unitOfWork.EmpresaRepository.GetAsync(id);
            if (!response.WasSuccess)
            {
                return NotFound(response.Message);
            }

            return Ok(response.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la empresa {EmpresaId}", id);
            return StatusCode(500, "Error interno del servidor al obtener la empresa.");
        }
    }

    /// <summary>
    /// Obtiene una empresa por su número de identificación
    /// </summary>
    [HttpGet("by-identification/{numeroIdentificacion}")]
    public async Task<IActionResult> GetByIdentificationAsync(string numeroIdentificacion)
    {
        try
        {
            var response = await _unitOfWork.EmpresaRepository.GetByIdentificationAsync(numeroIdentificacion);
            if (!response.WasSuccess)
            {
                return NotFound(response.Message);
            }

            return Ok(response.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la empresa con identificación {NumeroIdentificacion}", numeroIdentificacion);
            return StatusCode(500, "Error interno del servidor al obtener la empresa.");
        }
    }

    /// <summary>
    /// Crea una nueva empresa
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Empresa empresa)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obtener el ID del usuario autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            // Asignar usuario de creación
            empresa.UsuarioCreacionId = userId;
            empresa.FechaCreacion = DateTime.UtcNow;
            empresa.Activa = true;
            empresa.IsDeleted = false;

            var response = await _unitOfWork.EmpresaRepository.AddAsync(empresa);
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }

            _logger.LogInformation("Empresa creada: {EmpresaId} - {NombreComercial} por usuario {UserId}",
                empresa.Id, empresa.NombreComercial, userId);

            return CreatedAtAction(nameof(GetAsync), new { id = empresa.Id }, response.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la empresa");
            return StatusCode(500, "Error interno del servidor al crear la empresa.");
        }
    }

    /// <summary>
    /// Actualiza una empresa existente
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Empresa empresa)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != empresa.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID de la empresa.");
            }

            // Verificar que la empresa existe
            var existingResponse = await _unitOfWork.EmpresaRepository.GetAsync(id);
            if (!existingResponse.WasSuccess)
            {
                return NotFound(existingResponse.Message);
            }

            // Obtener el ID del usuario autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            // Preservar datos de auditoría de creación
            var existing = existingResponse.Result!;
            empresa.UsuarioCreacionId = existing.UsuarioCreacionId;
            empresa.FechaCreacion = existing.FechaCreacion;

            // Asignar usuario de modificación
            empresa.UsuarioModificacionId = userId;
            empresa.FechaModificacion = DateTime.UtcNow;

            var response = await _unitOfWork.EmpresaRepository.UpdateAsync(empresa);
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }

            _logger.LogInformation("Empresa actualizada: {EmpresaId} - {NombreComercial} por usuario {UserId}",
                empresa.Id, empresa.NombreComercial, userId);

            return Ok(response.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la empresa {EmpresaId}", id);
            return StatusCode(500, "Error interno del servidor al actualizar la empresa.");
        }
    }

    /// <summary>
    /// Elimina una empresa (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            // Verificar que la empresa existe
            var existingResponse = await _unitOfWork.EmpresaRepository.GetAsync(id);
            if (!existingResponse.WasSuccess)
            {
                return NotFound(existingResponse.Message);
            }

            // Obtener el ID del usuario autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            var empresa = existingResponse.Result!;
            empresa.UsuarioEliminacionId = userId;
            empresa.FechaEliminacion = DateTime.UtcNow;
            empresa.IsDeleted = true;

            var response = await _unitOfWork.EmpresaRepository.DeleteAsync(id);
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }

            _logger.LogInformation("Empresa eliminada: {EmpresaId} - {NombreComercial} por usuario {UserId}",
                id, empresa.NombreComercial, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la empresa {EmpresaId}", id);
            return StatusCode(500, "Error interno del servidor al eliminar la empresa.");
        }
    }
}
