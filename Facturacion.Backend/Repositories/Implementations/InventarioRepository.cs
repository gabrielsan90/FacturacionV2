using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class InventarioRepository : IInventarioRepository
{
    private readonly DataContext _context;

    public InventarioRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Inventario?> GetAsync(Guid id)
    {
        return await _context.Inventarios
            .Include(i => i.Producto)
                .ThenInclude(p => p.UnidadMedida)
            .Include(i => i.Sucursal)
                .ThenInclude(s => s.Empresa)
            .Include(i => i.Movimientos.OrderByDescending(m => m.Fecha).Take(10))
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    public async Task<Inventario?> GetByProductoSucursalAsync(Guid productoId, Guid sucursalId)
    {
        return await _context.Inventarios
            .Include(i => i.Producto)
            .Include(i => i.Sucursal)
            .FirstOrDefaultAsync(i => i.ProductoId == productoId && 
                                     i.SucursalId == sucursalId && 
                                     !i.IsDeleted);
    }

    public async Task<IEnumerable<Inventario>> GetBySucursalAsync(Guid sucursalId)
    {
        return await _context.Inventarios
            .Include(i => i.Producto)
                .ThenInclude(p => p.UnidadMedida)
            .Include(i => i.Sucursal)
            .Where(i => i.SucursalId == sucursalId && !i.IsDeleted)
            .OrderBy(i => i.Producto.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventario>> GetByProductoAsync(Guid productoId)
    {
        return await _context.Inventarios
            .Include(i => i.Producto)
            .Include(i => i.Sucursal)
            .Where(i => i.ProductoId == productoId && !i.IsDeleted)
            .OrderBy(i => i.Sucursal.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventario>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.Inventarios
            .Include(i => i.Producto)
                .ThenInclude(p => p.UnidadMedida)
            .Include(i => i.Sucursal)
            .Where(i => i.Sucursal.EmpresaId == empresaId && !i.IsDeleted)
            .OrderBy(i => i.Sucursal.Nombre)
                .ThenBy(i => i.Producto.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventario>> GetBajoStockAsync(Guid empresaId)
    {
        return await _context.Inventarios
            .Include(i => i.Producto)
                .ThenInclude(p => p.UnidadMedida)
            .Include(i => i.Sucursal)
            .Where(i => i.Sucursal.EmpresaId == empresaId && 
                       !i.IsDeleted &&
                       i.Producto.StockMinimo.HasValue &&
                       i.CantidadDisponible <= i.Producto.StockMinimo.Value)
            .OrderBy(i => i.CantidadDisponible)
            .ToListAsync();
    }

    public async Task<Inventario> AddAsync(Inventario inventario)
    {
        inventario.FechaCreacion = DateTime.UtcNow;
        inventario.UltimaActualizacion = DateTime.UtcNow;
        _context.Inventarios.Add(inventario);
        await _context.SaveChangesAsync();
        return inventario;
    }

    public async Task UpdateAsync(Inventario inventario)
    {
        inventario.FechaModificacion = DateTime.UtcNow;
        inventario.UltimaActualizacion = DateTime.UtcNow;
        _context.Inventarios.Update(inventario);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var inventario = await _context.Inventarios.FindAsync(id);
        if (inventario != null)
        {
            inventario.IsDeleted = true;
            inventario.FechaEliminacion = DateTime.UtcNow;
            inventario.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> AjustarInventarioAsync(Guid id, decimal cantidad, string? referencia, string? observaciones, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Obtener el inventario actual
            var inventario = await _context.Inventarios
                .Include(i => i.Producto)
                .Include(i => i.Sucursal)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (inventario == null)
            {
                return false;
            }

            // Validar que la cantidad resultante no sea negativa
            var cantidadNueva = inventario.CantidadActual + cantidad;
            if (cantidadNueva < 0)
            {
                return false;
            }

            var cantidadAnterior = inventario.CantidadActual;

            // Actualizar inventario
            inventario.CantidadActual = cantidadNueva;
            inventario.UltimaActualizacion = DateTime.UtcNow;
            inventario.FechaModificacion = DateTime.UtcNow;
            inventario.UsuarioModificacionId = userId;

            // Crear movimiento de inventario
            var tipoMovimiento = cantidad > 0 ? TipoMovimientoInventario.AjusteEntrada : TipoMovimientoInventario.AjusteSalida;

            var movimiento = new MovimientoInventario
            {
                Id = Guid.NewGuid(),
                InventarioId = inventario.Id,
                TipoMovimiento = tipoMovimiento,
                Cantidad = cantidad,
                CantidadAnterior = cantidadAnterior,
                CantidadNueva = cantidadNueva,
                Referencia = referencia,
                Observaciones = observaciones,
                Fecha = DateTime.UtcNow,
                SucursalOrigenId = inventario.SucursalId,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacionId = userId
            };

            _context.MovimientosInventario.Add(movimiento);
            _context.Inventarios.Update(inventario);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> TrasladarInventarioAsync(Guid inventarioOrigenId, Guid sucursalDestinoId, decimal cantidad, string? referencia, string? observaciones, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Obtener inventario de origen
            var inventarioOrigen = await _context.Inventarios
                .Include(i => i.Producto)
                .Include(i => i.Sucursal)
                .FirstOrDefaultAsync(i => i.Id == inventarioOrigenId && !i.IsDeleted);

            if (inventarioOrigen == null)
            {
                return false;
            }

            // Validar que haya suficiente cantidad disponible
            var cantidadDisponible = inventarioOrigen.CantidadActual - (inventarioOrigen.CantidadReservada ?? 0);
            if (cantidadDisponible < cantidad)
            {
                return false;
            }

            // Validar que la sucursal destino exista
            var sucursalDestino = await _context.Sucursales
                .FirstOrDefaultAsync(s => s.Id == sucursalDestinoId && !s.IsDeleted);

            if (sucursalDestino == null)
            {
                return false;
            }

            // Validar que origen y destino sean diferentes
            if (inventarioOrigen.SucursalId == sucursalDestinoId)
            {
                return false;
            }

            // Validar que ambas sucursales pertenezcan a la misma empresa
            if (inventarioOrigen.Sucursal!.EmpresaId != sucursalDestino.EmpresaId)
            {
                return false;
            }

            // 1. REDUCIR STOCK EN ORIGEN
            var cantidadAnteriorOrigen = inventarioOrigen.CantidadActual;
            var cantidadNuevaOrigen = cantidadAnteriorOrigen - cantidad;

            inventarioOrigen.CantidadActual = cantidadNuevaOrigen;
            inventarioOrigen.UltimaActualizacion = DateTime.UtcNow;
            inventarioOrigen.FechaModificacion = DateTime.UtcNow;
            inventarioOrigen.UsuarioModificacionId = userId;

            // Crear movimiento de salida en origen
            var movimientoSalida = new MovimientoInventario
            {
                Id = Guid.NewGuid(),
                InventarioId = inventarioOrigen.Id,
                TipoMovimiento = TipoMovimientoInventario.TrasladoSalida,
                Cantidad = -cantidad,
                CantidadAnterior = cantidadAnteriorOrigen,
                CantidadNueva = cantidadNuevaOrigen,
                Referencia = referencia,
                Observaciones = $"Traslado a {sucursalDestino.Nombre}. {observaciones}",
                Fecha = DateTime.UtcNow,
                SucursalOrigenId = inventarioOrigen.SucursalId,
                SucursalDestinoId = sucursalDestinoId,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacionId = userId
            };

            _context.MovimientosInventario.Add(movimientoSalida);
            _context.Inventarios.Update(inventarioOrigen);

            // 2. AUMENTAR STOCK EN DESTINO (o crear si no existe)
            var inventarioDestino = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.ProductoId == inventarioOrigen.ProductoId &&
                                         i.SucursalId == sucursalDestinoId &&
                                         !i.IsDeleted);

            if (inventarioDestino == null)
            {
                // Crear nuevo inventario en destino
                inventarioDestino = new Inventario
                {
                    Id = Guid.NewGuid(),
                    ProductoId = inventarioOrigen.ProductoId,
                    SucursalId = sucursalDestinoId,
                    CantidadActual = cantidad,
                    CantidadReservada = 0,
                    UltimaActualizacion = DateTime.UtcNow,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacionId = userId,
                    IsDeleted = false
                };

                _context.Inventarios.Add(inventarioDestino);
                await _context.SaveChangesAsync(); // Guardar para obtener el ID

                // Crear movimiento de entrada en destino
                var movimientoEntrada = new MovimientoInventario
                {
                    Id = Guid.NewGuid(),
                    InventarioId = inventarioDestino.Id,
                    TipoMovimiento = TipoMovimientoInventario.TrasladoEntrada,
                    Cantidad = cantidad,
                    CantidadAnterior = 0,
                    CantidadNueva = cantidad,
                    Referencia = referencia,
                    Observaciones = $"Traslado desde {inventarioOrigen.Sucursal.Nombre}. {observaciones}",
                    Fecha = DateTime.UtcNow,
                    SucursalOrigenId = inventarioOrigen.SucursalId,
                    SucursalDestinoId = sucursalDestinoId,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacionId = userId
                };

                _context.MovimientosInventario.Add(movimientoEntrada);
            }
            else
            {
                // Actualizar inventario existente en destino
                var cantidadAnteriorDestino = inventarioDestino.CantidadActual;
                var cantidadNuevaDestino = cantidadAnteriorDestino + cantidad;

                inventarioDestino.CantidadActual = cantidadNuevaDestino;
                inventarioDestino.UltimaActualizacion = DateTime.UtcNow;
                inventarioDestino.FechaModificacion = DateTime.UtcNow;
                inventarioDestino.UsuarioModificacionId = userId;

                // Crear movimiento de entrada en destino
                var movimientoEntrada = new MovimientoInventario
                {
                    Id = Guid.NewGuid(),
                    InventarioId = inventarioDestino.Id,
                    TipoMovimiento = TipoMovimientoInventario.TrasladoEntrada,
                    Cantidad = cantidad,
                    CantidadAnterior = cantidadAnteriorDestino,
                    CantidadNueva = cantidadNuevaDestino,
                    Referencia = referencia,
                    Observaciones = $"Traslado desde {inventarioOrigen.Sucursal.Nombre}. {observaciones}",
                    Fecha = DateTime.UtcNow,
                    SucursalOrigenId = inventarioOrigen.SucursalId,
                    SucursalDestinoId = sucursalDestinoId,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacionId = userId
                };

                _context.MovimientosInventario.Add(movimientoEntrada);
                _context.Inventarios.Update(inventarioDestino);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
