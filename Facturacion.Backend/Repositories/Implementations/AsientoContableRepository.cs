using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class AsientoContableRepository : IAsientoContableRepository
{
    private readonly DataContext _context;

    public AsientoContableRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<AsientoContable?> GetAsync(Guid id)
    {
        return await _context.AsientosContables
            .Include(a => a.Empresa)
            .Include(a => a.PeriodoContable)
            .Include(a => a.CreadoPor)
            .Include(a => a.ModificadoPor)
            .Include(a => a.AprobadoPor)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<AsientoContable?> GetWithMovimientosAsync(Guid id)
    {
        return await _context.AsientosContables
            .Include(a => a.Empresa)
            .Include(a => a.PeriodoContable)
            .Include(a => a.CreadoPor)
            .Include(a => a.ModificadoPor)
            .Include(a => a.AprobadoPor)
            .Include(a => a.Movimientos!)
                .ThenInclude(m => m.CuentaContable)
            .Include(a => a.Movimientos!)
                .ThenInclude(m => m.Cliente)
            .Include(a => a.Movimientos!)
                .ThenInclude(m => m.Proveedor)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<IEnumerable<AsientoContable>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.AsientosContables
            .Include(a => a.PeriodoContable)
            .Include(a => a.CreadoPor)
            .Include(a => a.AprobadoPor)
            .Where(a => a.EmpresaId == empresaId && !a.IsDeleted)
            .OrderByDescending(a => a.Fecha)
            .ThenByDescending(a => a.Numero)
            .ToListAsync();
    }

    public async Task<IEnumerable<AsientoContable>> GetByPeriodoAsync(Guid periodoContableId)
    {
        return await _context.AsientosContables
            .Include(a => a.PeriodoContable)
            .Include(a => a.CreadoPor)
            .Include(a => a.AprobadoPor)
            .Where(a => a.PeriodoContableId == periodoContableId && !a.IsDeleted)
            .OrderBy(a => a.Numero)
            .ToListAsync();
    }

    public async Task<AsientoContable> AddAsync(AsientoContable asiento)
    {
        asiento.FechaCreacion = FechaCostaRicaHelper.Ahora;
        _context.AsientosContables.Add(asiento);
        await _context.SaveChangesAsync();
        return asiento;
    }

    public async Task UpdateAsync(AsientoContable asiento)
    {
        asiento.FechaModificacion = FechaCostaRicaHelper.Ahora;

        var asientoExistente = await _context.AsientosContables
            .Include(a => a.Movimientos)
            .FirstOrDefaultAsync(a => a.Id == asiento.Id);

        if (asientoExistente != null)
        {
            _context.Entry(asientoExistente).CurrentValues.SetValues(asiento);

            // Manejar movimientos: eliminar los que ya no están y agregar/actualizar los nuevos
            if (asiento.Movimientos != null)
            {
                // Eliminar movimientos que ya no están en la lista
                var movimientosAEliminar = asientoExistente.Movimientos!
                    .Where(m => !asiento.Movimientos.Any(nm => nm.Id == m.Id))
                    .ToList();

                foreach (var movimiento in movimientosAEliminar)
                {
                    _context.MovimientosContables.Remove(movimiento);
                }

                // Agregar o actualizar movimientos
                foreach (var movimiento in asiento.Movimientos)
                {
                    movimiento.AsientoContableId = asiento.Id;

                    if (movimiento.Id == Guid.Empty)
                    {
                        // Nuevo movimiento
                        asientoExistente.Movimientos!.Add(movimiento);
                    }
                    else
                    {
                        // Actualizar movimiento existente
                        var movimientoExistente = asientoExistente.Movimientos!
                            .FirstOrDefault(m => m.Id == movimiento.Id);
                        if (movimientoExistente != null)
                        {
                            _context.Entry(movimientoExistente).CurrentValues.SetValues(movimiento);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
        else
        {
            _context.AsientosContables.Update(asiento);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var asiento = await _context.AsientosContables.FindAsync(id);
        if (asiento != null)
        {
            asiento.IsDeleted = true;
            asiento.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            asiento.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task AprobarAsync(Guid id, string userId)
    {
        var asiento = await _context.AsientosContables.FindAsync(id);
        if (asiento != null && asiento.Estado == "BOR")
        {
            asiento.Estado = "APR";
            asiento.FechaAprobacion = FechaCostaRicaHelper.Ahora;
            asiento.AprobadoPorId = userId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task AnularAsync(Guid id, string userId)
    {
        var asiento = await _context.AsientosContables.FindAsync(id);
        if (asiento != null && asiento.Estado == "APR")
        {
            asiento.Estado = "ANU";
            asiento.FechaModificacion = FechaCostaRicaHelper.Ahora;
            asiento.ModificadoPorId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
