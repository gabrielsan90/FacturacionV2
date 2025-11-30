using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class CantonRepository : ICantonRepository
{
    private readonly DataContext _context;

    public CantonRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Canton?> GetAsync(int id)
    {
        return await _context.Cantones
            .Include(c => c.Provincia)
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
    }

    public async Task<IEnumerable<Canton>> GetAllAsync()
    {
        return await _context.Cantones
            .Include(c => c.Provincia)
            .Where(c => c.Activo)
            .OrderBy(c => c.ProvinciaId)
            .ThenBy(c => c.Codigo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Canton>> GetByProvinciaAsync(int provinciaId)
    {
        return await _context.Cantones
            .Where(c => c.ProvinciaId == provinciaId && c.Activo)
            .OrderBy(c => c.Codigo)
            .ToListAsync();
    }

    public async Task<Canton?> GetByCodigoAsync(string codigo)
    {
        return await _context.Cantones
            .Include(c => c.Provincia)
            .FirstOrDefaultAsync(c => c.Codigo == codigo && c.Activo);
    }
}
