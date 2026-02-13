using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class ActivoFijoRepository : IActivoFijoRepository
{
    private readonly DataContext _context;

    public ActivoFijoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse<ActivoFijo>> GetAsync(Guid id)
    {
        try
        {
            var activo = await _context.ActivosFijos
                .Include(a => a.Empresa)
                .Include(a => a.CategoriaActivo)
                .Include(a => a.Sucursal)
                .Include(a => a.Responsable)
                .Include(a => a.ProveedorEntity)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (activo == null)
            {
                return new ActionResponse<ActivoFijo>
                {
                    WasSuccess = false,
                    Message = "Activo fijo no encontrado"
                };
            }

            return new ActionResponse<ActivoFijo>
            {
                WasSuccess = true,
                Result = activo
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ActivoFijo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ActivoFijo>> GetWithDepreciacionesAsync(Guid id)
    {
        try
        {
            var activo = await _context.ActivosFijos
                .Include(a => a.Empresa)
                .Include(a => a.CategoriaActivo)
                .Include(a => a.Sucursal)
                .Include(a => a.Responsable)
                .Include(a => a.Depreciaciones!.Where(d => d.Estado != "REV"))
                    .ThenInclude(d => d.UsuarioCreacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (activo == null)
            {
                return new ActionResponse<ActivoFijo>
                {
                    WasSuccess = false,
                    Message = "Activo fijo no encontrado"
                };
            }

            return new ActionResponse<ActivoFijo>
            {
                WasSuccess = true,
                Result = activo
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ActivoFijo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ActivoFijo>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var activos = await _context.ActivosFijos
                .Include(a => a.CategoriaActivo)
                .Include(a => a.Sucursal)
                .Include(a => a.Responsable)
                .Where(a => a.EmpresaId == empresaId && !a.IsDeleted)
                .OrderBy(a => a.Codigo)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ActivoFijo>>
            {
                WasSuccess = true,
                Result = activos
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<ActivoFijo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ActivoFijo>>> GetByCategoriaAsync(Guid categoriaActivoId)
    {
        try
        {
            var activos = await _context.ActivosFijos
                .Include(a => a.CategoriaActivo)
                .Include(a => a.Sucursal)
                .Include(a => a.Responsable)
                .Where(a => a.CategoriaActivoId == categoriaActivoId && !a.IsDeleted)
                .OrderBy(a => a.Codigo)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ActivoFijo>>
            {
                WasSuccess = true,
                Result = activos
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<ActivoFijo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ActivoFijo>>> GetByUbicacionAsync(Guid empresaId, Guid? sucursalId, Guid? departamentoId)
    {
        try
        {
            var query = _context.ActivosFijos
                .Include(a => a.CategoriaActivo)
                .Include(a => a.Sucursal)
                .Include(a => a.Responsable)
                .Where(a => a.EmpresaId == empresaId && !a.IsDeleted);

            if (sucursalId.HasValue)
            {
                query = query.Where(a => a.SucursalId == sucursalId.Value);
            }

            if (departamentoId.HasValue)
            {
                query = query.Where(a => a.Responsable != null && a.Responsable.DepartamentoId == departamentoId.Value);
            }

            var activos = await query
                .OrderBy(a => a.Codigo)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ActivoFijo>>
            {
                WasSuccess = true,
                Result = activos
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<ActivoFijo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ActivoFijo>>> GetActivosAsync(Guid empresaId)
    {
        try
        {
            var activos = await _context.ActivosFijos
                .Include(a => a.CategoriaActivo)
                .Include(a => a.Sucursal)
                .Include(a => a.Responsable)
                .Where(a => a.EmpresaId == empresaId && !a.IsDeleted && a.Estado == "ACT")
                .OrderBy(a => a.Codigo)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ActivoFijo>>
            {
                WasSuccess = true,
                Result = activos
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<ActivoFijo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ActivoFijo>> GetByCodigoAsync(Guid empresaId, string codigo)
    {
        try
        {
            var activo = await _context.ActivosFijos
                .Include(a => a.CategoriaActivo)
                .Include(a => a.Sucursal)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.EmpresaId == empresaId && a.Codigo == codigo && !a.IsDeleted);

            if (activo == null)
            {
                return new ActionResponse<ActivoFijo>
                {
                    WasSuccess = false,
                    Message = "Activo fijo no encontrado con el código especificado"
                };
            }

            return new ActionResponse<ActivoFijo>
            {
                WasSuccess = true,
                Result = activo
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ActivoFijo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ActivoFijo>> AddAsync(ActivoFijo activo)
    {
        try
        {
            // Validar que el código sea único para la empresa
            var existeCodigo = await _context.ActivosFijos
                .AnyAsync(a => a.EmpresaId == activo.EmpresaId && a.Codigo == activo.Codigo && !a.IsDeleted);

            if (existeCodigo)
            {
                return new ActionResponse<ActivoFijo>
                {
                    WasSuccess = false,
                    Message = $"Ya existe un activo fijo con el código {activo.Codigo}"
                };
            }

            activo.FechaCreacion = FechaCostaRicaHelper.Ahora;
            activo.Estado = "ACT";
            activo.DepreciacionAcumulada = 0;

            _context.ActivosFijos.Add(activo);
            await _context.SaveChangesAsync();

            return new ActionResponse<ActivoFijo>
            {
                WasSuccess = true,
                Result = activo
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ActivoFijo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ActivoFijo>> UpdateAsync(ActivoFijo activo)
    {
        try
        {
            // Validar que el código sea único para la empresa (excluyendo el activo actual)
            var existeCodigo = await _context.ActivosFijos
                .AnyAsync(a => a.EmpresaId == activo.EmpresaId &&
                              a.Codigo == activo.Codigo &&
                              a.Id != activo.Id &&
                              !a.IsDeleted);

            if (existeCodigo)
            {
                return new ActionResponse<ActivoFijo>
                {
                    WasSuccess = false,
                    Message = $"Ya existe otro activo fijo con el código {activo.Codigo}"
                };
            }

            var activoExistente = await _context.ActivosFijos.FindAsync(activo.Id);

            if (activoExistente == null)
            {
                return new ActionResponse<ActivoFijo>
                {
                    WasSuccess = false,
                    Message = "Activo fijo no encontrado"
                };
            }

            activo.FechaModificacion = FechaCostaRicaHelper.Ahora;

            _context.Entry(activoExistente).CurrentValues.SetValues(activo);
            await _context.SaveChangesAsync();

            return new ActionResponse<ActivoFijo>
            {
                WasSuccess = true,
                Result = activoExistente
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<ActivoFijo>
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
            var activo = await _context.ActivosFijos.FindAsync(id);

            if (activo == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Activo fijo no encontrado"
                };
            }

            // Soft delete
            activo.IsDeleted = true;
            activo.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            activo.UsuarioEliminacionId = usuarioId;

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
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

    public async Task<ActionResponse<bool>> DarDeBajaAsync(Guid id, string usuarioId, string motivo, DateTime fechaBaja)
    {
        try
        {
            var activo = await _context.ActivosFijos.FindAsync(id);

            if (activo == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Activo fijo no encontrado"
                };
            }

            if (activo.Estado == "BAJ")
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "El activo ya se encuentra dado de baja"
                };
            }

            activo.Estado = "BAJ";
            activo.FechaBaja = fechaBaja;
            activo.MotivoBaja = motivo;
            activo.FechaModificacion = FechaCostaRicaHelper.Ahora;
            activo.UsuarioModificacionId = usuarioId;

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Activo dado de baja correctamente"
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
