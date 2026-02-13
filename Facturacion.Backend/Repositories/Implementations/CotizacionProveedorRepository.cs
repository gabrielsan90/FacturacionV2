using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class CotizacionProveedorRepository : ICotizacionProveedorRepository
{
    private readonly DataContext _context;
    private readonly ILogger<CotizacionProveedorRepository> _logger;

    public CotizacionProveedorRepository(DataContext context, ILogger<CotizacionProveedorRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<CotizacionProveedor>> GetAsync(Guid id)
    {
        try
        {
            var cotizacion = await _context.CotizacionesProveedor
                .Include(c => c.Requisicion)
                .Include(c => c.Proveedor)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = true,
                Result = cotizacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotización {Id}", id);
            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CotizacionProveedor>> GetWithDetallesAsync(Guid id)
    {
        try
        {
            var cotizacion = await _context.CotizacionesProveedor
                .Include(c => c.Requisicion)
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = true,
                Result = cotizacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotización con detalles {Id}", id);
            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetByRequisicionAsync(Guid requisicionId)
    {
        try
        {
            var cotizaciones = await _context.CotizacionesProveedor
                .Include(c => c.Proveedor)
                .Where(c => c.RequisicionId == requisicionId)
                .OrderByDescending(c => c.FechaSolicitud)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CotizacionProveedor>>
            {
                WasSuccess = true,
                Result = cotizaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones de requisición {RequisicionId}", requisicionId);
            return new ActionResponse<IEnumerable<CotizacionProveedor>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetByProveedorAsync(Guid proveedorId)
    {
        try
        {
            var cotizaciones = await _context.CotizacionesProveedor
                .Include(c => c.Requisicion)
                .Where(c => c.ProveedorId == proveedorId)
                .OrderByDescending(c => c.FechaSolicitud)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CotizacionProveedor>>
            {
                WasSuccess = true,
                Result = cotizaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones de proveedor {ProveedorId}", proveedorId);
            return new ActionResponse<IEnumerable<CotizacionProveedor>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetByEstadoAsync(Guid empresaId, string estado)
    {
        try
        {
            var cotizaciones = await _context.CotizacionesProveedor
                .Include(c => c.Requisicion)
                .Include(c => c.Proveedor)
                .Where(c => c.EmpresaId == empresaId && c.Estado == estado)
                .OrderByDescending(c => c.FechaSolicitud)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CotizacionProveedor>>
            {
                WasSuccess = true,
                Result = cotizaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones por estado {Estado}", estado);
            return new ActionResponse<IEnumerable<CotizacionProveedor>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetVencidasAsync(Guid empresaId)
    {
        try
        {
            var ahora = DateTime.Now;

            var cotizaciones = await _context.CotizacionesProveedor
                .Include(c => c.Requisicion)
                .Include(c => c.Proveedor)
                .Where(c => c.EmpresaId == empresaId &&
                           c.Estado == "ENV" &&
                           c.FechaVencimiento.HasValue &&
                           c.FechaVencimiento.Value < ahora)
                .OrderBy(c => c.FechaVencimiento)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CotizacionProveedor>>
            {
                WasSuccess = true,
                Result = cotizaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones vencidas");
            return new ActionResponse<IEnumerable<CotizacionProveedor>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CotizacionProveedor>> AddAsync(CotizacionProveedor cotizacion)
    {
        try
        {
            // Validar que la requisición exista y esté aprobada
            var requisicion = await _context.Requisiciones
                .FirstOrDefaultAsync(r => r.Id == cotizacion.RequisicionId && !r.IsDeleted);

            if (requisicion == null)
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = "Requisición no encontrada"
                };
            }

            if (requisicion.Estado != "APR" && requisicion.Estado != "COT")
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = $"La requisición debe estar aprobada o en cotización. Estado actual: {requisicion.EstadoDescripcion}"
                };
            }

            // Validar que el proveedor exista
            var proveedorExiste = await _context.Proveedores.AnyAsync(p => p.Id == cotizacion.ProveedorId && !p.IsDeleted);
            if (!proveedorExiste)
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = "El proveedor no existe"
                };
            }

            // Generar número si no tiene
            if (string.IsNullOrWhiteSpace(cotizacion.Numero))
            {
                var numeroResponse = await GenerarNumeroAsync(cotizacion.EmpresaId);
                if (!numeroResponse.WasSuccess)
                {
                    return new ActionResponse<CotizacionProveedor>
                    {
                        WasSuccess = false,
                        Message = numeroResponse.Message
                    };
                }
                cotizacion.Numero = numeroResponse.Result!;
            }

            // Establecer fechas
            cotizacion.FechaSolicitud = DateTime.Now;
            cotizacion.FechaCreacion = DateTime.Now;
            cotizacion.Estado = "ENV";

            _context.CotizacionesProveedor.Add(cotizacion);

            // Actualizar estado de requisición a COT (En Cotización) si estaba en APR
            if (requisicion.Estado == "APR")
            {
                requisicion.Estado = "COT";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización creada: {Numero} para requisición {RequisicionNumero}",
                cotizacion.Numero, requisicion.Numero);

            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = true,
                Result = cotizacion
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear cotización");
            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cotización");
            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CotizacionProveedor>> UpdateAsync(CotizacionProveedor cotizacion)
    {
        try
        {
            var cotizacionExistente = await _context.CotizacionesProveedor
                .FirstOrDefaultAsync(c => c.Id == cotizacion.Id);

            if (cotizacionExistente == null)
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            // Validar que se pueda editar (solo en estados ENV o REC)
            if (cotizacionExistente.Estado != "ENV" && cotizacionExistente.Estado != "REC")
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = $"No se puede modificar una cotización en estado {cotizacionExistente.EstadoDescripcion}"
                };
            }

            // Actualizar propiedades escalares
            cotizacionExistente.FechaVencimiento = cotizacion.FechaVencimiento;
            cotizacionExistente.FechaRespuesta = cotizacion.FechaRespuesta;
            cotizacionExistente.ReferenciaProveedor = cotizacion.ReferenciaProveedor;
            cotizacionExistente.MontoSubtotal = cotizacion.MontoSubtotal;
            cotizacionExistente.MontoImpuestos = cotizacion.MontoImpuestos;
            cotizacionExistente.MontoTotal = cotizacion.MontoTotal;
            cotizacionExistente.Moneda = cotizacion.Moneda;
            cotizacionExistente.TipoCambio = cotizacion.TipoCambio;
            cotizacionExistente.TiempoEntregaDias = cotizacion.TiempoEntregaDias;
            cotizacionExistente.ValidezDias = cotizacion.ValidezDias;
            cotizacionExistente.CondicionesPago = cotizacion.CondicionesPago;
            cotizacionExistente.LugarEntrega = cotizacion.LugarEntrega;
            cotizacionExistente.IncluyeFlete = cotizacion.IncluyeFlete;
            cotizacionExistente.MontoFlete = cotizacion.MontoFlete;
            cotizacionExistente.PuntuacionTotal = cotizacion.PuntuacionTotal;
            cotizacionExistente.Observaciones = cotizacion.Observaciones;
            cotizacionExistente.NombreArchivo = cotizacion.NombreArchivo;
            cotizacionExistente.RutaArchivo = cotizacion.RutaArchivo;
            cotizacionExistente.FechaModificacion = DateTime.Now;
            cotizacionExistente.UsuarioModificacionId = cotizacion.UsuarioModificacionId;

            // Si tiene respuesta, cambiar estado a REC
            if (cotizacionExistente.FechaRespuesta.HasValue && cotizacionExistente.Estado == "ENV")
            {
                cotizacionExistente.Estado = "REC";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización actualizada: {Numero}", cotizacionExistente.Numero);

            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = true,
                Result = cotizacionExistente
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al actualizar cotización");
            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cotización");
            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CotizacionProveedor>> SeleccionarAsync(Guid id, string usuarioId)
    {
        try
        {
            var cotizacion = await _context.CotizacionesProveedor
                .Include(c => c.Requisicion)
                    .ThenInclude(r => r!.Cotizaciones)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            // Validar que esté en estado REC (Recibida) o APR (Aprobada)
            if (cotizacion.Estado != "REC" && cotizacion.Estado != "APR")
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = $"No se puede seleccionar una cotización en estado {cotizacion.EstadoDescripcion}"
                };
            }

            // Desmarcar cualquier otra cotización seleccionada de la misma requisición
            if (cotizacion.Requisicion?.Cotizaciones != null)
            {
                foreach (var otraCotizacion in cotizacion.Requisicion.Cotizaciones.Where(c => c.Seleccionada))
                {
                    otraCotizacion.Seleccionada = false;
                }
            }

            // Marcar esta cotización como seleccionada
            cotizacion.Seleccionada = true;
            cotizacion.Estado = "SEL";
            cotizacion.FechaModificacion = DateTime.Now;
            cotizacion.UsuarioModificacionId = usuarioId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización seleccionada: {Numero} por usuario {UsuarioId}",
                cotizacion.Numero, usuarioId);

            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = true,
                Result = cotizacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al seleccionar cotización");
            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CotizacionProveedor>> RechazarAsync(Guid id, string usuarioId, string motivo)
    {
        try
        {
            var cotizacion = await _context.CotizacionesProveedor
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            // Validar que esté en estado REC (Recibida)
            if (cotizacion.Estado != "REC")
            {
                return new ActionResponse<CotizacionProveedor>
                {
                    WasSuccess = false,
                    Message = $"No se puede rechazar una cotización en estado {cotizacion.EstadoDescripcion}"
                };
            }

            // Cambiar estado a REJ (Rechazada)
            cotizacion.Estado = "REJ";
            cotizacion.FechaModificacion = DateTime.Now;
            cotizacion.UsuarioModificacionId = usuarioId;
            cotizacion.Observaciones = string.IsNullOrWhiteSpace(cotizacion.Observaciones)
                ? $"Rechazada: {motivo}"
                : $"{cotizacion.Observaciones}\nRechazada: {motivo}";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización rechazada: {Numero} por usuario {UsuarioId}. Motivo: {Motivo}",
                cotizacion.Numero, usuarioId, motivo);

            return new ActionResponse<CotizacionProveedor>
            {
                WasSuccess = true,
                Result = cotizacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al rechazar cotización");
            return new ActionResponse<CotizacionProveedor>
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
            var prefijo = $"COT-{fecha:yyyyMMdd}";

            // Buscar el último número del día
            var ultimaCotizacion = await _context.CotizacionesProveedor
                .Where(c => c.EmpresaId == empresaId && c.Numero.StartsWith(prefijo))
                .OrderByDescending(c => c.Numero)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;

            if (ultimaCotizacion != null)
            {
                // Extraer el número secuencial del formato COT-YYYYMMDD-####
                var partes = ultimaCotizacion.Numero.Split('-');
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
            _logger.LogError(ex, "Error al generar número de cotización");
            return new ActionResponse<string>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
