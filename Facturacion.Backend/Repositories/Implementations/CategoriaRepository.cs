using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly DataContext _context;

    public CategoriaRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Categoria?> GetAsync(Guid id)
    {
        return await _context.Categorias
            .Include(c => c.Empresa)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<IEnumerable<Categoria>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.Categorias
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Categoria?> GetByNombreAsync(Guid empresaId, string nombre)
    {
        return await _context.Categorias
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && 
                                     c.Nombre == nombre && 
                                     !c.IsDeleted);
    }

    public async Task<Categoria> AddAsync(Categoria categoria)
    {
        categoria.FechaCreacion = DateTime.UtcNow;
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        categoria.FechaModificacion = DateTime.UtcNow;
        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria != null)
        {
            categoria.IsDeleted = true;
            categoria.FechaEliminacion = DateTime.UtcNow;
            categoria.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
