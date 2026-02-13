using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class TrasladoActivoRepository : ITrasladoActivoRepository
{
    private readonly DataContext _context;

    public TrasladoActivoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse<TrasladoActivo>> GetAsync(Guid id)
    {
        try
        {
            var traslado = await _context.TrasladosActivo
                .Include(t => t.Empresa)
                .Include(t => t.ActivoFijo)
                    .ThenInclude(a => a!.CategoriaActivo)
                .Include(t => t.SucursalOrigen)
                .Include(t => t.SucursalDestino)
                .Include(t => t.ResponsableOrigen)
                .Include(t => t.ResponsableDestino)
                .Include(t => t.AprobadoPor)
                .Include(t => t.RecibidoPor)
                .Include(t => t.UsuarioCreacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (traslado == null)
            {
                return new ActionResponse<TrasladoActivo>
                {
                    WasSuccess = false,
                    Message = "Traslado de activo no encontrado"
                };
            }

            return new ActionResponse<TrasladoActivo>
            {
                WasSuccess = true,
                Result = traslado
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<TrasladoActivo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<TrasladoActivo>>> GetByActivoAsync(Guid activoFijoId)
    {
        try
        {
            var traslados = await _context.TrasladosActivo
                .Include(t => t.SucursalOrigen)
                .Include(t => t.SucursalDestino)
                .Include(t => t.ResponsableOrigen)
                .Include(t => t.ResponsableDestino)
                .Include(t => t.UsuarioCreacion)
                .Where(t => t.ActivoFijoId == activoFijoId)
                .OrderByDescending(t => t.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<TrasladoActivo>>
            {
                WasSuccess = true,
                Result = traslados
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<TrasladoActivo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<TrasladoActivo>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var traslados = await _context.TrasladosActivo
                .Include(t => t.ActivoFijo)
                    .ThenInclude(a => a!.CategoriaActivo)
                .Include(t => t.SucursalOrigen)
                .Include(t => t.SucursalDestino)
                .Include(t => t.ResponsableOrigen)
                .Include(t => t.ResponsableDestino)
                .Where(t => t.EmpresaId == empresaId)
                .OrderByDescending(t => t.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<TrasladoActivo>>
            {
                WasSuccess = true,
                Result = traslados
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<TrasladoActivo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<TrasladoActivo>>> GetPendientesAsync(Guid empresaId)
    {
        try
        {
            var traslados = await _context.TrasladosActivo
                .Include(t => t.ActivoFijo)
                    .ThenInclude(a => a!.CategoriaActivo)
                .Include(t => t.SucursalOrigen)
                .Include(t => t.SucursalDestino)
                .Include(t => t.ResponsableOrigen)
                .Include(t => t.ResponsableDestino)
                .Where(t => t.EmpresaId == empresaId && (t.Estado == "PEN" || t.Estado == "APR"))
                .OrderBy(t => t.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<TrasladoActivo>>
            {
                WasSuccess = true,
                Result = traslados
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<TrasladoActivo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<TrasladoActivo>> AddAsync(TrasladoActivo traslado)
    {
        try
        {
            // Validar que el activo exista
            var activo = await _context.ActivosFijos.FindAsync(traslado.ActivoFijoId);

            if (activo == null)
            {
                return new ActionResponse<TrasladoActivo>
                {
                    WasSuccess = false,
                    Message = "Activo fijo no encontrado"
                };
            }

            // Validar que el activo esté activo
            if (activo.Estado != "ACT")
            {
                return new ActionResponse<TrasladoActivo>
                {
                    WasSuccess = false,
                    Message = "Solo se pueden trasladar activos en estado activo"
                };
            }

            // Validar que origen y destino sean diferentes
            if (traslado.SucursalOrigenId == traslado.SucursalDestinoId &&
                traslado.ResponsableOrigenId == traslado.ResponsableDestinoId)
            {
                return new ActionResponse<TrasladoActivo>
                {
                    WasSuccess = false,
                    Message = "La sucursal y responsable de origen deben ser diferentes al destino"
                };
            }

            // Generar número de traslado si no se proporciona
            if (string.IsNullOrEmpty(traslado.Numero))
            {
                var ultimoTraslado = await _context.TrasladosActivo
                    .Where(t => t.EmpresaId == traslado.EmpresaId)
                    .OrderByDescending(t => t.Numero)
                    .FirstOrDefaultAsync();

                int siguienteNumero = 1;
                if (ultimoTraslado != null && int.TryParse(ultimoTraslado.Numero.Split('-').LastOrDefault(), out int ultimo))
                {
                    siguienteNumero = ultimo + 1;
                }

                traslado.Numero = $"TRAS-{siguienteNumero:D8}";
            }

            traslado.FechaCreacion = FechaCostaRicaHelper.Ahora;
            traslado.Estado = "PEN";

            _context.TrasladosActivo.Add(traslado);
            await _context.SaveChangesAsync();

            return new ActionResponse<TrasladoActivo>
            {
                WasSuccess = true,
                Result = traslado
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<TrasladoActivo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<TrasladoActivo>> UpdateAsync(TrasladoActivo traslado)
    {
        try
        {
            var trasladoExistente = await _context.TrasladosActivo.FindAsync(traslado.Id);

            if (trasladoExistente == null)
            {
                return new ActionResponse<TrasladoActivo>
                {
                    WasSuccess = false,
                    Message = "Traslado de activo no encontrado"
                };
            }

            // Solo se pueden modificar traslados pendientes
            if (trasladoExistente.Estado != "PEN")
            {
                return new ActionResponse<TrasladoActivo>
                {
                    WasSuccess = false,
                    Message = "Solo se pueden modificar traslados en estado pendiente"
                };
            }

            traslado.FechaModificacion = FechaCostaRicaHelper.Ahora;

            _context.Entry(trasladoExistente).CurrentValues.SetValues(traslado);
            await _context.SaveChangesAsync();

            return new ActionResponse<TrasladoActivo>
            {
                WasSuccess = true,
                Result = trasladoExistente
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<TrasladoActivo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> AprobarTrasladoAsync(Guid id, string usuarioId)
    {
        try
        {
            var traslado = await _context.TrasladosActivo.FindAsync(id);

            if (traslado == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Traslado de activo no encontrado"
                };
            }

            if (traslado.Estado != "PEN")
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Solo se pueden aprobar traslados en estado pendiente"
                };
            }

            traslado.Estado = "APR";
            traslado.FechaAprobacion = FechaCostaRicaHelper.Ahora;
            traslado.AprobadoPorId = usuarioId;

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Traslado aprobado correctamente"
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

    public async Task<ActionResponse<bool>> RecibirTrasladoAsync(Guid id, string usuarioId)
    {
        try
        {
            var traslado = await _context.TrasladosActivo
                .Include(t => t.ActivoFijo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (traslado == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Traslado de activo no encontrado"
                };
            }

            if (traslado.Estado != "APR")
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Solo se pueden recibir traslados en estado aprobado"
                };
            }

            // Actualizar ubicación del activo
            if (traslado.ActivoFijo != null)
            {
                traslado.ActivoFijo.SucursalId = traslado.SucursalDestinoId;
                traslado.ActivoFijo.ResponsableId = traslado.ResponsableDestinoId;
                traslado.ActivoFijo.Estado = "TRA"; // Trasladado
                traslado.ActivoFijo.FechaModificacion = FechaCostaRicaHelper.Ahora;
                traslado.ActivoFijo.UsuarioModificacionId = usuarioId;
            }

            traslado.Estado = "REC";
            traslado.FechaRecepcion = FechaCostaRicaHelper.Ahora;
            traslado.RecibidoPorId = usuarioId;

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Traslado recibido correctamente. El activo ha sido actualizado en su nueva ubicación."
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

    public async Task<ActionResponse<bool>> CancelarTrasladoAsync(Guid id)
    {
        try
        {
            var traslado = await _context.TrasladosActivo.FindAsync(id);

            if (traslado == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Traslado de activo no encontrado"
                };
            }

            if (traslado.Estado == "REC")
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede cancelar un traslado que ya fue recibido"
                };
            }

            if (traslado.Estado == "CAN")
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "El traslado ya está cancelado"
                };
            }

            traslado.Estado = "CAN";

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Traslado cancelado correctamente"
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
