using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador de reportes empresariales
/// Sistema de Facturación Electrónica - Costa Rica
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _reportesService;
    private readonly DataContext _context;

    public ReportesController(IReportesService reportesService, DataContext context)
    {
        _reportesService = reportesService;
        _context = context;
    }

    /// <summary>
    /// Genera reporte de ventas consolidado por período
    /// GET: api/reportes/ventas?empresaId=xxx&fechaInicio=xxx&fechaFin=xxx&clienteId=xxx&tipoDocumento=1
    /// </summary>
    [HttpGet("ventas")]
    public async Task<IActionResult> GetReporteVentasAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] Guid? clienteId = null,
        [FromQuery] DocumentoTipo? tipoDocumento = null)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _reportesService.GetReporteVentasAsync(
            empresaId,
            fechaInicio,
            fechaFin,
            clienteId,
            tipoDocumento);

        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Genera reporte de gastos consolidado por período
    /// GET: api/reportes/gastos?empresaId=xxx&fechaInicio=xxx&fechaFin=xxx&proveedorId=xxx&categoriaId=1
    /// </summary>
    [HttpGet("gastos")]
    public async Task<IActionResult> GetReporteGastosAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] Guid? proveedorId = null,
        [FromQuery] int? categoriaId = null)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _reportesService.GetReporteGastosAsync(
            empresaId,
            fechaInicio,
            fechaFin,
            proveedorId,
            categoriaId);

        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Genera reporte de inventario actual
    /// GET: api/reportes/inventario?empresaId=xxx&sucursalId=xxx&soloBajoStock=true
    /// </summary>
    [HttpGet("inventario")]
    public async Task<IActionResult> GetReporteInventarioAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] Guid? sucursalId = null,
        [FromQuery] bool soloBajoStock = false)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _reportesService.GetReporteInventarioAsync(
            empresaId,
            sucursalId,
            soloBajoStock);

        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Genera reporte de impuestos para declaraciones a Hacienda
    /// Calcula IVA de ventas, IVA de compras y saldo por pagar
    /// GET: api/reportes/impuestos?empresaId=xxx&fechaInicio=xxx&fechaFin=xxx
    /// </summary>
    [HttpGet("impuestos")]
    public async Task<IActionResult> GetReporteImpuestosAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _reportesService.GetReporteImpuestosAsync(
            empresaId,
            fechaInicio,
            fechaFin);

        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Genera reporte de actividad de clientes
    /// GET: api/reportes/clientes?empresaId=xxx&fechaInicio=xxx&fechaFin=xxx&soloActivos=true
    /// </summary>
    [HttpGet("clientes")]
    public async Task<IActionResult> GetReporteClientesAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] bool soloActivos = false)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _reportesService.GetReporteClientesAsync(
            empresaId,
            fechaInicio,
            fechaFin,
            soloActivos);

        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Genera reporte de ventas por producto
    /// GET: api/reportes/productos?empresaId=xxx&fechaInicio=xxx&fechaFin=xxx&categoriaId=xxx
    /// </summary>
    [HttpGet("productos")]
    public async Task<IActionResult> GetReporteProductosAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] Guid? categoriaId = null)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _reportesService.GetReporteProductosAsync(
            empresaId,
            fechaInicio,
            fechaFin,
            categoriaId);

        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Genera reporte de movimientos de inventario
    /// GET: api/reportes/movimientos-inventario?empresaId=xxx&fechaInicio=xxx&fechaFin=xxx&productoId=xxx&tipoMovimiento=1
    /// </summary>
    [HttpGet("movimientos-inventario")]
    public async Task<IActionResult> GetReporteMovimientosInventarioAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] Guid? productoId = null,
        [FromQuery] TipoMovimientoInventario? tipoMovimiento = null)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _reportesService.GetReporteMovimientosInventarioAsync(
            empresaId,
            fechaInicio,
            fechaFin,
            productoId,
            tipoMovimiento);

        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    /// <summary>
    /// Genera Libro de Ventas mensual (requerido por Hacienda)
    /// Lista cronológica de todas las facturas emitidas en el mes
    /// GET: api/reportes/libro-ventas?empresaId=xxx&mes=1&ano=2025
    /// </summary>
    [HttpGet("libro-ventas")]
    public async Task<IActionResult> GetReporteLibroVentasAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] int mes,
        [FromQuery] int ano)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var action = await _reportesService.GetReporteLibroVentasAsync(
            empresaId,
            mes,
            ano);

        return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
    }

    #region Helper Methods

    /// <summary>
    /// Verifica si el usuario tiene acceso a la empresa (multi-tenant security)
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // Admin tiene acceso a todas las empresas
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a esta empresa
        var tieneAcceso = await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);

        return tieneAcceso;
    }

    #endregion
}
