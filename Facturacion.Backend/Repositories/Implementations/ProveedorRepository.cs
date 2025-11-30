using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class ProveedorRepository : IProveedorRepository
{
    private readonly DataContext _context;

    public ProveedorRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Proveedor?> GetAsync(Guid id)
    {
        return await _context.Proveedores
            .Include(p => p.Empresa)
            .Include(p => p.Telefonos)
            .Include(p => p.Emails)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<IEnumerable<Proveedor>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.Proveedores
            .Include(p => p.Empresa)
            .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Proveedor?> GetByIdentificationAsync(Guid empresaId, string numeroIdentificacion)
    {
        return await _context.Proveedores
            .FirstOrDefaultAsync(p => p.EmpresaId == empresaId && 
                                     p.NumeroIdentificacion == numeroIdentificacion && 
                                     !p.IsDeleted);
    }

    public async Task<Proveedor> AddAsync(Proveedor proveedor)
    {
        proveedor.FechaCreacion = DateTime.UtcNow;
        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();
        return proveedor;
    }

    public async Task UpdateAsync(Proveedor proveedor)
    {
        proveedor.FechaModificacion = DateTime.UtcNow;
        _context.Proveedores.Update(proveedor);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor != null)
        {
            proveedor.IsDeleted = true;
            proveedor.FechaEliminacion = DateTime.UtcNow;
            proveedor.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
