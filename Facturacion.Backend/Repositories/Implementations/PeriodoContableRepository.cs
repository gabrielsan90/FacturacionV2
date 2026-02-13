using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class PeriodoContableRepository : IPeriodoContableRepository
{
    private readonly DataContext _context;

    public PeriodoContableRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<PeriodoContable?> GetAsync(Guid id)
    {
        return await _context.PeriodosContables
            .Include(p => p.Empresa)
            .Include(p => p.PeriodoFiscal)
            .Include(p => p.CreadoPor)
            .Include(p => p.ModificadoPor)
            .Include(p => p.CerradoPor)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<IEnumerable<PeriodoContable>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.PeriodosContables
            .Include(p => p.CreadoPor)
            .Include(p => p.CerradoPor)
            .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
            .OrderByDescending(p => p.Anio)
            .ThenByDescending(p => p.Mes)
            .ToListAsync();
    }

    public async Task<PeriodoContable?> GetAbiertoAsync(Guid empresaId)
    {
        return await _context.PeriodosContables
            .Where(p => p.EmpresaId == empresaId &&
                       p.Estado == "ABT" &&
                       !p.IsDeleted)
            .OrderByDescending(p => p.Anio)
            .ThenByDescending(p => p.Mes)
            .FirstOrDefaultAsync();
    }

    public async Task<PeriodoContable> AddAsync(PeriodoContable periodo)
    {
        periodo.FechaCreacion = FechaCostaRicaHelper.Ahora;
        _context.PeriodosContables.Add(periodo);
        await _context.SaveChangesAsync();
        return periodo;
    }

    public async Task UpdateAsync(PeriodoContable periodo)
    {
        periodo.FechaModificacion = FechaCostaRicaHelper.Ahora;
        _context.PeriodosContables.Update(periodo);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var periodo = await _context.PeriodosContables.FindAsync(id);
        if (periodo != null)
        {
            periodo.IsDeleted = true;
            periodo.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            periodo.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task CerrarPeriodoAsync(Guid id, string userId)
    {
        var periodo = await _context.PeriodosContables.FindAsync(id);
        if (periodo != null && periodo.Estado == "ABT")
        {
            periodo.Estado = "CER";
            periodo.FechaCierre = FechaCostaRicaHelper.Ahora;
            periodo.CerradoPorId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
