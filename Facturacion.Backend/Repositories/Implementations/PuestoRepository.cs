using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class PuestoRepository : IPuestoRepository
{
    private readonly DataContext _context;
    private readonly ILogger<PuestoRepository> _logger;

    public PuestoRepository(DataContext context, ILogger<PuestoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Puesto?> GetAsync(Guid id)
    {
        try
        {
            return await _context.Puestos
                .Include(p => p.Empresa)
                .Include(p => p.Departamento)
                .Include(p => p.PuestoSuperior)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting puesto {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Puesto>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            return await _context.Puestos
                .Include(p => p.Departamento)
                .Include(p => p.PuestoSuperior)
                .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
                .OrderBy(p => p.NivelJerarquico)
                .ThenBy(p => p.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting puestos for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Puesto>> GetActivosAsync(Guid empresaId)
    {
        try
        {
            return await _context.Puestos
                .Include(p => p.Departamento)
                .Where(p => p.EmpresaId == empresaId && p.Activo && !p.IsDeleted)
                .OrderBy(p => p.NivelJerarquico)
                .ThenBy(p => p.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active puestos for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<Puesto?> GetByCodigoAsync(Guid empresaId, string codigo)
    {
        try
        {
            return await _context.Puestos
                .FirstOrDefaultAsync(p => p.EmpresaId == empresaId &&
                                         p.Codigo == codigo &&
                                         !p.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting puesto by codigo {Codigo} for empresa {EmpresaId}", codigo, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Puesto>> GetByDepartamentoAsync(Guid departamentoId)
    {
        try
        {
            return await _context.Puestos
                .Where(p => p.DepartamentoId == departamentoId && !p.IsDeleted)
                .OrderBy(p => p.NivelJerarquico)
                .ThenBy(p => p.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting puestos for departamento {DepartamentoId}", departamentoId);
            throw;
        }
    }

    public async Task<IEnumerable<Puesto>> GetByNivelJerarquicoAsync(Guid empresaId, int nivelJerarquico)
    {
        try
        {
            return await _context.Puestos
                .Include(p => p.Departamento)
                .Where(p => p.EmpresaId == empresaId &&
                           p.NivelJerarquico == nivelJerarquico &&
                           !p.IsDeleted)
                .OrderBy(p => p.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting puestos for nivel {NivelJerarquico} in empresa {EmpresaId}", nivelJerarquico, empresaId);
            throw;
        }
    }

    public async Task<Puesto> AddAsync(Puesto puesto)
    {
        try
        {
            puesto.FechaCreacion = FechaCostaRicaHelper.Ahora;
            _context.Puestos.Add(puesto);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Puesto {Id} created successfully", puesto.Id);
            return puesto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding puesto {Codigo}", puesto.Codigo);
            throw;
        }
    }

    public async Task UpdateAsync(Puesto puesto)
    {
        try
        {
            puesto.FechaModificacion = FechaCostaRicaHelper.Ahora;
            _context.Puestos.Update(puesto);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Puesto {Id} updated successfully", puesto.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating puesto {Id}", puesto.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        try
        {
            var puesto = await _context.Puestos.FindAsync(id);
            if (puesto != null)
            {
                puesto.IsDeleted = true;
                puesto.FechaEliminacion = FechaCostaRicaHelper.Ahora;
                puesto.UsuarioEliminacionId = userId;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Puesto {Id} deleted successfully by user {UserId}", id, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting puesto {Id}", id);
            throw;
        }
    }
}
