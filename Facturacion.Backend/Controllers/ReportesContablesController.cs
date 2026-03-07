using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperUser,Administrador de Empresa,Contador")]
public class ReportesContablesController : ControllerBase
{
    private readonly IReportesContablesService _reportesService;
    private readonly DataContext _context;

    public ReportesContablesController(
        IReportesContablesService reportesService,
        DataContext context)
    {
        _reportesService = reportesService;
        _context = context;
    }

    /// <summary>
    /// Balanza de Comprobación
    /// </summary>
    [HttpGet("balanza-comprobacion")]
    public async Task<IActionResult> GetBalanzaComprobacionAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaDesde,
        [FromQuery] DateTime fechaHasta,
        [FromQuery] int? nivel,
        [FromQuery] bool incluirSinMovimiento = false)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
            return Forbid();

        var result = await _reportesService.GetBalanzaComprobacionAsync(
            empresaId, fechaDesde, fechaHasta, nivel, incluirSinMovimiento);
        return Ok(result);
    }

    /// <summary>
    /// Balance General (Estado de Situación Financiera)
    /// </summary>
    [HttpGet("balance-general")]
    public async Task<IActionResult> GetBalanceGeneralAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaCorte)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
            return Forbid();

        var result = await _reportesService.GetBalanceGeneralAsync(empresaId, fechaCorte);
        return Ok(result);
    }

    /// <summary>
    /// Estado de Resultados (P&L)
    /// </summary>
    [HttpGet("estado-resultados")]
    public async Task<IActionResult> GetEstadoResultadosAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaDesde,
        [FromQuery] DateTime fechaHasta)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
            return Forbid();

        var result = await _reportesService.GetEstadoResultadosAsync(empresaId, fechaDesde, fechaHasta);
        return Ok(result);
    }

    /// <summary>
    /// Libro Diario
    /// </summary>
    [HttpGet("libro-diario")]
    public async Task<IActionResult> GetLibroDiarioAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaDesde,
        [FromQuery] DateTime fechaHasta)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
            return Forbid();

        var result = await _reportesService.GetLibroDiarioAsync(empresaId, fechaDesde, fechaHasta);
        return Ok(result);
    }

    /// <summary>
    /// Flujo de Efectivo
    /// </summary>
    [HttpGet("flujo-efectivo")]
    public async Task<IActionResult> GetFlujoEfectivoAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaDesde,
        [FromQuery] DateTime fechaHasta)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
            return Forbid();

        var result = await _reportesService.GetFlujoEfectivoAsync(empresaId, fechaDesde, fechaHasta);
        return Ok(result);
    }

    /// <summary>
    /// Balance de Sumas y Saldos
    /// </summary>
    [HttpGet("balance-sumas-saldos")]
    public async Task<IActionResult> GetBalanceSumasySaldosAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaDesde,
        [FromQuery] DateTime fechaHasta,
        [FromQuery] int? nivel,
        [FromQuery] bool incluirSinMovimiento = false)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
            return Forbid();

        var result = await _reportesService.GetBalanceSumasySaldosAsync(
            empresaId, fechaDesde, fechaHasta, nivel, incluirSinMovimiento);
        return Ok(result);
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (userRoles.Contains("SuperUser"))
            return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
