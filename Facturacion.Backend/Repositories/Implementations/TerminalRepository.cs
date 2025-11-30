using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class TerminalRepository : ITerminalRepository
{
    private readonly DataContext _context;

    public TerminalRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Terminal?> GetAsync(Guid id)
    {
        return await _context.Terminales
            .Include(t => t.Sucursal)
                .ThenInclude(s => s.Empresa)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<IEnumerable<Terminal>> GetBySucursalAsync(Guid sucursalId)
    {
        return await _context.Terminales
            .Include(t => t.Sucursal)
            .Where(t => t.SucursalId == sucursalId && !t.IsDeleted)
            .OrderBy(t => t.Codigo)
            .ToListAsync();
    }

    public async Task<Terminal?> GetByCodigoAsync(Guid sucursalId, string codigo)
    {
        return await _context.Terminales
            .FirstOrDefaultAsync(t => t.SucursalId == sucursalId && 
                                     t.Codigo == codigo && 
                                     !t.IsDeleted);
    }

    public async Task<Terminal> AddAsync(Terminal terminal)
    {
        terminal.FechaCreacion = DateTime.UtcNow;
        _context.Terminales.Add(terminal);
        await _context.SaveChangesAsync();
        return terminal;
    }

    public async Task UpdateAsync(Terminal terminal)
    {
        terminal.FechaModificacion = DateTime.UtcNow;
        _context.Terminales.Update(terminal);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var terminal = await _context.Terminales.FindAsync(id);
        if (terminal != null)
        {
            terminal.IsDeleted = true;
            terminal.FechaEliminacion = DateTime.UtcNow;
            terminal.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
