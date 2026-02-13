using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class CuentaContableRepository : ICuentaContableRepository
{
    private readonly DataContext _context;

    public CuentaContableRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<CuentaContable?> GetAsync(Guid id)
    {
        return await _context.CuentasContables
            .Include(c => c.Empresa)
            .Include(c => c.CuentaPadre)
            .Include(c => c.CreadoPor)
            .Include(c => c.ModificadoPor)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<IEnumerable<CuentaContable>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.CuentasContables
            .Include(c => c.CuentaPadre)
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
            .OrderBy(c => c.Codigo)
            .ToListAsync();
    }

    public async Task<CuentaContable?> GetByCodigoAsync(Guid empresaId, string codigo)
    {
        return await _context.CuentasContables
            .Include(c => c.CuentaPadre)
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId &&
                                     c.Codigo == codigo &&
                                     !c.IsDeleted);
    }

    public async Task<IEnumerable<CuentaContable>> GetArbolAsync(Guid empresaId)
    {
        return await _context.CuentasContables
            .Include(c => c.CuentaPadre)
            .Include(c => c.CuentasHijas)
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
            .OrderBy(c => c.Codigo)
            .ToListAsync();
    }

    public async Task<CuentaContable> AddAsync(CuentaContable cuenta)
    {
        cuenta.FechaCreacion = FechaCostaRicaHelper.Ahora;
        _context.CuentasContables.Add(cuenta);
        await _context.SaveChangesAsync();
        return cuenta;
    }

    public async Task UpdateAsync(CuentaContable cuenta)
    {
        cuenta.FechaModificacion = FechaCostaRicaHelper.Ahora;
        _context.CuentasContables.Update(cuenta);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var cuenta = await _context.CuentasContables.FindAsync(id);
        if (cuenta != null)
        {
            cuenta.IsDeleted = true;
            cuenta.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            cuenta.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleActivaAsync(Guid id)
    {
        var cuenta = await _context.CuentasContables.FindAsync(id);
        if (cuenta != null)
        {
            cuenta.Activo = !cuenta.Activo;
            cuenta.FechaModificacion = FechaCostaRicaHelper.Ahora;
            await _context.SaveChangesAsync();
        }
    }
}
