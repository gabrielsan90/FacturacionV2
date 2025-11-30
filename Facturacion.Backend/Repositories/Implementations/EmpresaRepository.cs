using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class EmpresaRepository : GenericRepository<Empresa>, IEmpresaRepository
{
    private readonly DataContext _context;

    public EmpresaRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ActionResponse<Empresa>> GetAsync(Guid id)
    {
        try
        {
            var empresa = await _context.Empresas
                .Include(e => e.Telefonos)
                .Include(e => e.Emails)
                .Include(e => e.ActividadesEconomicas!)
                    .ThenInclude(ea => ea.ActividadEconomica)
                .Include(e => e.UsuarioCreacion)
                .Include(e => e.UsuarioModificacion)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (empresa == null)
            {
                return new ActionResponse<Empresa>
                {
                    WasSuccess = false,
                    Message = "Empresa no encontrada."
                };
            }

            return new ActionResponse<Empresa>
            {
                WasSuccess = true,
                Result = empresa
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Empresa>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public override async Task<ActionResponse<IEnumerable<Empresa>>> GetAsync()
    {
        try
        {
            var empresas = await _context.Empresas
                .Include(e => e.Telefonos)
                .Include(e => e.Emails)
                .Include(e => e.ActividadesEconomicas!)
                    .ThenInclude(ea => ea.ActividadEconomica)
                .Include(e => e.UsuarioCreacion)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Empresa>>
            {
                WasSuccess = true,
                Result = empresas
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<IEnumerable<Empresa>>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<Empresa>>> GetAllActiveAsync()
    {
        try
        {
            var empresas = await _context.Empresas
                .Include(e => e.Telefonos)
                .Include(e => e.Emails)
                .Where(e => !e.IsDeleted && e.Activa)
                .OrderBy(e => e.NombreComercial)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Empresa>>
            {
                WasSuccess = true,
                Result = empresas
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<IEnumerable<Empresa>>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<Empresa>> GetByIdentificationAsync(string numeroIdentificacion)
    {
        try
        {
            var empresa = await _context.Empresas
                .Include(e => e.Telefonos)
                .Include(e => e.Emails)
                .Include(e => e.ActividadesEconomicas!)
                    .ThenInclude(ea => ea.ActividadEconomica)
                .FirstOrDefaultAsync(e => e.NumeroIdentificacion == numeroIdentificacion && !e.IsDeleted);

            if (empresa == null)
            {
                return new ActionResponse<Empresa>
                {
                    WasSuccess = false,
                    Message = "Empresa no encontrada."
                };
            }

            return new ActionResponse<Empresa>
            {
                WasSuccess = true,
                Result = empresa
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Empresa>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public override async Task<ActionResponse<Empresa>> AddAsync(Empresa empresa)
    {
        try
        {
            empresa.FechaCreacion = DateTime.UtcNow;
            empresa.Activa = true;
            empresa.IsDeleted = false;

            _context.Add(empresa);
            await _context.SaveChangesAsync();

            return new ActionResponse<Empresa>
            {
                WasSuccess = true,
                Result = empresa
            };
        }
        catch (DbUpdateException)
        {
            return new ActionResponse<Empresa>
            {
                WasSuccess = false,
                Message = "Ya existe una empresa con ese número de identificación."
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Empresa>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public override async Task<ActionResponse<Empresa>> UpdateAsync(Empresa empresa)
    {
        try
        {
            // Detach any existing tracked entity with the same ID to avoid tracking conflicts
            var local = _context.Set<Empresa>()
                .Local
                .FirstOrDefault(e => e.Id == empresa.Id);
            if (local != null)
            {
                _context.Entry(local).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }

            empresa.FechaModificacion = DateTime.UtcNow;

            _context.Update(empresa);
            await _context.SaveChangesAsync();

            return new ActionResponse<Empresa>
            {
                WasSuccess = true,
                Result = empresa
            };
        }
        catch (DbUpdateException)
        {
            return new ActionResponse<Empresa>
            {
                WasSuccess = false,
                Message = "Ya existe una empresa con ese número de identificación."
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Empresa>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    public async Task<ActionResponse<Empresa>> DeleteAsync(Guid id)
    {
        try
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
            {
                return new ActionResponse<Empresa>
                {
                    WasSuccess = false,
                    Message = "Empresa no encontrada."
                };
            }

            // Soft delete
            empresa.IsDeleted = true;
            empresa.FechaEliminacion = DateTime.UtcNow;

            _context.Update(empresa);
            await _context.SaveChangesAsync();

            return new ActionResponse<Empresa>
            {
                WasSuccess = true
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<Empresa>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }
}
