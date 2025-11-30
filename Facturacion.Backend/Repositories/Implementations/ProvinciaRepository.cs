using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class ProvinciaRepository : IProvinciaRepository
{
    private readonly DataContext _context;

    public ProvinciaRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Provincia?> GetAsync(int id)
    {
        return await _context.Provincias
            .FirstOrDefaultAsync(p => p.Id == id && p.Activo);
    }

    public async Task<IEnumerable<Provincia>> GetAllAsync()
    {
        return await _context.Provincias
            .Where(p => p.Activo)
            .OrderBy(p => p.Codigo)
            .ToListAsync();
    }

    public async Task<Provincia?> GetByCodigoAsync(string codigo)
    {
        return await _context.Provincias
            .FirstOrDefaultAsync(p => p.Codigo == codigo && p.Activo);
    }
}
