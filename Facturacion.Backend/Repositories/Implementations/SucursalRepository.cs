using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class SucursalRepository : ISucursalRepository
{
    private readonly DataContext _context;

    public SucursalRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Sucursal?> GetAsync(Guid id)
    {
        return await _context.Sucursales
            .Include(s => s.Empresa)
            .Include(s => s.Terminales)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<Sucursal>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.Sucursales
            .Include(s => s.Empresa)
            .Where(s => s.EmpresaId == empresaId && !s.IsDeleted)
            .OrderBy(s => s.Nombre)
            .ToListAsync();
    }

    public async Task<Sucursal?> GetByCodigoAsync(Guid empresaId, string codigo)
    {
        return await _context.Sucursales
            .FirstOrDefaultAsync(s => s.EmpresaId == empresaId && 
                                     s.Codigo == codigo && 
                                     !s.IsDeleted);
    }

    public async Task<Sucursal?> GetPrincipalAsync(Guid empresaId)
    {
        return await _context.Sucursales
            .FirstOrDefaultAsync(s => s.EmpresaId == empresaId && 
                                     s.EsPrincipal && 
                                     !s.IsDeleted);
    }

    public async Task<Sucursal> AddAsync(Sucursal sucursal)
    {
        sucursal.FechaCreacion = DateTime.UtcNow;
        _context.Sucursales.Add(sucursal);
        await _context.SaveChangesAsync();
        return sucursal;
    }

    public async Task UpdateAsync(Sucursal sucursal)
    {
        sucursal.FechaModificacion = DateTime.UtcNow;
        _context.Sucursales.Update(sucursal);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var sucursal = await _context.Sucursales.FindAsync(id);
        if (sucursal != null)
        {
            sucursal.IsDeleted = true;
            sucursal.FechaEliminacion = DateTime.UtcNow;
            sucursal.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
