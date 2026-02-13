using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class OrdenCompraRepository : IOrdenCompraRepository
{
    private readonly DataContext _context;
    private readonly ILogger<OrdenCompraRepository> _logger;

    public OrdenCompraRepository(DataContext context, ILogger<OrdenCompraRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<OrdenCompra>> GetAsync(Guid id)
    {
        try
        {
            var orden = await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Sucursal)
                .Include(o => o.BodegaDestino)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (orden == null)
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = "Orden de compra no encontrada"
                };
            }

            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = true,
                Result = orden
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener orden de compra {Id}", id);
            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<OrdenCompra>> GetWithDetallesAsync(Guid id)
    {
        try
        {
            var orden = await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Sucursal)
                .Include(o => o.BodegaDestino)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(o => o.Recepciones)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (orden == null)
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = "Orden de compra no encontrada"
                };
            }

            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = true,
                Result = orden
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener orden de compra con detalles {Id}", id);
            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<OrdenCompra>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var ordenes = await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Sucursal)
                .Include(o => o.BodegaDestino)
                .Where(o => o.EmpresaId == empresaId && !o.IsDeleted)
                .OrderByDescending(o => o.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<OrdenCompra>>
            {
                WasSuccess = true,
                Result = ordenes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes de compra de empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<OrdenCompra>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<OrdenCompra>>> GetByProveedorAsync(Guid proveedorId)
    {
        try
        {
            var ordenes = await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Sucursal)
                .Include(o => o.BodegaDestino)
                .Where(o => o.ProveedorId == proveedorId && !o.IsDeleted)
                .OrderByDescending(o => o.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<OrdenCompra>>
            {
                WasSuccess = true,
                Result = ordenes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes de compra de proveedor {ProveedorId}", proveedorId);
            return new ActionResponse<IEnumerable<OrdenCompra>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<OrdenCompra>>> GetByEstadoAsync(Guid empresaId, string estado)
    {
        try
        {
            var ordenes = await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Sucursal)
                .Include(o => o.BodegaDestino)
                .Where(o => o.EmpresaId == empresaId && o.Estado == estado && !o.IsDeleted)
                .OrderByDescending(o => o.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<OrdenCompra>>
            {
                WasSuccess = true,
                Result = ordenes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes de compra por estado {Estado}", estado);
            return new ActionResponse<IEnumerable<OrdenCompra>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<OrdenCompra>>> GetPendientesRecepcionAsync(Guid empresaId)
    {
        try
        {
            // Estados APR (Aprobada) y PAR (Parcial) son las que pueden recibirse
            var ordenes = await _context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Sucursal)
                .Include(o => o.BodegaDestino)
                .Where(o => o.EmpresaId == empresaId &&
                           (o.Estado == "APR" || o.Estado == "PAR") &&
                           !o.IsDeleted)
                .OrderByDescending(o => o.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<OrdenCompra>>
            {
                WasSuccess = true,
                Result = ordenes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes pendientes de recepción");
            return new ActionResponse<IEnumerable<OrdenCompra>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<OrdenCompra>> AddAsync(OrdenCompra orden)
    {
        try
        {
            // Validar que la empresa exista
            var empresaExiste = await _context.Empresas.AnyAsync(e => e.Id == orden.EmpresaId && !e.IsDeleted);
            if (!empresaExiste)
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = "La empresa no existe"
                };
            }

            // Validar que el proveedor exista
            var proveedorExiste = await _context.Proveedores.AnyAsync(p => p.Id == orden.ProveedorId && !p.IsDeleted);
            if (!proveedorExiste)
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = "El proveedor no existe"
                };
            }

            // Generar número si no tiene
            if (string.IsNullOrWhiteSpace(orden.Numero))
            {
                var numeroResponse = await GenerarNumeroAsync(orden.EmpresaId);
                if (!numeroResponse.WasSuccess)
                {
                    return new ActionResponse<OrdenCompra>
                    {
                        WasSuccess = false,
                        Message = numeroResponse.Message
                    };
                }
                orden.Numero = numeroResponse.Result!;
            }

            // Establecer fechas de auditoría
            orden.FechaCreacion = DateTime.Now;

            _context.OrdenesCompra.Add(orden);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Orden de compra creada: {Numero}", orden.Numero);

            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = true,
                Result = orden
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear orden de compra");
            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear orden de compra");
            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<OrdenCompra>> UpdateAsync(OrdenCompra orden)
    {
        try
        {
            var ordenExistente = await _context.OrdenesCompra
                .Include(o => o.Detalles)
                .FirstOrDefaultAsync(o => o.Id == orden.Id && !o.IsDeleted);

            if (ordenExistente == null)
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = "Orden de compra no encontrada"
                };
            }

            // Validar que se pueda editar (solo en estado BOR o PEN)
            if (ordenExistente.Estado != "BOR" && ordenExistente.Estado != "PEN")
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = $"No se puede modificar una orden en estado {ordenExistente.EstadoDescripcion}"
                };
            }

            // Actualizar solo propiedades escalares
            ordenExistente.ProveedorId = orden.ProveedorId;
            ordenExistente.SucursalId = orden.SucursalId;
            ordenExistente.BodegaDestinoId = orden.BodegaDestinoId;
            ordenExistente.Fecha = orden.Fecha;
            ordenExistente.Moneda = orden.Moneda;
            ordenExistente.TipoCambio = orden.TipoCambio;
            ordenExistente.Subtotal = orden.Subtotal;
            ordenExistente.Descuento = orden.Descuento;
            ordenExistente.Impuesto = orden.Impuesto;
            ordenExistente.Total = orden.Total;
            ordenExistente.FechaEntregaEsperada = orden.FechaEntregaEsperada;
            ordenExistente.CondicionPago = orden.CondicionPago;
            ordenExistente.DiasCredito = orden.DiasCredito;
            ordenExistente.Observaciones = orden.Observaciones;
            ordenExistente.FechaModificacion = DateTime.Now;
            ordenExistente.ModificadoPorId = orden.ModificadoPorId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Orden de compra actualizada: {Numero}", ordenExistente.Numero);

            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = true,
                Result = ordenExistente
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al actualizar orden de compra");
            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar orden de compra");
            return new ActionResponse<OrdenCompra>
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
            var orden = await _context.OrdenesCompra
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (orden == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Orden de compra no encontrada"
                };
            }

            // Solo se pueden eliminar órdenes en estado BOR (Borrador)
            if (orden.Estado != "BOR")
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = $"No se puede eliminar una orden en estado {orden.EstadoDescripcion}"
                };
            }

            // Soft delete
            orden.IsDeleted = true;
            orden.FechaEliminacion = DateTime.Now;
            orden.UsuarioEliminacionId = usuarioId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Orden de compra eliminada: {Numero}", orden.Numero);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar orden de compra");
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<OrdenCompra>> AprobarAsync(Guid id, string usuarioId)
    {
        try
        {
            var orden = await _context.OrdenesCompra
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (orden == null)
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = "Orden de compra no encontrada"
                };
            }

            // Validar que esté en estado PEN (Pendiente) o BOR (Borrador)
            if (orden.Estado != "PEN" && orden.Estado != "BOR")
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = $"No se puede aprobar una orden en estado {orden.EstadoDescripcion}"
                };
            }

            // Cambiar estado a APR (Aprobada)
            orden.Estado = "APR";
            orden.FechaAprobacion = DateTime.Now;
            orden.AprobadoPorId = usuarioId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Orden de compra aprobada: {Numero} por usuario {UsuarioId}", orden.Numero, usuarioId);

            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = true,
                Result = orden
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar orden de compra");
            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<OrdenCompra>> AnularAsync(Guid id, string usuarioId, string motivo)
    {
        try
        {
            var orden = await _context.OrdenesCompra
                .Include(o => o.Recepciones)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (orden == null)
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = "Orden de compra no encontrada"
                };
            }

            // Validar que no esté ya anulada o completada
            if (orden.Estado == "ANU")
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = "La orden ya está anulada"
                };
            }

            if (orden.Estado == "COM")
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = "No se puede anular una orden completada"
                };
            }

            // Validar que no tenga recepciones aplicadas
            if (orden.Recepciones != null && orden.Recepciones.Any(r => r.Estado == "APL"))
            {
                return new ActionResponse<OrdenCompra>
                {
                    WasSuccess = false,
                    Message = "No se puede anular una orden con recepciones aplicadas. Debe anular las recepciones primero."
                };
            }

            // Cambiar estado a ANU (Anulada)
            orden.Estado = "ANU";
            orden.FechaModificacion = DateTime.Now;
            orden.ModificadoPorId = usuarioId;
            orden.Observaciones = string.IsNullOrWhiteSpace(orden.Observaciones)
                ? $"Anulada: {motivo}"
                : $"{orden.Observaciones}\nAnulada: {motivo}";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Orden de compra anulada: {Numero} por usuario {UsuarioId}. Motivo: {Motivo}",
                orden.Numero, usuarioId, motivo);

            return new ActionResponse<OrdenCompra>
            {
                WasSuccess = true,
                Result = orden
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular orden de compra");
            return new ActionResponse<OrdenCompra>
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
            var prefijo = $"OC-{fecha:yyyyMMdd}";

            // Buscar el último número del día
            var ultimaOrden = await _context.OrdenesCompra
                .Where(o => o.EmpresaId == empresaId && o.Numero.StartsWith(prefijo))
                .OrderByDescending(o => o.Numero)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;

            if (ultimaOrden != null)
            {
                // Extraer el número secuencial del formato OC-YYYYMMDD-####
                var partes = ultimaOrden.Numero.Split('-');
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
            _logger.LogError(ex, "Error al generar número de orden de compra");
            return new ActionResponse<string>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
