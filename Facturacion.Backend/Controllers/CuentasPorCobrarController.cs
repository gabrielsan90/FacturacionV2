using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
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
public class CuentasPorCobrarController : ControllerBase
{
    private readonly ICuentaPorCobrarUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly IContabilidadIntegracionService _contabilidadIntegracion;
    private readonly ILogger<CuentasPorCobrarController> _logger;

    public CuentasPorCobrarController(
        ICuentaPorCobrarUnitOfWork unitOfWork,
        DataContext context,
        IContabilidadIntegracionService contabilidadIntegracion,
        ILogger<CuentasPorCobrarController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _contabilidadIntegracion = contabilidadIntegracion;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las cuentas por cobrar de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Acceso denegado a empresa {EmpresaId}", empresaId);
                return Forbid();
            }

            var action = await _unitOfWork.CuentaPorCobrarRepository.GetByEmpresaAsync(empresaId);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Error al obtener cuentas por cobrar de empresa {EmpresaId}: {Message}", empresaId, action.Message);
            }

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener cuentas por cobrar de empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene solo las cuentas pendientes o parciales de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/pendientes")]
    public async Task<IActionResult> GetPendientesAsync(Guid empresaId)
    {
        // Verificar acceso a la empresa
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _unitOfWork.CuentaPorCobrarRepository.GetPendientesAsync(empresaId);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Obtiene solo las cuentas vencidas de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/vencidas")]
    public async Task<IActionResult> GetVencidasAsync(Guid empresaId)
    {
        // Verificar acceso a la empresa
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _unitOfWork.CuentaPorCobrarRepository.GetVencidasAsync(empresaId);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Obtiene una cuenta por cobrar específica con sus abonos
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var action = await _unitOfWork.CuentaPorCobrarRepository.GetAsync(id);

        if (!action.WasSuccess)
        {
            return NotFound(action.Message);
        }

        // Verificar acceso a la empresa
        if (!await TieneAccesoEmpresaAsync(action.Result!.EmpresaId))
        {
            return Forbid();
        }

        return Ok(action.Result);
    }

    /// <summary>
    /// Crea una nueva cuenta por cobrar (normalmente auto-creada desde factura)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CuentaPorCobrar cuentaPorCobrar)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar acceso a la empresa
        if (!await TieneAccesoEmpresaAsync(cuentaPorCobrar.EmpresaId))
        {
            return Forbid();
        }

        // Establecer usuario de creación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        cuentaPorCobrar.UsuarioCreacionId = userId;

        var action = await _unitOfWork.CuentaPorCobrarRepository.AddAsync(cuentaPorCobrar);

        if (!action.WasSuccess)
        {
            return BadRequest(action.Message);
        }

        return Ok(action.Result);
    }

    /// <summary>
    /// Aplica un abono (pago parcial o total) a una cuenta por cobrar
    /// </summary>
    [HttpPost("{id:guid}/abonar")]
    public async Task<IActionResult> AbonarAsync(Guid id, [FromBody] AbonoCobranza abono)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Obtener la cuenta para verificar acceso
        var cuentaAction = await _unitOfWork.CuentaPorCobrarRepository.GetAsync(id);
        if (!cuentaAction.WasSuccess)
        {
            return NotFound(cuentaAction.Message);
        }

        // Verificar acceso a la empresa
        if (!await TieneAccesoEmpresaAsync(cuentaAction.Result!.EmpresaId))
        {
            return Forbid();
        }

        // Establecer usuario de creación del abono
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        abono.UsuarioCreacionId = userId;

        var action = await _unitOfWork.CuentaPorCobrarRepository.AplicarAbonoAsync(id, abono);

        if (!action.WasSuccess)
        {
            return BadRequest(action.Message);
        }

        // Generar asiento contable para el cobro (si está habilitado)
        try
        {
            await _contabilidadIntegracion.GenerarAsientoCobroAsync(cuentaAction.Result, abono, userId!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generando asiento contable para cobro {AbonoId} - operación continúa", abono.Id);
        }

        return Ok(action);
    }

    /// <summary>
    /// Obtiene el estado de cuenta de un cliente (todas sus cuentas por cobrar)
    /// </summary>
    [HttpGet("cliente/{clienteId:guid}/estado")]
    public async Task<IActionResult> GetEstadoCuentaClienteAsync(Guid clienteId)
    {
        // Obtener el cliente para verificar acceso a la empresa
        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clienteId && !c.IsDeleted);

        if (cliente == null)
        {
            return NotFound("Cliente no encontrado");
        }

        // Verificar acceso a la empresa del cliente
        if (!await TieneAccesoEmpresaAsync(cliente.EmpresaId))
        {
            return Forbid();
        }

        var action = await _unitOfWork.CuentaPorCobrarRepository.GetByClienteAsync(clienteId);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Obtiene el reporte de antigüedad de saldos de una empresa
    /// Agrupa las cuentas por cliente y rangos de días vencidos
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/antiguedad")]
    public async Task<IActionResult> GetAntiguedadSaldosAsync(Guid empresaId)
    {
        // Verificar acceso a la empresa
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _unitOfWork.CuentaPorCobrarRepository.GetAntiguedadSaldosAsync(empresaId);
        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Actualiza una cuenta por cobrar existente
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] CuentaPorCobrar cuentaPorCobrar)
    {
        if (id != cuentaPorCobrar.Id)
        {
            return BadRequest("El ID de la URL no coincide con el ID de la cuenta por cobrar");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que la cuenta existe
        var existente = await _unitOfWork.CuentaPorCobrarRepository.GetAsync(id);
        if (!existente.WasSuccess)
        {
            return NotFound(existente.Message);
        }

        // Verificar acceso a la empresa
        if (!await TieneAccesoEmpresaAsync(existente.Result!.EmpresaId))
        {
            return Forbid();
        }

        // Establecer usuario de modificación
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        cuentaPorCobrar.UsuarioModificacionId = userId;

        var action = await _unitOfWork.CuentaPorCobrarRepository.UpdateAsync(cuentaPorCobrar);

        if (!action.WasSuccess)
        {
            return BadRequest(action.Message);
        }

        return Ok(action.Result);
    }

    /// <summary>
    /// Elimina una cuenta por cobrar (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var cuentaAction = await _unitOfWork.CuentaPorCobrarRepository.GetAsync(id);

        if (!cuentaAction.WasSuccess)
        {
            return NotFound(cuentaAction.Message);
        }

        // Verificar acceso a la empresa
        if (!await TieneAccesoEmpresaAsync(cuentaAction.Result!.EmpresaId))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var action = await _unitOfWork.CuentaPorCobrarRepository.DeleteAsync(id, userId!);

        if (!action.WasSuccess)
        {
            return BadRequest(action.Message);
        }

        return NoContent();
    }

    /// <summary>
    /// Verifica si el usuario tiene acceso a una empresa específica
    /// SuperUser tiene acceso a todas las empresas
    /// Otros usuarios solo tienen acceso a sus empresas asignadas
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
