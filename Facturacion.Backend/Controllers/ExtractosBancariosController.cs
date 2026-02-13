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
/// Controlador para gestionar extractos bancarios importados y su conciliación.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ExtractosBancariosController : ControllerBase
{
    private readonly IExtractoBancarioRepository _extractoBancarioRepository;
    private readonly DataContext _context;
    private readonly ILogger<ExtractosBancariosController> _logger;

    public ExtractosBancariosController(
        IExtractoBancarioRepository extractoBancarioRepository,
        DataContext context,
        ILogger<ExtractosBancariosController> logger)
    {
        _extractoBancarioRepository = extractoBancarioRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los extractos bancarios de una cuenta específica.
    /// </summary>
    /// <param name="cuentaBancariaId">ID de la cuenta bancaria</param>
    /// <returns>Lista de extractos bancarios</returns>
    [HttpGet("cuenta/{cuentaBancariaId:guid}")]
    public async Task<IActionResult> GetByCuentaAsync(Guid cuentaBancariaId)
    {
        try
        {
            // Verificar que la cuenta existe y obtener su EmpresaId
            var cuenta = await _context.CuentasBancarias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cuentaBancariaId && !c.IsDeleted);

            if (cuenta == null)
            {
                return NotFound("Cuenta bancaria no encontrada.");
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
            {
                return Forbid();
            }

            var extractosResponse = await _extractoBancarioRepository.GetByCuentaBancariaAsync(cuentaBancariaId);

            if (!extractosResponse.WasSuccess)
            {
                return BadRequest(new ActionResponse<object>
                {
                    WasSuccess = false,
                    Message = extractosResponse.Message
                });
            }

            // Proyectar a formato personalizado para la respuesta
            var extractos = extractosResponse.Result?.Select(e => new
            {
                e.Id,
                e.Numero,
                e.FechaInicio,
                e.FechaFin,
                e.SaldoInicial,
                e.SaldoFinal,
                e.TotalCreditos,
                e.TotalDebitos,
                e.CantidadTransacciones,
                e.FechaImportacion,
                e.ArchivoOrigen,
                e.FormatoArchivo,
                e.Estado,
                e.EstadoDescripcion,
                e.Periodo,
                CantidadLineas = e.Lineas?.Count ?? 0,
                LineasConciliadas = e.Lineas?.Count(l => l.EstadoConciliacion == EstadosConciliacionLinea.Conciliado) ?? 0,
                LineasPendientes = e.Lineas?.Count(l => l.EstadoConciliacion == EstadosConciliacionLinea.Pendiente) ?? 0,
                CuentaBancaria = new
                {
                    e.CuentaBancaria?.NumeroCuenta,
                    e.CuentaBancaria?.Nombre,
                    e.CuentaBancaria?.Moneda
                }
            }).ToList();

            return Ok(new ActionResponse<object>
            {
                WasSuccess = true,
                Result = extractos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo extractos de cuenta {CuentaId}", cuentaBancariaId);
            return StatusCode(500, new ActionResponse<object>
            {
                WasSuccess = false,
                Message = $"Error interno del servidor: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Obtiene un extracto bancario específico con todas sus líneas.
    /// </summary>
    /// <param name="id">ID del extracto bancario</param>
    /// <returns>Extracto bancario con sus líneas</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var extractoResponse = await _extractoBancarioRepository.GetAsync(id);

            if (!extractoResponse.WasSuccess || extractoResponse.Result == null)
            {
                return NotFound(new ActionResponse<ExtractoBancario>
                {
                    WasSuccess = false,
                    Message = "Extracto bancario no encontrado."
                });
            }

            var extracto = extractoResponse.Result;

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(extracto.EmpresaId))
            {
                return Forbid();
            }

            return Ok(new ActionResponse<ExtractoBancario>
            {
                WasSuccess = true,
                Result = extracto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo extracto {ExtractoId}", id);
            return StatusCode(500, new ActionResponse<ExtractoBancario>
            {
                WasSuccess = false,
                Message = $"Error interno del servidor: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Crea/importa un nuevo extracto bancario con sus líneas.
    /// </summary>
    /// <param name="extracto">Datos del extracto bancario</param>
    /// <returns>Extracto bancario creado</returns>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] ExtractoBancario extracto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ActionResponse<ExtractoBancario>
                {
                    WasSuccess = false,
                    Message = "Datos del extracto bancario inválidos.",
                    Result = null
                });
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(extracto.EmpresaId))
            {
                return Forbid();
            }

            // Verificar que la cuenta bancaria existe y pertenece a la empresa
            var cuenta = await _context.CuentasBancarias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == extracto.CuentaBancariaId &&
                                         c.EmpresaId == extracto.EmpresaId &&
                                         !c.IsDeleted);

            if (cuenta == null)
            {
                return BadRequest(new ActionResponse<ExtractoBancario>
                {
                    WasSuccess = false,
                    Message = "Cuenta bancaria no encontrada o no pertenece a la empresa."
                });
            }

            // Asignar valores de auditoría
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            extracto.Id = Guid.NewGuid();
            extracto.FechaCreacion = DateTime.UtcNow;
            extracto.CreadoPorId = userId;
            extracto.FechaImportacion = DateTime.UtcNow;
            extracto.Estado = EstadosExtractoBancario.Pendiente;

            // Procesar las líneas si las hay
            if (extracto.Lineas != null && extracto.Lineas.Any())
            {
                int numeroLinea = 1;
                foreach (var linea in extracto.Lineas)
                {
                    linea.Id = Guid.NewGuid();
                    linea.EmpresaId = extracto.EmpresaId;
                    linea.ExtractoBancarioId = extracto.Id;
                    linea.NumeroLinea = numeroLinea++;
                    linea.EstadoConciliacion = EstadosConciliacionLinea.Pendiente;
                    linea.ConciliacionAutomatica = false;
                }

                // Actualizar totales del extracto
                extracto.CantidadTransacciones = extracto.Lineas.Count;
                extracto.TotalCreditos = extracto.Lineas.Sum(l => l.Credito);
                extracto.TotalDebitos = extracto.Lineas.Sum(l => l.Debito);
            }

            var addResponse = await _extractoBancarioRepository.AddAsync(extracto);

            if (!addResponse.WasSuccess)
            {
                return BadRequest(new ActionResponse<ExtractoBancario>
                {
                    WasSuccess = false,
                    Message = addResponse.Message
                });
            }

            _logger.LogInformation("Extracto bancario {Numero} creado para cuenta {CuentaId} por usuario {UserId}",
                extracto.Numero, extracto.CuentaBancariaId, userId);

            return Ok(new ActionResponse<ExtractoBancario>
            {
                WasSuccess = true,
                Result = addResponse.Result,
                Message = "Extracto bancario creado exitosamente."
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear extracto bancario");
            return StatusCode(500, new ActionResponse<ExtractoBancario>
            {
                WasSuccess = false,
                Message = $"Error al guardar el extracto bancario: {ex.InnerException?.Message ?? ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando extracto bancario");
            return StatusCode(500, new ActionResponse<ExtractoBancario>
            {
                WasSuccess = false,
                Message = $"Error interno del servidor: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Agrega líneas adicionales a un extracto bancario existente.
    /// </summary>
    /// <param name="id">ID del extracto bancario</param>
    /// <param name="lineas">Lista de líneas a agregar</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("{id:guid}/lineas")]
    public async Task<IActionResult> PostLineasAsync(Guid id, [FromBody] List<LineaExtractoBancario> lineas)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ActionResponse<object>
                {
                    WasSuccess = false,
                    Message = "Datos de líneas inválidos."
                });
            }

            var extracto = await _context.ExtractosBancarios
                .Include(e => e.Lineas)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extracto == null)
            {
                return NotFound(new ActionResponse<object>
                {
                    WasSuccess = false,
                    Message = "Extracto bancario no encontrado."
                });
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(extracto.EmpresaId))
            {
                return Forbid();
            }

            // Verificar que el extracto no esté cerrado
            if (extracto.Estado == EstadosExtractoBancario.Cerrado)
            {
                return BadRequest(new ActionResponse<object>
                {
                    WasSuccess = false,
                    Message = "No se pueden agregar líneas a un extracto cerrado."
                });
            }

            // Obtener el último número de línea
            int ultimoNumeroLinea = extracto.Lineas?.Max(l => l.NumeroLinea) ?? 0;

            // Procesar las nuevas líneas
            foreach (var linea in lineas)
            {
                linea.Id = Guid.NewGuid();
                linea.EmpresaId = extracto.EmpresaId;
                linea.ExtractoBancarioId = extracto.Id;
                linea.NumeroLinea = ++ultimoNumeroLinea;
                linea.EstadoConciliacion = EstadosConciliacionLinea.Pendiente;
                linea.ConciliacionAutomatica = false;

                _context.LineasExtractoBancario.Add(linea);
            }

            // Actualizar totales del extracto
            extracto.CantidadTransacciones += lineas.Count;
            extracto.TotalCreditos += lineas.Sum(l => l.Credito);
            extracto.TotalDebitos += lineas.Sum(l => l.Debito);
            extracto.FechaModificacion = DateTime.UtcNow;
            extracto.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            _logger.LogInformation("{Count} líneas agregadas al extracto {ExtractoId}", lineas.Count, id);

            return Ok(new ActionResponse<object>
            {
                WasSuccess = true,
                Message = $"{lineas.Count} línea(s) agregada(s) exitosamente.",
                Result = new { LineasAgregadas = lineas.Count }
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al agregar líneas al extracto {ExtractoId}", id);
            return StatusCode(500, new ActionResponse<object>
            {
                WasSuccess = false,
                Message = $"Error al guardar las líneas: {ex.InnerException?.Message ?? ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error agregando líneas al extracto {ExtractoId}", id);
            return StatusCode(500, new ActionResponse<object>
            {
                WasSuccess = false,
                Message = $"Error interno del servidor: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Elimina un extracto bancario (soft delete si tiene conciliaciones, hard delete si no).
    /// </summary>
    /// <param name="id">ID del extracto bancario</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var extractoResponse = await _extractoBancarioRepository.GetAsync(id);

            if (!extractoResponse.WasSuccess || extractoResponse.Result == null)
            {
                return NotFound(new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Extracto bancario no encontrado."
                });
            }

            var extracto = extractoResponse.Result;

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(extracto.EmpresaId))
            {
                return Forbid();
            }

            // Verificar si el extracto está cerrado
            if (extracto.Estado == EstadosExtractoBancario.Cerrado)
            {
                return BadRequest(new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar un extracto cerrado."
                });
            }

            // Verificar si tiene líneas conciliadas
            var tieneLineasConciliadas = extracto.Lineas?.Any(l =>
                l.EstadoConciliacion == EstadosConciliacionLinea.Conciliado) ?? false;

            if (tieneLineasConciliadas)
            {
                return BadRequest(new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar un extracto con líneas conciliadas. Revierta primero las conciliaciones."
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deleteResponse = await _extractoBancarioRepository.DeleteAsync(id, userId!);

            if (!deleteResponse.WasSuccess)
            {
                return BadRequest(new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = deleteResponse.Message
                });
            }

            _logger.LogInformation("Extracto bancario {ExtractoId} eliminado por usuario {UserId}", id, userId);

            return Ok(new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = "Extracto bancario eliminado exitosamente."
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al eliminar extracto {ExtractoId}", id);
            return StatusCode(500, new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = $"Error al eliminar el extracto: {ex.InnerException?.Message ?? ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando extracto {ExtractoId}", id);
            return StatusCode(500, new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = $"Error interno del servidor: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Procesa un extracto bancario para prepararlo para conciliación.
    /// Actualiza el estado y clasifica las transacciones automáticamente.
    /// </summary>
    /// <param name="id">ID del extracto bancario</param>
    /// <returns>Resultado del procesamiento</returns>
    [HttpPost("{id:guid}/procesar")]
    public async Task<IActionResult> ProcesarAsync(Guid id)
    {
        try
        {
            var extracto = await _context.ExtractosBancarios
                .Include(e => e.Lineas)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extracto == null)
            {
                return NotFound(new ActionResponse<object>
                {
                    WasSuccess = false,
                    Message = "Extracto bancario no encontrado."
                });
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(extracto.EmpresaId))
            {
                return Forbid();
            }

            if (extracto.Estado == EstadosExtractoBancario.Cerrado)
            {
                return BadRequest(new ActionResponse<object>
                {
                    WasSuccess = false,
                    Message = "El extracto ya está cerrado."
                });
            }

            // Clasificar automáticamente las transacciones basándose en la descripción
            if (extracto.Lineas != null)
            {
                foreach (var linea in extracto.Lineas.Where(l => string.IsNullOrEmpty(l.TipoTransaccion)))
                {
                    linea.TipoTransaccion = ClasificarTransaccion(linea.Descripcion, linea.Credito, linea.Debito);
                }
            }

            // Verificar balances
            var calculadoSaldoFinal = extracto.SaldoInicial + extracto.TotalCreditos - extracto.TotalDebitos;
            if (Math.Abs(calculadoSaldoFinal - extracto.SaldoFinal) > 0.01m)
            {
                _logger.LogWarning("Diferencia en saldo final del extracto {ExtractoId}: esperado {Esperado}, calculado {Calculado}",
                    id, extracto.SaldoFinal, calculadoSaldoFinal);
            }

            // Actualizar estado
            extracto.Estado = EstadosExtractoBancario.Pendiente;
            extracto.FechaModificacion = DateTime.UtcNow;
            extracto.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Extracto bancario {ExtractoId} procesado exitosamente", id);

            return Ok(new ActionResponse<object>
            {
                WasSuccess = true,
                Message = "Extracto bancario procesado exitosamente.",
                Result = new
                {
                    TotalLineas = extracto.Lineas?.Count ?? 0,
                    LineasClasificadas = extracto.Lineas?.Count(l => !string.IsNullOrEmpty(l.TipoTransaccion)) ?? 0,
                    SaldoInicial = extracto.SaldoInicial,
                    SaldoFinal = extracto.SaldoFinal,
                    DiferenciaSaldo = Math.Abs(calculadoSaldoFinal - extracto.SaldoFinal)
                }
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al procesar extracto {ExtractoId}", id);
            return StatusCode(500, new ActionResponse<object>
            {
                WasSuccess = false,
                Message = $"Error al procesar el extracto: {ex.InnerException?.Message ?? ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando extracto {ExtractoId}", id);
            return StatusCode(500, new ActionResponse<object>
            {
                WasSuccess = false,
                Message = $"Error interno del servidor: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Obtiene las líneas pendientes de conciliación de un extracto bancario.
    /// </summary>
    /// <param name="id">ID del extracto bancario</param>
    /// <returns>Lista de líneas pendientes</returns>
    [HttpGet("{id:guid}/lineas-pendientes")]
    public async Task<IActionResult> GetLineasPendientesAsync(Guid id)
    {
        try
        {
            var extracto = await _context.ExtractosBancarios
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extracto == null)
            {
                return NotFound(new ActionResponse<object>
                {
                    WasSuccess = false,
                    Message = "Extracto bancario no encontrado."
                });
            }

            // Verificar acceso a la empresa
            if (!await TieneAccesoEmpresaAsync(extracto.EmpresaId))
            {
                return Forbid();
            }

            var lineasPendientes = await _context.LineasExtractoBancario
                .Where(l => l.ExtractoBancarioId == id &&
                           l.EstadoConciliacion == EstadosConciliacionLinea.Pendiente)
                .OrderBy(l => l.Fecha)
                .ThenBy(l => l.NumeroLinea)
                .Select(l => new
                {
                    l.Id,
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
                    l.EsCredito,
                    l.EsDebito,
                    l.EstadoConciliacion,
                    l.EstadoConciliacionDescripcion
                })
                .ToListAsync();

            return Ok(new ActionResponse<object>
            {
                WasSuccess = true,
                Result = new
                {
                    ExtractoId = id,
                    TotalPendientes = lineasPendientes.Count,
                    TotalDebitos = lineasPendientes.Sum(l => l.Debito),
                    TotalCreditos = lineasPendientes.Sum(l => l.Credito),
                    Lineas = lineasPendientes
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo líneas pendientes del extracto {ExtractoId}", id);
            return StatusCode(500, new ActionResponse<object>
            {
                WasSuccess = false,
                Message = $"Error interno del servidor: {ex.Message}"
            });
        }
    }

    #region Helper Methods

    /// <summary>
    /// Verifica si el usuario actual tiene acceso a la empresa especificada.
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // Verificar si es SuperUser
        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a la empresa
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }

    /// <summary>
    /// Clasifica automáticamente una transacción basándose en su descripción y tipo de movimiento.
    /// </summary>
    private static string ClasificarTransaccion(string descripcion, decimal credito, decimal debito)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return "OTR"; // Otro
        }

        var desc = descripcion.ToUpperInvariant();

        // Depósitos
        if (credito > 0)
        {
            if (desc.Contains("DEPOSITO") || desc.Contains("DEPÓSITO") || desc.Contains("DEP "))
                return "DEP";
            if (desc.Contains("TRANSFER") || desc.Contains("TRANSF "))
                return "TRA";
            if (desc.Contains("INTERES") || desc.Contains("INTERÉS"))
                return "INT";
            return "DEP";
        }

        // Débitos
        if (debito > 0)
        {
            if (desc.Contains("RETIRO") || desc.Contains("RET "))
                return "RET";
            if (desc.Contains("CHEQUE") || desc.Contains("CHE ") || desc.Contains("CK "))
                return "CHE";
            if (desc.Contains("COMISION") || desc.Contains("COMISIÓN") || desc.Contains("COM "))
                return "COM";
            if (desc.Contains("TRANSFER") || desc.Contains("TRANSF "))
                return "TRA";
            return "RET";
        }

        return "OTR";
    }

    #endregion
}
