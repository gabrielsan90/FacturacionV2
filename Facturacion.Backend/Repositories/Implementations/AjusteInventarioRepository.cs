using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class AjusteInventarioRepository : IAjusteInventarioRepository
{
    private readonly DataContext _context;
    private readonly ILogger<AjusteInventarioRepository> _logger;

    public AjusteInventarioRepository(DataContext context, ILogger<AjusteInventarioRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<AjusteInventario>> GetAsync(Guid id)
    {
        try
        {
            var ajuste = await _context.AjustesInventario
                .Include(a => a.Empresa)
                .Include(a => a.Bodega)
                .Include(a => a.AprobadoPor)
                .Include(a => a.UsuarioCreacion)
                .Include(a => a.Detalles)
                    .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (ajuste == null)
            {
                return new ActionResponse<AjusteInventario>
                {
                    WasSuccess = false,
                    Message = "Ajuste de inventario no encontrado"
                };
            }

            return new ActionResponse<AjusteInventario>
            {
                WasSuccess = true,
                Result = ajuste
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ajuste de inventario {Id}", id);
            return new ActionResponse<AjusteInventario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<AjusteInventario>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var ajustes = await _context.AjustesInventario
                .Include(a => a.Bodega)
                .Include(a => a.AprobadoPor)
                .Include(a => a.UsuarioCreacion)
                .Where(a => a.EmpresaId == empresaId && !a.IsDeleted)
                .OrderByDescending(a => a.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<AjusteInventario>>
            {
                WasSuccess = true,
                Result = ajustes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ajustes de inventario de empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<AjusteInventario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<AjusteInventario>>> GetByProductoAsync(Guid productoId)
    {
        try
        {
            var ajustes = await _context.AjustesInventario
                .Include(a => a.Bodega)
                .Include(a => a.UsuarioCreacion)
                .Include(a => a.Detalles)
                .Where(a => a.Detalles!.Any(d => d.ProductoId == productoId) && !a.IsDeleted)
                .OrderByDescending(a => a.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<AjusteInventario>>
            {
                WasSuccess = true,
                Result = ajustes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ajustes de inventario del producto {ProductoId}", productoId);
            return new ActionResponse<IEnumerable<AjusteInventario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<AjusteInventario>>> GetByBodegaAsync(Guid bodegaId)
    {
        try
        {
            var ajustes = await _context.AjustesInventario
                .Include(a => a.UsuarioCreacion)
                .Include(a => a.AprobadoPor)
                .Where(a => a.BodegaId == bodegaId && !a.IsDeleted)
                .OrderByDescending(a => a.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<AjusteInventario>>
            {
                WasSuccess = true,
                Result = ajustes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ajustes de inventario de bodega {BodegaId}", bodegaId);
            return new ActionResponse<IEnumerable<AjusteInventario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<AjusteInventario>> AddAsync(AjusteInventario ajuste)
    {
        try
        {
            // Verificar que no exista otro ajuste con el mismo número en la misma empresa
            var existente = await _context.AjustesInventario
                .FirstOrDefaultAsync(a => a.EmpresaId == ajuste.EmpresaId &&
                                         a.Numero == ajuste.Numero &&
                                         !a.IsDeleted);

            if (existente != null)
            {
                return new ActionResponse<AjusteInventario>
                {
                    WasSuccess = false,
                    Message = $"Ya existe un ajuste de inventario con el número {ajuste.Numero}"
                };
            }

            ajuste.FechaCreacion = FechaCostaRicaHelper.Ahora;
            ajuste.Estado = "PEN"; // Pendiente por defecto

            _context.AjustesInventario.Add(ajuste);
            await _context.SaveChangesAsync();

            return new ActionResponse<AjusteInventario>
            {
                WasSuccess = true,
                Result = ajuste,
                Message = "Ajuste de inventario creado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear ajuste de inventario");
            return new ActionResponse<AjusteInventario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<AjusteInventario>> AprobarAsync(Guid id, string aprobadoPorId)
    {
        try
        {
            var ajuste = await _context.AjustesInventario
                .Include(a => a.Detalles)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (ajuste == null)
            {
                return new ActionResponse<AjusteInventario>
                {
                    WasSuccess = false,
                    Message = "Ajuste de inventario no encontrado"
                };
            }

            if (ajuste.Estado != "PEN")
            {
                return new ActionResponse<AjusteInventario>
                {
                    WasSuccess = false,
                    Message = "Solo se pueden aprobar ajustes en estado Pendiente"
                };
            }

            ajuste.Estado = "APR";
            ajuste.FechaAprobacion = FechaCostaRicaHelper.Ahora;
            ajuste.AprobadoPorId = aprobadoPorId;
            ajuste.FechaModificacion = FechaCostaRicaHelper.Ahora;

            await _context.SaveChangesAsync();

            return new ActionResponse<AjusteInventario>
            {
                WasSuccess = true,
                Result = ajuste,
                Message = "Ajuste de inventario aprobado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar ajuste de inventario");
            return new ActionResponse<AjusteInventario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<AjusteInventario>> AnularAsync(Guid id, string usuarioId)
    {
        try
        {
            var ajuste = await _context.AjustesInventario
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (ajuste == null)
            {
                return new ActionResponse<AjusteInventario>
                {
                    WasSuccess = false,
                    Message = "Ajuste de inventario no encontrado"
                };
            }

            if (ajuste.Estado == "ANU")
            {
                return new ActionResponse<AjusteInventario>
                {
                    WasSuccess = false,
                    Message = "El ajuste de inventario ya está anulado"
                };
            }

            if (ajuste.Estado == "APR")
            {
                return new ActionResponse<AjusteInventario>
                {
                    WasSuccess = false,
                    Message = "No se pueden anular ajustes aprobados. Los movimientos de inventario ya fueron aplicados."
                };
            }

            ajuste.Estado = "ANU";
            ajuste.FechaModificacion = FechaCostaRicaHelper.Ahora;
            ajuste.UsuarioModificacionId = usuarioId;

            await _context.SaveChangesAsync();

            return new ActionResponse<AjusteInventario>
            {
                WasSuccess = true,
                Result = ajuste,
                Message = "Ajuste de inventario anulado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular ajuste de inventario");
            return new ActionResponse<AjusteInventario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
