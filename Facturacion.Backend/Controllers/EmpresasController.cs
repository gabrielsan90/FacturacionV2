using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperUser")]
public class EmpresasController : ControllerBase
{
    private readonly IEmpresaUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<EmpresasController> _logger;

    public EmpresasController(
        IEmpresaUnitOfWork unitOfWork,
        DataContext context,
        ILogger<EmpresasController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
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
            empresa.FechaCreacion = FechaCostaRicaHelper.Ahora;
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

            // IMPORTANTE: Preservar certificado digital si no viene uno nuevo
            // Esto evita sobrescribir el certificado existente cuando el usuario no selecciona archivo
            if (empresa.CertificadoDigital == null || empresa.CertificadoDigital.Length == 0)
            {
                empresa.CertificadoDigital = existing.CertificadoDigital;
            }

            // Preservar logo si no viene uno nuevo
            if (string.IsNullOrEmpty(empresa.Logo))
            {
                empresa.Logo = existing.Logo;
            }

            // Asignar usuario de modificación
            empresa.UsuarioModificacionId = userId;
            empresa.FechaModificacion = FechaCostaRicaHelper.Ahora;

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
            empresa.FechaEliminacion = FechaCostaRicaHelper.Ahora;
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

    // ========================================
    // ACTIVIDADES ECONÓMICAS
    // ========================================

    /// <summary>
    /// Obtiene las actividades económicas de una empresa
    /// </summary>
    [HttpGet("{empresaId:guid}/actividades-economicas")]
    public async Task<IActionResult> GetActividadesEconomicasAsync(Guid empresaId)
    {
        try
        {
            var actividades = await _context.Set<EmpresaActividadEconomica>()
                .Include(ea => ea.ActividadEconomica)
                .Where(ea => ea.EmpresaId == empresaId)
                .Select(ea => new
                {
                    ea.Id,
                    ea.EmpresaId,
                    ea.ActividadEconomicaId,
                    ea.EsPrincipal,
                    Codigo = ea.ActividadEconomica != null ? ea.ActividadEconomica.CodigoCIIU4 : "",
                    Descripcion = ea.ActividadEconomica != null ? ea.ActividadEconomica.Descripcion : ""
                })
                .ToListAsync();

            return Ok(actividades);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las actividades económicas de la empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno del servidor.");
        }
    }

    /// <summary>
    /// Agrega una actividad económica a una empresa
    /// </summary>
    [HttpPost("{empresaId:guid}/actividades-economicas")]
    public async Task<IActionResult> AddActividadEconomicaAsync(Guid empresaId, [FromBody] AddActividadEconomicaRequest request)
    {
        try
        {
            // Verificar que la empresa existe
            var empresaExists = await _context.Empresas.AnyAsync(e => e.Id == empresaId && !e.IsDeleted);
            if (!empresaExists)
            {
                return NotFound("Empresa no encontrada.");
            }

            // Buscar la actividad económica por código
            var actividad = await _context.Set<ActividadEconomica>()
                .FirstOrDefaultAsync(a => a.CodigoCIIU4 == request.CodigoActividad);

            if (actividad == null)
            {
                return NotFound($"Actividad económica no encontrada: {request.CodigoActividad}");
            }

            // Verificar si ya existe la relación
            var existeRelacion = await _context.Set<EmpresaActividadEconomica>()
                .AnyAsync(ea => ea.EmpresaId == empresaId && ea.ActividadEconomicaId == actividad.Id);

            if (existeRelacion)
            {
                return BadRequest("La actividad económica ya está asignada a esta empresa.");
            }

            // Si es principal, quitar el flag de las otras
            if (request.EsPrincipal)
            {
                var otrasActividades = await _context.Set<EmpresaActividadEconomica>()
                    .Where(ea => ea.EmpresaId == empresaId && ea.EsPrincipal)
                    .ToListAsync();

                foreach (var otra in otrasActividades)
                {
                    otra.EsPrincipal = false;
                }
            }

            var nuevaRelacion = new EmpresaActividadEconomica
            {
                EmpresaId = empresaId,
                ActividadEconomicaId = actividad.Id,
                EsPrincipal = request.EsPrincipal
            };

            _context.Add(nuevaRelacion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Actividad económica {CodigoActividad} agregada a empresa {EmpresaId}",
                request.CodigoActividad, empresaId);

            return Ok(new
            {
                nuevaRelacion.Id,
                nuevaRelacion.EmpresaId,
                nuevaRelacion.ActividadEconomicaId,
                nuevaRelacion.EsPrincipal,
                Codigo = actividad.CodigoCIIU4,
                Descripcion = actividad.Descripcion
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar actividad económica a empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno del servidor.");
        }
    }

    /// <summary>
    /// Elimina una actividad económica de una empresa
    /// </summary>
    [HttpDelete("{empresaId:guid}/actividades-economicas/{actividadId:int}")]
    public async Task<IActionResult> DeleteActividadEconomicaAsync(Guid empresaId, int actividadId)
    {
        try
        {
            var relacion = await _context.Set<EmpresaActividadEconomica>()
                .FirstOrDefaultAsync(ea => ea.EmpresaId == empresaId && ea.Id == actividadId);

            if (relacion == null)
            {
                return NotFound("Relación no encontrada.");
            }

            _context.Remove(relacion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Actividad económica {ActividadId} eliminada de empresa {EmpresaId}",
                actividadId, empresaId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar actividad económica de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno del servidor.");
        }
    }

    /// <summary>
    /// Establece una actividad económica como principal
    /// </summary>
    [HttpPut("{empresaId:guid}/actividades-economicas/{actividadId:int}/principal")]
    public async Task<IActionResult> SetActividadPrincipalAsync(Guid empresaId, int actividadId)
    {
        try
        {
            // Quitar principal de todas las actividades de esta empresa
            var actividades = await _context.Set<EmpresaActividadEconomica>()
                .Where(ea => ea.EmpresaId == empresaId)
                .ToListAsync();

            var actividadSeleccionada = actividades.FirstOrDefault(a => a.Id == actividadId);
            if (actividadSeleccionada == null)
            {
                return NotFound("Actividad no encontrada.");
            }

            foreach (var actividad in actividades)
            {
                actividad.EsPrincipal = actividad.Id == actividadId;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Actividad económica {ActividadId} establecida como principal para empresa {EmpresaId}",
                actividadId, empresaId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al establecer actividad principal para empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno del servidor.");
        }
    }
}

/// <summary>
/// DTO para agregar actividad económica
/// </summary>
public class AddActividadEconomicaRequest
{
    public string CodigoActividad { get; set; } = null!;
    public bool EsPrincipal { get; set; }
}
