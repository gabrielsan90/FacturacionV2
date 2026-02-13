using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class CentroCostoRepository : ICentroCostoRepository
{
    private readonly DataContext _context;

    public CentroCostoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<CentroCosto?> GetAsync(Guid id)
    {
        return await _context.CentrosCosto
            .Include(c => c.Empresa)
            .Include(c => c.Padre)
            .Include(c => c.Sucursal)
            .Include(c => c.Departamento)
            .Include(c => c.Responsable)
            .Include(c => c.CreadoPor)
            .Include(c => c.ModificadoPor)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<IEnumerable<CentroCosto>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.CentrosCosto
            .Include(c => c.Padre)
            .Include(c => c.Sucursal)
            .Include(c => c.Departamento)
            .Include(c => c.Responsable)
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
            .OrderBy(c => c.Codigo)
            .ToListAsync();
    }

    public async Task<CentroCosto> AddAsync(CentroCosto centroCosto)
    {
        centroCosto.FechaCreacion = FechaCostaRicaHelper.Ahora;
        _context.CentrosCosto.Add(centroCosto);
        await _context.SaveChangesAsync();
        return centroCosto;
    }

    public async Task UpdateAsync(CentroCosto centroCosto)
    {
        centroCosto.FechaModificacion = FechaCostaRicaHelper.Ahora;
        _context.CentrosCosto.Update(centroCosto);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var centroCosto = await _context.CentrosCosto.FindAsync(id);
        if (centroCosto != null)
        {
            centroCosto.IsDeleted = true;
            centroCosto.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            centroCosto.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
