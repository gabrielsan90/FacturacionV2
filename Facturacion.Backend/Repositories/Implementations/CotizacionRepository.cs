using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

/// <summary>
/// Repositorio para gestión de Cotizaciones
/// </summary>
public class CotizacionRepository : ICotizacionRepository
{
    private readonly DataContext _context;
    private readonly ILogger<CotizacionRepository> _logger;

    public CotizacionRepository(DataContext context, ILogger<CotizacionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<Cotizacion>> GetAsync(Guid id)
    {
        try
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Empresa)
                .Include(c => c.Sucursal)
                .Include(c => c.Terminal)
                .Include(c => c.Cliente)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (cotizacion == null)
            {
                return new ActionResponse<Cotizacion>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            return new ActionResponse<Cotizacion>
            {
                WasSuccess = true,
                Result = cotizacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotización con ID {Id}", id);
            return new ActionResponse<Cotizacion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<Cotizacion>> GetWithDetallesAsync(Guid id)
    {
        try
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Empresa)
                .Include(c => c.Sucursal)
                .Include(c => c.Terminal)
                .Include(c => c.Cliente)
                .Include(c => c.Detalles.OrderBy(d => d.NumeroLinea))
                    .ThenInclude(d => d.Producto)
                .Include(c => c.DocumentoGenerado)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (cotizacion == null)
            {
                return new ActionResponse<Cotizacion>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            return new ActionResponse<Cotizacion>
            {
                WasSuccess = true,
                Result = cotizacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotización con detalles con ID {Id}", id);
            return new ActionResponse<Cotizacion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<Cotizacion>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var cotizaciones = await _context.Cotizaciones
                .Include(c => c.Cliente)
                .Include(c => c.Sucursal)
                .Include(c => c.Terminal)
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
                .OrderByDescending(c => c.FechaEmision)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Cotizacion>>
            {
                WasSuccess = true,
                Result = cotizaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<Cotizacion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<Cotizacion>>> GetByClienteAsync(Guid clienteId)
    {
        try
        {
            var cotizaciones = await _context.Cotizaciones
                .Include(c => c.Cliente)
                .Include(c => c.Sucursal)
                .Include(c => c.Terminal)
                .Where(c => c.ClienteId == clienteId && !c.IsDeleted)
                .OrderByDescending(c => c.FechaEmision)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Cotizacion>>
            {
                WasSuccess = true,
                Result = cotizaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones del cliente {ClienteId}", clienteId);
            return new ActionResponse<IEnumerable<Cotizacion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<Cotizacion>>> GetByEstadoAsync(Guid empresaId, EstadoCotizacion estado)
    {
        try
        {
            var cotizaciones = await _context.Cotizaciones
                .Include(c => c.Cliente)
                .Include(c => c.Sucursal)
                .Include(c => c.Terminal)
                .Where(c => c.EmpresaId == empresaId &&
                           !c.IsDeleted &&
                           c.Estado == estado)
                .OrderByDescending(c => c.FechaEmision)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Cotizacion>>
            {
                WasSuccess = true,
                Result = cotizaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones por estado de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<Cotizacion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<Cotizacion>>> GetVencidasAsync(Guid empresaId)
    {
        try
        {
            var fechaActual = FechaCostaRicaHelper.Ahora.Date;

            var cotizaciones = await _context.Cotizaciones
                .Include(c => c.Cliente)
                .Include(c => c.Sucursal)
                .Include(c => c.Terminal)
                .Where(c => c.EmpresaId == empresaId &&
                           !c.IsDeleted &&
                           c.FechaVencimiento < fechaActual &&
                           (c.Estado == EstadoCotizacion.Borrador ||
                            c.Estado == EstadoCotizacion.Enviada))
                .OrderBy(c => c.FechaVencimiento)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Cotizacion>>
            {
                WasSuccess = true,
                Result = cotizaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cotizaciones vencidas de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<Cotizacion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<Cotizacion>> AddAsync(Cotizacion cotizacion)
    {
        try
        {
            // Verificar que no exista una cotización con el mismo número
            // IgnoreQueryFilters to check soft-deleted records too (unique index is global)
            var existente = await _context.Cotizaciones
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Numero == cotizacion.Numero &&
                                         c.EmpresaId == cotizacion.EmpresaId);

            if (existente != null)
            {
                return new ActionResponse<Cotizacion>
                {
                    WasSuccess = false,
                    Message = $"Ya existe una cotización con el número {cotizacion.Numero}"
                };
            }

            // Establecer valores iniciales
            cotizacion.FechaCreacion = FechaCostaRicaHelper.Ahora;
            cotizacion.Estado = EstadoCotizacion.Borrador;

            _context.Cotizaciones.Add(cotizacion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización {Numero} creada exitosamente para empresa {EmpresaId}",
                cotizacion.Numero, cotizacion.EmpresaId);

            return new ActionResponse<Cotizacion>
            {
                WasSuccess = true,
                Result = cotizacion
            };
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Error de base de datos al crear cotización {Numero}", cotizacion.Numero);
            // Walk exception chain to find the actual SQL error
            var innerEx = dbEx.InnerException;
            while (innerEx?.InnerException != null) innerEx = innerEx.InnerException;
            var innerMsg = innerEx?.Message ?? dbEx.InnerException?.Message ?? dbEx.Message;

            // Detectar FK violations comunes y dar mensajes claros
            string mensajeUsuario;
            if (innerMsg.Contains("FK_") && innerMsg.Contains("Sucursal"))
                mensajeUsuario = "La sucursal especificada no existe. Configure una sucursal válida.";
            else if (innerMsg.Contains("FK_") && innerMsg.Contains("Terminal"))
                mensajeUsuario = "La terminal especificada no existe. Configure una terminal válida.";
            else if (innerMsg.Contains("FK_") && innerMsg.Contains("Cliente"))
                mensajeUsuario = "El cliente especificado no existe o fue eliminado.";
            else if (innerMsg.Contains("FK_") && innerMsg.Contains("Empresa"))
                mensajeUsuario = "La empresa especificada no es válida.";
            else
                mensajeUsuario = $"Error al guardar la cotización: {innerMsg}";

            return new ActionResponse<Cotizacion>
            {
                WasSuccess = false,
                Message = mensajeUsuario
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cotización {Numero}", cotizacion.Numero);
            return new ActionResponse<Cotizacion>
            {
                WasSuccess = false,
                Message = $"Error inesperado: {ex.Message}"
            };
        }
    }

    public async Task<ActionResponse<Cotizacion>> UpdateAsync(Cotizacion cotizacion)
    {
        try
        {
            var existente = await _context.Cotizaciones
                .FirstOrDefaultAsync(c => c.Id == cotizacion.Id);

            if (existente == null)
            {
                return new ActionResponse<Cotizacion>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            // Validar que no esté convertida a factura
            if (existente.Estado == EstadoCotizacion.Convertida)
            {
                return new ActionResponse<Cotizacion>
                {
                    WasSuccess = false,
                    Message = "No se puede modificar una cotización que ya ha sido convertida a factura"
                };
            }

            // Preservar valores de auditoría
            cotizacion.FechaModificacion = FechaCostaRicaHelper.Ahora;
            cotizacion.FechaCreacion = existente.FechaCreacion;
            cotizacion.UsuarioCreacionId = existente.UsuarioCreacionId;

            _context.Entry(existente).CurrentValues.SetValues(cotizacion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización {Id} actualizada exitosamente", cotizacion.Id);

            return new ActionResponse<Cotizacion>
            {
                WasSuccess = true,
                Result = cotizacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cotización {Id}", cotizacion.Id);
            return new ActionResponse<Cotizacion>
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
            var cotizacion = await _context.Cotizaciones.FindAsync(id);

            if (cotizacion == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            // Validar que no esté convertida a factura
            if (cotizacion.Estado == EstadoCotizacion.Convertida)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar una cotización que ya ha sido convertida a factura"
                };
            }

            // Soft delete
            cotizacion.IsDeleted = true;
            cotizacion.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            cotizacion.UsuarioEliminacionId = usuarioId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización {Id} eliminada por usuario {UsuarioId}", id, usuarioId);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cotización eliminada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cotización {Id}", id);
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> AprobarAsync(Guid id, string usuarioId)
    {
        try
        {
            var cotizacion = await _context.Cotizaciones.FindAsync(id);

            if (cotizacion == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            // Validar estado actual
            if (cotizacion.Estado != EstadoCotizacion.Enviada && cotizacion.Estado != EstadoCotizacion.Borrador)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = $"No se puede aprobar una cotización en estado {cotizacion.Estado}"
                };
            }

            // Actualizar estado y fecha de aprobación
            cotizacion.Estado = EstadoCotizacion.Aprobada;
            cotizacion.FechaAprobacion = FechaCostaRicaHelper.Ahora;
            cotizacion.FechaModificacion = FechaCostaRicaHelper.Ahora;
            cotizacion.UsuarioModificacionId = usuarioId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización {Id} aprobada por usuario {UsuarioId}", id, usuarioId);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cotización aprobada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar cotización {Id}", id);
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> ConvertirAFacturaAsync(Guid id, Guid documentoId, string usuarioId)
    {
        try
        {
            var cotizacion = await _context.Cotizaciones.FindAsync(id);

            if (cotizacion == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Cotización no encontrada"
                };
            }

            // Validar estado actual
            if (cotizacion.Estado != EstadoCotizacion.Aprobada)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Solo se pueden convertir cotizaciones aprobadas"
                };
            }

            // Actualizar estado y relación con documento
            cotizacion.Estado = EstadoCotizacion.Convertida;
            cotizacion.DocumentoGeneradoId = documentoId;
            cotizacion.FechaConversion = FechaCostaRicaHelper.Ahora;
            cotizacion.FechaModificacion = FechaCostaRicaHelper.Ahora;
            cotizacion.UsuarioModificacionId = usuarioId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cotización {Id} convertida a documento {DocumentoId} por usuario {UsuarioId}",
                id, documentoId, usuarioId);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cotización convertida a factura exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al convertir cotización {Id} a factura", id);
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
