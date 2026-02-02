using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class ConsecutivoRepository : IConsecutivoRepository
{
    private readonly DataContext _context;

    public ConsecutivoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Consecutivo?> GetAsync(Guid id)
    {
        return await _context.Consecutivos
            .Include(c => c.Terminal)
                .ThenInclude(t => t!.Sucursal)
                    .ThenInclude(s => s!.Empresa)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<IEnumerable<Consecutivo>> GetByTerminalAsync(Guid terminalId)
    {
        return await _context.Consecutivos
            .Include(c => c.Terminal)
            .Where(c => c.TerminalId == terminalId && !c.IsDeleted)
            .OrderBy(c => c.TipoDocumento)
            .ToListAsync();
    }

    public async Task<IEnumerable<Consecutivo>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.Consecutivos
            .Include(c => c.Terminal)
                .ThenInclude(t => t!.Sucursal)
            .Where(c => c.Terminal!.Sucursal!.EmpresaId == empresaId && !c.IsDeleted)
            .OrderBy(c => c.Terminal!.Sucursal!.Nombre)
            .ThenBy(c => c.Terminal!.Nombre)
            .ThenBy(c => c.TipoDocumento)
            .ToListAsync();
    }

    public async Task<Consecutivo?> GetByClaveAsync(string claveNumeracion)
    {
        return await _context.Consecutivos
            .FirstOrDefaultAsync(c => c.ClaveNumeracion == claveNumeracion && !c.IsDeleted);
    }

    public async Task<Consecutivo?> GetByTipoDocumentoAsync(Guid terminalId, string tipoDocumento, Ambiente ambiente)
    {
        return await _context.Consecutivos
            .FirstOrDefaultAsync(c => c.TerminalId == terminalId &&
                                     c.TipoDocumento == tipoDocumento &&
                                     c.Ambiente == ambiente &&
                                     !c.IsDeleted &&
                                     c.Activo);
    }

    public async Task<Consecutivo> AddAsync(Consecutivo consecutivo)
    {
        consecutivo.FechaCreacion = FechaCostaRicaHelper.Ahora;
        _context.Consecutivos.Add(consecutivo);
        await _context.SaveChangesAsync();
        return consecutivo;
    }

    public async Task UpdateAsync(Consecutivo consecutivo)
    {
        consecutivo.FechaModificacion = FechaCostaRicaHelper.Ahora;

        // Detach any existing tracked entity with the same key to avoid tracking conflicts
        var existingEntry = _context.ChangeTracker.Entries<Consecutivo>()
            .FirstOrDefault(e => e.Entity.Id == consecutivo.Id);

        if (existingEntry != null)
        {
            existingEntry.State = EntityState.Detached;
        }

        _context.Consecutivos.Update(consecutivo);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var consecutivo = await _context.Consecutivos.FindAsync(id);
        if (consecutivo != null)
        {
            consecutivo.IsDeleted = true;
            consecutivo.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            consecutivo.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<long> IncrementAsync(Guid id)
    {
        var consecutivo = await _context.Consecutivos.FindAsync(id);
        if (consecutivo != null && !consecutivo.IsDeleted && consecutivo.Activo)
        {
            if (consecutivo.NumeroActual >= consecutivo.NumeroFin)
            {
                throw new InvalidOperationException("Se ha alcanzado el límite del rango de numeración consecutiva.");
            }

            consecutivo.NumeroActual++;
            await _context.SaveChangesAsync();
            return consecutivo.NumeroActual;
        }
        throw new InvalidOperationException("Consecutivo no encontrado, está eliminado o inactivo.");
    }
}
