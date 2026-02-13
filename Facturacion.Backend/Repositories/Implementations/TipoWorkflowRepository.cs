using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class TipoWorkflowRepository : ITipoWorkflowRepository
{
    private readonly DataContext _context;
    private readonly ILogger<TipoWorkflowRepository> _logger;

    public TipoWorkflowRepository(DataContext context, ILogger<TipoWorkflowRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<TipoWorkflow>> GetAsync(Guid id)
    {
        try
        {
            var tipoWorkflow = await _context.TiposWorkflow
                .Include(t => t.Empresa)
                .Include(t => t.CreadoPor)
                .Include(t => t.ModificadoPor)
                .Include(t => t.Niveles)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (tipoWorkflow == null)
            {
                return new ActionResponse<TipoWorkflow>
                {
                    WasSuccess = false,
                    Message = "Tipo de workflow no encontrado"
                };
            }

            return new ActionResponse<TipoWorkflow>
            {
                WasSuccess = true,
                Result = tipoWorkflow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipo de workflow {Id}", id);
            return new ActionResponse<TipoWorkflow>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<TipoWorkflow>>> GetAllAsync(Guid empresaId)
    {
        try
        {
            var tipos = await _context.TiposWorkflow
                .Include(t => t.CreadoPor)
                .Include(t => t.Niveles)
                .Where(t => t.EmpresaId == empresaId && !t.IsDeleted)
                .OrderBy(t => t.Nombre)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<TipoWorkflow>>
            {
                WasSuccess = true,
                Result = tipos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de workflow de empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<TipoWorkflow>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<TipoWorkflow>>> GetActivosAsync(Guid empresaId)
    {
        try
        {
            var tipos = await _context.TiposWorkflow
                .Include(t => t.Niveles)
                .Where(t => t.EmpresaId == empresaId && t.Activo && !t.IsDeleted)
                .OrderBy(t => t.Nombre)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<TipoWorkflow>>
            {
                WasSuccess = true,
                Result = tipos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de workflow activos");
            return new ActionResponse<IEnumerable<TipoWorkflow>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<TipoWorkflow>> GetByCodigoAsync(Guid empresaId, string codigo)
    {
        try
        {
            var tipoWorkflow = await _context.TiposWorkflow
                .Include(t => t.Niveles)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.EmpresaId == empresaId &&
                                         t.Codigo == codigo &&
                                         !t.IsDeleted);

            if (tipoWorkflow == null)
            {
                return new ActionResponse<TipoWorkflow>
                {
                    WasSuccess = false,
                    Message = $"Tipo de workflow con código '{codigo}' no encontrado"
                };
            }

            return new ActionResponse<TipoWorkflow>
            {
                WasSuccess = true,
                Result = tipoWorkflow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar tipo de workflow por código");
            return new ActionResponse<TipoWorkflow>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<TipoWorkflow>> AddAsync(TipoWorkflow tipoWorkflow)
    {
        try
        {
            // Verificar que no exista otro tipo de workflow con el mismo código en la misma empresa
            var existenteCodigo = await _context.TiposWorkflow
                .FirstOrDefaultAsync(t => t.EmpresaId == tipoWorkflow.EmpresaId &&
                                         t.Codigo == tipoWorkflow.Codigo &&
                                         !t.IsDeleted);

            if (existenteCodigo != null)
            {
                return new ActionResponse<TipoWorkflow>
                {
                    WasSuccess = false,
                    Message = $"Ya existe un tipo de workflow con el código '{tipoWorkflow.Codigo}'"
                };
            }

            // Verificar que no exista otro tipo de workflow con el mismo nombre
            var existenteNombre = await _context.TiposWorkflow
                .FirstOrDefaultAsync(t => t.EmpresaId == tipoWorkflow.EmpresaId &&
                                         t.Nombre == tipoWorkflow.Nombre &&
                                         !t.IsDeleted);

            if (existenteNombre != null)
            {
                return new ActionResponse<TipoWorkflow>
                {
                    WasSuccess = false,
                    Message = $"Ya existe un tipo de workflow con el nombre '{tipoWorkflow.Nombre}'"
                };
            }

            tipoWorkflow.FechaCreacion = FechaCostaRicaHelper.Ahora;

            _context.TiposWorkflow.Add(tipoWorkflow);
            await _context.SaveChangesAsync();

            return new ActionResponse<TipoWorkflow>
            {
                WasSuccess = true,
                Result = tipoWorkflow,
                Message = "Tipo de workflow creado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tipo de workflow");
            return new ActionResponse<TipoWorkflow>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<TipoWorkflow>> UpdateAsync(TipoWorkflow tipoWorkflow)
    {
        try
        {
            var existente = await _context.TiposWorkflow
                .FirstOrDefaultAsync(t => t.Id == tipoWorkflow.Id && !t.IsDeleted);

            if (existente == null)
            {
                return new ActionResponse<TipoWorkflow>
                {
                    WasSuccess = false,
                    Message = "Tipo de workflow no encontrado"
                };
            }

            // Verificar que no exista otro tipo de workflow con el mismo código
            var duplicadoCodigo = await _context.TiposWorkflow
                .FirstOrDefaultAsync(t => t.Id != tipoWorkflow.Id &&
                                         t.EmpresaId == tipoWorkflow.EmpresaId &&
                                         t.Codigo == tipoWorkflow.Codigo &&
                                         !t.IsDeleted);

            if (duplicadoCodigo != null)
            {
                return new ActionResponse<TipoWorkflow>
                {
                    WasSuccess = false,
                    Message = $"Ya existe otro tipo de workflow con el código '{tipoWorkflow.Codigo}'"
                };
            }

            // Verificar que no exista otro tipo de workflow con el mismo nombre
            var duplicadoNombre = await _context.TiposWorkflow
                .FirstOrDefaultAsync(t => t.Id != tipoWorkflow.Id &&
                                         t.EmpresaId == tipoWorkflow.EmpresaId &&
                                         t.Nombre == tipoWorkflow.Nombre &&
                                         !t.IsDeleted);

            if (duplicadoNombre != null)
            {
                return new ActionResponse<TipoWorkflow>
                {
                    WasSuccess = false,
                    Message = $"Ya existe otro tipo de workflow con el nombre '{tipoWorkflow.Nombre}'"
                };
            }

            tipoWorkflow.FechaModificacion = FechaCostaRicaHelper.Ahora;
            tipoWorkflow.FechaCreacion = existente.FechaCreacion;
            tipoWorkflow.CreadoPorId = existente.CreadoPorId;

            _context.Entry(existente).CurrentValues.SetValues(tipoWorkflow);
            await _context.SaveChangesAsync();

            return new ActionResponse<TipoWorkflow>
            {
                WasSuccess = true,
                Result = tipoWorkflow,
                Message = "Tipo de workflow actualizado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tipo de workflow");
            return new ActionResponse<TipoWorkflow>
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
            var tipoWorkflow = await _context.TiposWorkflow.FindAsync(id);

            if (tipoWorkflow == null || tipoWorkflow.IsDeleted)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Tipo de workflow no encontrado"
                };
            }

            // Verificar si tiene solicitudes asociadas
            var tieneSolicitudes = await _context.SolicitudesAprobacion
                .AnyAsync(s => s.TipoWorkflowId == id && s.Estado != "CAN");

            if (tieneSolicitudes)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar el tipo de workflow porque tiene solicitudes de aprobación asociadas. Considere desactivarlo en su lugar."
                };
            }

            // Soft delete
            tipoWorkflow.IsDeleted = true;
            tipoWorkflow.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            tipoWorkflow.UsuarioEliminacionId = usuarioId;

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Tipo de workflow eliminado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tipo de workflow");
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<TipoWorkflow>> ToggleActivoAsync(Guid id)
    {
        try
        {
            var tipoWorkflow = await _context.TiposWorkflow
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (tipoWorkflow == null)
            {
                return new ActionResponse<TipoWorkflow>
                {
                    WasSuccess = false,
                    Message = "Tipo de workflow no encontrado"
                };
            }

            tipoWorkflow.Activo = !tipoWorkflow.Activo;
            tipoWorkflow.FechaModificacion = FechaCostaRicaHelper.Ahora;

            await _context.SaveChangesAsync();

            return new ActionResponse<TipoWorkflow>
            {
                WasSuccess = true,
                Result = tipoWorkflow,
                Message = $"Tipo de workflow {(tipoWorkflow.Activo ? "activado" : "desactivado")} exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de tipo de workflow");
            return new ActionResponse<TipoWorkflow>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
