using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class DistritoRepository : IDistritoRepository
{
    private readonly DataContext _context;

    public DistritoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Distrito?> GetAsync(int id)
    {
        return await _context.Distritos
            .Include(d => d.Canton)
                .ThenInclude(c => c.Provincia)
            .FirstOrDefaultAsync(d => d.Id == id && d.Activo);
    }

    public async Task<IEnumerable<Distrito>> GetAllAsync()
    {
        return await _context.Distritos
            .Include(d => d.Canton)
                .ThenInclude(c => c.Provincia)
            .Where(d => d.Activo)
            .OrderBy(d => d.Canton.ProvinciaId)
            .ThenBy(d => d.CantonId)
            .ThenBy(d => d.Codigo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Distrito>> GetByCantonAsync(int cantonId)
    {
        return await _context.Distritos
            .Where(d => d.CantonId == cantonId && d.Activo)
            .OrderBy(d => d.Codigo)
            .ToListAsync();
    }

    public async Task<Distrito?> GetByCodigoAsync(string codigo)
    {
        return await _context.Distritos
            .Include(d => d.Canton)
                .ThenInclude(c => c.Provincia)
            .FirstOrDefaultAsync(d => d.Codigo == codigo && d.Activo);
    }
}
