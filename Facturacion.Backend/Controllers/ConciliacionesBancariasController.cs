using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para la gestión de conciliaciones bancarias.
/// Permite crear, actualizar, conciliar movimientos con extractos y cerrar conciliaciones.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ConciliacionesBancariasController : ControllerBase
{
    private readonly IConciliacionBancariaRepository _conciliacionBancariaRepository;
    private readonly DataContext _context;
    private readonly ILogger<ConciliacionesBancariasController> _logger;

    public ConciliacionesBancariasController(
        IConciliacionBancariaRepository conciliacionBancariaRepository,
        DataContext context,
        ILogger<ConciliacionesBancariasController> logger)
    {
        _conciliacionBancariaRepository = conciliacionBancariaRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las conciliaciones de una cuenta bancaria.
    /// </summary>
    [HttpGet("cuenta/{cuentaBancariaId:guid}")]
    public async Task<IActionResult> GetByCuentaBancariaAsync(Guid cuentaBancariaId)
    {
        try
        {
            // Verificar que la cuenta bancaria existe y el usuario tiene acceso
            var cuenta = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.Id == cuentaBancariaId && !c.IsDeleted);

            if (cuenta == null)
            {
                return NotFound("La cuenta bancaria no existe.");
            }

            if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
            {
                return Forbid();
            }

            var conciliacionesResponse = await _conciliacionBancariaRepository.GetByCuentaBancariaAsync(cuentaBancariaId);

            if (!conciliacionesResponse.WasSuccess)
            {
                return BadRequest(conciliacionesResponse.Message);
            }

            // Proyectar a formato personalizado para la respuesta
            var conciliaciones = conciliacionesResponse.Result?.Select(c => new
            {
                c.Id,
                c.Numero,
                c.CuentaBancariaId,
                CuentaBancaria = c.CuentaBancaria?.CuentaCompleta,
                c.Mes,
                c.Anio,
                c.Periodo,
                c.FechaInicio,
                c.FechaFin,
                c.SaldoInicialLibros,
                c.SaldoFinalLibros,
                c.SaldoEstadoCuenta,
                c.DepositosEnTransito,
                c.ChequesEnTransito,
                c.NotasCredito,
                c.NotasDebito,
                c.Diferencia,
                c.Estado,
                c.EstadoDescripcion,
                c.EstaConciliada,
                c.FechaConciliacion,
                ConciliadoPor = c.ConciliadoPor?.FullName,
                c.Observaciones,
                c.FechaCreacion
            }).ToList();

            return Ok(conciliaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conciliaciones de cuenta {CuentaId}", cuentaBancariaId);
            return StatusCode(500, $"Error al obtener las conciliaciones: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el detalle completo de una conciliación bancaria.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var conciliacionResponse = await _conciliacionBancariaRepository.GetAsync(id);

            if (!conciliacionResponse.WasSuccess || conciliacionResponse.Result == null)
            {
                return NotFound("La conciliación no existe.");
            }

            var conciliacion = conciliacionResponse.Result;

            if (!await TieneAccesoEmpresaAsync(conciliacion.EmpresaId))
            {
                return Forbid();
            }

            // Obtener movimientos y extractos relacionados (consulta personalizada con _context)
            var movimientos = await _context.MovimientosBancarios
                .Where(m => m.ConciliacionId == id)
                .Include(m => m.LineasExtracto)
                .OrderBy(m => m.Fecha)
                .ToListAsync();

            var extractos = await _context.ExtractosBancarios
                .Where(e => e.ConciliacionBancariaId == id)
                .Include(e => e.Lineas)
                .OrderBy(e => e.FechaInicio)
                .ToListAsync();

            var resultado = new
            {
                Conciliacion = conciliacion,
                Movimientos = movimientos,
                Extractos = extractos
            };

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conciliación {Id}", id);
            return StatusCode(500, $"Error al obtener la conciliación: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene los movimientos y líneas de extracto pendientes de conciliar para una cuenta.
    /// </summary>
    [HttpGet("cuenta/{cuentaBancariaId:guid}/pendientes")]
    public async Task<IActionResult> GetPendientesAsync(
        Guid cuentaBancariaId,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            // Verificar que la cuenta bancaria existe y el usuario tiene acceso
            var cuenta = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.Id == cuentaBancariaId && !c.IsDeleted);

            if (cuenta == null)
            {
                return NotFound("La cuenta bancaria no existe.");
            }

            if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
            {
                return Forbid();
            }

            // Movimientos bancarios no conciliados
            var queryMovimientos = _context.MovimientosBancarios
                .Where(m => m.CuentaBancariaId == cuentaBancariaId &&
                           !m.Conciliado &&
                           m.Estado == EstadosMovimientoBancario.Registrado);

            if (fechaInicio.HasValue)
            {
                queryMovimientos = queryMovimientos.Where(m => m.Fecha >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                queryMovimientos = queryMovimientos.Where(m => m.Fecha <= fechaFin.Value);
            }

            var movimientosPendientes = await queryMovimientos
                .OrderBy(m => m.Fecha)
                .Select(m => new
                {
                    m.Id,
                    m.Numero,
                    m.Fecha,
                    m.TipoMovimiento,
                    m.TipoMovimientoDescripcion,
                    m.Naturaleza,
                    m.NaturalezaDescripcion,
                    m.Monto,
                    m.Beneficiario,
                    m.NumeroReferencia,
                    m.Descripcion,
                    m.Conciliado
                })
                .ToListAsync();

            // Líneas de extracto no conciliadas
            var queryLineas = _context.LineasExtractoBancario
                .Where(l => l.ExtractoBancario!.CuentaBancariaId == cuentaBancariaId &&
                           l.EstadoConciliacion == EstadosConciliacionLinea.Pendiente);

            if (fechaInicio.HasValue)
            {
                queryLineas = queryLineas.Where(l => l.Fecha >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                queryLineas = queryLineas.Where(l => l.Fecha <= fechaFin.Value);
            }

            var lineasPendientes = await queryLineas
                .Include(l => l.ExtractoBancario)
                .OrderBy(l => l.Fecha)
                .Select(l => new
                {
                    l.Id,
                    l.ExtractoBancarioId,
                    ExtractoNumero = l.ExtractoBancario!.Numero,
                    l.NumeroLinea,
                    l.Fecha,
                    l.FechaValor,
                    l.ReferenciaExterna,
                    l.NumeroDocumento,
                    l.Descripcion,
                    l.Debito,
                    l.Credito,
                    l.Monto,
                    l.SaldoAcumulado,
                    l.TipoTransaccion,
                    l.TipoTransaccionDescripcion,
                    l.EstadoConciliacion,
                    l.EstadoConciliacionDescripcion,
                    l.EsCredito,
                    l.EsDebito
                })
                .ToListAsync();

            var resultado = new
            {
                MovimientosPendientes = movimientosPendientes,
                LineasPendientes = lineasPendientes
            };

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener items pendientes: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una nueva conciliación bancaria.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] ConciliacionBancaria conciliacion)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(conciliacion.EmpresaId))
        {
            return Forbid();
        }

        try
        {
            // Verificar que la cuenta bancaria existe
            var cuenta = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.Id == conciliacion.CuentaBancariaId && !c.IsDeleted);

            if (cuenta == null)
            {
                return BadRequest("La cuenta bancaria no existe.");
            }

            // Verificar que no exista otra conciliación para el mismo periodo
            var existenteResponse = await _conciliacionBancariaRepository.GetByPeriodoAsync(
                conciliacion.CuentaBancariaId,
                conciliacion.Mes,
                conciliacion.Anio);

            if (existenteResponse.WasSuccess && existenteResponse.Result != null &&
                existenteResponse.Result.Estado != EstadosConciliacionBancaria.Cerrada)
            {
                return BadRequest($"Ya existe una conciliación activa para el periodo {conciliacion.Mes:00}/{conciliacion.Anio}.");
            }

            // Calcular saldos iniciales desde la última conciliación cerrada (consulta compleja con _context)
            var ultimaConciliacion = await _context.ConciliacionesBancarias
                .Where(c => c.CuentaBancariaId == conciliacion.CuentaBancariaId &&
                           c.Estado == EstadosConciliacionBancaria.Cerrada)
                .OrderByDescending(c => c.FechaFin)
                .FirstOrDefaultAsync();

            if (ultimaConciliacion != null)
            {
                conciliacion.SaldoInicialLibros = ultimaConciliacion.SaldoFinalLibros;
            }
            else
            {
                conciliacion.SaldoInicialLibros = cuenta.SaldoInicial;
            }

            // Asignar valores
            conciliacion.Id = Guid.NewGuid();
            conciliacion.FechaCreacion = DateTime.Now;
            conciliacion.CreadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            conciliacion.Estado = EstadosConciliacionBancaria.Pendiente;
            conciliacion.Diferencia = 0; // Se calculará al conciliar

            var addResponse = await _conciliacionBancariaRepository.AddAsync(conciliacion);

            if (!addResponse.WasSuccess)
            {
                return BadRequest(addResponse.Message);
            }

            return Ok(addResponse.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear conciliación bancaria");
            return StatusCode(500, $"Error al crear la conciliación: {ex.Message}");
        }
    }

    /// <summary>
    /// Concilia un movimiento bancario con una línea de extracto.
    /// </summary>
    [HttpPost("{id:guid}/conciliar")]
    public async Task<IActionResult> ConciliarAsync(
        Guid id,
        [FromBody] ConciliarMovimientoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Verificar que la conciliación existe
            var conciliacion = await _context.ConciliacionesBancarias
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conciliacion == null)
            {
                return NotFound("La conciliación no existe.");
            }

            if (!await TieneAccesoEmpresaAsync(conciliacion.EmpresaId))
            {
                return Forbid();
            }

            // Verificar que no esté cerrada
            if (conciliacion.Estado == EstadosConciliacionBancaria.Cerrada)
            {
                return BadRequest("No se puede modificar una conciliación cerrada.");
            }

            // Verificar que el movimiento existe
            var movimiento = await _context.MovimientosBancarios
                .FirstOrDefaultAsync(m => m.Id == request.MovimientoBancarioId);

            if (movimiento == null)
            {
                return BadRequest("El movimiento bancario no existe.");
            }

            // Verificar que la línea de extracto existe
            var lineaExtracto = await _context.LineasExtractoBancario
                .FirstOrDefaultAsync(l => l.Id == request.LineaExtractoBancarioId);

            if (lineaExtracto == null)
            {
                return BadRequest("La línea de extracto no existe.");
            }

            // Verificar que no estén ya conciliados
            if (movimiento.Conciliado)
            {
                return BadRequest("El movimiento ya está conciliado.");
            }

            if (lineaExtracto.EstadoConciliacion == EstadosConciliacionLinea.Conciliado)
            {
                return BadRequest("La línea de extracto ya está conciliada.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var fechaConciliacion = DateTime.Now;

            // Marcar movimiento como conciliado
            movimiento.Conciliado = true;
            movimiento.FechaConciliacion = fechaConciliacion;
            movimiento.ConciliacionId = id;
            movimiento.Estado = EstadosMovimientoBancario.Conciliado;
            movimiento.FechaModificacion = fechaConciliacion;
            movimiento.ModificadoPorId = userId;

            // Marcar línea de extracto como conciliada
            lineaExtracto.EstadoConciliacion = EstadosConciliacionLinea.Conciliado;
            lineaExtracto.MovimientoBancarioId = request.MovimientoBancarioId;
            lineaExtracto.FechaConciliacion = fechaConciliacion;
            lineaExtracto.ConciliadoPorId = userId;
            lineaExtracto.NotaConciliacion = request.Nota;

            _context.MovimientosBancarios.Update(movimiento);
            _context.LineasExtractoBancario.Update(lineaExtracto);

            // Recalcular diferencia de la conciliación
            await RecalcularDiferenciaAsync(id);

            // Actualizar estado de la conciliación
            var movimientosPendientes = await _context.MovimientosBancarios
                .Where(m => m.CuentaBancariaId == conciliacion.CuentaBancariaId &&
                           m.Fecha >= conciliacion.FechaInicio &&
                           m.Fecha <= conciliacion.FechaFin &&
                           !m.Conciliado &&
                           m.Estado == EstadosMovimientoBancario.Registrado)
                .CountAsync();

            var lineasPendientes = await _context.LineasExtractoBancario
                .Where(l => l.ExtractoBancario!.CuentaBancariaId == conciliacion.CuentaBancariaId &&
                           l.Fecha >= conciliacion.FechaInicio &&
                           l.Fecha <= conciliacion.FechaFin &&
                           l.EstadoConciliacion == EstadosConciliacionLinea.Pendiente)
                .CountAsync();

            if (movimientosPendientes == 0 && lineasPendientes == 0)
            {
                conciliacion.Estado = EstadosConciliacionBancaria.Conciliada;
            }
            else
            {
                conciliacion.Estado = EstadosConciliacionBancaria.Parcial;
            }

            conciliacion.FechaModificacion = fechaConciliacion;
            conciliacion.ModificadoPorId = userId;

            _context.ConciliacionesBancarias.Update(conciliacion);
            await _context.SaveChangesAsync();

            return Ok(new ActionResponse<string>
            {
                WasSuccess = true,
                Message = "Movimiento conciliado exitosamente.",
                Result = "OK"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al conciliar movimiento: {ex.Message}");
        }
    }

    /// <summary>
    /// Desconcilia un movimiento bancario previamente conciliado.
    /// </summary>
    [HttpPost("{id:guid}/desconciliar")]
    public async Task<IActionResult> DesconciliarAsync(
        Guid id,
        [FromBody] DesconciliarMovimientoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Verificar que la conciliación existe
            var conciliacion = await _context.ConciliacionesBancarias
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conciliacion == null)
            {
                return NotFound("La conciliación no existe.");
            }

            if (!await TieneAccesoEmpresaAsync(conciliacion.EmpresaId))
            {
                return Forbid();
            }

            // Verificar que no esté cerrada
            if (conciliacion.Estado == EstadosConciliacionBancaria.Cerrada)
            {
                return BadRequest("No se puede modificar una conciliación cerrada.");
            }

            // Verificar que el movimiento existe
            var movimiento = await _context.MovimientosBancarios
                .Include(m => m.LineasExtracto)
                .FirstOrDefaultAsync(m => m.Id == request.MovimientoBancarioId);

            if (movimiento == null)
            {
                return BadRequest("El movimiento bancario no existe.");
            }

            if (!movimiento.Conciliado)
            {
                return BadRequest("El movimiento no está conciliado.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Desmarcar movimiento
            movimiento.Conciliado = false;
            movimiento.FechaConciliacion = null;
            movimiento.ConciliacionId = null;
            movimiento.Estado = EstadosMovimientoBancario.Registrado;
            movimiento.FechaModificacion = DateTime.Now;
            movimiento.ModificadoPorId = userId;

            // Desmarcar líneas de extracto asociadas
            if (movimiento.LineasExtracto != null)
            {
                foreach (var linea in movimiento.LineasExtracto)
                {
                    linea.EstadoConciliacion = EstadosConciliacionLinea.Pendiente;
                    linea.MovimientoBancarioId = null;
                    linea.FechaConciliacion = null;
                    linea.ConciliadoPorId = null;
                    linea.NotaConciliacion = null;
                    _context.LineasExtractoBancario.Update(linea);
                }
            }

            _context.MovimientosBancarios.Update(movimiento);

            // Recalcular diferencia
            await RecalcularDiferenciaAsync(id);

            // Actualizar estado de la conciliación
            conciliacion.Estado = EstadosConciliacionBancaria.Parcial;
            conciliacion.FechaModificacion = DateTime.Now;
            conciliacion.ModificadoPorId = userId;

            _context.ConciliacionesBancarias.Update(conciliacion);
            await _context.SaveChangesAsync();

            return Ok(new ActionResponse<string>
            {
                WasSuccess = true,
                Message = "Conciliación revertida exitosamente.",
                Result = "OK"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al desconciliar movimiento: {ex.Message}");
        }
    }

    /// <summary>
    /// Cierra y finaliza una conciliación bancaria.
    /// Una vez cerrada, no se puede modificar.
    /// </summary>
    [HttpPost("{id:guid}/cerrar")]
    public async Task<IActionResult> CerrarAsync(Guid id)
    {
        try
        {
            var conciliacionResponse = await _conciliacionBancariaRepository.GetAsync(id);

            if (!conciliacionResponse.WasSuccess || conciliacionResponse.Result == null)
            {
                return NotFound("La conciliación no existe.");
            }

            var conciliacion = conciliacionResponse.Result;

            if (!await TieneAccesoEmpresaAsync(conciliacion.EmpresaId))
            {
                return Forbid();
            }

            if (conciliacion.Estado == EstadosConciliacionBancaria.Cerrada)
            {
                return BadRequest("La conciliación ya está cerrada.");
            }

            // Verificar si hay diferencias significativas
            if (Math.Abs(conciliacion.Diferencia) >= 0.01m)
            {
                return BadRequest($"No se puede cerrar la conciliación con una diferencia de {conciliacion.Diferencia:N2}. Debe conciliar todos los movimientos.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Actualizar extractos asociados (lógica compleja con _context)
            var extractos = await _context.ExtractosBancarios
                .Where(e => e.ConciliacionBancariaId == id)
                .ToListAsync();

            var fechaCierre = DateTime.Now;
            foreach (var extracto in extractos)
            {
                extracto.Estado = EstadosExtractoBancario.Cerrado;
                extracto.FechaModificacion = fechaCierre;
                extracto.ModificadoPorId = userId;
                _context.ExtractosBancarios.Update(extracto);
            }

            await _context.SaveChangesAsync();

            // Cerrar conciliación usando repository
            var cerrarResponse = await _conciliacionBancariaRepository.CerrarConciliacionAsync(id, userId!);

            if (!cerrarResponse.WasSuccess)
            {
                return BadRequest(cerrarResponse.Message);
            }

            return Ok(new ActionResponse<ConciliacionBancaria>
            {
                WasSuccess = true,
                Message = "Conciliación cerrada exitosamente.",
                Result = cerrarResponse.Result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar conciliación {Id}", id);
            return StatusCode(500, $"Error al cerrar la conciliación: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina (soft delete) una conciliación bancaria.
    /// Solo se pueden eliminar conciliaciones en estado Pendiente.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var conciliacionResponse = await _conciliacionBancariaRepository.GetAsync(id);

            if (!conciliacionResponse.WasSuccess || conciliacionResponse.Result == null)
            {
                return NotFound("La conciliación no existe.");
            }

            var conciliacion = conciliacionResponse.Result;

            if (!await TieneAccesoEmpresaAsync(conciliacion.EmpresaId))
            {
                return Forbid();
            }

            // Validaciones (las hace el repository también, pero las verificamos aquí)
            if (conciliacion.Estado != EstadosConciliacionBancaria.Pendiente)
            {
                return BadRequest("Solo se pueden eliminar conciliaciones en estado Pendiente.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deleteResponse = await _conciliacionBancariaRepository.DeleteAsync(id, userId!);

            if (!deleteResponse.WasSuccess)
            {
                return BadRequest(deleteResponse.Message);
            }

            return Ok(new ActionResponse<string>
            {
                WasSuccess = true,
                Message = "Conciliación eliminada exitosamente.",
                Result = "OK"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar conciliación {Id}", id);
            return StatusCode(500, $"Error al eliminar la conciliación: {ex.Message}");
        }
    }

    /// <summary>
    /// Recalcula la diferencia de una conciliación bancaria.
    /// </summary>
    private async Task RecalcularDiferenciaAsync(Guid conciliacionId)
    {
        var conciliacion = await _context.ConciliacionesBancarias
            .FirstOrDefaultAsync(c => c.Id == conciliacionId);

        if (conciliacion == null)
        {
            return;
        }

        // Calcular totales de movimientos conciliados en el periodo
        var movimientosConciliados = await _context.MovimientosBancarios
            .Where(m => m.CuentaBancariaId == conciliacion.CuentaBancariaId &&
                       m.Fecha >= conciliacion.FechaInicio &&
                       m.Fecha <= conciliacion.FechaFin &&
                       m.Conciliado)
            .ToListAsync();

        var totalCreditos = movimientosConciliados
            .Where(m => m.Naturaleza == NaturalezaMovimiento.Credito)
            .Sum(m => m.Monto);

        var totalDebitos = movimientosConciliados
            .Where(m => m.Naturaleza == NaturalezaMovimiento.Debito)
            .Sum(m => m.Monto);

        // Calcular saldo final según libros
        conciliacion.SaldoFinalLibros = conciliacion.SaldoInicialLibros + totalCreditos - totalDebitos;

        // Calcular movimientos en tránsito
        var depositosEnTransito = await _context.MovimientosBancarios
            .Where(m => m.CuentaBancariaId == conciliacion.CuentaBancariaId &&
                       m.Fecha <= conciliacion.FechaFin &&
                       !m.Conciliado &&
                       m.Naturaleza == NaturalezaMovimiento.Credito &&
                       m.Estado == EstadosMovimientoBancario.Registrado)
            .SumAsync(m => m.Monto);

        var chequesEnTransito = await _context.MovimientosBancarios
            .Where(m => m.CuentaBancariaId == conciliacion.CuentaBancariaId &&
                       m.Fecha <= conciliacion.FechaFin &&
                       !m.Conciliado &&
                       m.Naturaleza == NaturalezaMovimiento.Debito &&
                       m.Estado == EstadosMovimientoBancario.Registrado)
            .SumAsync(m => m.Monto);

        conciliacion.DepositosEnTransito = depositosEnTransito;
        conciliacion.ChequesEnTransito = chequesEnTransito;

        // Calcular diferencia
        // Fórmula: Saldo Estado Cuenta + Depósitos en Tránsito - Cheques en Tránsito + Notas Crédito - Notas Débito = Saldo Libros
        conciliacion.Diferencia = conciliacion.SaldoFinalLibros - conciliacion.SaldoConciliado;

        _context.ConciliacionesBancarias.Update(conciliacion);
    }

    /// <summary>
    /// Verifica si el usuario tiene acceso a la empresa.
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // SuperUser tiene acceso a todo
        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a esta empresa
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}

/// <summary>
/// Request para conciliar un movimiento con una línea de extracto.
/// </summary>
public class ConciliarMovimientoRequest
{
    public Guid MovimientoBancarioId { get; set; }
    public Guid LineaExtractoBancarioId { get; set; }
    public string? Nota { get; set; }
}

/// <summary>
/// Request para desconciliar un movimiento.
/// </summary>
public class DesconciliarMovimientoRequest
{
    public Guid MovimientoBancarioId { get; set; }
}
