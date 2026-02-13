using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class PlanillaRepository : IPlanillaRepository
{
    private readonly DataContext _context;
    private readonly ILogger<PlanillaRepository> _logger;

    public PlanillaRepository(DataContext context, ILogger<PlanillaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Planilla?> GetAsync(Guid id)
    {
        try
        {
            return await _context.Planillas
                .Include(p => p.Empresa)
                .Include(p => p.Detalles!)
                    .ThenInclude(d => d.Empleado)
                        .ThenInclude(e => e!.Departamento)
                .Include(p => p.Detalles!)
                    .ThenInclude(d => d.Empleado)
                        .ThenInclude(e => e!.Puesto)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting planilla {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Planilla>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            return await _context.Planillas
                .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
                .OrderByDescending(p => p.Anio)
                .ThenByDescending(p => p.Mes)
                .ThenByDescending(p => p.Periodo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting planillas for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Planilla>> GetByPeriodoAsync(Guid empresaId, int anio, int mes)
    {
        try
        {
            return await _context.Planillas
                .Where(p => p.EmpresaId == empresaId &&
                           p.Anio == anio &&
                           p.Mes == mes &&
                           !p.IsDeleted)
                .OrderBy(p => p.Periodo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting planillas for periodo {Anio}-{Mes} in empresa {EmpresaId}", anio, mes, empresaId);
            throw;
        }
    }

    public async Task<Planilla?> GetByPeriodoEspecificoAsync(Guid empresaId, int anio, int mes, int periodo)
    {
        try
        {
            return await _context.Planillas
                .Include(p => p.Detalles!)
                    .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(p => p.EmpresaId == empresaId &&
                                         p.Anio == anio &&
                                         p.Mes == mes &&
                                         p.Periodo == periodo &&
                                         !p.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting planilla for periodo {Anio}-{Mes}-P{Periodo} in empresa {EmpresaId}", anio, mes, periodo, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Planilla>> GetByEstadoAsync(Guid empresaId, string estado)
    {
        try
        {
            return await _context.Planillas
                .Where(p => p.EmpresaId == empresaId &&
                           p.Estado == estado &&
                           !p.IsDeleted)
                .OrderByDescending(p => p.Anio)
                .ThenByDescending(p => p.Mes)
                .ThenByDescending(p => p.Periodo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting planillas by estado {Estado} for empresa {EmpresaId}", estado, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Planilla>> GetByTipoAsync(Guid empresaId, string tipoPlanilla)
    {
        try
        {
            return await _context.Planillas
                .Where(p => p.EmpresaId == empresaId &&
                           p.TipoPlanilla == tipoPlanilla &&
                           !p.IsDeleted)
                .OrderByDescending(p => p.Anio)
                .ThenByDescending(p => p.Mes)
                .ThenByDescending(p => p.Periodo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting planillas by tipo {TipoPlanilla} for empresa {EmpresaId}", tipoPlanilla, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Planilla>> GetByAnioAsync(Guid empresaId, int anio)
    {
        try
        {
            return await _context.Planillas
                .Where(p => p.EmpresaId == empresaId &&
                           p.Anio == anio &&
                           !p.IsDeleted)
                .OrderByDescending(p => p.Mes)
                .ThenByDescending(p => p.Periodo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting planillas for anio {Anio} in empresa {EmpresaId}", anio, empresaId);
            throw;
        }
    }

    public async Task<Planilla> AddAsync(Planilla planilla)
    {
        try
        {
            planilla.FechaCreacion = FechaCostaRicaHelper.Ahora;
            _context.Planillas.Add(planilla);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Planilla {Id} created successfully", planilla.Id);
            return planilla;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding planilla {Codigo}", planilla.Codigo);
            throw;
        }
    }

    public async Task UpdateAsync(Planilla planilla)
    {
        try
        {
            planilla.FechaModificacion = FechaCostaRicaHelper.Ahora;
            _context.Planillas.Update(planilla);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Planilla {Id} updated successfully", planilla.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating planilla {Id}", planilla.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        try
        {
            var planilla = await _context.Planillas.FindAsync(id);
            if (planilla != null)
            {
                planilla.IsDeleted = true;
                planilla.FechaEliminacion = FechaCostaRicaHelper.Ahora;
                planilla.UsuarioEliminacionId = userId;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Planilla {Id} deleted successfully by user {UserId}", id, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting planilla {Id}", id);
            throw;
        }
    }
}
