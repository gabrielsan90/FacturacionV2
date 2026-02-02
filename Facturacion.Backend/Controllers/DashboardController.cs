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

    public DashboardController(IDashboardService dashboardService, DataContext context)
    {
        _dashboardService = dashboardService;
        _context = context;
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
            var empresaId = await ObtenerEmpresaIdAsync();
            if (empresaId == Guid.Empty)
            {
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            var resumen = await _dashboardService.GetResumenAsync(empresaId);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener el resumen del dashboard: {ex.Message}");
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
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            // Si no se especifica año, usar el año actual
            var anoConsulta = ano ?? FechaCostaRicaHelper.Ahora.Year;

            var ventasPorMes = await _dashboardService.GetVentasPorMesAsync(empresaId, anoConsulta);
            return Ok(ventasPorMes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener ventas por mes: {ex.Message}");
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
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            // Si no se especifican fechas, usar el mes actual
            var inicio = fechaInicio ?? new DateTime(FechaCostaRicaHelper.Ahora.Year, FechaCostaRicaHelper.Ahora.Month, 1);
            var fin = fechaFin ?? FechaCostaRicaHelper.Ahora;

            var ventasPorTipo = await _dashboardService.GetVentasPorTipoDocumentoAsync(empresaId, inicio, fin);
            return Ok(ventasPorTipo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener ventas por tipo de documento: {ex.Message}");
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
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            var topClientes = await _dashboardService.GetTopClientesAsync(
                empresaId,
                top ?? 10,
                fechaInicio,
                fechaFin);

            return Ok(topClientes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener top clientes: {ex.Message}");
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
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            var topProductos = await _dashboardService.GetTopProductosAsync(
                empresaId,
                top ?? 10,
                fechaInicio,
                fechaFin);

            return Ok(topProductos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener top productos: {ex.Message}");
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
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            var estadoInventario = await _dashboardService.GetEstadoInventarioAsync(
                empresaId,
                soloBajoStock ?? false);

            return Ok(estadoInventario);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener estado de inventario: {ex.Message}");
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
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            var documentosPendientes = await _dashboardService.GetDocumentosPendientesAsync(empresaId);
            return Ok(documentosPendientes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener documentos pendientes: {ex.Message}");
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
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            // Si no se especifican fechas, usar el mes actual
            var inicio = fechaInicio ?? new DateTime(FechaCostaRicaHelper.Ahora.Year, FechaCostaRicaHelper.Ahora.Month, 1);
            var fin = fechaFin ?? FechaCostaRicaHelper.Ahora;

            var flujoCaja = await _dashboardService.GetFlujoCajaAsync(empresaId, inicio, fin);
            return Ok(flujoCaja);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener flujo de caja: {ex.Message}");
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
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            // Si no se especifican fechas, usar el mes actual
            var inicio = fechaInicio ?? new DateTime(FechaCostaRicaHelper.Ahora.Year, FechaCostaRicaHelper.Ahora.Month, 1);
            var fin = fechaFin ?? FechaCostaRicaHelper.Ahora;

            var ventasPorDia = await _dashboardService.GetVentasPorDiaAsync(empresaId, inicio, fin);
            return Ok(ventasPorDia);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener ventas por día: {ex.Message}");
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
                return BadRequest("No se pudo obtener la empresa del usuario.");
            }

            var comparativo = await _dashboardService.GetComparativoMensualAsync(empresaId);
            return Ok(comparativo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener comparativo mensual: {ex.Message}");
        }
    }

    // Método auxiliar para obtener el empresaId del usuario autenticado
    private async Task<Guid> ObtenerEmpresaIdAsync()
    {
        try
        {
            // Primero intentar obtener EmpresaId del claim (método preferido)
            var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
            if (!string.IsNullOrEmpty(empresaIdClaim) && Guid.TryParse(empresaIdClaim, out var empresaIdFromClaim))
            {
                return empresaIdFromClaim;
            }

            // Fallback: buscar en la base de datos
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Guid.Empty;
            }

            // Obtener la primera empresa del usuario
            var usuarioEmpresa = await _context.UsuariosEmpresas
                .Where(ue => ue.UserId == userId)
                .FirstOrDefaultAsync();

            return usuarioEmpresa?.EmpresaId ?? Guid.Empty;
        }
        catch
        {
            return Guid.Empty;
        }
    }
}
