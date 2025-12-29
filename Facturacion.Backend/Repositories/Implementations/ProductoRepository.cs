using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class ProductoRepository : IProductoRepository
{
    private readonly DataContext _context;

    public ProductoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Producto?> GetAsync(Guid id)
    {
        return await _context.Productos
            .Include(p => p.Empresa)
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .Include(p => p.Impuesto)
            .Include(p => p.Cabys)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<IEnumerable<Producto>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .Include(p => p.Impuesto)
            .Include(p => p.Cabys)
            .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Producto?> GetByCodigoAsync(Guid empresaId, string codigo)
    {
        return await _context.Productos
            .FirstOrDefaultAsync(p => p.EmpresaId == empresaId && 
                                     p.Codigo == codigo && 
                                     !p.IsDeleted);
    }

    public async Task<IEnumerable<Producto>> GetByCategoriaAsync(Guid categoriaId)
    {
        return await _context.Productos
            .Include(p => p.UnidadMedida)
            .Include(p => p.Impuesto)
            .Where(p => p.CategoriaId == categoriaId && !p.IsDeleted)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Producto> AddAsync(Producto producto)
    {
        producto.FechaCreacion = DateTime.UtcNow;
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return producto;
    }

    public async Task UpdateAsync(Producto producto)
    {
        producto.FechaModificacion = DateTime.UtcNow;

        // Detach any existing tracked entity with the same key to avoid conflicts
        var existingEntry = _context.ChangeTracker.Entries<Producto>()
            .FirstOrDefault(e => e.Entity.Id == producto.Id);
        if (existingEntry != null)
        {
            existingEntry.State = EntityState.Detached;
        }

        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            producto.IsDeleted = true;
            producto.FechaEliminacion = DateTime.UtcNow;
            producto.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
