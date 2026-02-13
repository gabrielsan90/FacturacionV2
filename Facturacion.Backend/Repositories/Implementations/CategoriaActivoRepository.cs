using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class CategoriaActivoRepository : ICategoriaActivoRepository
{
    private readonly DataContext _context;

    public CategoriaActivoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse<CategoriaActivo>> GetAsync(Guid id)
    {
        try
        {
            var categoria = await _context.CategoriasActivo
                .Include(c => c.Empresa)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (categoria == null)
            {
                return new ActionResponse<CategoriaActivo>
                {
                    WasSuccess = false,
                    Message = "Categoría de activo no encontrada"
                };
            }

            return new ActionResponse<CategoriaActivo>
            {
                WasSuccess = true,
                Result = categoria
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<CategoriaActivo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CategoriaActivo>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var categorias = await _context.CategoriasActivo
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
                .OrderBy(c => c.Codigo)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CategoriaActivo>>
            {
                WasSuccess = true,
                Result = categorias
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<CategoriaActivo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CategoriaActivo>>> GetActivasAsync(Guid empresaId)
    {
        try
        {
            var categorias = await _context.CategoriasActivo
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.Activo)
                .OrderBy(c => c.Codigo)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CategoriaActivo>>
            {
                WasSuccess = true,
                Result = categorias
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<CategoriaActivo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CategoriaActivo>> GetByCodigoAsync(Guid empresaId, string codigo)
    {
        try
        {
            var categoria = await _context.CategoriasActivo
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Codigo == codigo && !c.IsDeleted);

            if (categoria == null)
            {
                return new ActionResponse<CategoriaActivo>
                {
                    WasSuccess = false,
                    Message = "Categoría de activo no encontrada con el código especificado"
                };
            }

            return new ActionResponse<CategoriaActivo>
            {
                WasSuccess = true,
                Result = categoria
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<CategoriaActivo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CategoriaActivo>> AddAsync(CategoriaActivo categoria)
    {
        try
        {
            // Validar que el código sea único para la empresa
            var existeCodigo = await _context.CategoriasActivo
                .AnyAsync(c => c.EmpresaId == categoria.EmpresaId && c.Codigo == categoria.Codigo && !c.IsDeleted);

            if (existeCodigo)
            {
                return new ActionResponse<CategoriaActivo>
                {
                    WasSuccess = false,
                    Message = $"Ya existe una categoría con el código {categoria.Codigo}"
                };
            }

            categoria.FechaCreacion = FechaCostaRicaHelper.Ahora;
            categoria.Activo = true;

            _context.CategoriasActivo.Add(categoria);
            await _context.SaveChangesAsync();

            return new ActionResponse<CategoriaActivo>
            {
                WasSuccess = true,
                Result = categoria
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<CategoriaActivo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CategoriaActivo>> UpdateAsync(CategoriaActivo categoria)
    {
        try
        {
            // Validar que el código sea único para la empresa (excluyendo la categoría actual)
            var existeCodigo = await _context.CategoriasActivo
                .AnyAsync(c => c.EmpresaId == categoria.EmpresaId &&
                              c.Codigo == categoria.Codigo &&
                              c.Id != categoria.Id &&
                              !c.IsDeleted);

            if (existeCodigo)
            {
                return new ActionResponse<CategoriaActivo>
                {
                    WasSuccess = false,
                    Message = $"Ya existe otra categoría con el código {categoria.Codigo}"
                };
            }

            var categoriaExistente = await _context.CategoriasActivo.FindAsync(categoria.Id);

            if (categoriaExistente == null)
            {
                return new ActionResponse<CategoriaActivo>
                {
                    WasSuccess = false,
                    Message = "Categoría de activo no encontrada"
                };
            }

            categoria.FechaModificacion = FechaCostaRicaHelper.Ahora;

            _context.Entry(categoriaExistente).CurrentValues.SetValues(categoria);
            await _context.SaveChangesAsync();

            return new ActionResponse<CategoriaActivo>
            {
                WasSuccess = true,
                Result = categoriaExistente
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<CategoriaActivo>
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
            var categoria = await _context.CategoriasActivo.FindAsync(id);

            if (categoria == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Categoría de activo no encontrada"
                };
            }

            // Verificar si tiene activos asociados
            var tieneActivos = await _context.ActivosFijos
                .AnyAsync(a => a.CategoriaActivoId == id && !a.IsDeleted);

            if (tieneActivos)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar la categoría porque tiene activos fijos asociados"
                };
            }

            // Soft delete
            categoria.IsDeleted = true;
            categoria.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            categoria.UsuarioEliminacionId = usuarioId;

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
}
