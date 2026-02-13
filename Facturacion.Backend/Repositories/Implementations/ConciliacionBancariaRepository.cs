using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class ConciliacionBancariaRepository : IConciliacionBancariaRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ConciliacionBancariaRepository> _logger;

    public ConciliacionBancariaRepository(DataContext context, ILogger<ConciliacionBancariaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<ConciliacionBancaria>> GetAsync(Guid id)
    {
        try
        {
            var conciliacion = await _context.ConciliacionesBancarias
                .Include(c => c.Empresa)
                .Include(c => c.CuentaBancaria)
                .Include(c => c.Movimientos)
                .Include(c => c.Extractos)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conciliacion == null)
            {
                return new ActionResponse<ConciliacionBancaria>
                {
                    WasSuccess = false,
                    Message = "Conciliación bancaria no encontrada"
                };
            }

            return new ActionResponse<ConciliacionBancaria>
            {
                WasSuccess = true,
                Result = conciliacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conciliación bancaria {Id}", id);
            return new ActionResponse<ConciliacionBancaria>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ConciliacionBancaria>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var conciliaciones = await _context.ConciliacionesBancarias
                .Include(c => c.CuentaBancaria)
                .Where(c => c.EmpresaId == empresaId)
                .OrderByDescending(c => c.Anio)
                .ThenByDescending(c => c.Mes)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ConciliacionBancaria>>
            {
                WasSuccess = true,
                Result = conciliaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conciliaciones de empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<ConciliacionBancaria>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ConciliacionBancaria>>> GetByCuentaBancariaAsync(Guid cuentaBancariaId)
    {
        try
        {
            var conciliaciones = await _context.ConciliacionesBancarias
                .Include(c => c.CuentaBancaria)
                .Where(c => c.CuentaBancariaId == cuentaBancariaId)
                .OrderByDescending(c => c.Anio)
                .ThenByDescending(c => c.Mes)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ConciliacionBancaria>>
            {
                WasSuccess = true,
                Result = conciliaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conciliaciones de cuenta bancaria");
            return new ActionResponse<IEnumerable<ConciliacionBancaria>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ConciliacionBancaria>> GetByPeriodoAsync(Guid cuentaBancariaId, int mes, int anio)
    {
        try
        {
            var conciliacion = await _context.ConciliacionesBancarias
                .Include(c => c.CuentaBancaria)
                .Include(c => c.Movimientos)
                .Include(c => c.Extractos)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CuentaBancariaId == cuentaBancariaId &&
                                         c.Mes == mes &&
                                         c.Anio == anio);

            if (conciliacion == null)
            {
                return new ActionResponse<ConciliacionBancaria>
                {
                    WasSuccess = false,
                    Message = $"No se encontró conciliación para el período {mes:00}/{anio}"
                };
            }

            return new ActionResponse<ConciliacionBancaria>
            {
                WasSuccess = true,
                Result = conciliacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conciliación por período");
            return new ActionResponse<ConciliacionBancaria>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ConciliacionBancaria>>> GetPendientesAsync(Guid empresaId)
    {
        try
        {
            var conciliaciones = await _context.ConciliacionesBancarias
                .Include(c => c.CuentaBancaria)
                .Where(c => c.EmpresaId == empresaId &&
                           (c.Estado == EstadosConciliacionBancaria.Pendiente ||
                            c.Estado == EstadosConciliacionBancaria.Parcial))
                .OrderBy(c => c.Anio)
                .ThenBy(c => c.Mes)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ConciliacionBancaria>>
            {
                WasSuccess = true,
                Result = conciliaciones
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conciliaciones pendientes");
            return new ActionResponse<IEnumerable<ConciliacionBancaria>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ConciliacionBancaria>> AddAsync(ConciliacionBancaria conciliacion)
    {
        try
        {
            // Verificar que no exista otra conciliación para el mismo período
            var existente = await _context.ConciliacionesBancarias
                .FirstOrDefaultAsync(c => c.CuentaBancariaId == conciliacion.CuentaBancariaId &&
                                         c.Mes == conciliacion.Mes &&
                                         c.Anio == conciliacion.Anio);

            if (existente != null)
            {
                return new ActionResponse<ConciliacionBancaria>
                {
                    WasSuccess = false,
                    Message = $"Ya existe una conciliación para el período {conciliacion.Mes:00}/{conciliacion.Anio}"
                };
            }

            conciliacion.FechaCreacion = FechaCostaRicaHelper.Ahora;
            conciliacion.Estado = EstadosConciliacionBancaria.Pendiente;

            _context.ConciliacionesBancarias.Add(conciliacion);
            await _context.SaveChangesAsync();

            return new ActionResponse<ConciliacionBancaria>
            {
                WasSuccess = true,
                Result = conciliacion,
                Message = "Conciliación bancaria creada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear conciliación bancaria");
            return new ActionResponse<ConciliacionBancaria>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ConciliacionBancaria>> UpdateAsync(ConciliacionBancaria conciliacion)
    {
        try
        {
            var existente = await _context.ConciliacionesBancarias
                .FirstOrDefaultAsync(c => c.Id == conciliacion.Id);

            if (existente == null)
            {
                return new ActionResponse<ConciliacionBancaria>
                {
                    WasSuccess = false,
                    Message = "Conciliación bancaria no encontrada"
                };
            }

            // No permitir modificar conciliaciones cerradas
            if (existente.Estado == EstadosConciliacionBancaria.Cerrada)
            {
                return new ActionResponse<ConciliacionBancaria>
                {
                    WasSuccess = false,
                    Message = "No se puede modificar una conciliación que ya ha sido cerrada"
                };
            }

            conciliacion.FechaModificacion = FechaCostaRicaHelper.Ahora;
            conciliacion.FechaCreacion = existente.FechaCreacion;
            conciliacion.CreadoPorId = existente.CreadoPorId;

            // Recalcular el saldo conciliado y la diferencia
            conciliacion.Diferencia = Math.Abs(conciliacion.SaldoFinalLibros - conciliacion.SaldoConciliado);

            // Actualizar estado según el resultado de la conciliación
            if (conciliacion.EstaConciliada)
            {
                conciliacion.Estado = EstadosConciliacionBancaria.Conciliada;
            }
            else if (conciliacion.Diferencia < Math.Abs(existente.SaldoFinalLibros - existente.SaldoEstadoCuenta))
            {
                conciliacion.Estado = EstadosConciliacionBancaria.Parcial;
            }

            _context.Entry(existente).CurrentValues.SetValues(conciliacion);
            await _context.SaveChangesAsync();

            return new ActionResponse<ConciliacionBancaria>
            {
                WasSuccess = true,
                Result = conciliacion,
                Message = "Conciliación bancaria actualizada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar conciliación bancaria");
            return new ActionResponse<ConciliacionBancaria>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ConciliacionBancaria>> CerrarConciliacionAsync(Guid id, string usuarioId)
    {
        try
        {
            var conciliacion = await _context.ConciliacionesBancarias.FindAsync(id);

            if (conciliacion == null)
            {
                return new ActionResponse<ConciliacionBancaria>
                {
                    WasSuccess = false,
                    Message = "Conciliación bancaria no encontrada"
                };
            }

            if (conciliacion.Estado == EstadosConciliacionBancaria.Cerrada)
            {
                return new ActionResponse<ConciliacionBancaria>
                {
                    WasSuccess = false,
                    Message = "La conciliación ya está cerrada"
                };
            }

            if (!conciliacion.EstaConciliada)
            {
                return new ActionResponse<ConciliacionBancaria>
                {
                    WasSuccess = false,
                    Message = $"No se puede cerrar la conciliación porque existe una diferencia de {conciliacion.Diferencia:N2}"
                };
            }

            conciliacion.Estado = EstadosConciliacionBancaria.Cerrada;
            conciliacion.FechaConciliacion = FechaCostaRicaHelper.Ahora;
            conciliacion.ConciliadoPorId = usuarioId;
            conciliacion.FechaModificacion = FechaCostaRicaHelper.Ahora;

            await _context.SaveChangesAsync();

            return new ActionResponse<ConciliacionBancaria>
            {
                WasSuccess = true,
                Result = conciliacion,
                Message = "Conciliación cerrada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar conciliación bancaria");
            return new ActionResponse<ConciliacionBancaria>
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
            var conciliacion = await _context.ConciliacionesBancarias.FindAsync(id);

            if (conciliacion == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Conciliación bancaria no encontrada"
                };
            }

            // No permitir eliminar conciliaciones cerradas
            if (conciliacion.Estado == EstadosConciliacionBancaria.Cerrada)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar una conciliación que ya ha sido cerrada"
                };
            }

            // Desasociar movimientos bancarios conciliados
            var movimientos = await _context.MovimientosBancarios
                .Where(m => m.ConciliacionId == id)
                .ToListAsync();

            foreach (var movimiento in movimientos)
            {
                movimiento.Conciliado = false;
                movimiento.FechaConciliacion = null;
                movimiento.ConciliacionId = null;
                movimiento.Estado = EstadosMovimientoBancario.Registrado;
            }

            // Desasociar extractos bancarios
            var extractos = await _context.ExtractosBancarios
                .Where(e => e.ConciliacionBancariaId == id)
                .ToListAsync();

            foreach (var extracto in extractos)
            {
                extracto.ConciliacionBancariaId = null;
                extracto.Estado = EstadosExtractoBancario.Pendiente;
            }

            // Eliminar la conciliación
            _context.ConciliacionesBancarias.Remove(conciliacion);
            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Conciliación bancaria eliminada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar conciliación bancaria");
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
