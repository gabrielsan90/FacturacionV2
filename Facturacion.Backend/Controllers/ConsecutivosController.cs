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

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var consecutivos = await _consecutivoRepository.GetByEmpresaAsync(empresaId);
        return Ok(consecutivos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
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

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Consecutivo consecutivo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

        return Ok(nuevoConsecutivo);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Consecutivo consecutivo)
    {
        if (id != consecutivo.Id)
        {
            return BadRequest("El ID de la URL no coincide con el ID del consecutivo.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

        return Ok(consecutivo);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
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

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _consecutivoRepository.DeleteAsync(id, userId!);

        return NoContent();
    }

    [HttpPost("{id:guid}/increment")]
    public async Task<IActionResult> IncrementAsync(Guid id)
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

            var nuevoNumero = await _consecutivoRepository.IncrementAsync(id);

            return Ok(new { numeroConsecutivo = nuevoNumero });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
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
