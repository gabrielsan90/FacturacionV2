using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
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
public class PeriodosContablesController : ControllerBase
{
    private readonly IContabilidadUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<PeriodosContablesController> _logger;

    public PeriodosContablesController(
        IContabilidadUnitOfWork unitOfWork,
        DataContext context,
        ILogger<PeriodosContablesController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los períodos contables de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var periodos = await _unitOfWork.PeriodoContableRepository.GetByEmpresaAsync(empresaId);

            return Ok(periodos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving periodos contables for empresa {EmpresaId}", empresaId);
            return BadRequest($"Error al obtener los períodos contables: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el período abierto actual de una empresa
    /// </summary>
    [HttpGet("periodo-abierto/{empresaId:guid}")]
    public async Task<IActionResult> GetPeriodoAbiertoAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var periodo = await _unitOfWork.PeriodoContableRepository.GetAbiertoAsync(empresaId);

            if (periodo == null)
            {
                return NotFound("No existe un período abierto para esta empresa.");
            }

            return Ok(periodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving periodo abierto for empresa {EmpresaId}", empresaId);
            return BadRequest($"Error al obtener el período abierto: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene un período contable por su ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var periodo = await _unitOfWork.PeriodoContableRepository.GetAsync(id);

            if (periodo == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(periodo.EmpresaId))
            {
                return Forbid();
            }

            return Ok(periodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving periodo contable {Id}", id);
            return BadRequest($"Error al obtener el período contable: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea un nuevo período contable
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] PeriodoContable periodo)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(periodo.EmpresaId))
            {
                return Forbid();
            }

            // Verificar que no exista un período duplicado (usando DataContext para query compleja)
            var existente = await _context.PeriodosContables
                .FirstOrDefaultAsync(p => p.EmpresaId == periodo.EmpresaId &&
                                         p.Anio == periodo.Anio &&
                                         p.Mes == periodo.Mes &&
                                         !p.IsDeleted);

            if (existente != null)
            {
                return BadRequest($"Ya existe un período contable para {periodo.NombreMes} {periodo.Anio}.");
            }

            // Calcular fechas inicio y fin del mes
            var fechaInicio = new DateTime(periodo.Anio, periodo.Mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            // Configurar período
            periodo.Id = Guid.NewGuid();
            periodo.FechaInicio = fechaInicio;
            periodo.FechaFin = fechaFin;
            periodo.Nombre = $"{periodo.NombreMes} {periodo.Anio}";
            periodo.Estado = "ABT";
            periodo.UltimoNumeroAsiento = 0;
            periodo.TotalDebe = 0;
            periodo.TotalHaber = 0;
            periodo.CantidadAsientos = 0;
            periodo.CreadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _unitOfWork.PeriodoContableRepository.AddAsync(periodo);
            await _unitOfWork.SaveAsync();

            return Ok(periodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating periodo contable");
            return BadRequest($"Error al crear el período contable: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza un período contable
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] PeriodoContable periodo)
    {
        try
        {
            if (id != periodo.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _unitOfWork.PeriodoContableRepository.GetAsync(id);

            if (existente == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                return Forbid();
            }

            // No permitir modificar período cerrado
            if (existente.Estado == "CER")
            {
                return BadRequest("No se puede modificar un período cerrado.");
            }

            // Verificar duplicados (usando DataContext para query compleja)
            var duplicado = await _context.PeriodosContables
                .FirstOrDefaultAsync(p => p.EmpresaId == existente.EmpresaId &&
                                         p.Anio == periodo.Anio &&
                                         p.Mes == periodo.Mes &&
                                         p.Id != id &&
                                         !p.IsDeleted);

            if (duplicado != null)
            {
                return BadRequest($"Ya existe otro período para {periodo.NombreMes} {periodo.Anio}.");
            }

            // Recalcular fechas si cambió año o mes
            if (existente.Anio != periodo.Anio || existente.Mes != periodo.Mes)
            {
                var fechaInicio = new DateTime(periodo.Anio, periodo.Mes, 1);
                var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
                periodo.FechaInicio = fechaInicio;
                periodo.FechaFin = fechaFin;
                periodo.Nombre = $"{periodo.NombreMes} {periodo.Anio}";
            }

            // Preservar campos de auditoría y control
            periodo.FechaCreacion = existente.FechaCreacion;
            periodo.CreadoPorId = existente.CreadoPorId;
            periodo.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            periodo.Estado = existente.Estado;
            periodo.FechaCierre = existente.FechaCierre;
            periodo.CerradoPorId = existente.CerradoPorId;
            periodo.UltimoNumeroAsiento = existente.UltimoNumeroAsiento;
            periodo.TotalDebe = existente.TotalDebe;
            periodo.TotalHaber = existente.TotalHaber;
            periodo.CantidadAsientos = existente.CantidadAsientos;

            // Update using repository (handles FechaModificacion)
            await _unitOfWork.PeriodoContableRepository.UpdateAsync(periodo);
            await _unitOfWork.SaveAsync();

            return Ok(periodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating periodo contable {Id}", id);
            return BadRequest($"Error al actualizar el período contable: {ex.Message}");
        }
    }

    /// <summary>
    /// Cierra un período contable
    /// </summary>
    [HttpPost("{id:guid}/cerrar")]
    public async Task<IActionResult> CerrarPeriodoAsync(Guid id)
    {
        try
        {
            // Use DataContext for complex query with Include
            var periodo = await _context.PeriodosContables
                .Include(p => p.Asientos)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (periodo == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(periodo.EmpresaId))
            {
                return Forbid();
            }

            // Validar que el período esté abierto
            if (periodo.Estado == "CER")
            {
                return BadRequest("El período ya está cerrado.");
            }

            // Validar que el período esté balanceado
            if (!periodo.EstaBalanceado)
            {
                return BadRequest($"El período no está balanceado. Diferencia: {periodo.Diferencia:N2}. No se puede cerrar.");
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Close using repository
            await _unitOfWork.PeriodoContableRepository.CerrarPeriodoAsync(id, userId!);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Periodo contable {Id} closed by user {UserId}", id, userId);

            return Ok(periodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing periodo contable {Id}", id);
            return BadRequest($"Error al cerrar el período contable: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera automáticamente los 12 períodos mensuales para un año específico
    /// </summary>
    [HttpPost("generar-anio/{empresaId:guid}")]
    public async Task<IActionResult> GenerarPeriodosAnioAsync(Guid empresaId, [FromBody] GenerarAnioRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            if (request.Anio < 2000 || request.Anio > 2100)
            {
                return BadRequest("El año debe estar entre 2000 y 2100.");
            }

            var periodosCreados = new List<PeriodoContable>();
            var periodosOmitidos = new List<string>();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Generar 12 meses
            for (int mes = 1; mes <= 12; mes++)
            {
                // Verificar si ya existe el período (usando DataContext para query compleja)
                var existente = await _context.PeriodosContables
                    .FirstOrDefaultAsync(p => p.EmpresaId == empresaId &&
                                             p.Anio == request.Anio &&
                                             p.Mes == mes &&
                                             !p.IsDeleted);

                if (existente != null)
                {
                    var nombreMes = new DateTime(request.Anio, mes, 1).ToString("MMMM", new System.Globalization.CultureInfo("es-CR"));
                    periodosOmitidos.Add($"{nombreMes} {request.Anio}");
                    continue;
                }

                // Crear período
                var fechaInicio = new DateTime(request.Anio, mes, 1);
                var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
                var nombreMesPeriodo = new PeriodoContable { Mes = mes }.NombreMes;

                var periodo = new PeriodoContable
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaId,
                    Anio = request.Anio,
                    Mes = mes,
                    Nombre = $"{nombreMesPeriodo} {request.Anio}",
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    Estado = "ABT",
                    UltimoNumeroAsiento = 0,
                    TotalDebe = 0,
                    TotalHaber = 0,
                    CantidadAsientos = 0,
                    CreadoPorId = userId
                };

                await _unitOfWork.PeriodoContableRepository.AddAsync(periodo);
                periodosCreados.Add(periodo);
            }

            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Generated {Count} periodos for year {Year} and empresa {EmpresaId}", periodosCreados.Count, request.Anio, empresaId);

            var resultado = new
            {
                PeriodosCreados = periodosCreados.Count,
                PeriodosOmitidos = periodosOmitidos.Count,
                Mensaje = $"Se crearon {periodosCreados.Count} períodos contables para el año {request.Anio}.",
                Detalles = new
                {
                    Creados = periodosCreados.Select(p => p.Nombre).ToList(),
                    Omitidos = periodosOmitidos
                }
            };

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating periodos for year {Year}", request.Anio);
            return BadRequest($"Error al generar los períodos: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina (soft delete) un período contable
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            // Use DataContext for complex query with Include
            var periodo = await _context.PeriodosContables
                .Include(p => p.Asientos)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (periodo == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(periodo.EmpresaId))
            {
                return Forbid();
            }

            // No permitir eliminar período cerrado
            if (periodo.Estado == "CER")
            {
                return BadRequest("No se puede eliminar un período cerrado.");
            }

            // No permitir eliminar si tiene asientos
            if (periodo.Asientos?.Any() == true)
            {
                return BadRequest("No se puede eliminar un período que tiene asientos contables.");
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Delete using repository
            await _unitOfWork.PeriodoContableRepository.DeleteAsync(id, userId!);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Periodo contable {Id} deleted by user {UserId}", id, userId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting periodo contable {Id}", id);
            return BadRequest($"Error al eliminar el período contable: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si el usuario tiene acceso a la empresa
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
}

/// <summary>
/// Request para generar períodos de un año completo
/// </summary>
public class GenerarAnioRequest
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El año es obligatorio.")]
    [System.ComponentModel.DataAnnotations.Range(2000, 2100, ErrorMessage = "El año debe estar entre 2000 y 2100.")]
    public int Anio { get; set; }
}
