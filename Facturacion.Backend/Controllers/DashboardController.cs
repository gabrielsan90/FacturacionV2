using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para métricas y estadísticas del dashboard empresarial
/// Proporciona datos agregados de ventas, gastos, inventario y documentos
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly DataContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        DataContext context,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GET: api/dashboard/resumen
    /// Obtiene el resumen general del dashboard con todas las métricas principales
    /// </summary>
    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumenAsync()
    {
        try
        {
            _logger.LogInformation("Getting dashboard summary for user {UserId}",
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                _logger.LogWarning("Could not obtain company ID for user {UserId}",
                    User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            var resumen = await _dashboardService.GetResumenAsync(empresaId);
            _logger.LogInformation("Successfully retrieved dashboard summary for company {EmpresaId}", empresaId);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary for user {UserId}",
                User.FindFirstValue(ClaimTypes.NameIdentifier));
            return StatusCode(500, "Error al obtener el resumen del dashboard");
        }
    }

    /// <summary>
    /// GET: api/dashboard/ventas-mes?ano=2024
    /// Obtiene las ventas agrupadas por mes para un año específico
    /// </summary>
    [HttpGet("ventas-mes")]
    public async Task<IActionResult> GetVentasPorMesAsync([FromQuery] int? ano)
    {
        try
        {
            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                _logger.LogWarning("Could not obtain company ID for sales by month query");
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            // Si no se especifica año, usar el año actual
            var anoConsulta = ano ?? FechaCostaRicaHelper.Ahora.Year;
            _logger.LogInformation("Getting sales by month for company {EmpresaId}, year {Year}",
                empresaId, anoConsulta);

            var ventasPorMes = await _dashboardService.GetVentasPorMesAsync(empresaId, anoConsulta);
            _logger.LogInformation("Successfully retrieved sales by month for company {EmpresaId}", empresaId);
            return Ok(ventasPorMes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales by month for year {Year}", ano);
            return StatusCode(500, "Error al obtener ventas por mes");
        }
    }

    /// <summary>
    /// GET: api/dashboard/ventas-tipo?fechaInicio=2024-01-01&fechaFin=2024-12-31
    /// Obtiene la distribución de ventas por tipo de documento en un rango de fechas
    /// </summary>
    [HttpGet("ventas-tipo")]
    public async Task<IActionResult> GetVentasPorTipoDocumentoAsync(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                _logger.LogWarning("Could not obtain company ID for sales by document type query");
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            // Si no se especifican fechas, usar el mes actual
            var inicio = fechaInicio ?? new DateTime(FechaCostaRicaHelper.Ahora.Year, FechaCostaRicaHelper.Ahora.Month, 1);
            var fin = fechaFin ?? FechaCostaRicaHelper.Ahora;

            _logger.LogInformation("Getting sales by document type for company {EmpresaId}, date range {Start} to {End}",
                empresaId, inicio, fin);

            var ventasPorTipo = await _dashboardService.GetVentasPorTipoDocumentoAsync(empresaId, inicio, fin);
            return Ok(ventasPorTipo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales by document type for date range {Start} to {End}",
                fechaInicio, fechaFin);
            return StatusCode(500, "Error al obtener ventas por tipo de documento");
        }
    }

    /// <summary>
    /// GET: api/dashboard/top-clientes?top=10&fechaInicio=2024-01-01&fechaFin=2024-12-31
    /// Obtiene los principales clientes ordenados por volumen de compras
    /// </summary>
    [HttpGet("top-clientes")]
    public async Task<IActionResult> GetTopClientesAsync(
        [FromQuery] int? top,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                _logger.LogWarning("Could not obtain company ID for top customers query");
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            var topValue = top ?? 10;
            _logger.LogInformation("Getting top {Top} customers for company {EmpresaId}", topValue, empresaId);

            var topClientes = await _dashboardService.GetTopClientesAsync(
                empresaId,
                topValue,
                fechaInicio,
                fechaFin);

            return Ok(topClientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top customers");
            return StatusCode(500, "Error al obtener top clientes");
        }
    }

    /// <summary>
    /// GET: api/dashboard/top-productos?top=10&fechaInicio=2024-01-01&fechaFin=2024-12-31
    /// Obtiene los productos más vendidos ordenados por cantidad y monto
    /// </summary>
    [HttpGet("top-productos")]
    public async Task<IActionResult> GetTopProductosAsync(
        [FromQuery] int? top,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                _logger.LogWarning("Could not obtain company ID for top products query");
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            var topValue = top ?? 10;
            _logger.LogInformation("Getting top {Top} products for company {EmpresaId}", topValue, empresaId);

            var topProductos = await _dashboardService.GetTopProductosAsync(
                empresaId,
                topValue,
                fechaInicio,
                fechaFin);

            return Ok(topProductos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top products");
            return StatusCode(500, "Error al obtener top productos");
        }
    }

    /// <summary>
    /// GET: api/dashboard/inventario?soloBajoStock=false
    /// Obtiene el estado del inventario de productos con alertas de stock
    /// </summary>
    [HttpGet("inventario")]
    public async Task<IActionResult> GetEstadoInventarioAsync([FromQuery] bool? soloBajoStock)
    {
        try
        {
            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                _logger.LogWarning("Could not obtain company ID for inventory status query");
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            var bajoStock = soloBajoStock ?? false;
            _logger.LogInformation("Getting inventory status for company {EmpresaId}, low stock only: {LowStockOnly}",
                empresaId, bajoStock);

            var estadoInventario = await _dashboardService.GetEstadoInventarioAsync(empresaId, bajoStock);
            return Ok(estadoInventario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory status");
            return StatusCode(500, "Error al obtener estado de inventario");
        }
    }

    /// <summary>
    /// GET: api/dashboard/pendientes
    /// Obtiene los documentos pendientes de envío y rechazados
    /// </summary>
    [HttpGet("pendientes")]
    public async Task<IActionResult> GetDocumentosPendientesAsync()
    {
        try
        {
            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                _logger.LogWarning("Could not obtain company ID for pending documents query");
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            _logger.LogInformation("Getting pending documents for company {EmpresaId}", empresaId);
            var documentosPendientes = await _dashboardService.GetDocumentosPendientesAsync(empresaId);
            return Ok(documentosPendientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending documents");
            return StatusCode(500, "Error al obtener documentos pendientes");
        }
    }

    /// <summary>
    /// GET: api/dashboard/flujo-caja?fechaInicio=2024-01-01&fechaFin=2024-12-31
    /// Obtiene el flujo de caja agrupado por día en un rango de fechas
    /// </summary>
    [HttpGet("flujo-caja")]
    public async Task<IActionResult> GetFlujoCajaAsync(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                _logger.LogWarning("Could not obtain company ID for cash flow query");
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            // Si no se especifican fechas, usar el mes actual
            var inicio = fechaInicio ?? new DateTime(FechaCostaRicaHelper.Ahora.Year, FechaCostaRicaHelper.Ahora.Month, 1);
            var fin = fechaFin ?? FechaCostaRicaHelper.Ahora;

            _logger.LogInformation("Getting cash flow for company {EmpresaId}, date range {Start} to {End}",
                empresaId, inicio, fin);

            var flujoCaja = await _dashboardService.GetFlujoCajaAsync(empresaId, inicio, fin);
            return Ok(flujoCaja);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cash flow for date range {Start} to {End}",
                fechaInicio, fechaFin);
            return StatusCode(500, "Error al obtener flujo de caja");
        }
    }

    /// <summary>
    /// GET: api/dashboard/ventas-dia?fechaInicio=2024-01-01&fechaFin=2024-12-31
    /// Obtiene las ventas agrupadas por día en un rango de fechas
    /// </summary>
    [HttpGet("ventas-dia")]
    public async Task<IActionResult> GetVentasPorDiaAsync(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        try
        {
            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                _logger.LogWarning("Could not obtain company ID for sales by day query");
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            // Si no se especifican fechas, usar el mes actual
            var inicio = fechaInicio ?? new DateTime(FechaCostaRicaHelper.Ahora.Year, FechaCostaRicaHelper.Ahora.Month, 1);
            var fin = fechaFin ?? FechaCostaRicaHelper.Ahora;

            _logger.LogInformation("Getting sales by day for company {EmpresaId}, date range {Start} to {End}",
                empresaId, inicio, fin);

            var ventasPorDia = await _dashboardService.GetVentasPorDiaAsync(empresaId, inicio, fin);
            return Ok(ventasPorDia);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales by day for date range {Start} to {End}",
                fechaInicio, fechaFin);
            return StatusCode(500, "Error al obtener ventas por día");
        }
    }

    /// <summary>
    /// GET: api/dashboard/comparativo
    /// Obtiene comparativo de ventas: mes actual vs mes anterior
    /// </summary>
    [HttpGet("comparativo")]
    public async Task<IActionResult> GetComparativoMensualAsync()
    {
        try
        {
            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                _logger.LogWarning("Could not obtain company ID for monthly comparison query");
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            _logger.LogInformation("Getting monthly sales comparison for company {EmpresaId}", empresaId);
            var comparativo = await _dashboardService.GetComparativoMensualAsync(empresaId);
            return Ok(comparativo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly sales comparison");
            return StatusCode(500, "Error al obtener comparativo mensual");
        }
    }

    /// <summary>
    /// Obtiene el empresaId del usuario autenticado (intenta claim primero, luego DB)
    /// </summary>
    private async Task<Guid> ObtenerEmpresaIdAsync()
    {
        try
        {
            // Primero intentar obtener EmpresaId del claim (método preferido)
            var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
            if (!string.IsNullOrEmpty(empresaIdClaim) && Guid.TryParse(empresaIdClaim, out var empresaIdFromClaim))
            {
                _logger.LogDebug("Company ID {EmpresaId} obtained from JWT claim", empresaIdFromClaim);
                return empresaIdFromClaim;
            }

            // Fallback: buscar en la base de datos
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Guid.Empty;
            }

            // Obtener la primera empresa del usuario
            var usuarioEmpresa = await _context.UsuariosEmpresas
                .Where(ue => ue.UserId == userId)
                .FirstOrDefaultAsync();

            if (usuarioEmpresa != null)
            {
                _logger.LogDebug("Company ID {EmpresaId} obtained from database for user {UserId}",
                    usuarioEmpresa.EmpresaId, userId);
                return usuarioEmpresa.EmpresaId;
            }

            _logger.LogWarning("No company association found for user {UserId}", userId);
            return Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtaining company ID for user {UserId}",
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Guid.Empty;
        }
    }
}
