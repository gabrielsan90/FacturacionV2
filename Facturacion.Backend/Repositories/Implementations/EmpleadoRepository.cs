using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly DataContext _context;
    private readonly ILogger<EmpleadoRepository> _logger;

    public EmpleadoRepository(DataContext context, ILogger<EmpleadoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Empleado?> GetAsync(Guid id)
    {
        try
        {
            return await _context.Empleados
                .Include(e => e.Empresa)
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .Include(e => e.Banco)
                .Include(e => e.JefeDirecto)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting empleado {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Empleado>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            return await _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .Include(e => e.JefeDirecto)
                .Where(e => e.EmpresaId == empresaId && !e.IsDeleted)
                .OrderBy(e => e.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting empleados for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Empleado>> GetByDepartamentoAsync(Guid departamentoId)
    {
        try
        {
            return await _context.Empleados
                .Include(e => e.Puesto)
                .Include(e => e.JefeDirecto)
                .Where(e => e.DepartamentoId == departamentoId && !e.IsDeleted)
                .OrderBy(e => e.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting empleados for departamento {DepartamentoId}", departamentoId);
            throw;
        }
    }

    public async Task<IEnumerable<Empleado>> GetActivosAsync(Guid empresaId)
    {
        try
        {
            return await _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .Where(e => e.EmpresaId == empresaId &&
                           e.Estado == "ACT" &&
                           !e.IsDeleted)
                .OrderBy(e => e.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active empleados for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<Empleado?> GetByIdentificacionAsync(Guid empresaId, string identificacion)
    {
        try
        {
            return await _context.Empleados
                .FirstOrDefaultAsync(e => e.EmpresaId == empresaId &&
                                         e.Identificacion == identificacion &&
                                         !e.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting empleado by identificacion {Identificacion} for empresa {EmpresaId}", identificacion, empresaId);
            throw;
        }
    }

    public async Task<Empleado?> GetByCodigoAsync(Guid empresaId, string codigo)
    {
        try
        {
            return await _context.Empleados
                .FirstOrDefaultAsync(e => e.EmpresaId == empresaId &&
                                         e.Codigo == codigo &&
                                         !e.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting empleado by codigo {Codigo} for empresa {EmpresaId}", codigo, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Empleado>> GetByEstadoAsync(Guid empresaId, string estado)
    {
        try
        {
            return await _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .Where(e => e.EmpresaId == empresaId &&
                           e.Estado == estado &&
                           !e.IsDeleted)
                .OrderBy(e => e.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting empleados by estado {Estado} for empresa {EmpresaId}", estado, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Empleado>> GetSubordinadosAsync(Guid jefeId)
    {
        try
        {
            return await _context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Puesto)
                .Where(e => e.JefeDirectoId == jefeId && !e.IsDeleted)
                .OrderBy(e => e.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subordinados for jefe {JefeId}", jefeId);
            throw;
        }
    }

    public async Task<Empleado> AddAsync(Empleado empleado)
    {
        try
        {
            empleado.FechaCreacion = FechaCostaRicaHelper.Ahora;
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Empleado {Id} created successfully", empleado.Id);
            return empleado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding empleado {Codigo}", empleado.Codigo);
            throw;
        }
    }

    public async Task UpdateAsync(Empleado empleado)
    {
        try
        {
            empleado.FechaModificacion = FechaCostaRicaHelper.Ahora;
            _context.Empleados.Update(empleado);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Empleado {Id} updated successfully", empleado.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating empleado {Id}", empleado.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        try
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                empleado.IsDeleted = true;
                empleado.FechaEliminacion = FechaCostaRicaHelper.Ahora;
                empleado.UsuarioEliminacionId = userId;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Empleado {Id} deleted successfully by user {UserId}", id, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting empleado {Id}", id);
            throw;
        }
    }
}
