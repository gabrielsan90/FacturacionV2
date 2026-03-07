using ClosedXML.Excel;
using Facturacion.Backend.Data;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<VentasController> _logger;

    public VentasController(DataContext context, ILogger<VentasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el historial de ventas (documentos emitidos) de una empresa
    /// </summary>
    [HttpGet("historial/empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetHistorialAsync(
        Guid empresaId,
        [FromQuery] string? fechaInicio,
        [FromQuery] string? fechaFin,
        [FromQuery] Guid? clienteId,
        [FromQuery] Guid? sucursalId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var query = _context.Documentos
                .Where(d => d.EmpresaId == empresaId && !d.EsDocumentoRecibido && !d.IsDeleted)
                .AsNoTracking();

            if (DateTime.TryParse(fechaInicio, out var desde))
                query = query.Where(d => d.FechaEmision >= desde);

            if (DateTime.TryParse(fechaFin, out var hasta))
                query = query.Where(d => d.FechaEmision <= hasta.AddDays(1));

            if (clienteId.HasValue && clienteId != Guid.Empty)
                query = query.Where(d => d.ClienteId == clienteId);

            if (sucursalId.HasValue && sucursalId != Guid.Empty)
                query = query.Where(d => d.SucursalId == sucursalId);

            var documentos = await query
                .OrderByDescending(d => d.FechaEmision)
                .Select(d => new
                {
                    d.Id,
                    d.NumeroConsecutivo,
                    d.Clave,
                    tipoDocumento = d.TipoDocumento.ToString(),
                    d.FechaEmision,
                    d.ReceptorNombre,
                    d.ReceptorNumeroIdentificacion,
                    d.Subtotal,
                    d.TotalImpuestos,
                    d.TotalDescuentos,
                    d.TotalVenta,
                    estado = d.Estado.ToString(),
                    d.CondicionVenta,
                    d.MedioPago,
                    moneda = d.Moneda.ToString(),
                    d.CorreoEnviado,
                    d.Observaciones,
                    sucursalNombre = d.Sucursal != null ? d.Sucursal.Nombre : "",
                    clienteNombre = d.ReceptorNombre ?? ""
                })
                .ToListAsync();

            return Ok(documentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo historial de ventas para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener historial de ventas: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene resumen estadístico de ventas de una empresa
    /// </summary>
    [HttpGet("resumen/empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetResumenAsync(
        Guid empresaId,
        [FromQuery] string? fechaInicio,
        [FromQuery] string? fechaFin)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var query = _context.Documentos
                .Where(d => d.EmpresaId == empresaId && !d.EsDocumentoRecibido && !d.IsDeleted)
                .AsNoTracking();

            if (DateTime.TryParse(fechaInicio, out var desde))
                query = query.Where(d => d.FechaEmision >= desde);

            if (DateTime.TryParse(fechaFin, out var hasta))
                query = query.Where(d => d.FechaEmision <= hasta.AddDays(1));

            var totalDocumentos = await query.CountAsync();
            var totalVentas = await query.SumAsync(d => d.TotalVenta);
            var totalImpuestos = await query.SumAsync(d => d.TotalImpuestos);
            var totalDescuentos = await query.SumAsync(d => d.TotalDescuentos);

            var aceptados = await query.CountAsync(d => d.Estado == EstadoDocumento.Aceptado);
            var rechazados = await query.CountAsync(d => d.Estado == EstadoDocumento.Rechazado);
            var pendientes = await query.CountAsync(d => d.Estado == EstadoDocumento.Pendiente || d.Estado == EstadoDocumento.Borrador);

            return Ok(new
            {
                totalDocumentos,
                totalVentas,
                totalImpuestos,
                totalDescuentos,
                aceptados,
                rechazados,
                pendientes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo resumen de ventas para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener resumen: {ex.Message}");
        }
    }

    /// <summary>
    /// Exporta historial de ventas a Excel
    /// </summary>
    [HttpGet("exportar/empresa/{empresaId:guid}")]
    public async Task<IActionResult> ExportarAsync(
        Guid empresaId,
        [FromQuery] string? fechaInicio,
        [FromQuery] string? fechaFin,
        [FromQuery] Guid? clienteId,
        [FromQuery] Guid? sucursalId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var query = _context.Documentos
                .Where(d => d.EmpresaId == empresaId && !d.EsDocumentoRecibido && !d.IsDeleted)
                .AsNoTracking();

            if (DateTime.TryParse(fechaInicio, out var desde))
                query = query.Where(d => d.FechaEmision >= desde);

            if (DateTime.TryParse(fechaFin, out var hasta))
                query = query.Where(d => d.FechaEmision <= hasta.AddDays(1));

            if (clienteId.HasValue && clienteId != Guid.Empty)
                query = query.Where(d => d.ClienteId == clienteId);

            if (sucursalId.HasValue && sucursalId != Guid.Empty)
                query = query.Where(d => d.SucursalId == sucursalId);

            var documentos = await query
                .OrderByDescending(d => d.FechaEmision)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Historial Ventas");

            // Headers
            ws.Cell(1, 1).Value = "Consecutivo";
            ws.Cell(1, 2).Value = "Fecha";
            ws.Cell(1, 3).Value = "Tipo";
            ws.Cell(1, 4).Value = "Cliente";
            ws.Cell(1, 5).Value = "Identificación";
            ws.Cell(1, 6).Value = "Subtotal";
            ws.Cell(1, 7).Value = "Impuestos";
            ws.Cell(1, 8).Value = "Descuentos";
            ws.Cell(1, 9).Value = "Total";
            ws.Cell(1, 10).Value = "Estado";
            ws.Range(1, 1, 1, 10).Style.Font.Bold = true;

            int row = 2;
            foreach (var doc in documentos)
            {
                ws.Cell(row, 1).Value = doc.NumeroConsecutivo;
                ws.Cell(row, 2).Value = doc.FechaEmision.ToString("dd/MM/yyyy");
                ws.Cell(row, 3).Value = doc.TipoDocumento.ToString();
                ws.Cell(row, 4).Value = doc.ReceptorNombre ?? "";
                ws.Cell(row, 5).Value = doc.ReceptorNumeroIdentificacion ?? "";
                ws.Cell(row, 6).Value = doc.Subtotal;
                ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 7).Value = doc.TotalImpuestos;
                ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 8).Value = doc.TotalDescuentos;
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 9).Value = doc.TotalVenta;
                ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 10).Value = doc.Estado.ToString();
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HistorialVentas_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exportando historial de ventas para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al exportar: {ex.Message}");
        }
    }

    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        if (User.IsInRole("SuperUser"))
            return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
