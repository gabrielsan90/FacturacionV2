using ClosedXML.Excel;
using Facturacion.Backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class AuditoriaController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<AuditoriaController> _logger;

    public AuditoriaController(DataContext context, ILogger<AuditoriaController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene registros de auditoría filtrados
    /// </summary>
    [HttpGet("accesos")]
    public async Task<IActionResult> GetAccesosAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] string? fechaInicio,
        [FromQuery] string? fechaFin,
        [FromQuery] string? usuarioId,
        [FromQuery] string? accion,
        [FromQuery] string? exitoso)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var query = _context.Auditorias
                .Where(a => a.EmpresaId == empresaId)
                .AsNoTracking();

            if (DateTime.TryParse(fechaInicio, out var desde))
                query = query.Where(a => a.Fecha >= desde);

            if (DateTime.TryParse(fechaFin, out var hasta))
                query = query.Where(a => a.Fecha <= hasta.AddDays(1));

            if (!string.IsNullOrEmpty(usuarioId))
                query = query.Where(a => a.UsuarioId == usuarioId);

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(a => a.Accion == accion);

            if (!string.IsNullOrEmpty(exitoso) && bool.TryParse(exitoso, out var esExitoso))
                query = query.Where(a => a.Exitoso == esExitoso);

            var registros = await query
                .OrderByDescending(a => a.Fecha)
                .Take(1000)
                .Select(a => new
                {
                    a.Id,
                    a.Fecha,
                    a.UsuarioId,
                    a.NombreUsuario,
                    a.Accion,
                    a.Tabla,
                    a.RegistroId,
                    a.Descripcion,
                    a.DireccionIP,
                    a.Exitoso,
                    a.MensajeError,
                    a.MetodoHttp,
                    a.Endpoint,
                    a.DuracionMs
                })
                .ToListAsync();

            return Ok(registros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo registros de auditoría para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener registros de auditoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene un registro de auditoría por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var registro = await _context.Auditorias
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (registro == null)
                return NotFound("Registro de auditoría no encontrado.");

            if (!await TieneAccesoEmpresaAsync(registro.EmpresaId))
                return Forbid();

            return Ok(registro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo registro de auditoría {Id}", id);
            return StatusCode(500, $"Error al obtener registro: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene estadísticas de auditoría
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<IActionResult> GetEstadisticasAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] string? fechaInicio,
        [FromQuery] string? fechaFin)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var query = _context.Auditorias
                .Where(a => a.EmpresaId == empresaId)
                .AsNoTracking();

            if (DateTime.TryParse(fechaInicio, out var desde))
                query = query.Where(a => a.Fecha >= desde);

            if (DateTime.TryParse(fechaFin, out var hasta))
                query = query.Where(a => a.Fecha <= hasta.AddDays(1));

            var totalRegistros = await query.CountAsync();
            var exitosos = await query.CountAsync(a => a.Exitoso);
            var fallidos = totalRegistros - exitosos;

            var porAccion = await query
                .GroupBy(a => a.Accion)
                .Select(g => new { accion = g.Key, cantidad = g.Count() })
                .OrderByDescending(g => g.cantidad)
                .ToListAsync();

            var porUsuario = await query
                .GroupBy(a => new { a.UsuarioId, a.NombreUsuario })
                .Select(g => new { usuarioId = g.Key.UsuarioId, nombre = g.Key.NombreUsuario, cantidad = g.Count() })
                .OrderByDescending(g => g.cantidad)
                .Take(10)
                .ToListAsync();

            var porTabla = await query
                .GroupBy(a => a.Tabla)
                .Select(g => new { tabla = g.Key, cantidad = g.Count() })
                .OrderByDescending(g => g.cantidad)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                totalRegistros,
                exitosos,
                fallidos,
                porAccion,
                porUsuario,
                porTabla
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo estadísticas de auditoría para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error al obtener estadísticas: {ex.Message}");
        }
    }

    /// <summary>
    /// Exporta registros de auditoría a Excel
    /// </summary>
    [HttpGet("exportar")]
    public async Task<IActionResult> ExportarAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] string? fechaInicio,
        [FromQuery] string? fechaFin,
        [FromQuery] string? usuarioId,
        [FromQuery] string? accion)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
                return Forbid();

            var query = _context.Auditorias
                .Where(a => a.EmpresaId == empresaId)
                .AsNoTracking();

            if (DateTime.TryParse(fechaInicio, out var desde))
                query = query.Where(a => a.Fecha >= desde);

            if (DateTime.TryParse(fechaFin, out var hasta))
                query = query.Where(a => a.Fecha <= hasta.AddDays(1));

            if (!string.IsNullOrEmpty(usuarioId))
                query = query.Where(a => a.UsuarioId == usuarioId);

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(a => a.Accion == accion);

            var registros = await query
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Auditoría");

            ws.Cell(1, 1).Value = "Fecha";
            ws.Cell(1, 2).Value = "Usuario";
            ws.Cell(1, 3).Value = "Acción";
            ws.Cell(1, 4).Value = "Tabla";
            ws.Cell(1, 5).Value = "Descripción";
            ws.Cell(1, 6).Value = "IP";
            ws.Cell(1, 7).Value = "Exitoso";
            ws.Cell(1, 8).Value = "Error";
            ws.Range(1, 1, 1, 8).Style.Font.Bold = true;

            int row = 2;
            foreach (var reg in registros)
            {
                ws.Cell(row, 1).Value = reg.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
                ws.Cell(row, 2).Value = reg.NombreUsuario ?? reg.UsuarioId;
                ws.Cell(row, 3).Value = reg.Accion;
                ws.Cell(row, 4).Value = reg.Tabla;
                ws.Cell(row, 5).Value = reg.Descripcion ?? "";
                ws.Cell(row, 6).Value = reg.DireccionIP ?? "";
                ws.Cell(row, 7).Value = reg.Exitoso ? "Sí" : "No";
                ws.Cell(row, 8).Value = reg.MensajeError ?? "";
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Auditoria_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exportando auditoría para empresa {EmpresaId}", empresaId);
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
