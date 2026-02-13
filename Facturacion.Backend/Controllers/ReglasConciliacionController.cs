using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para gestión de reglas de conciliación bancaria automática.
/// Permite definir criterios para auto-matching de transacciones bancarias con movimientos del sistema.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ReglasConciliacionController : ControllerBase
{
    private readonly IReglaConciliacionRepository _repository;
    private readonly DataContext _context;
    private readonly ILogger<ReglasConciliacionController> _logger;

    public ReglasConciliacionController(
        IReglaConciliacionRepository repository,
        DataContext context,
        ILogger<ReglasConciliacionController> logger)
    {
        _repository = repository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las reglas de conciliación de una empresa.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de reglas ordenadas por prioridad</returns>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            _logger.LogInformation("GetByEmpresaAsync called for EmpresaId: {EmpresaId}", empresaId);

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                _logger.LogWarning("Access denied to EmpresaId: {EmpresaId} for user: {UserId}",
                    empresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var action = await _repository.GetByEmpresaAsync(empresaId);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to get conciliation rules for EmpresaId: {EmpresaId}: {Message}",
                    empresaId, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Retrieved {Count} conciliation rules for EmpresaId: {EmpresaId}",
                action.Result?.Count() ?? 0, empresaId);

            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conciliation rules for EmpresaId: {EmpresaId}", empresaId);
            return StatusCode(500, new ActionResponse<IEnumerable<ReglaConciliacion>>
            {
                WasSuccess = false,
                Message = $"Error al obtener reglas de conciliación: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Obtiene las reglas de conciliación para una cuenta bancaria específica.
    /// </summary>
    /// <param name="cuentaBancariaId">ID de la cuenta bancaria</param>
    /// <returns>Lista de reglas activas para la cuenta</returns>
    [HttpGet("cuenta/{cuentaBancariaId:guid}")]
    public async Task<IActionResult> GetByCuentaBancariaAsync(Guid cuentaBancariaId)
    {
        try
        {
            // Verificar que la cuenta existe y obtener su empresaId para verificar acceso
            var cuenta = await _context.CuentasBancarias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cuentaBancariaId);

            if (cuenta == null)
            {
                return NotFound(new ActionResponse<IEnumerable<ReglaConciliacion>>
                {
                    WasSuccess = false,
                    Message = "Cuenta bancaria no encontrada."
                });
            }

            if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
            {
                return Forbid();
            }

            var action = await _repository.GetByCuentaBancariaAsync(cuentaBancariaId);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to get conciliation rules for CuentaBancariaId: {CuentaBancariaId}: {Message}",
                    cuentaBancariaId, action.Message);
                return BadRequest(action);
            }

            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conciliation rules for CuentaBancariaId: {CuentaBancariaId}", cuentaBancariaId);
            return StatusCode(500, new ActionResponse<IEnumerable<ReglaConciliacion>>
            {
                WasSuccess = false,
                Message = $"Error al obtener reglas de conciliación: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Obtiene una regla de conciliación por ID.
    /// </summary>
    /// <param name="id">ID de la regla</param>
    /// <returns>Regla de conciliación</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var action = await _repository.GetAsync(id);

            if (!action.WasSuccess)
            {
                return NotFound(action);
            }

            if (!await TieneAccesoEmpresaAsync(action.Result!.EmpresaId))
            {
                return Forbid();
            }

            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conciliation rule: {Id}", id);
            return StatusCode(500, new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = false,
                Message = $"Error al obtener regla de conciliación: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Crea una nueva regla de conciliación.
    /// </summary>
    /// <param name="regla">Datos de la regla</param>
    /// <returns>Regla creada</returns>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] ReglaConciliacion regla)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ActionResponse<ReglaConciliacion>
                {
                    WasSuccess = false,
                    Message = "Datos inválidos. Verifique los campos requeridos."
                });
            }

            if (!await TieneAccesoEmpresaAsync(regla.EmpresaId))
            {
                return Forbid();
            }

            var action = await _repository.AddAsync(regla);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to create conciliation rule: {Message}", action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Created conciliation rule: {Id}, Name: {Nombre}", action.Result!.Id, action.Result.Nombre);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conciliation rule");
            return StatusCode(500, new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = false,
                Message = $"Error al crear la regla de conciliación: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Actualiza una regla de conciliación existente.
    /// </summary>
    /// <param name="id">ID de la regla</param>
    /// <param name="regla">Datos actualizados</param>
    /// <returns>Regla actualizada</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] ReglaConciliacion regla)
    {
        try
        {
            if (id != regla.Id)
            {
                return BadRequest(new ActionResponse<ReglaConciliacion>
                {
                    WasSuccess = false,
                    Message = "El ID de la regla no coincide."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ActionResponse<ReglaConciliacion>
                {
                    WasSuccess = false,
                    Message = "Datos inválidos. Verifique los campos requeridos."
                });
            }

            // Get existing to check access
            var existingAction = await _repository.GetAsync(id);
            if (!existingAction.WasSuccess)
            {
                return NotFound(existingAction);
            }

            if (!await TieneAccesoEmpresaAsync(existingAction.Result!.EmpresaId))
            {
                return Forbid();
            }

            var action = await _repository.UpdateAsync(regla);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to update conciliation rule {Id}: {Message}", id, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Updated conciliation rule: {Id}", id);
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conciliation rule: {Id}", id);
            return StatusCode(500, new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = false,
                Message = $"Error al actualizar la regla de conciliación: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Elimina una regla de conciliación.
    /// No permite eliminar si hay líneas conciliadas con esta regla.
    /// </summary>
    /// <param name="id">ID de la regla</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            // Get existing to check access
            var existingAction = await _repository.GetAsync(id);
            if (!existingAction.WasSuccess)
            {
                return NotFound(new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Regla de conciliación no encontrada."
                });
            }

            if (!await TieneAccesoEmpresaAsync(existingAction.Result!.EmpresaId))
            {
                return Forbid();
            }

            var action = await _repository.DeleteAsync(id);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to delete conciliation rule {Id}: {Message}", id, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Deleted conciliation rule: {Id}", id);
            return Ok(new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = "Regla de conciliación eliminada exitosamente."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conciliation rule: {Id}", id);
            return StatusCode(500, new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = $"Error al eliminar la regla de conciliación: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Activa o desactiva una regla de conciliación.
    /// </summary>
    /// <param name="id">ID de la regla</param>
    /// <returns>Regla actualizada</returns>
    [HttpPost("{id:guid}/toggle")]
    public async Task<IActionResult> ToggleAsync(Guid id)
    {
        try
        {
            // Get existing to check access
            var existingAction = await _repository.GetAsync(id);
            if (!existingAction.WasSuccess)
            {
                return NotFound(existingAction);
            }

            if (!await TieneAccesoEmpresaAsync(existingAction.Result!.EmpresaId))
            {
                return Forbid();
            }

            var action = await _repository.ToggleActivaAsync(id);

            if (!action.WasSuccess)
            {
                _logger.LogWarning("Repository failed to toggle conciliation rule {Id}: {Message}", id, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Toggled conciliation rule {Id} to {Estado}", id,
                action.Result!.Activa ? "activated" : "deactivated");
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling conciliation rule: {Id}", id);
            return StatusCode(500, new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = false,
                Message = $"Error al cambiar estado de la regla: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Aplica las reglas de conciliación a líneas de extracto pendientes.
    /// Ejecuta matching automático basado en los criterios definidos en las reglas activas.
    /// </summary>
    /// <param name="request">Solicitud de aplicación de reglas</param>
    /// <returns>Resultado con estadísticas de conciliación</returns>
    [HttpPost("aplicar")]
    public async Task<IActionResult> AplicarReglasAsync([FromBody] AplicarReglasRequest request)
    {
        try
        {
            _logger.LogInformation("AplicarReglasAsync called for EmpresaId: {EmpresaId}, ExtractoBancarioId: {ExtractoBancarioId}, CuentaBancariaId: {CuentaBancariaId}",
                request.EmpresaId, request.ExtractoBancarioId, request.CuentaBancariaId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for applying conciliation rules");
                return BadRequest(new ActionResponse<AplicarReglasResult>
                {
                    WasSuccess = false,
                    Message = "Datos inválidos."
                });
            }

            if (!await TieneAccesoEmpresaAsync(request.EmpresaId))
            {
                _logger.LogWarning("Access denied to apply rules for EmpresaId: {EmpresaId}, user: {UserId}",
                    request.EmpresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Forbid();
            }

            var resultado = new AplicarReglasResult();

            // Obtener reglas activas ordenadas por prioridad
            var reglas = await _context.ReglasConciliacion
                .Where(r => r.EmpresaId == request.EmpresaId && r.Activa)
                .OrderBy(r => r.Prioridad)
                .ToListAsync();

            if (!reglas.Any())
            {
                _logger.LogWarning("No active conciliation rules found for EmpresaId: {EmpresaId}", request.EmpresaId);
                return Ok(new ActionResponse<AplicarReglasResult>
                {
                    WasSuccess = true,
                    Result = resultado,
                    Message = "No hay reglas activas para aplicar."
                });
            }

            _logger.LogInformation("Found {Count} active conciliation rules for EmpresaId: {EmpresaId}",
                reglas.Count, request.EmpresaId);

            // Obtener líneas pendientes de conciliación
            var query = _context.LineasExtractoBancario
                .Include(l => l.ExtractoBancario)
                .Where(l => l.EmpresaId == request.EmpresaId &&
                           l.EstadoConciliacion == EstadosConciliacionLinea.Pendiente);

            // Filtrar por extracto si se especificó
            if (request.ExtractoBancarioId.HasValue)
            {
                query = query.Where(l => l.ExtractoBancarioId == request.ExtractoBancarioId.Value);
            }
            // Filtrar por cuenta si se especificó
            else if (request.CuentaBancariaId.HasValue)
            {
                query = query.Where(l => l.ExtractoBancario != null &&
                                        l.ExtractoBancario.CuentaBancariaId == request.CuentaBancariaId.Value);
            }

            var lineasPendientes = await query.ToListAsync();
            resultado.TotalLineasProcesadas = lineasPendientes.Count;

            // Obtener movimientos bancarios no conciliados para matching
            var movimientosQuery = _context.MovimientosBancarios
                .Where(m => m.EmpresaId == request.EmpresaId &&
                           m.Conciliado == false);

            if (request.CuentaBancariaId.HasValue)
            {
                movimientosQuery = movimientosQuery.Where(m => m.CuentaBancariaId == request.CuentaBancariaId.Value);
            }

            var movimientosDisponibles = await movimientosQuery.ToListAsync();

            // Aplicar reglas a cada línea pendiente
            foreach (var linea in lineasPendientes)
            {
                var mejorMatch = await BuscarMejorMatchAsync(linea, movimientosDisponibles, reglas);

                if (mejorMatch != null)
                {
                    // Verificar si cumple el umbral de confianza de la regla
                    if (mejorMatch.ConfianzaMatch >= mejorMatch.Regla.ConfianzaMinima)
                    {
                        // Si la regla permite auto-conciliar
                        if (mejorMatch.Regla.AutoConciliar)
                        {
                            linea.MovimientoBancarioId = mejorMatch.Movimiento.Id;
                            linea.EstadoConciliacion = EstadosConciliacionLinea.Conciliado;
                            linea.FechaConciliacion = DateTime.UtcNow;
                            linea.ConciliadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                            linea.ConciliacionAutomatica = true;
                            linea.ReglaConciliacionId = mejorMatch.Regla.Id;
                            linea.ConfianzaMatch = mejorMatch.ConfianzaMatch;
                            linea.NotaConciliacion = $"Conciliado automáticamente con regla: {mejorMatch.Regla.Nombre}";

                            // Marcar movimiento como conciliado
                            mejorMatch.Movimiento.Conciliado = true;
                            mejorMatch.Movimiento.FechaConciliacion = DateTime.UtcNow;

                            // Remover de disponibles
                            movimientosDisponibles.Remove(mejorMatch.Movimiento);

                            resultado.LineasConciliadas++;
                        }
                        else
                        {
                            // Solo marcar el match sugerido sin conciliar
                            linea.ConfianzaMatch = mejorMatch.ConfianzaMatch;
                            linea.ReglaConciliacionId = mejorMatch.Regla.Id;
                            linea.NotaConciliacion = $"Match sugerido por regla: {mejorMatch.Regla.Nombre} (Confianza: {mejorMatch.ConfianzaMatch:N2}%)";

                            resultado.LineasConSugerencia++;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            resultado.ReglasAplicadas = reglas.Count;
            resultado.MovimientosDisponibles = movimientosDisponibles.Count;

            _logger.LogInformation("Conciliation rules applied successfully. Processed: {TotalProcessed}, Reconciled: {Reconciled}, Suggested: {Suggested}",
                resultado.TotalLineasProcesadas, resultado.LineasConciliadas, resultado.LineasConSugerencia);

            return Ok(new ActionResponse<AplicarReglasResult>
            {
                WasSuccess = true,
                Result = resultado,
                Message = $"Procesamiento completado. {resultado.LineasConciliadas} línea(s) conciliadas automáticamente, {resultado.LineasConSugerencia} con sugerencias."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying conciliation rules for EmpresaId: {EmpresaId}", request.EmpresaId);
            return BadRequest(new ActionResponse<AplicarReglasResult>
            {
                WasSuccess = false,
                Message = $"Error al aplicar reglas de conciliación: {ex.Message}"
            });
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Busca el mejor match para una línea de extracto aplicando las reglas en orden de prioridad.
    /// </summary>
    private async Task<MatchResult?> BuscarMejorMatchAsync(
        LineaExtractoBancario linea,
        List<MovimientoBancario> movimientos,
        List<ReglaConciliacion> reglas)
    {
        MatchResult? mejorMatch = null;
        decimal mejorConfianza = 0;

        // Aplicar cada regla
        foreach (var regla in reglas)
        {
            // Si la regla es específica de cuenta, verificar que aplique
            if (regla.CuentaBancariaId.HasValue &&
                linea.ExtractoBancario?.CuentaBancariaId != regla.CuentaBancariaId.Value)
            {
                continue;
            }

            // Evaluar cada movimiento
            foreach (var movimiento in movimientos)
            {
                var confianza = CalcularConfianzaMatch(linea, movimiento, regla);

                if (confianza > mejorConfianza)
                {
                    mejorConfianza = confianza;
                    mejorMatch = new MatchResult
                    {
                        Movimiento = movimiento,
                        Regla = regla,
                        ConfianzaMatch = confianza
                    };
                }
            }

            // Si encontramos un match perfecto (100%), no seguir buscando
            if (mejorConfianza >= 100)
            {
                break;
            }
        }

        return mejorMatch;
    }

    /// <summary>
    /// Calcula el porcentaje de confianza del match entre una línea y un movimiento según una regla.
    /// </summary>
    private decimal CalcularConfianzaMatch(
        LineaExtractoBancario linea,
        MovimientoBancario movimiento,
        ReglaConciliacion regla)
    {
        decimal puntosObtenidos = 0;
        decimal puntosPosibles = 0;

        // Comparar Monto (peso: 40 puntos)
        if (regla.CompararMonto)
        {
            puntosPosibles += 40;

            var montoLinea = Math.Abs(linea.Monto);
            var montoMovimiento = Math.Abs(movimiento.Monto);
            var diferencia = Math.Abs(montoLinea - montoMovimiento);

            // Tolerancia absoluta
            if (diferencia <= regla.ToleranciaMonto)
            {
                puntosObtenidos += 40;
            }
            // Tolerancia porcentual
            else if (regla.ToleranciaPorcentaje.HasValue)
            {
                var toleranciaCalculada = montoMovimiento * (regla.ToleranciaPorcentaje.Value / 100);
                if (diferencia <= toleranciaCalculada)
                {
                    // Puntos proporcionales según qué tan cerca está
                    var factorCercania = 1 - (diferencia / toleranciaCalculada);
                    puntosObtenidos += 40 * factorCercania;
                }
            }
        }

        // Comparar Fecha (peso: 30 puntos)
        if (regla.CompararFecha)
        {
            puntosPosibles += 30;

            var fechaLinea = linea.Fecha.Date;
            var fechaMovimiento = movimiento.Fecha.Date;
            var diferenciaDias = Math.Abs((fechaLinea - fechaMovimiento).Days);

            if (diferenciaDias <= regla.ToleranciaFechaDias)
            {
                // Puntos proporcionales: mismo día = 30, tolerancia máxima = 15
                var factor = 1 - ((decimal)diferenciaDias / (regla.ToleranciaFechaDias + 1));
                puntosObtenidos += 30 * factor;
            }
        }

        // Comparar Referencia (peso: 20 puntos)
        if (regla.CompararReferencia && !string.IsNullOrEmpty(regla.PatronReferencia))
        {
            puntosPosibles += 20;

            // Buscar coincidencia de referencia en descripción usando patrón regex
            if (!string.IsNullOrEmpty(linea.ReferenciaExterna) &&
                !string.IsNullOrEmpty(movimiento.NumeroReferencia))
            {
                if (linea.ReferenciaExterna.Contains(movimiento.NumeroReferencia, StringComparison.OrdinalIgnoreCase) ||
                    movimiento.NumeroReferencia.Contains(linea.ReferenciaExterna, StringComparison.OrdinalIgnoreCase))
                {
                    puntosObtenidos += 20;
                }
            }
        }

        // Comparar Descripción (peso: 10 puntos)
        if (regla.CompararDescripcion && !string.IsNullOrEmpty(regla.PalabrasClaveDescripcion))
        {
            puntosPosibles += 10;

            var palabrasClave = regla.PalabrasClaveDescripcion
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLower())
                .ToList();

            if (palabrasClave.Any())
            {
                var descripcionLinea = linea.Descripcion?.ToLower() ?? "";
                var descripcionMovimiento = movimiento.Descripcion?.ToLower() ?? "";

                var coincidencias = palabrasClave.Count(p =>
                    descripcionLinea.Contains(p) || descripcionMovimiento.Contains(p));

                // Puntos proporcionales a coincidencias
                puntosObtenidos += 10 * ((decimal)coincidencias / palabrasClave.Count);
            }
        }

        // Calcular porcentaje de confianza
        if (puntosPosibles == 0)
        {
            return 0;
        }

        return (puntosObtenidos / puntosPosibles) * 100;
    }

    /// <summary>
    /// Verifica si el usuario tiene acceso a la empresa especificada.
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // SuperUser tiene acceso a todas las empresas
        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        // Verificar relación usuario-empresa
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }

    #endregion
}

#region DTOs and Helper Classes

/// <summary>
/// Request para aplicar reglas de conciliación.
/// </summary>
public class AplicarReglasRequest
{
    [Required]
    public Guid EmpresaId { get; set; }

    /// <summary>
    /// Opcional: Aplicar solo a un extracto específico
    /// </summary>
    public Guid? ExtractoBancarioId { get; set; }

    /// <summary>
    /// Opcional: Aplicar solo a una cuenta bancaria específica
    /// </summary>
    public Guid? CuentaBancariaId { get; set; }
}

/// <summary>
/// Resultado de aplicar reglas de conciliación.
/// </summary>
public class AplicarReglasResult
{
    public int TotalLineasProcesadas { get; set; }
    public int LineasConciliadas { get; set; }
    public int LineasConSugerencia { get; set; }
    public int ReglasAplicadas { get; set; }
    public int MovimientosDisponibles { get; set; }
}

/// <summary>
/// Resultado de matching interno.
/// </summary>
internal class MatchResult
{
    public MovimientoBancario Movimiento { get; set; } = null!;
    public ReglaConciliacion Regla { get; set; } = null!;
    public decimal ConfianzaMatch { get; set; }
}

#endregion
