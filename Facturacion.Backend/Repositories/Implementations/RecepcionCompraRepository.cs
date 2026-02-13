using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class RecepcionCompraRepository : IRecepcionCompraRepository
{
    private readonly DataContext _context;
    private readonly ILogger<RecepcionCompraRepository> _logger;

    public RecepcionCompraRepository(DataContext context, ILogger<RecepcionCompraRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<RecepcionCompra>> GetAsync(Guid id)
    {
        try
        {
            var recepcion = await _context.RecepcionesCompra
                .Include(r => r.OrdenCompra)
                    .ThenInclude(o => o!.Proveedor)
                .Include(r => r.Bodega)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (recepcion == null)
            {
                return new ActionResponse<RecepcionCompra>
                {
                    WasSuccess = false,
                    Message = "Recepción de compra no encontrada"
                };
            }

            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = true,
                Result = recepcion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recepción de compra {Id}", id);
            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<RecepcionCompra>> GetWithDetallesAsync(Guid id)
    {
        try
        {
            var recepcion = await _context.RecepcionesCompra
                .Include(r => r.OrdenCompra)
                    .ThenInclude(o => o!.Proveedor)
                .Include(r => r.Bodega)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.OrdenCompraDetalle)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (recepcion == null)
            {
                return new ActionResponse<RecepcionCompra>
                {
                    WasSuccess = false,
                    Message = "Recepción de compra no encontrada"
                };
            }

            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = true,
                Result = recepcion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recepción de compra con detalles {Id}", id);
            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<RecepcionCompra>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var recepciones = await _context.RecepcionesCompra
                .Include(r => r.OrdenCompra)
                    .ThenInclude(o => o!.Proveedor)
                .Include(r => r.Bodega)
                .Where(r => r.EmpresaId == empresaId && !r.IsDeleted)
                .OrderByDescending(r => r.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<RecepcionCompra>>
            {
                WasSuccess = true,
                Result = recepciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recepciones de compra de empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<RecepcionCompra>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<RecepcionCompra>>> GetByOrdenCompraAsync(Guid ordenCompraId)
    {
        try
        {
            var recepciones = await _context.RecepcionesCompra
                .Include(r => r.Bodega)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(r => r.OrdenCompraId == ordenCompraId && !r.IsDeleted)
                .OrderByDescending(r => r.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<RecepcionCompra>>
            {
                WasSuccess = true,
                Result = recepciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recepciones de orden de compra {OrdenCompraId}", ordenCompraId);
            return new ActionResponse<IEnumerable<RecepcionCompra>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<RecepcionCompra>>> GetPendientesAsync(Guid empresaId)
    {
        try
        {
            // Recepciones en estado APL que aún no han completado la orden
            var recepciones = await _context.RecepcionesCompra
                .Include(r => r.OrdenCompra)
                    .ThenInclude(o => o!.Proveedor)
                .Include(r => r.Bodega)
                .Where(r => r.EmpresaId == empresaId &&
                           r.Estado == "APL" &&
                           r.OrdenCompra!.Estado == "PAR" &&
                           !r.IsDeleted)
                .OrderByDescending(r => r.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<RecepcionCompra>>
            {
                WasSuccess = true,
                Result = recepciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recepciones pendientes");
            return new ActionResponse<IEnumerable<RecepcionCompra>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> ValidarCantidadesAsync(Guid ordenCompraId, List<RecepcionCompraDetalle> detalles)
    {
        try
        {
            var ordenCompra = await _context.OrdenesCompra
                .Include(o => o.Detalles)
                .FirstOrDefaultAsync(o => o.Id == ordenCompraId && !o.IsDeleted);

            if (ordenCompra == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Orden de compra no encontrada"
                };
            }

            foreach (var detalle in detalles)
            {
                var detalleOrden = ordenCompra.Detalles?.FirstOrDefault(d => d.Id == detalle.OrdenCompraDetalleId);
                if (detalleOrden == null)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = $"Detalle de orden de compra no encontrado para producto {detalle.ProductoId}"
                    };
                }

                // Validar que la cantidad a recibir no exceda la cantidad pendiente
                var cantidadPendiente = detalleOrden.CantidadPendiente;
                if (detalle.CantidadRecibida > cantidadPendiente)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = $"La cantidad a recibir ({detalle.CantidadRecibida}) excede la cantidad pendiente ({cantidadPendiente}) para el producto"
                    };
                }
            }

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar cantidades de recepción");
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<RecepcionCompra>> AddAsync(RecepcionCompra recepcion)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Validar que la orden de compra exista y esté aprobada
            var ordenCompra = await _context.OrdenesCompra
                .Include(o => o.Detalles)
                .FirstOrDefaultAsync(o => o.Id == recepcion.OrdenCompraId && !o.IsDeleted);

            if (ordenCompra == null)
            {
                return new ActionResponse<RecepcionCompra>
                {
                    WasSuccess = false,
                    Message = "Orden de compra no encontrada"
                };
            }

            if (ordenCompra.Estado != "APR" && ordenCompra.Estado != "PAR")
            {
                return new ActionResponse<RecepcionCompra>
                {
                    WasSuccess = false,
                    Message = $"La orden de compra debe estar aprobada o parcialmente recibida. Estado actual: {ordenCompra.EstadoDescripcion}"
                };
            }

            // Validar cantidades
            if (recepcion.Detalles != null && recepcion.Detalles.Any())
            {
                var validacion = await ValidarCantidadesAsync(recepcion.OrdenCompraId, recepcion.Detalles.ToList());
                if (!validacion.WasSuccess)
                {
                    return new ActionResponse<RecepcionCompra>
                    {
                        WasSuccess = false,
                        Message = validacion.Message
                    };
                }
            }

            // Generar número si no tiene
            if (string.IsNullOrWhiteSpace(recepcion.Numero))
            {
                var numeroResponse = await GenerarNumeroAsync(recepcion.EmpresaId);
                if (!numeroResponse.WasSuccess)
                {
                    return new ActionResponse<RecepcionCompra>
                    {
                        WasSuccess = false,
                        Message = numeroResponse.Message
                    };
                }
                recepcion.Numero = numeroResponse.Result!;
            }

            // Establecer fechas de auditoría
            recepcion.FechaCreacion = DateTime.Now;
            recepcion.Estado = "APL";

            _context.RecepcionesCompra.Add(recepcion);
            await _context.SaveChangesAsync();

            // Actualizar cantidades recibidas en la orden de compra
            if (recepcion.Detalles != null)
            {
                foreach (var detalle in recepcion.Detalles)
                {
                    var detalleOrden = ordenCompra.Detalles?.FirstOrDefault(d => d.Id == detalle.OrdenCompraDetalleId);
                    if (detalleOrden != null)
                    {
                        detalleOrden.CantidadRecibida += detalle.CantidadRecibida;
                    }
                }

                // Verificar si la orden está completada
                bool todasLasLineasCompletas = ordenCompra.Detalles?.All(d => d.EstaCompleto) ?? false;
                if (todasLasLineasCompletas)
                {
                    ordenCompra.Estado = "COM"; // Completada
                }
                else
                {
                    ordenCompra.Estado = "PAR"; // Parcial
                }

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            _logger.LogInformation("Recepción de compra creada: {Numero} para OC: {OrdenNumero}",
                recepcion.Numero, ordenCompra.Numero);

            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = true,
                Result = recepcion
            };
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error de base de datos al crear recepción de compra");
            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al crear recepción de compra");
            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<RecepcionCompra>> UpdateAsync(RecepcionCompra recepcion)
    {
        try
        {
            var recepcionExistente = await _context.RecepcionesCompra
                .FirstOrDefaultAsync(r => r.Id == recepcion.Id && !r.IsDeleted);

            if (recepcionExistente == null)
            {
                return new ActionResponse<RecepcionCompra>
                {
                    WasSuccess = false,
                    Message = "Recepción de compra no encontrada"
                };
            }

            // Validar que se pueda editar (solo en estado APL)
            if (recepcionExistente.Estado != "APL")
            {
                return new ActionResponse<RecepcionCompra>
                {
                    WasSuccess = false,
                    Message = $"No se puede modificar una recepción en estado {recepcionExistente.EstadoDescripcion}"
                };
            }

            // Actualizar solo propiedades escalares
            recepcionExistente.NumeroFacturaProveedor = recepcion.NumeroFacturaProveedor;
            recepcionExistente.FechaFacturaProveedor = recepcion.FechaFacturaProveedor;
            recepcionExistente.Observaciones = recepcion.Observaciones;
            recepcionExistente.FechaModificacion = DateTime.Now;
            recepcionExistente.ModificadoPorId = recepcion.ModificadoPorId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Recepción de compra actualizada: {Numero}", recepcionExistente.Numero);

            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = true,
                Result = recepcionExistente
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al actualizar recepción de compra");
            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar recepción de compra");
            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId)
    {
        try
        {
            var recepcion = await _context.RecepcionesCompra
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (recepcion == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Recepción de compra no encontrada"
                };
            }

            // Solo se pueden eliminar recepciones anuladas
            if (recepcion.Estado != "ANU")
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Solo se pueden eliminar recepciones anuladas. Debe anular la recepción primero."
                };
            }

            // Soft delete
            recepcion.IsDeleted = true;
            recepcion.FechaEliminacion = DateTime.Now;
            recepcion.UsuarioEliminacionId = usuarioId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Recepción de compra eliminada: {Numero}", recepcion.Numero);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar recepción de compra");
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<RecepcionCompra>> AnularAsync(Guid id, string usuarioId, string motivo)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var recepcion = await _context.RecepcionesCompra
                .Include(r => r.Detalles)
                .Include(r => r.OrdenCompra)
                    .ThenInclude(o => o!.Detalles)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (recepcion == null)
            {
                return new ActionResponse<RecepcionCompra>
                {
                    WasSuccess = false,
                    Message = "Recepción de compra no encontrada"
                };
            }

            // Validar que esté en estado APL
            if (recepcion.Estado != "APL")
            {
                return new ActionResponse<RecepcionCompra>
                {
                    WasSuccess = false,
                    Message = $"No se puede anular una recepción en estado {recepcion.EstadoDescripcion}"
                };
            }

            // Revertir las cantidades recibidas en la orden de compra
            if (recepcion.Detalles != null && recepcion.OrdenCompra?.Detalles != null)
            {
                foreach (var detalle in recepcion.Detalles)
                {
                    var detalleOrden = recepcion.OrdenCompra.Detalles.FirstOrDefault(d => d.Id == detalle.OrdenCompraDetalleId);
                    if (detalleOrden != null)
                    {
                        detalleOrden.CantidadRecibida -= detalle.CantidadRecibida;
                        if (detalleOrden.CantidadRecibida < 0)
                        {
                            detalleOrden.CantidadRecibida = 0;
                        }
                    }
                }

                // Actualizar estado de la orden
                bool todasLasLineasCompletas = recepcion.OrdenCompra.Detalles.All(d => d.EstaCompleto);
                bool algunaLineaParcial = recepcion.OrdenCompra.Detalles.Any(d => d.CantidadRecibida > 0);

                if (todasLasLineasCompletas)
                {
                    recepcion.OrdenCompra.Estado = "COM";
                }
                else if (algunaLineaParcial)
                {
                    recepcion.OrdenCompra.Estado = "PAR";
                }
                else
                {
                    recepcion.OrdenCompra.Estado = "APR";
                }
            }

            // Cambiar estado a ANU (Anulada)
            recepcion.Estado = "ANU";
            recepcion.FechaModificacion = DateTime.Now;
            recepcion.ModificadoPorId = usuarioId;
            recepcion.Observaciones = string.IsNullOrWhiteSpace(recepcion.Observaciones)
                ? $"Anulada: {motivo}"
                : $"{recepcion.Observaciones}\nAnulada: {motivo}";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Recepción de compra anulada: {Numero} por usuario {UsuarioId}. Motivo: {Motivo}",
                recepcion.Numero, usuarioId, motivo);

            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = true,
                Result = recepcion
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al anular recepción de compra");
            return new ActionResponse<RecepcionCompra>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<string>> GenerarNumeroAsync(Guid empresaId)
    {
        try
        {
            var fecha = DateTime.Now;
            var prefijo = $"RC-{fecha:yyyyMMdd}";

            // Buscar el último número del día
            var ultimaRecepcion = await _context.RecepcionesCompra
                .Where(r => r.EmpresaId == empresaId && r.Numero.StartsWith(prefijo))
                .OrderByDescending(r => r.Numero)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;

            if (ultimaRecepcion != null)
            {
                // Extraer el número secuencial del formato RC-YYYYMMDD-####
                var partes = ultimaRecepcion.Numero.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int numeroActual))
                {
                    siguienteNumero = numeroActual + 1;
                }
            }

            var numeroGenerado = $"{prefijo}-{siguienteNumero:D4}";

            return new ActionResponse<string>
            {
                WasSuccess = true,
                Result = numeroGenerado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar número de recepción de compra");
            return new ActionResponse<string>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
