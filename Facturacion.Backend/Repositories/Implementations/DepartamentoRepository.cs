using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class DepartamentoRepository : IDepartamentoRepository
{
    private readonly DataContext _context;
    private readonly ILogger<DepartamentoRepository> _logger;

    public DepartamentoRepository(DataContext context, ILogger<DepartamentoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Departamento?> GetAsync(Guid id)
    {
        try
        {
            return await _context.Departamentos
                .Include(d => d.Empresa)
                .Include(d => d.Jefe)
                .Include(d => d.DepartamentoPadre)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departamento {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Departamento>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            return await _context.Departamentos
                .Include(d => d.Jefe)
                .Include(d => d.DepartamentoPadre)
                .Where(d => d.EmpresaId == empresaId && !d.IsDeleted)
                .OrderBy(d => d.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departamentos for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Departamento>> GetActivosAsync(Guid empresaId)
    {
        try
        {
            return await _context.Departamentos
                .Include(d => d.Jefe)
                .Where(d => d.EmpresaId == empresaId && d.Activo && !d.IsDeleted)
                .OrderBy(d => d.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active departamentos for empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    public async Task<Departamento?> GetByCodigoAsync(Guid empresaId, string codigo)
    {
        try
        {
            return await _context.Departamentos
                .FirstOrDefaultAsync(d => d.EmpresaId == empresaId &&
                                         d.Codigo == codigo &&
                                         !d.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departamento by codigo {Codigo} for empresa {EmpresaId}", codigo, empresaId);
            throw;
        }
    }

    public async Task<IEnumerable<Departamento>> GetByPadreAsync(Guid departamentoPadreId)
    {
        try
        {
            return await _context.Departamentos
                .Include(d => d.Jefe)
                .Where(d => d.DepartamentoPadreId == departamentoPadreId && !d.IsDeleted)
                .OrderBy(d => d.Codigo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departamentos for padre {DepartamentoPadreId}", departamentoPadreId);
            throw;
        }
    }

    public async Task<Departamento> AddAsync(Departamento departamento)
    {
        try
        {
            departamento.FechaCreacion = FechaCostaRicaHelper.Ahora;
            _context.Departamentos.Add(departamento);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Departamento {Id} created successfully", departamento.Id);
            return departamento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding departamento {Codigo}", departamento.Codigo);
            throw;
        }
    }

    public async Task UpdateAsync(Departamento departamento)
    {
        try
        {
            departamento.FechaModificacion = FechaCostaRicaHelper.Ahora;
            _context.Departamentos.Update(departamento);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Departamento {Id} updated successfully", departamento.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating departamento {Id}", departamento.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        try
        {
            var departamento = await _context.Departamentos.FindAsync(id);
            if (departamento != null)
            {
                departamento.IsDeleted = true;
                departamento.FechaEliminacion = FechaCostaRicaHelper.Ahora;
                departamento.UsuarioEliminacionId = userId;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Departamento {Id} deleted successfully by user {UserId}", id, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting departamento {Id}", id);
            throw;
        }
    }
}
