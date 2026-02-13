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
public class IncapacidadesController : ControllerBase
{
    private readonly IIncapacidadRepository _incapacidadRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly DataContext _context;
    private readonly ILogger<IncapacidadesController> _logger;

    public IncapacidadesController(
        IIncapacidadRepository incapacidadRepository,
        IEmpleadoRepository empleadoRepository,
        DataContext context,
        ILogger<IncapacidadesController> logger)
    {
        _incapacidadRepository = incapacidadRepository;
        _empleadoRepository = empleadoRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las incapacidades de una empresa con filtros opcionales.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(
        Guid empresaId,
        [FromQuery] string? tipo = null,
        [FromQuery] int? anio = null,
        [FromQuery] Guid? departamentoId = null,
        [FromQuery] bool? soloActivas = null)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        var query = _context.Incapacidades
            .Include(i => i.Empleado)
                .ThenInclude(e => e!.Departamento)
            .Include(i => i.Empleado)
                .ThenInclude(e => e!.Puesto)
            .Where(i => i.Empleado!.EmpresaId == empresaId)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(tipo))
        {
            query = query.Where(i => i.Tipo == tipo);
        }

        if (anio.HasValue)
        {
            query = query.Where(i => i.FechaInicio.Year == anio.Value || i.FechaFin.Year == anio.Value);
        }

        if (departamentoId.HasValue)
        {
            query = query.Where(i => i.Empleado!.DepartamentoId == departamentoId.Value);
        }

        if (soloActivas.HasValue && soloActivas.Value)
        {
            var hoy = DateTime.Today;
            query = query.Where(i => i.FechaInicio <= hoy && i.FechaFin >= hoy);
        }

        var incapacidades = await query
            .OrderByDescending(i => i.FechaInicio)
            .Select(i => new
            {
                i.Id,
                i.EmpleadoId,
                EmpleadoCodigo = i.Empleado!.Codigo,
                EmpleadoNombre = i.Empleado!.NombreCompleto,
                EmpleadoIdentificacion = i.Empleado!.Identificacion,
                DepartamentoNombre = i.Empleado!.Departamento != null ? i.Empleado.Departamento.Nombre : null,
                PuestoNombre = i.Empleado!.Puesto != null ? i.Empleado.Puesto.Nombre : i.Empleado.PuestoNombre,
                i.Tipo,
                i.TipoDescripcion,
                i.FechaInicio,
                i.FechaFin,
                i.Dias,
                i.NumeroDocumento,
                i.InstitucionEmisora,
                i.Diagnostico,
                i.PorcentajePagoCCSS,
                i.PorcentajePagoPatrono,
                i.EstaActiva,
                i.DiasTranscurridos,
                i.FechaCreacion
            })
            .ToListAsync();

