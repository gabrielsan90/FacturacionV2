using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador de Libro Mayor (General Ledger).
/// Genera reportes de movimientos contables por cuenta con saldos.
/// Sistema de Facturación Electrónica - Costa Rica
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class MayoresContablesController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IContabilidadUnitOfWork _unitOfWork;
    private readonly ILogger<MayoresContablesController> _logger;

    public MayoresContablesController(
        DataContext context,
        IContabilidadUnitOfWork unitOfWork,
        ILogger<MayoresContablesController> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Genera reporte de Libro Mayor para un rango de fechas.
    /// Calcula saldos anteriores, movimientos del período y saldos finales por cuenta.
    /// GET: api/mayorescontables?empresaId={guid}&amp;fechaDesde={date}&amp;fechaHasta={date}&amp;cuentaId={guid?}
    /// </summary>
    /// <param name="empresaId">ID de la empresa (multi-tenancy)</param>
    /// <param name="fechaDesde">Fecha inicial del período a consultar</param>
    /// <param name="fechaHasta">Fecha final del período a consultar</param>
    /// <param name="cuentaId">ID de cuenta específica (opcional, si se omite trae todas las cuentas con movimientos)</param>
    /// <returns>ResumenMayorDTO con totales y detalle por cuenta</returns>
    [HttpGet]
    public async Task<IActionResult> GetMayorAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] DateTime fechaDesde,
        [FromQuery] DateTime fechaHasta,
        [FromQuery] Guid? cuentaId = null)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            // Validar fechas
            if (fechaDesde > fechaHasta)
            {
                return BadRequest("La fecha desde no puede ser mayor que la fecha hasta.");
            }

            // Query base: Movimientos aprobados de la empresa
            var movimientosQuery = _context.MovimientosContables
                .Include(m => m.AsientoContable)
                .Include(m => m.CuentaContable)
                .Where(m => m.AsientoContable!.EmpresaId == empresaId
                    && m.AsientoContable.Estado == "APR"
                    && !m.AsientoContable.IsDeleted);

            // Filtrar por cuenta si se especifica
            if (cuentaId.HasValue)
            {
                movimientosQuery = movimientosQuery.Where(m => m.CuentaContableId == cuentaId.Value);
            }

            // Obtener todos los movimientos (antes y durante el período)
            var todosMovimientos = await movimientosQuery
                .Where(m => m.AsientoContable!.Fecha <= fechaHasta)
                .OrderBy(m => m.AsientoContable!.Fecha)
                .ThenBy(m => m.AsientoContable!.Numero)
                .ToListAsync();

            // Agrupar por cuenta
            var cuentasConMovimientos = todosMovimientos
                .GroupBy(m => new
                {
                    m.CuentaContableId,
                    CuentaCodigo = m.CuentaContable!.Codigo,
                    CuentaNombre = m.CuentaContable.Nombre
                })
                .Select(g => new
                {
                    g.Key.CuentaContableId,
                    g.Key.CuentaCodigo,
                    g.Key.CuentaNombre,
                    Movimientos = g.ToList()
                })
                .ToList();

            // Calcular saldos por cuenta
            var mayores = new List<MayorDTO>();

            foreach (var cuenta in cuentasConMovimientos)
            {
                // Saldo anterior (movimientos antes de fechaDesde)
                var movimientosAnteriores = cuenta.Movimientos
                    .Where(m => m.AsientoContable!.Fecha < fechaDesde)
                    .ToList();

                var saldoAnterior = movimientosAnteriores.Sum(m => m.Debe - m.Haber);

                // Movimientos del período
                var movimientosPeriodo = cuenta.Movimientos
                    .Where(m => m.AsientoContable!.Fecha >= fechaDesde
                        && m.AsientoContable.Fecha <= fechaHasta)
                    .ToList();

                var totalDebe = movimientosPeriodo.Sum(m => m.Debe);
                var totalHaber = movimientosPeriodo.Sum(m => m.Haber);
                var saldoFinal = saldoAnterior + totalDebe - totalHaber;

                // Solo incluir cuentas con movimientos o saldo anterior diferente de cero
                if (movimientosPeriodo.Any() || Math.Abs(saldoAnterior) > 0.01m)
                {
                    mayores.Add(new MayorDTO
                    {
                        CuentaId = cuenta.CuentaContableId,
                        CuentaCodigo = cuenta.CuentaCodigo,
                        CuentaNombre = cuenta.CuentaNombre,
                        SaldoAnterior = saldoAnterior,
                        TotalDebe = totalDebe,
                        TotalHaber = totalHaber,
                        SaldoFinal = saldoFinal
                    });
                }
            }

            // Ordenar por código de cuenta
            mayores = mayores.OrderBy(m => m.CuentaCodigo).ToList();

            // Calcular totales generales
            var resumen = new ResumenMayorDTO
            {
                TotalDebitos = mayores.Sum(m => m.TotalDebe),
                TotalCreditos = mayores.Sum(m => m.TotalHaber),
                SaldoTotal = mayores.Sum(m => m.SaldoFinal),
                Mayores = mayores
            };

            _logger.LogInformation("Generated mayor report for empresa {EmpresaId} from {FechaDesde} to {FechaHasta} with {Count} accounts",
                empresaId, fechaDesde, fechaHasta, mayores.Count);

            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating mayor report for empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene los movimientos detallados de una cuenta específica en un período.
    /// Incluye saldo acumulado (running balance) para cada movimiento.
    /// GET: api/mayorescontables/movimientos?empresaId={guid}&amp;cuentaId={guid}&amp;fechaDesde={date}&amp;fechaHasta={date}
    /// </summary>
    /// <param name="empresaId">ID de la empresa (multi-tenancy)</param>
    /// <param name="cuentaId">ID de la cuenta contable</param>
    /// <param name="fechaDesde">Fecha inicial del período</param>
    /// <param name="fechaHasta">Fecha final del período</param>
    /// <returns>Lista de MovimientoMayorDTO con saldo acumulado</returns>
    [HttpGet("movimientos")]
    public async Task<IActionResult> GetMovimientosMayorAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] Guid cuentaId,
        [FromQuery] DateTime fechaDesde,
        [FromQuery] DateTime fechaHasta)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            // Validar fechas
            if (fechaDesde > fechaHasta)
            {
                return BadRequest("La fecha desde no puede ser mayor que la fecha hasta.");
            }

            // Calcular saldo anterior (antes de fechaDesde)
            var saldoAnterior = await _context.MovimientosContables
                .Include(m => m.AsientoContable)
                .Where(m => m.CuentaContableId == cuentaId
                    && m.AsientoContable!.EmpresaId == empresaId
                    && m.AsientoContable.Estado == "APR"
                    && !m.AsientoContable.IsDeleted
                    && m.AsientoContable.Fecha < fechaDesde)
                .SumAsync(m => m.Debe - m.Haber);

            // Obtener movimientos del período
            var movimientos = await _context.MovimientosContables
                .Include(m => m.AsientoContable)
                .Include(m => m.CuentaContable)
                .Where(m => m.CuentaContableId == cuentaId
                    && m.AsientoContable!.EmpresaId == empresaId
                    && m.AsientoContable.Estado == "APR"
                    && !m.AsientoContable.IsDeleted
                    && m.AsientoContable.Fecha >= fechaDesde
                    && m.AsientoContable.Fecha <= fechaHasta)
                .OrderBy(m => m.AsientoContable!.Fecha)
                .ThenBy(m => m.AsientoContable!.Numero)
                .ThenBy(m => m.NumeroLinea)
                .Select(m => new
                {
                    m.Id,
                    m.AsientoContable!.Fecha,
                    m.AsientoContable.Numero,
                    m.AsientoContable.Concepto,
                    m.AsientoContable.Referencia,
                    m.Debe,
                    m.Haber
                })
                .ToListAsync();

            // Calcular saldo acumulado (running balance)
            var movimientosDetalle = new List<MovimientoMayorDTO>();
            var saldoAcumulado = saldoAnterior;

            foreach (var mov in movimientos)
            {
                saldoAcumulado += mov.Debe - mov.Haber;

                movimientosDetalle.Add(new MovimientoMayorDTO
                {
                    Id = mov.Id,
                    Fecha = mov.Fecha,
                    AsientoNumero = mov.Numero,
                    Concepto = mov.Concepto,
                    Referencia = mov.Referencia ?? string.Empty,
                    Debe = mov.Debe,
                    Haber = mov.Haber,
                    SaldoAcumulado = saldoAcumulado
                });
            }

            _logger.LogInformation("Retrieved {Count} movements for cuenta {CuentaId} from {FechaDesde} to {FechaHasta}",
                movimientosDetalle.Count, cuentaId, fechaDesde, fechaHasta);

            return Ok(movimientosDetalle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movements for cuenta {CuentaId}", cuentaId);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    #region Helper Methods

    /// <summary>
    /// Verifica si el usuario tiene acceso a la empresa (multi-tenant security).
    /// Los administradores tienen acceso a todas las empresas.
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

    #region Internal DTOs

    /// <summary>
    /// DTO para resumen del reporte de Libro Mayor.
    /// Contiene totales generales y detalle por cuenta.
    /// </summary>
    internal class ResumenMayorDTO
    {
        public decimal TotalDebitos { get; set; }
        public decimal TotalCreditos { get; set; }
        public decimal SaldoTotal { get; set; }
        public List<MayorDTO> Mayores { get; set; } = new();
    }

    /// <summary>
    /// DTO para el detalle de una cuenta en el Libro Mayor.
    /// Incluye saldo anterior, movimientos del período y saldo final.
    /// </summary>
    internal class MayorDTO
    {
        public Guid CuentaId { get; set; }
        public string CuentaCodigo { get; set; } = null!;
        public string CuentaNombre { get; set; } = null!;
        public decimal SaldoAnterior { get; set; }
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }
        public decimal SaldoFinal { get; set; }
    }

    /// <summary>
    /// DTO para movimiento individual dentro del mayor de una cuenta.
    /// Incluye saldo acumulado (running balance).
    /// </summary>
    internal class MovimientoMayorDTO
    {
        public Guid Id { get; set; }
        public DateTime Fecha { get; set; }
        public int AsientoNumero { get; set; }
        public string Concepto { get; set; } = null!;
        public string Referencia { get; set; } = null!;
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        public decimal SaldoAcumulado { get; set; }
    }

    #endregion
}
