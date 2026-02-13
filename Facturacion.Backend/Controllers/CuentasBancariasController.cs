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
public class CuentasBancariasController : ControllerBase
{
    private readonly ICuentaBancariaRepository _cuentaBancariaRepository;
    private readonly DataContext _context;
    private readonly ILogger<CuentasBancariasController> _logger;

    public CuentasBancariasController(
        ICuentaBancariaRepository cuentaBancariaRepository,
        DataContext context,
        ILogger<CuentasBancariasController> logger)
    {
        _cuentaBancariaRepository = cuentaBancariaRepository;
        _context = context;
        _logger = logger;
    }

    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var cuentasResponse = await _cuentaBancariaRepository.GetByEmpresaAsync(empresaId);

        if (!cuentasResponse.WasSuccess)
        {
            return BadRequest(cuentasResponse.Message);
        }

        return Ok(cuentasResponse.Result);
    }

    [HttpGet("empresa/{empresaId:guid}/activas")]
    public async Task<IActionResult> GetActivasByEmpresaAsync(Guid empresaId)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var cuentasResponse = await _cuentaBancariaRepository.GetActivasAsync(empresaId);

        if (!cuentasResponse.WasSuccess)
        {
            return BadRequest(cuentasResponse.Message);
        }

        return Ok(cuentasResponse.Result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var cuentaResponse = await _cuentaBancariaRepository.GetAsync(id);

        if (!cuentaResponse.WasSuccess || cuentaResponse.Result == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(cuentaResponse.Result.EmpresaId))
        {
            return Forbid();
        }

        return Ok(cuentaResponse.Result);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CuentaBancaria cuenta)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
        {
            return Forbid();
        }

        // Verificar número de cuenta único
        var existenteResponse = await _cuentaBancariaRepository.GetByNumeroCuentaAsync(cuenta.EmpresaId, cuenta.NumeroCuenta);

        if (existenteResponse.WasSuccess && existenteResponse.Result != null)
        {
            return BadRequest("Ya existe una cuenta bancaria con este número.");
        }

        cuenta.Id = Guid.NewGuid();
        cuenta.FechaCreacion = DateTime.Now;
        cuenta.CreadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        cuenta.SaldoActual = cuenta.SaldoInicial;

        var addResponse = await _cuentaBancariaRepository.AddAsync(cuenta);

        if (!addResponse.WasSuccess)
        {
            return BadRequest(addResponse.Message);
        }

        return Ok(addResponse.Result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] CuentaBancaria cuenta)
    {
        if (id != cuenta.Id)
        {
            return BadRequest("El ID no coincide.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existenteResponse = await _cuentaBancariaRepository.GetAsync(id);

        if (!existenteResponse.WasSuccess || existenteResponse.Result == null)
        {
            return NotFound();
        }

        var existente = existenteResponse.Result;

        if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
        {
            return Forbid();
        }

        // Verificar número único (excluyendo la cuenta actual)
        var duplicadoResponse = await _cuentaBancariaRepository.GetByNumeroCuentaAsync(cuenta.EmpresaId, cuenta.NumeroCuenta);

        if (duplicadoResponse.WasSuccess && duplicadoResponse.Result != null && duplicadoResponse.Result.Id != id)
        {
            return BadRequest("Ya existe otra cuenta bancaria con este número.");
        }

        // Preservar campos de auditoría y saldo
        cuenta.FechaCreacion = existente.FechaCreacion;
        cuenta.CreadoPorId = existente.CreadoPorId;
        cuenta.SaldoActual = existente.SaldoActual; // Preservar saldo actual
        cuenta.FechaModificacion = DateTime.Now;
        cuenta.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var updateResponse = await _cuentaBancariaRepository.UpdateAsync(cuenta);

        if (!updateResponse.WasSuccess)
        {
            return BadRequest(updateResponse.Message);
        }

        return Ok(updateResponse.Result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var cuentaResponse = await _cuentaBancariaRepository.GetAsync(id);

        if (!cuentaResponse.WasSuccess || cuentaResponse.Result == null)
        {
            return NotFound();
        }

        var cuenta = cuentaResponse.Result;

        if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
        {
            return Forbid();
        }

        // Verificar si tiene movimientos (esto lo hace el repository)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var deleteResponse = await _cuentaBancariaRepository.DeleteAsync(id, userId!);

        if (!deleteResponse.WasSuccess)
        {
            return BadRequest(deleteResponse.Message);
        }

        return Ok();
    }

    [HttpGet("bancos")]
    public async Task<IActionResult> GetBancosAsync()
    {
        var bancos = await _context.Bancos
            .Where(b => b.Activo)
            .OrderBy(b => b.Nombre)
            .Select(b => new { b.Id, b.Nombre, b.CodigoSINPE })
            .ToListAsync();

        return Ok(bancos);
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // Verificar si es SuperUser
        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a la empresa
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