        return Ok(incapacidades);
    }

    /// <summary>
    /// Obtiene todas las incapacidades de un empleado específico.
    /// </summary>
    [HttpGet("empleado/{empleadoId:guid}")]
    public async Task<IActionResult> GetByEmpleadoAsync(Guid empleadoId, [FromQuery] int? anio = null)
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

            var incapacidades = await _incapacidadRepository.GetByEmpleadoAsync(empleadoId);

            if (anio.HasValue)
            {
                incapacidades = incapacidades.Where(i => i.FechaInicio.Year == anio.Value || i.FechaFin.Year == anio.Value);
            }

            var resultado = incapacidades
                .OrderByDescending(i => i.FechaInicio)
                .Select(i => new
                {
                    i.Id,
                    i.Tipo,
                    i.TipoDescripcion,
                    i.FechaInicio,
                    i.FechaFin,
                    i.Dias,
                    i.NumeroDocumento,
                    i.InstitucionEmisora,
                    i.Diagnostico,
                    i.Observaciones,
                    i.ArchivoAdjunto,
                    i.PorcentajePagoCCSS,
                    i.PorcentajePagoPatrono,
                    i.EstaActiva,
                    i.DiasTranscurridos,
                    i.FechaCreacion
                })
                .ToList();

            // Calcular resumen por tipo
            var resumenPorTipo = resultado
                .GroupBy(i => new { i.Tipo, i.TipoDescripcion })
                .Select(g => new
                {
                    g.Key.Tipo,
                    g.Key.TipoDescripcion,
                    Cantidad = g.Count(),
                    TotalDias = g.Sum(i => i.Dias)
                })
                .OrderByDescending(r => r.TotalDias)
                .ToList();

            var totalDias = resultado.Sum(i => i.Dias);

            return Ok(new
            {
                Empleado = new
                {
                    empleado.Id,
                    empleado.Codigo,
                    empleado.NombreCompleto,
                    empleado.Identificacion,
                    DepartamentoNombre = empleado.Departamento?.Nombre,
                    PuestoNombre = empleado.Puesto?.Nombre ?? empleado.PuestoNombre
                },
                Resumen = new
                {
                    TotalIncapacidades = resultado.Count,
                    TotalDias = totalDias,
                    IncapacidadesActivas = resultado.Count(i => i.EstaActiva),
                    ResumenPorTipo = resumenPorTipo
                },
                Incapacidades = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo incapacidades de empleado {EmpleadoId}", empleadoId);
            return BadRequest("Error al obtener las incapacidades del empleado.");
        }
    }

    /// <summary>
    /// Obtiene una incapacidad específica.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var incapacidad = await _context.Incapacidades
            .Include(i => i.Empleado)
                .ThenInclude(e => e!.Departamento)
            .Include(i => i.Empleado)
                .ThenInclude(e => e!.Puesto)
            .Include(i => i.UsuarioCreacion)
            .Include(i => i.UsuarioModificacion)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incapacidad == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(incapacidad.Empleado!.EmpresaId))
        {
            return Forbid();
        }

        return Ok(incapacidad);
    }

    /// <summary>
    /// Registra una nueva incapacidad.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] RegistrarIncapacidadRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validar que el empleado exista
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.Id == request.EmpleadoId && !e.IsDeleted);

        if (empleado == null)
        {
            return NotFound("Empleado no encontrado.");
        }

        if (!await TieneAccesoEmpresaAsync(empleado.EmpresaId))
        {
            return Forbid();
        }

        // Validar fechas
        if (request.FechaInicio >= request.FechaFin)
        {
            return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");
        }

        // Calcular días
        var dias = (request.FechaFin - request.FechaInicio).Days + 1;

        // Determinar porcentajes de pago según legislación de Costa Rica
        decimal porcentajePagoCCSS = 0;
        decimal porcentajePagoPatrono = 100;

        if (request.Tipo == "ENF") // Enfermedad
        {
            // CCSS paga 60% a partir del 4to día
            // Patrono paga 100% primeros 3 días, luego 40% complementario
            if (dias <= 3)
            {
                porcentajePagoPatrono = 100;
                porcentajePagoCCSS = 0;
            }
            else
            {
                porcentajePagoPatrono = 40; // Patrono complementa el 40%
                porcentajePagoCCSS = 60; // CCSS paga el 60%
            }
        }
        else if (request.Tipo == "MAT") // Maternidad
        {
            // CCSS paga 100% durante 4 meses (antes y después del parto)
            porcentajePagoCCSS = 100;
            porcentajePagoPatrono = 0;
        }
        else if (request.Tipo == "PAT") // Paternidad
        {
            // Patrono paga 100% (8 días según Código de Trabajo)
            porcentajePagoPatrono = 100;
            porcentajePagoCCSS = 0;
        }
        else if (request.Tipo == "RIE") // Riesgo de Trabajo
        {
            // INS paga desde el primer día (60%), patrono complementa (40%)
            porcentajePagoCCSS = 60; // En realidad es INS, pero usamos este campo
            porcentajePagoPatrono = 40;
        }

        // Crear incapacidad
        var incapacidad = new Incapacidad
        {
            Id = Guid.NewGuid(),
            EmpleadoId = request.EmpleadoId,
            Tipo = request.Tipo,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            Dias = dias,
            NumeroDocumento = request.NumeroDocumento,
            InstitucionEmisora = request.InstitucionEmisora,
            Diagnostico = request.Diagnostico,
            Observaciones = request.Observaciones,
            ArchivoAdjunto = request.ArchivoAdjunto,
            PorcentajePagoCCSS = porcentajePagoCCSS,
            PorcentajePagoPatrono = porcentajePagoPatrono,
            FechaCreacion = DateTime.Now,
            UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        // Actualizar estado del empleado si la incapacidad está activa
        if (incapacidad.EstaActiva && empleado.Estado == "ACT")
        {
            empleado.Estado = "INC"; // Cambiar a estado Incapacidad
            empleado.FechaModificacion = DateTime.Now;
            empleado.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Empleados.Update(empleado);
        }

        _context.Incapacidades.Add(incapacidad);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            incapacidad.Id,
            incapacidad.EmpleadoId,
            incapacidad.Tipo,
            incapacidad.TipoDescripcion,
            incapacidad.FechaInicio,
            incapacidad.FechaFin,
            incapacidad.Dias,
            incapacidad.PorcentajePagoCCSS,
            incapacidad.PorcentajePagoPatrono,
            incapacidad.EstaActiva,
            EmpleadoEstadoActualizado = empleado.Estado,
            Mensaje = "Incapacidad registrada exitosamente."
        });
    }

    /// <summary>
    /// Actualiza una incapacidad existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] ActualizarIncapacidadRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var incapacidad = await _context.Incapacidades
            .Include(i => i.Empleado)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incapacidad == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(incapacidad.Empleado!.EmpresaId))
        {
            return Forbid();
        }

        // Validar fechas
        if (request.FechaInicio >= request.FechaFin)
        {
            return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");
        }

        // Recalcular días
        var dias = (request.FechaFin - request.FechaInicio).Days + 1;

        // Actualizar campos
        incapacidad.FechaInicio = request.FechaInicio;
        incapacidad.FechaFin = request.FechaFin;
        incapacidad.Dias = dias;
        incapacidad.NumeroDocumento = request.NumeroDocumento;
        incapacidad.InstitucionEmisora = request.InstitucionEmisora;
        incapacidad.Diagnostico = request.Diagnostico;
        incapacidad.Observaciones = request.Observaciones;
        incapacidad.ArchivoAdjunto = request.ArchivoAdjunto;
        incapacidad.PorcentajePagoCCSS = request.PorcentajePagoCCSS ?? incapacidad.PorcentajePagoCCSS;
        incapacidad.PorcentajePagoPatrono = request.PorcentajePagoPatrono ?? incapacidad.PorcentajePagoPatrono;
        incapacidad.FechaModificacion = DateTime.Now;
        incapacidad.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _context.Incapacidades.Update(incapacidad);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            incapacidad.Id,
            incapacidad.Tipo,
            incapacidad.TipoDescripcion,
            incapacidad.FechaInicio,
            incapacidad.FechaFin,
            incapacidad.Dias,
            incapacidad.EstaActiva,
            Mensaje = "Incapacidad actualizada exitosamente."
        });
    }

    /// <summary>
    /// Elimina una incapacidad.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var incapacidad = await _context.Incapacidades
            .Include(i => i.Empleado)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incapacidad == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(incapacidad.Empleado!.EmpresaId))
        {
            return Forbid();
        }

        // Eliminar incapacidad
        _context.Incapacidades.Remove(incapacidad);
        await _context.SaveChangesAsync();

        // Verificar si el empleado tiene otras incapacidades activas
        var tieneOtrasIncapacidadesActivas = await _context.Incapacidades
            .AnyAsync(i => i.EmpleadoId == incapacidad.EmpleadoId && i.EstaActiva);

        // Si no tiene más incapacidades activas y está en estado INC, volver a ACT
        if (!tieneOtrasIncapacidadesActivas && incapacidad.Empleado!.Estado == "INC")
        {
            incapacidad.Empleado.Estado = "ACT";
            incapacidad.Empleado.FechaModificacion = DateTime.Now;
            incapacidad.Empleado.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Empleados.Update(incapacidad.Empleado);
            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            Mensaje = "Incapacidad eliminada exitosamente."
        });
    }

    /// <summary>
    /// Obtiene estadísticas de incapacidades por empresa para un período.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/estadisticas")]
    public async Task<IActionResult> GetEstadisticasAsync(
        Guid empresaId,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        if (!await TieneAccesoEmpresaAsync(empresaId))
        {
            return Forbid();
        }

        // Usar período del año actual si no se especifica
        fechaInicio ??= new DateTime(DateTime.Today.Year, 1, 1);
        fechaFin ??= DateTime.Today;

        var incapacidades = await _context.Incapacidades
            .Include(i => i.Empleado)
                .ThenInclude(e => e!.Departamento)
            .Where(i => i.Empleado!.EmpresaId == empresaId &&
                       i.FechaInicio <= fechaFin &&
                       i.FechaFin >= fechaInicio)
            .ToListAsync();

        var totalIncapacidades = incapacidades.Count;
        var totalDias = incapacidades.Sum(i => i.Dias);
        var incapacidadesActivas = incapacidades.Count(i => i.EstaActiva);

        var porTipo = incapacidades
            .GroupBy(i => new { i.Tipo, i.TipoDescripcion })
            .Select(g => new
            {
                g.Key.Tipo,
                g.Key.TipoDescripcion,
                Cantidad = g.Count(),
                TotalDias = g.Sum(i => i.Dias),
                Porcentaje = totalIncapacidades > 0 ? (g.Count() * 100.0 / totalIncapacidades) : 0
            })
            .OrderByDescending(x => x.Cantidad)
            .ToList();

        var porDepartamento = incapacidades
            .Where(i => i.Empleado!.Departamento != null)
            .GroupBy(i => new
            {
                DepartamentoId = i.Empleado!.DepartamentoId,
                DepartamentoNombre = i.Empleado!.Departamento!.Nombre
            })
            .Select(g => new
            {
                g.Key.DepartamentoId,
                g.Key.DepartamentoNombre,
                Cantidad = g.Count(),
                TotalDias = g.Sum(i => i.Dias)
            })
            .OrderByDescending(x => x.Cantidad)
            .ToList();

        var promedioDiasPorIncapacidad = totalIncapacidades > 0
            ? Math.Round((decimal)totalDias / totalIncapacidades, 2)
            : 0;

        return Ok(new
        {
            Periodo = new
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            },
            Resumen = new
            {
                TotalIncapacidades = totalIncapacidades,
                TotalDias = totalDias,
                IncapacidadesActivas = incapacidadesActivas,
                PromedioDiasPorIncapacidad = promedioDiasPorIncapacidad
            },
            PorTipo = porTipo,
            PorDepartamento = porDepartamento
        });
    }

    #region Métodos privados

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

public class RegistrarIncapacidadRequest
{
    public Guid EmpleadoId { get; set; }
    public string Tipo { get; set; } = "ENF"; // ENF, MAT, PAT, RIE
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? InstitucionEmisora { get; set; }
    public string? Diagnostico { get; set; }
    public string? Observaciones { get; set; }
    public string? ArchivoAdjunto { get; set; }
}

public class ActualizarIncapacidadRequest
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? InstitucionEmisora { get; set; }
    public string? Diagnostico { get; set; }
    public string? Observaciones { get; set; }
    public string? ArchivoAdjunto { get; set; }
    public decimal? PorcentajePagoCCSS { get; set; }
    public decimal? PorcentajePagoPatrono { get; set; }
}

#endregion
