using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class CategoriaGastoRepository : ICategoriaGastoRepository
{
    private readonly DataContext _context;

    public CategoriaGastoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<CategoriaGasto?> GetAsync(int id)
    {
        return await _context.CategoriasGasto
            .Include(c => c.Gastos)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<CategoriaGasto>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.CategoriasGasto.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.Activa);
        }

        return await query
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<CategoriaGasto>> GetActivasAsync()
    {
        return await _context.CategoriasGasto
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<CategoriaGasto?> GetByNombreAsync(string nombre)
    {
        return await _context.CategoriasGasto
            .FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower());
    }

    public async Task<CategoriaGasto> AddAsync(CategoriaGasto categoria)
    {
        _context.CategoriasGasto.Add(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task UpdateAsync(CategoriaGasto categoria)
    {
        _context.CategoriasGasto.Update(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var categoria = await _context.CategoriasGasto.FindAsync(id);
        if (categoria != null)
        {
            categoria.Activa = false;
            await _context.SaveChangesAsync();
        }
    }
}
