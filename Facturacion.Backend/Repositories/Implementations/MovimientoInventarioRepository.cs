using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class MovimientoInventarioRepository : IMovimientoInventarioRepository
{
    private readonly DataContext _context;

    public MovimientoInventarioRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<MovimientoInventario?> GetAsync(Guid id)
    {
        return await _context.MovimientosInventario
            .Include(m => m.Inventario)
                .ThenInclude(i => i.Producto)
            .Include(m => m.Inventario)
                .ThenInclude(i => i.Sucursal)
            .Include(m => m.SucursalOrigen)
            .Include(m => m.SucursalDestino)
            .Include(m => m.Cliente)
            .Include(m => m.Proveedor)
            .Include(m => m.UsuarioCreacion)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByInventarioAsync(Guid inventarioId)
    {
        return await _context.MovimientosInventario
            .Include(m => m.UsuarioCreacion)
            .Where(m => m.InventarioId == inventarioId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByFechaAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.MovimientosInventario
            .Include(m => m.Inventario)
                .ThenInclude(i => i.Producto)
            .Include(m => m.Inventario)
                .ThenInclude(i => i.Sucursal)
            .Where(m => m.Inventario.Sucursal.EmpresaId == empresaId &&
                       m.Fecha >= fechaInicio &&
                       m.Fecha <= fechaFin)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    public async Task<IEnumerable<MovimientoInventario>> GetByTipoAsync(Guid empresaId, TipoMovimientoInventario tipo)
    {
        return await _context.MovimientosInventario
            .Include(m => m.Inventario)
                .ThenInclude(i => i.Producto)
            .Include(m => m.Inventario)
                .ThenInclude(i => i.Sucursal)
            .Where(m => m.Inventario.Sucursal.EmpresaId == empresaId &&
                       m.TipoMovimiento == tipo)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    public async Task<MovimientoInventario> AddAsync(MovimientoInventario movimiento)
    {
        movimiento.FechaCreacion = DateTime.UtcNow;
        movimiento.Fecha = DateTime.UtcNow;
        _context.MovimientosInventario.Add(movimiento);
        await _context.SaveChangesAsync();
        return movimiento;
    }
}
