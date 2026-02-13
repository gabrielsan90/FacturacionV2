using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class RequisicionRepository : IRequisicionRepository
{
    private readonly DataContext _context;
    private readonly ILogger<RequisicionRepository> _logger;

    public RequisicionRepository(DataContext context, ILogger<RequisicionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<Requisicion>> GetAsync(Guid id)
    {
        try
        {
            var requisicion = await _context.Requisiciones
                .Include(r => r.Solicitante)
                .Include(r => r.Departamento)
                .Include(r => r.Sucursal)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (requisicion == null)
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "Requisición no encontrada"
                };
            }

            return new ActionResponse<Requisicion>
            {
                WasSuccess = true,
                Result = requisicion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener requisición {Id}", id);
            return new ActionResponse<Requisicion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<Requisicion>> GetWithDetallesAsync(Guid id)
    {
        try
        {
            var requisicion = await _context.Requisiciones
                .Include(r => r.Solicitante)
                .Include(r => r.Departamento)
                .Include(r => r.Sucursal)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(r => r.Cotizaciones)
                    .ThenInclude(c => c.Proveedor)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (requisicion == null)
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "Requisición no encontrada"
                };
            }

            return new ActionResponse<Requisicion>
            {
                WasSuccess = true,
                Result = requisicion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener requisición con detalles {Id}", id);
            return new ActionResponse<Requisicion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<Requisicion>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var requisiciones = await _context.Requisiciones
                .Include(r => r.Solicitante)
                .Include(r => r.Departamento)
                .Include(r => r.Sucursal)
                .Where(r => r.EmpresaId == empresaId && !r.IsDeleted)
                .OrderByDescending(r => r.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Requisicion>>
            {
                WasSuccess = true,
                Result = requisiciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener requisiciones de empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<Requisicion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<Requisicion>>> GetBySolicitanteAsync(string solicitanteId)
    {
        try
        {
            var requisiciones = await _context.Requisiciones
                .Include(r => r.Departamento)
                .Include(r => r.Sucursal)
                .Where(r => r.SolicitanteId == solicitanteId && !r.IsDeleted)
                .OrderByDescending(r => r.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Requisicion>>
            {
                WasSuccess = true,
                Result = requisiciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener requisiciones de solicitante {SolicitanteId}", solicitanteId);
            return new ActionResponse<IEnumerable<Requisicion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<Requisicion>>> GetByEstadoAsync(Guid empresaId, string estado)
    {
        try
        {
            var requisiciones = await _context.Requisiciones
                .Include(r => r.Solicitante)
                .Include(r => r.Departamento)
                .Include(r => r.Sucursal)
                .Where(r => r.EmpresaId == empresaId && r.Estado == estado && !r.IsDeleted)
                .OrderByDescending(r => r.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Requisicion>>
            {
                WasSuccess = true,
                Result = requisiciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener requisiciones por estado {Estado}", estado);
            return new ActionResponse<IEnumerable<Requisicion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<Requisicion>>> GetPendientesAprobacionAsync(Guid empresaId)
    {
        try
        {
            var requisiciones = await _context.Requisiciones
                .Include(r => r.Solicitante)
                .Include(r => r.Departamento)
                .Include(r => r.Sucursal)
                .Where(r => r.EmpresaId == empresaId && r.Estado == "ENV" && !r.IsDeleted)
                .OrderBy(r => r.Prioridad)
                .ThenBy(r => r.FechaRequerida)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Requisicion>>
            {
                WasSuccess = true,
                Result = requisiciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener requisiciones pendientes de aprobación");
            return new ActionResponse<IEnumerable<Requisicion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<Requisicion>>> GetAprobadasSinOCAsync(Guid empresaId)
    {
        try
        {
            var requisiciones = await _context.Requisiciones
                .Include(r => r.Solicitante)
                .Include(r => r.Departamento)
                .Include(r => r.Sucursal)
                .Where(r => r.EmpresaId == empresaId &&
                           (r.Estado == "APR" || r.Estado == "COT") &&
                           !r.IsDeleted)
                .OrderBy(r => r.FechaRequerida)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Requisicion>>
            {
                WasSuccess = true,
                Result = requisiciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener requisiciones aprobadas sin OC");
            return new ActionResponse<IEnumerable<Requisicion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<Requisicion>> AddAsync(Requisicion requisicion)
    {
        try
        {
            // Validar que la empresa exista
            var empresaExiste = await _context.Empresas.AnyAsync(e => e.Id == requisicion.EmpresaId && !e.IsDeleted);
            if (!empresaExiste)
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "La empresa no existe"
                };
            }

            // Validar que el solicitante exista
            var solicitanteExiste = await _context.Users.AnyAsync(u => u.Id == requisicion.SolicitanteId);
            if (!solicitanteExiste)
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "El solicitante no existe"
                };
            }

            // Generar número si no tiene
            if (string.IsNullOrWhiteSpace(requisicion.Numero))
            {
                var numeroResponse = await GenerarNumeroAsync(requisicion.EmpresaId);
                if (!numeroResponse.WasSuccess)
                {
                    return new ActionResponse<Requisicion>
                    {
                        WasSuccess = false,
                        Message = numeroResponse.Message
                    };
                }
                requisicion.Numero = numeroResponse.Result!;
            }

            // Establecer fechas de auditoría
            requisicion.FechaCreacion = DateTime.Now;
            requisicion.Fecha = DateTime.Now;

            _context.Requisiciones.Add(requisicion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Requisición creada: {Numero} por {SolicitanteId}",
                requisicion.Numero, requisicion.SolicitanteId);

            return new ActionResponse<Requisicion>
            {
                WasSuccess = true,
                Result = requisicion
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear requisición");
            return new ActionResponse<Requisicion>
            {
                WasSuccess = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear requisición");
            return new ActionResponse<Requisicion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<Requisicion>> UpdateAsync(Requisicion requisicion)
    {
        try
        {
            var requisicionExistente = await _context.Requisiciones
                .FirstOrDefaultAsync(r => r.Id == requisicion.Id && !r.IsDeleted);

            if (requisicionExistente == null)
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "Requisición no encontrada"
                };
            }

            // Validar que se pueda editar (solo en estado BOR)
            if (requisicionExistente.Estado != "BOR")
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = $"No se puede modificar una requisición en estado {requisicionExistente.EstadoDescripcion}"
                };
            }

            // Actualizar solo propiedades escalares
            requisicionExistente.DepartamentoId = requisicion.DepartamentoId;
            requisicionExistente.SucursalId = requisicion.SucursalId;
            requisicionExistente.Prioridad = requisicion.Prioridad;
            requisicionExistente.FechaRequerida = requisicion.FechaRequerida;
            requisicionExistente.Justificacion = requisicion.Justificacion;
            requisicionExistente.MontoEstimado = requisicion.MontoEstimado;
            requisicionExistente.Moneda = requisicion.Moneda;
            requisicionExistente.Observaciones = requisicion.Observaciones;
            requisicionExistente.FechaModificacion = DateTime.Now;
            requisicionExistente.UsuarioModificacionId = requisicion.UsuarioModificacionId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Requisición actualizada: {Numero}", requisicionExistente.Numero);

            return new ActionResponse<Requisicion>
            {
                WasSuccess = true,
                Result = requisicionExistente
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al actualizar requisición");
            return new ActionResponse<Requisicion>
            {
                WasSuccess = false,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar requisición");
            return new ActionResponse<Requisicion>
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
            var requisicion = await _context.Requisiciones
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (requisicion == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Requisición no encontrada"
                };
            }

            // Solo se pueden eliminar requisiciones en estado BOR
            if (requisicion.Estado != "BOR")
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = $"No se puede eliminar una requisición en estado {requisicion.EstadoDescripcion}"
                };
            }

            // Soft delete
            requisicion.IsDeleted = true;
            requisicion.FechaEliminacion = DateTime.Now;
            requisicion.UsuarioEliminacionId = usuarioId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Requisición eliminada: {Numero}", requisicion.Numero);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar requisición");
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<Requisicion>> AprobarAsync(Guid id, string usuarioId)
    {
        try
        {
            var requisicion = await _context.Requisiciones
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (requisicion == null)
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "Requisición no encontrada"
                };
            }

            // Validar que esté en estado ENV (Enviada)
            if (requisicion.Estado != "ENV")
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = $"No se puede aprobar una requisición en estado {requisicion.EstadoDescripcion}"
                };
            }

            // Cambiar estado a APR (Aprobada)
            requisicion.Estado = "APR";
            requisicion.FechaAprobacion = DateTime.Now;
            requisicion.AprobadoPorId = usuarioId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Requisición aprobada: {Numero} por usuario {UsuarioId}",
                requisicion.Numero, usuarioId);

            return new ActionResponse<Requisicion>
            {
                WasSuccess = true,
                Result = requisicion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar requisición");
            return new ActionResponse<Requisicion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<Requisicion>> RechazarAsync(Guid id, string usuarioId, string motivo)
    {
        try
        {
            var requisicion = await _context.Requisiciones
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (requisicion == null)
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "Requisición no encontrada"
                };
            }

            // Validar que esté en estado ENV (Enviada)
            if (requisicion.Estado != "ENV")
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = $"No se puede rechazar una requisición en estado {requisicion.EstadoDescripcion}"
                };
            }

            // Cambiar estado a REC (Rechazada)
            requisicion.Estado = "REC";
            requisicion.FechaAprobacion = DateTime.Now;
            requisicion.AprobadoPorId = usuarioId;
            requisicion.Observaciones = string.IsNullOrWhiteSpace(requisicion.Observaciones)
                ? $"Rechazada: {motivo}"
                : $"{requisicion.Observaciones}\nRechazada: {motivo}";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Requisición rechazada: {Numero} por usuario {UsuarioId}. Motivo: {Motivo}",
                requisicion.Numero, usuarioId, motivo);

            return new ActionResponse<Requisicion>
            {
                WasSuccess = true,
                Result = requisicion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al rechazar requisición");
            return new ActionResponse<Requisicion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<Requisicion>> AnularAsync(Guid id, string usuarioId, string motivo)
    {
        try
        {
            var requisicion = await _context.Requisiciones
                .Include(r => r.Cotizaciones)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (requisicion == null)
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "Requisición no encontrada"
                };
            }

            // Validar que no esté ya anulada o con OC generada
            if (requisicion.Estado == "ANU")
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "La requisición ya está anulada"
                };
            }

            if (requisicion.Estado == "OC")
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "No se puede anular una requisición con orden de compra generada"
                };
            }

            // Validar que no tenga cotizaciones seleccionadas
            if (requisicion.Cotizaciones != null && requisicion.Cotizaciones.Any(c => c.Seleccionada))
            {
                return new ActionResponse<Requisicion>
                {
                    WasSuccess = false,
                    Message = "No se puede anular una requisición con cotizaciones seleccionadas"
                };
            }

            // Cambiar estado a ANU (Anulada)
            requisicion.Estado = "ANU";
            requisicion.FechaModificacion = DateTime.Now;
            requisicion.UsuarioModificacionId = usuarioId;
            requisicion.Observaciones = string.IsNullOrWhiteSpace(requisicion.Observaciones)
                ? $"Anulada: {motivo}"
                : $"{requisicion.Observaciones}\nAnulada: {motivo}";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Requisición anulada: {Numero} por usuario {UsuarioId}. Motivo: {Motivo}",
                requisicion.Numero, usuarioId, motivo);

            return new ActionResponse<Requisicion>
            {
                WasSuccess = true,
                Result = requisicion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular requisición");
            return new ActionResponse<Requisicion>
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
            var prefijo = $"REQ-{fecha:yyyyMMdd}";

            // Buscar el último número del día
            var ultimaRequisicion = await _context.Requisiciones
                .Where(r => r.EmpresaId == empresaId && r.Numero.StartsWith(prefijo))
                .OrderByDescending(r => r.Numero)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;

            if (ultimaRequisicion != null)
            {
                // Extraer el número secuencial del formato REQ-YYYYMMDD-####
                var partes = ultimaRequisicion.Numero.Split('-');
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
            _logger.LogError(ex, "Error al generar número de requisición");
            return new ActionResponse<string>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
