using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class VacacionRepository : IVacacionRepository
{
    private readonly DataContext _context;
    private readonly ILogger<VacacionRepository> _logger;

    public VacacionRepository(DataContext context, ILogger<VacacionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Vacacion?> GetAsync(Guid id)
    {
        try
        {
            return await _context.Vacaciones
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .Include(v => v.AprobadoPor)
                .FirstOrDefaultAsync(v => v.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vacacion {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Vacacion>> GetByEmpleadoAsync(Guid empleadoId)
    {
        try
        {
            return await _context.Vacaciones
                .Include(v => v.AprobadoPor)
                .Where(v => v.EmpleadoId == empleadoId)
                .OrderByDescending(v => v.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vacaciones for empleado {EmpleadoId}", empleadoId);
            throw;
        }
    }

    public async Task<IEnumerable<Vacacion>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            return await _context.Vacaciones
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .Where(v => v.Empleado!.EmpresaId == empresaId)
                .OrderByDescending(v => v.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vacaciones for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Vacacion>> GetPendientesAsync(Guid empresaId)
    {
        try
        {
            return await _context.Vacaciones
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .Where(v => v.Empleado!.EmpresaId == empresaId &&
                           v.Estado == "SOL")
                .OrderBy(v => v.FechaCreacion)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending vacaciones for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Vacacion>> GetByPeriodoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            return await _context.Vacaciones
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .Where(v => v.Empleado!.EmpresaId == empresaId &&
                           ((v.FechaInicio >= fechaInicio && v.FechaInicio <= fechaFin) ||
                            (v.FechaFin >= fechaInicio && v.FechaFin <= fechaFin) ||
                            (v.FechaInicio <= fechaInicio && v.FechaFin >= fechaFin)))
                .OrderBy(v => v.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vacaciones for periodo {FechaInicio} to {FechaFin} in empresa {EmpresaId}", fechaInicio, fechaFin, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Vacacion>> GetByEstadoAsync(Guid empresaId, string estado)
    {
        try
        {
            return await _context.Vacaciones
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .Where(v => v.Empleado!.EmpresaId == empresaId &&
                           v.Estado == estado)
                .OrderByDescending(v => v.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vacaciones by estado {Estado} for empresa {EmpresaId}", estado, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Vacacion>> GetActivasAsync(Guid empresaId)
    {
        try
        {
            var hoy = FechaCostaRicaHelper.Ahora.Date;
            return await _context.Vacaciones
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Departamento)
                .Include(v => v.Empleado)
                    .ThenInclude(e => e!.Puesto)
                .Where(v => v.Empleado!.EmpresaId == empresaId &&
                           (v.Estado == "APR" || v.Estado == "DIS") &&
                           v.FechaInicio <= hoy &&
                           v.FechaFin >= hoy)
                .OrderBy(v => v.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active vacaciones for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<Vacacion> AddAsync(Vacacion vacacion)
    {
        try
        {
            vacacion.FechaCreacion = FechaCostaRicaHelper.Ahora;
            _context.Vacaciones.Add(vacacion);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Vacacion {Id} created successfully for empleado {EmpleadoId}", vacacion.Id, vacacion.EmpleadoId);
            return vacacion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vacacion for empleado {EmpleadoId}", vacacion.EmpleadoId);
            throw;
        }
    }

    public async Task UpdateAsync(Vacacion vacacion)
    {
        try
        {
            vacacion.FechaModificacion = FechaCostaRicaHelper.Ahora;
            _context.Vacaciones.Update(vacacion);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Vacacion {Id} updated successfully", vacacion.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vacacion {Id}", vacacion.Id);
            throw;
        }
    }
}
