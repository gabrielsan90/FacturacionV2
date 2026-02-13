using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class ReglaConciliacionRepository : IReglaConciliacionRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ReglaConciliacionRepository> _logger;

    public ReglaConciliacionRepository(DataContext context, ILogger<ReglaConciliacionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<ReglaConciliacion>> GetAsync(Guid id)
    {
        try
        {
            var regla = await _context.ReglasConciliacion
                .Include(r => r.Empresa)
                .Include(r => r.CuentaBancaria)
                .Include(r => r.CuentaContableDefault)
                .Include(r => r.CreadoPor)
                .Include(r => r.ModificadoPor)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (regla == null)
            {
                return new ActionResponse<ReglaConciliacion>
                {
                    WasSuccess = false,
                    Message = "Regla de conciliación no encontrada"
                };
            }

            return new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = true,
                Result = regla
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener regla de conciliación {Id}", id);
            return new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ReglaConciliacion>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var reglas = await _context.ReglasConciliacion
                .Include(r => r.CuentaBancaria)
                .Include(r => r.CuentaContableDefault)
                .Include(r => r.CreadoPor)
                .Where(r => r.EmpresaId == empresaId)
                .OrderBy(r => r.Prioridad)
                .ThenBy(r => r.Nombre)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReglaConciliacion>>
            {
                WasSuccess = true,
                Result = reglas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reglas de conciliación de empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<ReglaConciliacion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ReglaConciliacion>>> GetActivasAsync(Guid empresaId)
    {
        try
        {
            var reglas = await _context.ReglasConciliacion
                .Include(r => r.CuentaBancaria)
                .Include(r => r.CuentaContableDefault)
                .Where(r => r.EmpresaId == empresaId && r.Activa)
                .OrderBy(r => r.Prioridad)
                .ThenBy(r => r.Nombre)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReglaConciliacion>>
            {
                WasSuccess = true,
                Result = reglas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reglas de conciliación activas");
            return new ActionResponse<IEnumerable<ReglaConciliacion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ReglaConciliacion>>> GetByCuentaBancariaAsync(Guid cuentaBancariaId)
    {
        try
        {
            var reglas = await _context.ReglasConciliacion
                .Include(r => r.CuentaContableDefault)
                .Include(r => r.CreadoPor)
                .Where(r => r.CuentaBancariaId == cuentaBancariaId || r.CuentaBancariaId == null)
                .OrderBy(r => r.Prioridad)
                .ThenBy(r => r.Nombre)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ReglaConciliacion>>
            {
                WasSuccess = true,
                Result = reglas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reglas de conciliación de cuenta bancaria {CuentaBancariaId}", cuentaBancariaId);
            return new ActionResponse<IEnumerable<ReglaConciliacion>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ReglaConciliacion>> AddAsync(ReglaConciliacion regla)
    {
        try
        {
            // Verificar que no exista otra regla con el mismo nombre en la misma empresa
            var existente = await _context.ReglasConciliacion
                .FirstOrDefaultAsync(r => r.EmpresaId == regla.EmpresaId &&
                                         r.Nombre == regla.Nombre);

            if (existente != null)
            {
                return new ActionResponse<ReglaConciliacion>
                {
                    WasSuccess = false,
                    Message = $"Ya existe una regla de conciliación con el nombre '{regla.Nombre}'"
                };
            }

            regla.FechaCreacion = FechaCostaRicaHelper.Ahora;

            _context.ReglasConciliacion.Add(regla);
            await _context.SaveChangesAsync();

            return new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = true,
                Result = regla,
                Message = "Regla de conciliación creada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear regla de conciliación");
            return new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ReglaConciliacion>> UpdateAsync(ReglaConciliacion regla)
    {
        try
        {
            var existente = await _context.ReglasConciliacion
                .FirstOrDefaultAsync(r => r.Id == regla.Id);

            if (existente == null)
            {
                return new ActionResponse<ReglaConciliacion>
                {
                    WasSuccess = false,
                    Message = "Regla de conciliación no encontrada"
                };
            }

            // Verificar que no exista otra regla con el mismo nombre
            var duplicado = await _context.ReglasConciliacion
                .FirstOrDefaultAsync(r => r.Id != regla.Id &&
                                         r.EmpresaId == regla.EmpresaId &&
                                         r.Nombre == regla.Nombre);

            if (duplicado != null)
            {
                return new ActionResponse<ReglaConciliacion>
                {
                    WasSuccess = false,
                    Message = $"Ya existe otra regla de conciliación con el nombre '{regla.Nombre}'"
                };
            }

            regla.FechaModificacion = FechaCostaRicaHelper.Ahora;
            regla.FechaCreacion = existente.FechaCreacion;
            regla.CreadoPorId = existente.CreadoPorId;

            _context.Entry(existente).CurrentValues.SetValues(regla);
            await _context.SaveChangesAsync();

            return new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = true,
                Result = regla,
                Message = "Regla de conciliación actualizada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar regla de conciliación");
            return new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var regla = await _context.ReglasConciliacion.FindAsync(id);

            if (regla == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Regla de conciliación no encontrada"
                };
            }

            // Verificar si la regla ha sido utilizada en conciliaciones
            var tieneUso = await _context.LineasExtractoBancario
                .AnyAsync(l => l.ReglaConciliacionId == id);

            if (tieneUso)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar la regla porque ha sido utilizada en conciliaciones. Considere desactivarla en su lugar."
                };
            }

            // Eliminación física (no soft delete ya que la entidad no tiene IsDeleted)
            _context.ReglasConciliacion.Remove(regla);
            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Regla de conciliación eliminada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar regla de conciliación");
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ReglaConciliacion>> ToggleActivaAsync(Guid id)
    {
        try
        {
            var regla = await _context.ReglasConciliacion.FindAsync(id);

            if (regla == null)
            {
                return new ActionResponse<ReglaConciliacion>
                {
                    WasSuccess = false,
                    Message = "Regla de conciliación no encontrada"
                };
            }

            regla.Activa = !regla.Activa;
            regla.FechaModificacion = FechaCostaRicaHelper.Ahora;

            await _context.SaveChangesAsync();

            return new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = true,
                Result = regla,
                Message = $"Regla de conciliación {(regla.Activa ? "activada" : "desactivada")} exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de regla de conciliación");
            return new ActionResponse<ReglaConciliacion>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
