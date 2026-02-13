using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class PlantillaAsientoRepository : IPlantillaAsientoRepository
{
    private readonly DataContext _context;

    public PlantillaAsientoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse<PlantillaAsiento>> GetAsync(Guid id)
    {
        try
        {
            var plantilla = await _context.PlantillasAsiento
                .Include(p => p.Empresa)
                .Include(p => p.CreadoPor)
                .Include(p => p.ModificadoPor)
                .Include(p => p.Lineas!)
                    .ThenInclude(l => l.CuentaContable)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (plantilla == null)
            {
                return new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = "Plantilla de asiento no encontrada"
                };
            }

            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = true,
                Result = plantilla
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<PlantillaAsiento>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var plantillas = await _context.PlantillasAsiento
                .Include(p => p.CreadoPor)
                .Include(p => p.Lineas)
                .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
                .OrderBy(p => p.Codigo)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<PlantillaAsiento>>
            {
                WasSuccess = true,
                Result = plantillas
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<PlantillaAsiento>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<PlantillaAsiento>> GetByIdWithLinesAsync(Guid id)
    {
        try
        {
            var plantilla = await _context.PlantillasAsiento
                .Include(p => p.CreadoPor)
                .Include(p => p.ModificadoPor)
                .Include(p => p.Lineas!.OrderBy(l => l.Orden))
                    .ThenInclude(l => l.CuentaContable)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (plantilla == null)
            {
                return new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = "Plantilla de asiento no encontrada"
                };
            }

            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = true,
                Result = plantilla
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<PlantillaAsiento>> GetByIdForUseAsync(Guid id)
    {
        try
        {
            var plantilla = await _context.PlantillasAsiento
                .Include(p => p.Lineas!.OrderBy(l => l.Orden))
                    .ThenInclude(l => l.CuentaContable)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (plantilla == null)
            {
                return new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = "Plantilla de asiento no encontrada"
                };
            }

            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = true,
                Result = plantilla
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<PlantillaAsiento>> AddAsync(PlantillaAsiento plantilla, IEnumerable<PlantillaAsientoLinea> lineas)
    {
        try
        {
            plantilla.FechaCreacion = FechaCostaRicaHelper.Ahora;
            plantilla.IsDeleted = false;

            await _context.PlantillasAsiento.AddAsync(plantilla);
            await _context.PlantillasAsientoLineas.AddRangeAsync(lineas);
            await _context.SaveChangesAsync();

            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = true,
                Message = "Plantilla de asiento creada exitosamente",
                Result = plantilla
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<PlantillaAsiento>> UpdateAsync(PlantillaAsiento plantilla, IEnumerable<PlantillaAsientoLinea> lineas)
    {
        try
        {
            plantilla.FechaModificacion = FechaCostaRicaHelper.Ahora;

            var plantillaExistente = await _context.PlantillasAsiento
                .Include(p => p.Lineas)
                .FirstOrDefaultAsync(p => p.Id == plantilla.Id && !p.IsDeleted);

            if (plantillaExistente == null)
            {
                return new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = "Plantilla de asiento no encontrada"
                };
            }

            // Update plantilla properties
            _context.Entry(plantillaExistente).CurrentValues.SetValues(plantilla);

            // Delete old lines
            if (plantillaExistente.Lineas != null && plantillaExistente.Lineas.Any())
            {
                _context.PlantillasAsientoLineas.RemoveRange(plantillaExistente.Lineas);
            }

            // Add new lines
            await _context.PlantillasAsientoLineas.AddRangeAsync(lineas);
            await _context.SaveChangesAsync();

            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = true,
                Message = "Plantilla de asiento actualizada exitosamente",
                Result = plantillaExistente
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<PlantillaAsiento>
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
            var plantilla = await _context.PlantillasAsiento
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (plantilla == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Plantilla de asiento no encontrada"
                };
            }

            plantilla.IsDeleted = true;
            plantilla.FechaModificacion = FechaCostaRicaHelper.Ahora;
            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = "Plantilla de asiento eliminada exitosamente",
                Result = true
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<PlantillaAsiento>> ToggleActivoAsync(Guid id, string? userId)
    {
        try
        {
            var plantilla = await _context.PlantillasAsiento
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (plantilla == null)
            {
                return new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = "Plantilla de asiento no encontrada"
                };
            }

            plantilla.Activo = !plantilla.Activo;
            plantilla.FechaModificacion = FechaCostaRicaHelper.Ahora;
            plantilla.ModificadoPorId = userId;

            await _context.SaveChangesAsync();

            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = true,
                Message = plantilla.Activo
                    ? "Plantilla activada exitosamente"
                    : "Plantilla desactivada exitosamente",
                Result = plantilla
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> CodigoExistsAsync(Guid empresaId, string codigo, Guid? excludeId = null)
    {
        try
        {
            var query = _context.PlantillasAsiento
                .Where(p => p.EmpresaId == empresaId && p.Codigo == codigo && !p.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = exists
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
