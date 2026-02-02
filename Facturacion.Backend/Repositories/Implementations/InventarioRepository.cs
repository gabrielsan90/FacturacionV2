using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class InventarioRepository : IInventarioRepository
{
    private readonly DataContext _context;
    private readonly ILogger<InventarioRepository> _logger;

    public InventarioRepository(DataContext context, ILogger<InventarioRepository> logger)
    {
        _context = context;
        _logger = logger;
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
        inventario.FechaCreacion = FechaCostaRicaHelper.Ahora;
        inventario.UltimaActualizacion = FechaCostaRicaHelper.Ahora;
        _context.Inventarios.Add(inventario);
        await _context.SaveChangesAsync();
        return inventario;
    }

    public async Task UpdateAsync(Inventario inventario)
    {
        inventario.FechaModificacion = FechaCostaRicaHelper.Ahora;
        inventario.UltimaActualizacion = FechaCostaRicaHelper.Ahora;
        _context.Inventarios.Update(inventario);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var inventario = await _context.Inventarios.FindAsync(id);
        if (inventario != null)
        {
            inventario.IsDeleted = true;
            inventario.FechaEliminacion = FechaCostaRicaHelper.Ahora;
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
            inventario.UltimaActualizacion = FechaCostaRicaHelper.Ahora;
            inventario.FechaModificacion = FechaCostaRicaHelper.Ahora;
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
                Fecha = FechaCostaRicaHelper.Ahora,
                SucursalOrigenId = inventario.SucursalId,
                FechaCreacion = FechaCostaRicaHelper.Ahora,
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
            inventarioOrigen.UltimaActualizacion = FechaCostaRicaHelper.Ahora;
            inventarioOrigen.FechaModificacion = FechaCostaRicaHelper.Ahora;
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
                Fecha = FechaCostaRicaHelper.Ahora,
                SucursalOrigenId = inventarioOrigen.SucursalId,
                SucursalDestinoId = sucursalDestinoId,
                FechaCreacion = FechaCostaRicaHelper.Ahora,
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
                    UltimaActualizacion = FechaCostaRicaHelper.Ahora,
                    FechaCreacion = FechaCostaRicaHelper.Ahora,
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
                    Fecha = FechaCostaRicaHelper.Ahora,
                    SucursalOrigenId = inventarioOrigen.SucursalId,
                    SucursalDestinoId = sucursalDestinoId,
                    FechaCreacion = FechaCostaRicaHelper.Ahora,
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
                inventarioDestino.UltimaActualizacion = FechaCostaRicaHelper.Ahora;
                inventarioDestino.FechaModificacion = FechaCostaRicaHelper.Ahora;
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
                    Fecha = FechaCostaRicaHelper.Ahora,
                    SucursalOrigenId = inventarioOrigen.SucursalId,
                    SucursalDestinoId = sucursalDestinoId,
                    FechaCreacion = FechaCostaRicaHelper.Ahora,
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

    /// <summary>
    /// Procesa la venta de un documento, reduciendo el inventario de cada producto
    /// Solo procesa productos que controlan inventario
    /// </summary>
    public async Task<bool> ProcesarVentaDocumentoAsync(Guid documentoId, Guid sucursalId, string userId)
    {
        _logger.LogInformation("=== ProcesarVentaDocumentoAsync INICIO === DocId: {DocId}, SucursalId: {SucId}", documentoId, sucursalId);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Primero, ver todos los detalles del documento
            var todosDetalles = await _context.DocumentoDetalles
                .Include(d => d.Producto)
                .Where(d => d.DocumentoId == documentoId && !d.IsDeleted)
                .ToListAsync();

            _logger.LogInformation("Total detalles en documento: {Count}", todosDetalles.Count);
            foreach (var det in todosDetalles)
            {
                _logger.LogInformation("  Detalle: {Desc}, ProductoId: {ProdId}, Producto: {ProdNombre}, ControlarInventario: {Ctrl}",
                    det.Descripcion,
                    det.ProductoId,
                    det.Producto?.Nombre ?? "NULL",
                    det.Producto?.ControlarInventario ?? false);
            }

            // Obtener los detalles del documento con productos que controlan inventario
            var detalles = await _context.DocumentoDetalles
                .Include(d => d.Producto)
                .Where(d => d.DocumentoId == documentoId &&
                           !d.IsDeleted &&
                           d.ProductoId.HasValue &&
                           d.Producto != null &&
                           d.Producto.ControlarInventario)
                .ToListAsync();

            _logger.LogInformation("Detalles con ControlarInventario=true: {Count}", detalles.Count);

            if (!detalles.Any())
            {
                _logger.LogInformation("No hay productos que controlen inventario, retornando true");
                // No hay productos que controlen inventario
                await transaction.CommitAsync();
                return true;
            }

            // Obtener el documento para la referencia
            var documento = await _context.Documentos
                .FirstOrDefaultAsync(d => d.Id == documentoId);

            var referencia = documento?.NumeroConsecutivo ?? documentoId.ToString();

            foreach (var detalle in detalles)
            {
                _logger.LogInformation("Procesando detalle: {Desc}, ProductoId: {ProdId}, Cantidad: {Cant}",
                    detalle.Descripcion, detalle.ProductoId, detalle.Cantidad);

                // Buscar inventario del producto en la sucursal
                var inventario = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.ProductoId == detalle.ProductoId &&
                                             i.SucursalId == sucursalId &&
                                             !i.IsDeleted);

                if (inventario == null)
                {
                    _logger.LogInformation("No existe inventario para producto {ProdId} en sucursal {SucId}, creando nuevo",
                        detalle.ProductoId, sucursalId);

                    // No existe inventario para este producto en esta sucursal, crear uno con cantidad 0
                    inventario = new Inventario
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = detalle.ProductoId!.Value,
                        SucursalId = sucursalId,
                        CantidadActual = 0,
                        CantidadReservada = 0,
                        UltimaActualizacion = FechaCostaRicaHelper.Ahora,
                        FechaCreacion = FechaCostaRicaHelper.Ahora,
                        UsuarioCreacionId = userId,
                        IsDeleted = false
                    };
                    _context.Inventarios.Add(inventario);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Inventario creado con Id: {InvId}", inventario.Id);
                }
                else
                {
                    _logger.LogInformation("Inventario encontrado: Id={InvId}, CantidadActual={Cant}",
                        inventario.Id, inventario.CantidadActual);
                }

                var cantidadAnterior = inventario.CantidadActual;
                var cantidadNueva = cantidadAnterior - detalle.Cantidad;

                _logger.LogInformation("Actualizando inventario: Anterior={Ant}, Nueva={Nueva} (reducción de {Red})",
                    cantidadAnterior, cantidadNueva, detalle.Cantidad);

                // Actualizar inventario (permitir negativos para control posterior)
                inventario.CantidadActual = cantidadNueva;
                inventario.UltimaActualizacion = FechaCostaRicaHelper.Ahora;
                inventario.FechaModificacion = FechaCostaRicaHelper.Ahora;
                inventario.UsuarioModificacionId = userId;

                // Crear movimiento de inventario por venta
                var movimiento = new MovimientoInventario
                {
                    Id = Guid.NewGuid(),
                    InventarioId = inventario.Id,
                    TipoMovimiento = TipoMovimientoInventario.Venta,
                    Cantidad = -detalle.Cantidad, // Negativo porque es salida
                    CantidadAnterior = cantidadAnterior,
                    CantidadNueva = cantidadNueva,
                    Referencia = referencia,
                    Observaciones = $"Venta - Doc: {referencia}, Producto: {detalle.Descripcion}",
                    Fecha = FechaCostaRicaHelper.Ahora,
                    SucursalOrigenId = sucursalId,
                    DocumentoId = documentoId,
                    FechaCreacion = FechaCostaRicaHelper.Ahora,
                    UsuarioCreacionId = userId
                };

                _logger.LogInformation("Creando movimiento de inventario: Tipo=Venta, Cantidad={Cant}",
                    -detalle.Cantidad);

                _context.MovimientosInventario.Add(movimiento);
                _context.Inventarios.Update(inventario);
            }

            _logger.LogInformation("Guardando cambios en base de datos...");
            await _context.SaveChangesAsync();
            _logger.LogInformation("Haciendo commit de la transacción...");
            await transaction.CommitAsync();
            _logger.LogInformation("=== ProcesarVentaDocumentoAsync FIN - EXITOSO ===");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== ProcesarVentaDocumentoAsync ERROR === {Error}", ex.Message);
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Procesa la devolución de un documento (Nota de Crédito), aumentando el inventario
    /// </summary>
    public async Task<bool> ProcesarDevolucionDocumentoAsync(Guid documentoId, Guid sucursalId, string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var detalles = await _context.DocumentoDetalles
                .Include(d => d.Producto)
                .Where(d => d.DocumentoId == documentoId &&
                           !d.IsDeleted &&
                           d.ProductoId.HasValue &&
                           d.Producto != null &&
                           d.Producto.ControlarInventario)
                .ToListAsync();

            if (!detalles.Any())
            {
                return true;
            }

            var documento = await _context.Documentos
                .FirstOrDefaultAsync(d => d.Id == documentoId);

            var referencia = documento?.NumeroConsecutivo ?? documentoId.ToString();

            foreach (var detalle in detalles)
            {
                var inventario = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.ProductoId == detalle.ProductoId &&
                                             i.SucursalId == sucursalId &&
                                             !i.IsDeleted);

                if (inventario == null)
                {
                    inventario = new Inventario
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = detalle.ProductoId!.Value,
                        SucursalId = sucursalId,
                        CantidadActual = 0,
                        CantidadReservada = 0,
                        UltimaActualizacion = FechaCostaRicaHelper.Ahora,
                        FechaCreacion = FechaCostaRicaHelper.Ahora,
                        UsuarioCreacionId = userId,
                        IsDeleted = false
                    };
                    _context.Inventarios.Add(inventario);
                    await _context.SaveChangesAsync();
                }

                var cantidadAnterior = inventario.CantidadActual;
                var cantidadNueva = cantidadAnterior + detalle.Cantidad; // Suma porque es devolución

                inventario.CantidadActual = cantidadNueva;
                inventario.UltimaActualizacion = FechaCostaRicaHelper.Ahora;
                inventario.FechaModificacion = FechaCostaRicaHelper.Ahora;
                inventario.UsuarioModificacionId = userId;

                var movimiento = new MovimientoInventario
                {
                    Id = Guid.NewGuid(),
                    InventarioId = inventario.Id,
                    TipoMovimiento = TipoMovimientoInventario.DevolucionCliente,
                    Cantidad = detalle.Cantidad, // Positivo porque es entrada
                    CantidadAnterior = cantidadAnterior,
                    CantidadNueva = cantidadNueva,
                    Referencia = referencia,
                    Observaciones = $"Devolución - NC: {referencia}, Producto: {detalle.Descripcion}",
                    Fecha = FechaCostaRicaHelper.Ahora,
                    SucursalOrigenId = sucursalId,
                    DocumentoId = documentoId,
                    FechaCreacion = FechaCostaRicaHelper.Ahora,
                    UsuarioCreacionId = userId
                };

                _context.MovimientosInventario.Add(movimiento);
                _context.Inventarios.Update(inventario);
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
