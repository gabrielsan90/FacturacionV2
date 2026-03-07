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
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(
        IReportesService reportesService,
        DataContext context,
        ILogger<ReportesController> logger)
    {
        _reportesService = reportesService;
        _context = context;
        _logger = logger;
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
        try
        {
            _logger.LogInformation("Generating sales report for company {EmpresaId}, date range {Start} to {End}, customer {ClienteId}, docType {DocType}",
                empresaId, fechaInicio, fechaFin, clienteId, tipoDocumento);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("User {UserId} attempted to generate sales report for company {EmpresaId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var action = await _reportesService.GetReporteVentasAsync(
                empresaId,
                fechaInicio,
                fechaFin,
                clienteId,
                tipoDocumento);

            if (action.WasSuccess)
            {
                _logger.LogInformation("Successfully generated sales report for company {EmpresaId}", empresaId);
            }
            else
            {
                _logger.LogWarning("Failed to generate sales report for company {EmpresaId}: {Message}",
                    empresaId, action.Message);
            }

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales report for company {EmpresaId}", empresaId);
            return StatusCode(500, "Error al generar el reporte de ventas");
        }
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
        try
        {
            _logger.LogInformation("Generating expenses report for company {EmpresaId}, date range {Start} to {End}",
                empresaId, fechaInicio, fechaFin);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("User {UserId} attempted to generate expenses report for company {EmpresaId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var action = await _reportesService.GetReporteGastosAsync(
                empresaId,
                fechaInicio,
                fechaFin,
                proveedorId,
                categoriaId);

            if (action.WasSuccess)
            {
                _logger.LogInformation("Successfully generated expenses report for company {EmpresaId}", empresaId);
            }

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expenses report for company {EmpresaId}", empresaId);
            return StatusCode(500, "Error al generar el reporte de gastos");
        }
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
        try
        {
            _logger.LogInformation("Generating inventory report for company {EmpresaId}, branch {SucursalId}, low stock only: {LowStockOnly}",
                empresaId, sucursalId, soloBajoStock);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("User {UserId} attempted to generate inventory report for company {EmpresaId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var action = await _reportesService.GetReporteInventarioAsync(
                empresaId,
                sucursalId,
                soloBajoStock);

            if (action.WasSuccess)
            {
                _logger.LogInformation("Successfully generated inventory report for company {EmpresaId}", empresaId);
            }

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory report for company {EmpresaId}", empresaId);
            return StatusCode(500, "Error al generar el reporte de inventario");
        }
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
        try
        {
            _logger.LogInformation("Generating tax report for company {EmpresaId}, date range {Start} to {End}",
                empresaId, fechaInicio, fechaFin);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("User {UserId} attempted to generate tax report for company {EmpresaId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var action = await _reportesService.GetReporteImpuestosAsync(
                empresaId,
                fechaInicio,
                fechaFin);

            if (action.WasSuccess)
            {
                _logger.LogInformation("Successfully generated tax report for company {EmpresaId}", empresaId);
            }

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tax report for company {EmpresaId}", empresaId);
            return StatusCode(500, "Error al generar el reporte de impuestos");
        }
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
        try
        {
            _logger.LogInformation("Generating customer activity report for company {EmpresaId}, date range {Start} to {End}",
                empresaId, fechaInicio, fechaFin);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("User {UserId} attempted to generate customer report for company {EmpresaId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var action = await _reportesService.GetReporteClientesAsync(
                empresaId,
                fechaInicio,
                fechaFin,
                soloActivos);

            if (action.WasSuccess)
            {
                _logger.LogInformation("Successfully generated customer activity report for company {EmpresaId}", empresaId);
            }

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer activity report for company {EmpresaId}", empresaId);
            return StatusCode(500, "Error al generar el reporte de clientes");
        }
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
        try
        {
            _logger.LogInformation("Generating product sales report for company {EmpresaId}, date range {Start} to {End}, category {CategoriaId}",
                empresaId, fechaInicio, fechaFin, categoriaId);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("User {UserId} attempted to generate product report for company {EmpresaId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var action = await _reportesService.GetReporteProductosAsync(
                empresaId,
                fechaInicio,
                fechaFin,
                categoriaId);

            if (action.WasSuccess)
            {
                _logger.LogInformation("Successfully generated product sales report for company {EmpresaId}", empresaId);
            }

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating product sales report for company {EmpresaId}", empresaId);
            return StatusCode(500, "Error al generar el reporte de productos");
        }
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
        try
        {
            _logger.LogInformation("Generating inventory movements report for company {EmpresaId}, date range {Start} to {End}",
                empresaId, fechaInicio, fechaFin);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("User {UserId} attempted to generate inventory movements report for company {EmpresaId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var action = await _reportesService.GetReporteMovimientosInventarioAsync(
                empresaId,
                fechaInicio,
                fechaFin,
                productoId,
                tipoMovimiento);

            if (action.WasSuccess)
            {
                _logger.LogInformation("Successfully generated inventory movements report for company {EmpresaId}", empresaId);
            }

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating inventory movements report for company {EmpresaId}", empresaId);
            return StatusCode(500, "Error al generar el reporte de movimientos de inventario");
        }
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
        try
        {
            _logger.LogInformation("Generating sales book (Libro de Ventas) for company {EmpresaId}, period {Month}/{Year}",
                empresaId, mes, ano);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("User {UserId} attempted to generate sales book for company {EmpresaId} without permission",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
                return Forbid();
            }

            var action = await _reportesService.GetReporteLibroVentasAsync(
                empresaId,
                mes,
                ano);

            if (action.WasSuccess)
            {
                _logger.LogInformation("Successfully generated sales book for company {EmpresaId}, period {Month}/{Year}",
                    empresaId, mes, ano);
            }

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales book for company {EmpresaId}, period {Month}/{Year}",
                empresaId, mes, ano);
            return StatusCode(500, "Error al generar el libro de ventas");
        }
    }

    /// <summary>
    /// Genera reporte de retenciones de renta (D-103)
    /// GET: api/reportes/retenciones?empresaId=xxx&fechaInicio=xxx&fechaFin=xxx
    /// </summary>
    [HttpGet("retenciones")]
    public async Task<IActionResult> GetReporteRetencionesAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        try
        {
            _logger.LogInformation("Generating withholding tax report (D-103) for company {EmpresaId}, date range {Start} to {End}",
                empresaId, fechaInicio, fechaFin);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var action = await _reportesService.GetReporteRetencionesAsync(empresaId, fechaInicio, fechaFin);

            return action.WasSuccess ? Ok(action.Result) : BadRequest(action.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating withholding tax report for company {EmpresaId}", empresaId);
            return StatusCode(500, "Error al generar el reporte de retenciones");
        }
    }

    /// <summary>
    /// Exporta reporte de retenciones a Excel
    /// GET: api/reportes/retenciones/excel?empresaId=xxx&fechaInicio=xxx&fechaFin=xxx
    /// </summary>
    [HttpGet("retenciones/excel")]
    public async Task<IActionResult> ExportarRetencionesExcelAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var action = await _reportesService.ExportarRetencionesExcelAsync(empresaId, fechaInicio, fechaFin);

            if (!action.WasSuccess || action.Result == null)
            {
                return BadRequest(action.Message);
            }

            var fileName = $"D-103_Retenciones_{fechaInicio:yyyyMM}.xlsx";
            return File(action.Result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting withholding tax report for company {EmpresaId}", empresaId);
            return StatusCode(500, "Error al exportar el reporte de retenciones");
        }
    }

    #region Helper Methods

    /// <summary>
    /// Verifica si el usuario tiene acceso a la empresa (multi-tenant security)
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims for multi-tenant validation");
                return false;
            }

            // SuperUser tiene acceso a todas las empresas
            if (User.IsInRole("SuperUser"))
            {
                return true;
            }

            // Verificar si el usuario tiene acceso a esta empresa
            var tieneAcceso = await _context.UsuariosEmpresas
                .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);

            return tieneAcceso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating company access for user {UserId}, company {EmpresaId}",
                User.FindFirstValue(ClaimTypes.NameIdentifier), empresaId);
            return false;
        }
    }

    #endregion
}
