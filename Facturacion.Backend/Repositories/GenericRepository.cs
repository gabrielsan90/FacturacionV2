using Facturacion.Backend.Data;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DataContext _context;
    private readonly DbSet<T> _entity;

    public GenericRepository(DataContext context)
    {
        _context = context;
        _entity = context.Set<T>();
    }

    public virtual async Task<ActionResponse<T>> AddAsync(T entity)
    {
        try
        {
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return new ActionResponse<T>
            {
                WasSuccess = true,
                Result = entity
            };
        }
        catch (DbUpdateException)
        {
            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = "Ya existe un registro con el mismo valor."
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public virtual async Task<ActionResponse<T>> DeleteAsync(int id)
    {
        try
        {
            var entity = await _entity.FindAsync(id);
            if (entity == null)
            {
                return new ActionResponse<T>
                {
                    WasSuccess = false,
                    Message = "Registro no encontrado."
                };
            }

            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return new ActionResponse<T>
            {
                WasSuccess = true
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public virtual async Task<ActionResponse<T>> GetAsync(int id)
    {
        try
        {
            var entity = await _entity.FindAsync(id);
            if (entity == null)
            {
                return new ActionResponse<T>
                {
                    WasSuccess = false,
                    Message = "Registro no encontrado."
                };
            }

            return new ActionResponse<T>
            {
                WasSuccess = true,
                Result = entity
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public virtual async Task<ActionResponse<IEnumerable<T>>> GetAsync()
    {
        try
        {
            var entities = await _entity.AsNoTracking().ToListAsync();
            return new ActionResponse<IEnumerable<T>>
            {
                WasSuccess = true,
                Result = entities
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<IEnumerable<T>>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public virtual async Task<ActionResponse<T>> UpdateAsync(T entity)
    {
        try
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return new ActionResponse<T>
            {
                WasSuccess = true,
                Result = entity
            };
        }
        catch (DbUpdateException)
        {
            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = "Ya existe un registro con el mismo valor."
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<T>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }
}
