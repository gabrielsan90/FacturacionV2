using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class IncapacidadRepository : IIncapacidadRepository
{
    private readonly DataContext _context;
    private readonly ILogger<IncapacidadRepository> _logger;

    public IncapacidadRepository(DataContext context, ILogger<IncapacidadRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Incapacidad?> GetAsync(Guid id)
    {
        try
        {
            return await _context.Incapacidades
                .Include(i => i.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(i => i.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incapacidad {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Incapacidad>> GetByEmpleadoAsync(Guid empleadoId)
    {
        try
        {
            return await _context.Incapacidades
                .Where(i => i.EmpleadoId == empleadoId)
                .OrderByDescending(i => i.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incapacidades for empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }

    public async Task<IEnumerable<Incapacidad>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            return await _context.Incapacidades
                .Include(i => i.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(i => i.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .Where(i => i.Empleado!.EmpresaId == empresaId)
                .OrderByDescending(i => i.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incapacidades for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Incapacidad>> GetByTipoAsync(Guid empresaId, string tipo)
    {
        try
        {
            return await _context.Incapacidades
                .Include(i => i.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(i => i.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .Where(i => i.Empleado!.EmpresaId == empresaId &&
                           i.Tipo == tipo)
                .OrderByDescending(i => i.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incapacidades by tipo {Tipo} for empresa {EmpresaId}", tipo, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Incapacidad>> GetByPeriodoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            return await _context.Incapacidades
                .Include(i => i.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(i => i.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .Where(i => i.Empleado!.EmpresaId == empresaId &&
                           ((i.FechaInicio >= fechaInicio && i.FechaInicio <= fechaFin) ||
                            (i.FechaFin >= fechaInicio && i.FechaFin <= fechaFin) ||
                            (i.FechaInicio <= fechaInicio && i.FechaFin >= fechaFin)))
                .OrderBy(i => i.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incapacidades for periodo {FechaInicio} to {FechaFin} in empresa {EmpresaId}", fechaInicio, fechaFin, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Incapacidad>> GetActivasAsync(Guid empresaId)
    {
        try
        {
            var hoy = FechaCostaRicaHelper.Ahora.Date;
            return await _context.Incapacidades
                .Include(i => i.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(i => i.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .Where(i => i.Empleado!.EmpresaId == empresaId &&
                           i.FechaInicio <= hoy &&
                           i.FechaFin >= hoy)
                .OrderBy(i => i.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active incapacidades for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<Incapacidad> AddAsync(Incapacidad incapacidad)
    {
        try
        {
            incapacidad.FechaCreacion = FechaCostaRicaHelper.Ahora;
            _context.Incapacidades.Add(incapacidad);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Incapacidad {Id} created successfully for empleado {EmpleadoId}", incapacidad.Id, incapacidad.EmpleadoId);
            return incapacidad;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding incapacidad for empleado {EmpleadoId}", incapacidad.EmpleadoId);
            throw;
        }
    }

    public async Task UpdateAsync(Incapacidad incapacidad)
    {
        try
        {
            incapacidad.FechaModificacion = FechaCostaRicaHelper.Ahora;
            _context.Incapacidades.Update(incapacidad);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Incapacidad {Id} updated successfully", incapacidad.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating incapacidad {Id}", incapacidad.Id);
            throw;
        }
    }
}
