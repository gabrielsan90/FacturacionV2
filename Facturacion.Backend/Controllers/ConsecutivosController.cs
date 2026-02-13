using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
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
public class ConsecutivosController : ControllerBase
{
    private readonly IConsecutivoRepository _consecutivoRepository;
    private readonly DataContext _context;
    private readonly ILogger<ConsecutivosController> _logger;

    public ConsecutivosController(
        IConsecutivoRepository consecutivoRepository,
        DataContext context,
        ILogger<ConsecutivosController> logger)
    {
        _consecutivoRepository = consecutivoRepository;
        _context = context;
        _logger = logger;
    }

    [HttpGet("terminal/{terminalId:guid}")]
    public async Task<IActionResult> GetByTerminalAsync(Guid terminalId)
    {
        try
        {
            var terminal = await _context.Terminales
                .Include(t => t.Sucursal)
                .FirstOrDefaultAsync(t => t.Id == terminalId && !t.IsDeleted);

            if (terminal == null)
            {
                return NotFound("Terminal no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(terminal.Sucursal!.EmpresaId))
            {
                return Forbid();
            }

            var consecutivos = await _consecutivoRepository.GetByTerminalAsync(terminalId);
            return Ok(consecutivos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener consecutivos del terminal {TerminalId}", terminalId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var consecutivos = await _consecutivoRepository.GetByEmpresaAsync(empresaId);
            return Ok(consecutivos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener consecutivos de la empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var consecutivo = await _consecutivoRepository.GetAsync(id);

            if (consecutivo == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(consecutivo.Terminal!.Sucursal!.EmpresaId))
            {
                return Forbid();
            }

            return Ok(consecutivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener consecutivo {ConsecutivoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Consecutivo consecutivo)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creando consecutivo para terminal {TerminalId}, tipo {TipoDocumento}",
                consecutivo.TerminalId, consecutivo.TipoDocumento);

            // Verificar acceso al terminal
            var terminal = await _context.Terminales
                .Include(t => t.Sucursal)
                .FirstOrDefaultAsync(t => t.Id == consecutivo.TerminalId && !t.IsDeleted);

            if (terminal == null)
            {
                return NotFound("Terminal no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(terminal.Sucursal!.EmpresaId))
            {
                return Forbid();
            }

            // Verificar que no exista otro consecutivo activo para el mismo tipo de documento y ambiente en el terminal
            var existente = await _consecutivoRepository.GetByTipoDocumentoAsync(
                consecutivo.TerminalId, consecutivo.TipoDocumento, consecutivo.Ambiente);
            if (existente != null && consecutivo.Activo)
            {
                var ambienteNombre = consecutivo.Ambiente == Shared.Enums.Ambiente.Pruebas ? "Pruebas" : "Producción";
                return BadRequest($"Ya existe un consecutivo activo para el tipo de documento {consecutivo.TipoDocumento} en ambiente {ambienteNombre} en este terminal.");
            }

            // Asignar EmpresaId y SucursalId desde el terminal
            consecutivo.EmpresaId = terminal.Sucursal!.EmpresaId;
            consecutivo.SucursalId = terminal.SucursalId;

            // Validar numero actual no sea negativo
            if (consecutivo.NumeroActual < 0)
            {
                consecutivo.NumeroActual = 0;
            }

            // Auto-set defaults for simplified interface
            if (consecutivo.NumeroInicio <= 0)
            {
                consecutivo.NumeroInicio = 1;
            }
            if (consecutivo.NumeroFin <= consecutivo.NumeroInicio)
            {
                consecutivo.NumeroFin = 9999999999;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            consecutivo.UsuarioCreacionId = userId;

            var nuevoConsecutivo = await _consecutivoRepository.AddAsync(consecutivo);

            _logger.LogInformation("Consecutivo creado exitosamente: {ConsecutivoId}", nuevoConsecutivo.Id);

            return Ok(nuevoConsecutivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear consecutivo para terminal {TerminalId}", consecutivo.TerminalId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Consecutivo consecutivo)
    {
        try
        {
            if (id != consecutivo.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del consecutivo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Actualizando consecutivo {ConsecutivoId}", id);

            var consecutivoExistente = await _consecutivoRepository.GetAsync(id);
            if (consecutivoExistente == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(consecutivoExistente.Terminal!.Sucursal!.EmpresaId))
            {
                return Forbid();
            }

            // Validar numero actual no sea negativo
            if (consecutivo.NumeroActual < 0)
            {
                consecutivo.NumeroActual = 0;
            }

            // Auto-set defaults for simplified interface
            if (consecutivo.NumeroInicio <= 0)
            {
                consecutivo.NumeroInicio = 1;
            }
            if (consecutivo.NumeroFin <= consecutivo.NumeroInicio)
            {
                consecutivo.NumeroFin = 9999999999;
            }

            // Preservar campos de auditoría y claves foráneas de jerarquía
            consecutivo.EmpresaId = consecutivoExistente.EmpresaId;
            consecutivo.SucursalId = consecutivoExistente.SucursalId;
            consecutivo.FechaCreacion = consecutivoExistente.FechaCreacion;
            consecutivo.UsuarioCreacionId = consecutivoExistente.UsuarioCreacionId;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            consecutivo.UsuarioModificacionId = userId;

            await _consecutivoRepository.UpdateAsync(consecutivo);

            _logger.LogInformation("Consecutivo actualizado exitosamente: {ConsecutivoId}", id);

            return Ok(consecutivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar consecutivo {ConsecutivoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Eliminando consecutivo {ConsecutivoId}", id);

            var consecutivo = await _consecutivoRepository.GetAsync(id);

            if (consecutivo == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(consecutivo.Terminal!.Sucursal!.EmpresaId))
            {
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _consecutivoRepository.DeleteAsync(id, userId!);

            _logger.LogInformation("Consecutivo eliminado exitosamente: {ConsecutivoId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar consecutivo {ConsecutivoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost("{id:guid}/increment")]
    public async Task<IActionResult> IncrementAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Incrementando consecutivo {ConsecutivoId}", id);

            var consecutivo = await _consecutivoRepository.GetAsync(id);

            if (consecutivo == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(consecutivo.Terminal!.Sucursal!.EmpresaId))
            {
                return Forbid();
            }

            var nuevoNumero = await _consecutivoRepository.IncrementAsync(id);

            _logger.LogInformation("Consecutivo incrementado exitosamente: {ConsecutivoId}, nuevo número: {NuevoNumero}", id, nuevoNumero);

            return Ok(new { numeroConsecutivo = nuevoNumero });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de operación al incrementar consecutivo {ConsecutivoId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al incrementar consecutivo {ConsecutivoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (userRoles.Contains("SuperUser"))
        {
            return true;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
