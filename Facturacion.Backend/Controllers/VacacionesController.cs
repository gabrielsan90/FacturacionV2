using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class VacacionesController : ControllerBase
{
    private readonly IVacacionRepository _vacacionRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly DataContext _context;
    private readonly ILogger<VacacionesController> _logger;

    public VacacionesController(
        IVacacionRepository vacacionRepository,
        IEmpleadoRepository empleadoRepository,
        DataContext context,
        ILogger<VacacionesController> logger)
    {
        _vacacionRepository = vacacionRepository;
        _empleadoRepository = empleadoRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las vacaciones de una empresa con filtros opcionales.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(
        Guid empresaId,
        [FromQuery] string? estado = null,
        [FromQuery] int? anio = null,
        [FromQuery] Guid? departamentoId = null)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var vacaciones = await _vacacionRepository.GetByEmpresaAsync(empresaId);

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(estado))
            {
                vacaciones = vacaciones.Where(v => v.Estado == estado);
            }

            if (anio.HasValue)
            {
                vacaciones = vacaciones.Where(v => v.FechaInicio.Year == anio.Value || v.FechaFin.Year == anio.Value);
            }

            if (departamentoId.HasValue)
            {
                vacaciones = vacaciones.Where(v => v.Empleado?.DepartamentoId == departamentoId.Value);
            }

            var resultado = vacaciones
                .OrderByDescending(v => v.FechaCreacion)
                .Select(v => new
                {
                    v.Id,
                    v.EmpleadoId,
                    EmpleadoCodigo = v.Empleado!.Codigo,
                    EmpleadoNombre = v.Empleado!.NombreCompleto,
                    EmpleadoIdentificacion = v.Empleado!.Identificacion,
                    DepartamentoNombre = v.Empleado!.Departamento?.Nombre,
                    PuestoNombre = v.Empleado!.Puesto?.Nombre ?? v.Empleado.PuestoNombre,
                    v.FechaInicio,
                    v.FechaFin,
                    v.DiasHabiles,
                    v.Estado,
                    v.EstadoDescripcion,
                    v.Observaciones,
                    v.FechaAprobacion,
                    AprobadoPor = v.AprobadoPor?.FullName,
                    v.MotivoRechazo,
                    v.FechaCreacion,
                    v.PuedeAprobar,
                    v.PuedeRechazar,
                    v.PuedeCancelar
                });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo vacaciones de empresa {EmpresaId}", empresaId);
            return BadRequest("Error al obtener las vacaciones.");
        }
    }

    /// <summary>
    /// Obtiene todas las vacaciones de un empleado específico.
    /// </summary>
    [HttpGet("empleado/{empleadoId:guid}")]
    public async Task<IActionResult> GetByEmpleadoAsync(Guid empleadoId)
    {
        try
        {
            var empleado = await _empleadoRepository.GetAsync(empleadoId);

            if (empleado == null)
            {
                return NotFound("Empleado no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(empleado.EmpresaId))
            {
                return Forbid();
            }

            var vacaciones = (await _vacacionRepository.GetByEmpleadoAsync(empleadoId))
                .OrderByDescending(v => v.FechaInicio)
                .Select(v => new
                {
                    v.Id,
                    v.FechaInicio,
                    v.FechaFin,
                    v.DiasHabiles,
                    v.Estado,
                    v.EstadoDescripcion,
                    v.Observaciones,
                    v.FechaAprobacion,
                    AprobadoPor = v.AprobadoPor?.FullName,
                    v.MotivoRechazo,
                    v.FechaCreacion,
                    v.DiasTranscurridos,
                    v.PuedeAprobar,
                    v.PuedeRechazar,
                    v.PuedeCancelar
                })
                .ToList();

            // Calcular días disponibles de vacaciones (Costa Rica: 2 semanas por año = 14 días)
            var antiguedadAnios = empleado.AntiguedadAnios;
            var diasAcumulados = antiguedadAnios * 14;
            var diasUtilizados = (await _vacacionRepository.GetByEmpleadoAsync(empleadoId))
                .Where(v => v.Estado == "APR" || v.Estado == "DIS")
                .Sum(v => v.DiasHabiles);
            var diasDisponibles = diasAcumulados - diasUtilizados;

            return Ok(new
            {
                Empleado = new
                {
                    empleado.Id,
                    empleado.Codigo,
                    empleado.NombreCompleto,
                    empleado.FechaIngreso,
                    empleado.AntiguedadAnios
                },
                Balance = new
                {
                    DiasAcumulados = diasAcumulados,
                    DiasUtilizados = diasUtilizados,
                    DiasDisponibles = diasDisponibles
                },
                Vacaciones = vacaciones
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo vacaciones de empleado {EmpleadoId}", empleadoId);
            return BadRequest("Error al obtener las vacaciones del empleado.");
        }
    }

    /// <summary>
    /// Obtiene una solicitud de vacación específica.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var vacacion = await _vacacionRepository.GetAsync(id);

            if (vacacion == null)
            {
                return NotFound("Vacación no encontrada.");
            }

            if (!await TieneAccesoEmpresaAsync(vacacion.Empleado!.EmpresaId))
            {
                return Forbid();
            }

            return Ok(vacacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo vacación {Id}", id);
            return BadRequest("Error al obtener la vacación.");
        }
    }

    /// <summary>
    /// Crea una nueva solicitud de vacaciones.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] SolicitudVacacionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar que el empleado exista
            var empleado = await _empleadoRepository.GetAsync(request.EmpleadoId);
            if (empleado == null)
            {
                return NotFound("Empleado no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(empleado.EmpresaId))
            {
                return Forbid();
            }

            // Validar que el empleado esté activo
            if (empleado.Estado != "ACT")
            {
                return BadRequest("El empleado debe estar activo para solicitar vacaciones.");
            }

            // Validar fechas
            if (request.FechaInicio >= request.FechaFin)
            {
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");
            }

            if (request.FechaInicio < DateTime.Today)
            {
                return BadRequest("No se pueden solicitar vacaciones para fechas pasadas.");
            }

            // Calcular días hábiles (simplificado - debería excluir feriados)
            var diasHabiles = CalcularDiasHabiles(request.FechaInicio, request.FechaFin);

            if (diasHabiles <= 0)
            {
                return BadRequest("El período de vacaciones debe incluir al menos 1 día hábil.");
            }

            // Verificar que no tenga vacaciones aprobadas o solicitadas que se traslapen
            var vacacionesExistentes = await _vacacionRepository.GetByEmpleadoAsync(request.EmpleadoId);
            var traslapan = vacacionesExistentes.Any(v =>
                (v.Estado == "SOL" || v.Estado == "APR" || v.Estado == "DIS") &&
                ((request.FechaInicio >= v.FechaInicio && request.FechaInicio <= v.FechaFin) ||
                 (request.FechaFin >= v.FechaInicio && request.FechaFin <= v.FechaFin) ||
                 (request.FechaInicio <= v.FechaInicio && request.FechaFin >= v.FechaFin)));

            if (traslapan)
            {
                return BadRequest("Ya existe una solicitud de vacaciones aprobada o pendiente que se traslapa con estas fechas.");
            }

            // Verificar días disponibles
            var antiguedadAnios = empleado.AntiguedadAnios;
            var diasAcumulados = antiguedadAnios * 14; // Costa Rica: 2 semanas por año
            var diasUtilizados = vacacionesExistentes
                .Where(v => v.Estado == "APR" || v.Estado == "DIS")
                .Sum(v => v.DiasHabiles);
            var diasDisponibles = diasAcumulados - diasUtilizados;

            if (diasHabiles > diasDisponibles)
            {
                return BadRequest($"No tiene suficientes días de vacaciones disponibles. Disponibles: {diasDisponibles}, Solicitados: {diasHabiles}");
            }

            // Crear solicitud
            var vacacion = new Vacacion
            {
                Id = Guid.NewGuid(),
                EmpleadoId = request.EmpleadoId,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                DiasHabiles = diasHabiles,
                Estado = "SOL", // Solicitada
                Observaciones = request.Observaciones,
                UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            var nuevaVacacion = await _vacacionRepository.AddAsync(vacacion);

            return Ok(new
            {
                nuevaVacacion.Id,
                nuevaVacacion.EmpleadoId,
                nuevaVacacion.FechaInicio,
                nuevaVacacion.FechaFin,
                nuevaVacacion.DiasHabiles,
                nuevaVacacion.Estado,
                nuevaVacacion.EstadoDescripcion,
                DiasDisponiblesRestantes = diasDisponibles - diasHabiles,
                Mensaje = "Solicitud de vacaciones creada exitosamente."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando solicitud de vacaciones para empleado {EmpleadoId}", request.EmpleadoId);
            return BadRequest("Error al crear la solicitud de vacaciones.");
        }
    }

    /// <summary>
    /// Aprueba una solicitud de vacaciones.
    /// </summary>
    [HttpPost("{id:guid}/aprobar")]
    public async Task<IActionResult> AprobarAsync(Guid id, [FromBody] AprobarVacacionRequest? request)
    {
        var vacacion = await _context.Vacaciones
            .Include(v => v.Empleado)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacacion == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(vacacion.Empleado!.EmpresaId))
        {
            return Forbid();
        }

        if (!vacacion.PuedeAprobar)
        {
            return BadRequest($"No se puede aprobar esta solicitud. Estado actual: {vacacion.EstadoDescripcion}");
        }

        vacacion.Estado = "APR";
        vacacion.FechaAprobacion = DateTime.Now;
        vacacion.AprobadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        vacacion.FechaModificacion = DateTime.Now;
        vacacion.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (request != null && !string.IsNullOrWhiteSpace(request.Observaciones))
        {
            vacacion.Observaciones = string.IsNullOrWhiteSpace(vacacion.Observaciones)
                ? request.Observaciones
                : $"{vacacion.Observaciones}\n\nAprobación: {request.Observaciones}";
        }

        _context.Vacaciones.Update(vacacion);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            vacacion.Id,
            vacacion.Estado,
            vacacion.EstadoDescripcion,
            vacacion.FechaAprobacion,
            Mensaje = "Solicitud de vacaciones aprobada exitosamente."
        });
    }

    /// <summary>
    /// Rechaza una solicitud de vacaciones.
    /// </summary>
    [HttpPost("{id:guid}/rechazar")]
    public async Task<IActionResult> RechazarAsync(Guid id, [FromBody] RechazarVacacionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var vacacion = await _context.Vacaciones
            .Include(v => v.Empleado)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacacion == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(vacacion.Empleado!.EmpresaId))
        {
            return Forbid();
        }

        if (!vacacion.PuedeRechazar)
        {
            return BadRequest($"No se puede rechazar esta solicitud. Estado actual: {vacacion.EstadoDescripcion}");
        }

        vacacion.Estado = "REC";
        vacacion.MotivoRechazo = request.MotivoRechazo;
        vacacion.FechaModificacion = DateTime.Now;
        vacacion.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _context.Vacaciones.Update(vacacion);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            vacacion.Id,
            vacacion.Estado,
            vacacion.EstadoDescripcion,
            vacacion.MotivoRechazo,
            Mensaje = "Solicitud de vacaciones rechazada."
        });
    }

    /// <summary>
    /// Cancela una solicitud de vacaciones.
    /// </summary>
    [HttpPost("{id:guid}/cancelar")]
    public async Task<IActionResult> CancelarAsync(Guid id, [FromBody] CancelarVacacionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var vacacion = await _context.Vacaciones
            .Include(v => v.Empleado)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacacion == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(vacacion.Empleado!.EmpresaId))
        {
            return Forbid();
        }

        if (!vacacion.PuedeCancelar)
        {
            return BadRequest($"No se puede cancelar esta solicitud. Estado actual: {vacacion.EstadoDescripcion}");
        }

        vacacion.Estado = "CAN";
        vacacion.Observaciones = string.IsNullOrWhiteSpace(vacacion.Observaciones)
            ? $"Cancelado: {request.Motivo}"
            : $"{vacacion.Observaciones}\n\nCancelado: {request.Motivo}";
        vacacion.FechaModificacion = DateTime.Now;
        vacacion.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _context.Vacaciones.Update(vacacion);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            vacacion.Id,
            vacacion.Estado,
            vacacion.EstadoDescripcion,
            Mensaje = "Solicitud de vacaciones cancelada."
        });
    }

    /// <summary>
    /// Actualiza una solicitud de vacaciones (solo en estado Solicitada).
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] ActualizarVacacionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var vacacion = await _context.Vacaciones
            .Include(v => v.Empleado)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacacion == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(vacacion.Empleado!.EmpresaId))
        {
            return Forbid();
        }

        if (vacacion.Estado != "SOL")
        {
            return BadRequest("Solo se pueden modificar solicitudes en estado Solicitada.");
        }

        // Validar fechas
        if (request.FechaInicio >= request.FechaFin)
        {
            return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");
        }

        // Calcular nuevos días hábiles
        var diasHabiles = CalcularDiasHabiles(request.FechaInicio, request.FechaFin);

        // Verificar traslapes (excluyendo esta solicitud)
        var traslapan = await _context.Vacaciones
            .AnyAsync(v => v.Id != id &&
                          v.EmpleadoId == vacacion.EmpleadoId &&
                          (v.Estado == "SOL" || v.Estado == "APR" || v.Estado == "DIS") &&
                          ((request.FechaInicio >= v.FechaInicio && request.FechaInicio <= v.FechaFin) ||
                           (request.FechaFin >= v.FechaInicio && request.FechaFin <= v.FechaFin) ||
                           (request.FechaInicio <= v.FechaInicio && request.FechaFin >= v.FechaFin)));

        if (traslapan)
        {
            return BadRequest("Las nuevas fechas se traslapan con otra solicitud de vacaciones.");
        }

        vacacion.FechaInicio = request.FechaInicio;
        vacacion.FechaFin = request.FechaFin;
        vacacion.DiasHabiles = diasHabiles;
        vacacion.Observaciones = request.Observaciones;
        vacacion.FechaModificacion = DateTime.Now;
        vacacion.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _context.Vacaciones.Update(vacacion);
        await _context.SaveChangesAsync();

        return Ok(vacacion);
    }

    /// <summary>
    /// Elimina una solicitud de vacaciones (solo en estado Solicitada).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var vacacion = await _context.Vacaciones
                .Include(v => v.Empleado)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vacacion == null)
                return NotFound("Solicitud de vacaciones no encontrada.");

            if (!await TieneAccesoEmpresaAsync(vacacion.Empleado!.EmpresaId))
                return Forbid();

            if (vacacion.Estado != "SOL")
                return BadRequest("Solo se pueden eliminar solicitudes en estado Solicitada.");

            _context.Vacaciones.Remove(vacacion);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Solicitud de vacaciones eliminada exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar solicitud de vacaciones {Id}", id);
            return StatusCode(500, $"Error al eliminar la solicitud: {ex.Message}");
        }
    }

    #region Métodos privados

    /// <summary>
    /// Calcula días hábiles entre dos fechas (lunes a viernes).
    /// Simplificado - no considera feriados.
    /// </summary>
    private int CalcularDiasHabiles(DateTime inicio, DateTime fin)
    {
        int diasHabiles = 0;
        for (DateTime fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
        {
            if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
            {
                diasHabiles++;
            }
        }
        return diasHabiles;
    }

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

    #endregion
}

#region DTOs de Request

public class SolicitudVacacionRequest
{
    public Guid EmpleadoId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? Observaciones { get; set; }
}

public class AprobarVacacionRequest
{
    public string? Observaciones { get; set; }
}

public class RechazarVacacionRequest
{
    public string MotivoRechazo { get; set; } = null!;
}

public class CancelarVacacionRequest
{
    public string Motivo { get; set; } = null!;
}

public class ActualizarVacacionRequest
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? Observaciones { get; set; }
}

#endregion
