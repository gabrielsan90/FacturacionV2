using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class MovimientoBancarioRepository : IMovimientoBancarioRepository
{
    private readonly DataContext _context;
    private readonly ILogger<MovimientoBancarioRepository> _logger;

    public MovimientoBancarioRepository(DataContext context, ILogger<MovimientoBancarioRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<MovimientoBancario>> GetAsync(Guid id)
    {
        try
        {
            var movimiento = await _context.MovimientosBancarios
                .Include(m => m.Empresa)
                .Include(m => m.CuentaBancaria)
                .Include(m => m.Conciliacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id && m.Estado != EstadosMovimientoBancario.Anulado);

            if (movimiento == null)
            {
                return new ActionResponse<MovimientoBancario>
                {
                    WasSuccess = false,
                    Message = "Movimiento bancario no encontrado"
                };
            }

            return new ActionResponse<MovimientoBancario>
            {
                WasSuccess = true,
                Result = movimiento
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimiento bancario {Id}", id);
            return new ActionResponse<MovimientoBancario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<MovimientoBancario>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var movimientos = await _context.MovimientosBancarios
                .Include(m => m.CuentaBancaria)
                .Where(m => m.EmpresaId == empresaId && m.Estado != EstadosMovimientoBancario.Anulado)
                .OrderByDescending(m => m.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<MovimientoBancario>>
            {
                WasSuccess = true,
                Result = movimientos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos bancarios de empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<MovimientoBancario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<MovimientoBancario>>> GetByCuentaBancariaAsync(Guid cuentaBancariaId)
    {
        try
        {
            var movimientos = await _context.MovimientosBancarios
                .Include(m => m.CuentaBancaria)
                .Where(m => m.CuentaBancariaId == cuentaBancariaId && m.Estado != EstadosMovimientoBancario.Anulado)
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<MovimientoBancario>>
            {
                WasSuccess = true,
                Result = movimientos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos de cuenta bancaria {CuentaBancariaId}", cuentaBancariaId);
            return new ActionResponse<IEnumerable<MovimientoBancario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<MovimientoBancario>>> GetByFechaRangeAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var movimientos = await _context.MovimientosBancarios
                .Include(m => m.CuentaBancaria)
                .Where(m => m.EmpresaId == empresaId &&
                           m.Fecha >= fechaInicio &&
                           m.Fecha <= fechaFin &&
                           m.Estado != EstadosMovimientoBancario.Anulado)
                .OrderBy(m => m.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<MovimientoBancario>>
            {
                WasSuccess = true,
                Result = movimientos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos bancarios por rango de fechas");
            return new ActionResponse<IEnumerable<MovimientoBancario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<MovimientoBancario>>> GetPendientesConciliarAsync(Guid cuentaBancariaId)
    {
        try
        {
            var movimientos = await _context.MovimientosBancarios
                .Include(m => m.CuentaBancaria)
                .Where(m => m.CuentaBancariaId == cuentaBancariaId &&
                           !m.Conciliado &&
                           m.Estado == EstadosMovimientoBancario.Registrado)
                .OrderBy(m => m.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<MovimientoBancario>>
            {
                WasSuccess = true,
                Result = movimientos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos pendientes de conciliar");
            return new ActionResponse<IEnumerable<MovimientoBancario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<MovimientoBancario>>> GetConciliadosAsync(Guid cuentaBancariaId)
    {
        try
        {
            var movimientos = await _context.MovimientosBancarios
                .Include(m => m.CuentaBancaria)
                .Include(m => m.Conciliacion)
                .Where(m => m.CuentaBancariaId == cuentaBancariaId &&
                           m.Conciliado &&
                           m.Estado == EstadosMovimientoBancario.Conciliado)
                .OrderByDescending(m => m.FechaConciliacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<MovimientoBancario>>
            {
                WasSuccess = true,
                Result = movimientos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos conciliados");
            return new ActionResponse<IEnumerable<MovimientoBancario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<MovimientoBancario>> AddAsync(MovimientoBancario movimiento)
    {
        try
        {
            // Verificar que la cuenta bancaria existe
            var cuentaBancaria = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.Id == movimiento.CuentaBancariaId && !c.IsDeleted);

            if (cuentaBancaria == null)
            {
                return new ActionResponse<MovimientoBancario>
                {
                    WasSuccess = false,
                    Message = "La cuenta bancaria no existe o está eliminada"
                };
            }

            // Calcular el nuevo saldo
            movimiento.SaldoAnterior = cuentaBancaria.SaldoActual;

            if (movimiento.Naturaleza == NaturalezaMovimiento.Credito)
            {
                movimiento.SaldoNuevo = movimiento.SaldoAnterior + movimiento.Monto;
            }
            else // Débito
            {
                movimiento.SaldoNuevo = movimiento.SaldoAnterior - movimiento.Monto;
            }

            movimiento.FechaCreacion = FechaCostaRicaHelper.Ahora;
            movimiento.Estado = EstadosMovimientoBancario.Registrado;

            _context.MovimientosBancarios.Add(movimiento);

            // Actualizar el saldo de la cuenta bancaria
            cuentaBancaria.SaldoActual = movimiento.SaldoNuevo;
            cuentaBancaria.FechaModificacion = FechaCostaRicaHelper.Ahora;

            await _context.SaveChangesAsync();

            return new ActionResponse<MovimientoBancario>
            {
                WasSuccess = true,
                Result = movimiento,
                Message = "Movimiento bancario creado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear movimiento bancario");
            return new ActionResponse<MovimientoBancario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<MovimientoBancario>> UpdateAsync(MovimientoBancario movimiento)
    {
        try
        {
            var existente = await _context.MovimientosBancarios
                .Include(m => m.CuentaBancaria)
                .FirstOrDefaultAsync(m => m.Id == movimiento.Id);

            if (existente == null)
            {
                return new ActionResponse<MovimientoBancario>
                {
                    WasSuccess = false,
                    Message = "Movimiento bancario no encontrado"
                };
            }

            // No permitir modificar movimientos conciliados
            if (existente.Conciliado)
            {
                return new ActionResponse<MovimientoBancario>
                {
                    WasSuccess = false,
                    Message = "No se puede modificar un movimiento que ya ha sido conciliado"
                };
            }

            // Si cambió el monto o la naturaleza, recalcular saldos
            if (existente.Monto != movimiento.Monto || existente.Naturaleza != movimiento.Naturaleza)
            {
                // Revertir el impacto del movimiento anterior en el saldo
                var cuentaBancaria = existente.CuentaBancaria!;
                if (existente.Naturaleza == NaturalezaMovimiento.Credito)
                {
                    cuentaBancaria.SaldoActual -= existente.Monto;
                }
                else
                {
                    cuentaBancaria.SaldoActual += existente.Monto;
                }

                // Aplicar el nuevo movimiento
                movimiento.SaldoAnterior = cuentaBancaria.SaldoActual;
                if (movimiento.Naturaleza == NaturalezaMovimiento.Credito)
                {
                    movimiento.SaldoNuevo = movimiento.SaldoAnterior + movimiento.Monto;
                }
                else
                {
                    movimiento.SaldoNuevo = movimiento.SaldoAnterior - movimiento.Monto;
                }

                cuentaBancaria.SaldoActual = movimiento.SaldoNuevo;
                cuentaBancaria.FechaModificacion = FechaCostaRicaHelper.Ahora;
            }

            movimiento.FechaModificacion = FechaCostaRicaHelper.Ahora;
            movimiento.FechaCreacion = existente.FechaCreacion;
            movimiento.CreadoPorId = existente.CreadoPorId;

            _context.Entry(existente).CurrentValues.SetValues(movimiento);
            await _context.SaveChangesAsync();

            return new ActionResponse<MovimientoBancario>
            {
                WasSuccess = true,
                Result = movimiento,
                Message = "Movimiento bancario actualizado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar movimiento bancario");
            return new ActionResponse<MovimientoBancario>
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
            var movimiento = await _context.MovimientosBancarios
                .Include(m => m.CuentaBancaria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movimiento == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Movimiento bancario no encontrado"
                };
            }

            // No permitir anular movimientos conciliados
            if (movimiento.Conciliado)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede anular un movimiento que ya ha sido conciliado"
                };
            }

            // Revertir el impacto en el saldo de la cuenta bancaria
            var cuentaBancaria = movimiento.CuentaBancaria!;
            if (movimiento.Naturaleza == NaturalezaMovimiento.Credito)
            {
                cuentaBancaria.SaldoActual -= movimiento.Monto;
            }
            else
            {
                cuentaBancaria.SaldoActual += movimiento.Monto;
            }

            cuentaBancaria.FechaModificacion = FechaCostaRicaHelper.Ahora;

            // Cambiar estado a anulado (soft delete)
            movimiento.Estado = EstadosMovimientoBancario.Anulado;
            movimiento.FechaModificacion = FechaCostaRicaHelper.Ahora;
            movimiento.ModificadoPorId = usuarioId;

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Movimiento bancario anulado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular movimiento bancario");
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<decimal>> GetSaldoCuentaAsync(Guid cuentaBancariaId)
    {
        try
        {
            var cuentaBancaria = await _context.CuentasBancarias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cuentaBancariaId && !c.IsDeleted);

            if (cuentaBancaria == null)
            {
                return new ActionResponse<decimal>
                {
                    WasSuccess = false,
                    Message = "Cuenta bancaria no encontrada"
                };
            }

            return new ActionResponse<decimal>
            {
                WasSuccess = true,
                Result = cuentaBancaria.SaldoActual
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener saldo de cuenta bancaria");
            return new ActionResponse<decimal>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<MovimientoBancario>> MarcarComoConciliadoAsync(Guid id, Guid conciliacionId)
    {
        try
        {
            var movimiento = await _context.MovimientosBancarios.FindAsync(id);

            if (movimiento == null)
            {
                return new ActionResponse<MovimientoBancario>
                {
                    WasSuccess = false,
                    Message = "Movimiento bancario no encontrado"
                };
            }

            if (movimiento.Estado == EstadosMovimientoBancario.Anulado)
            {
                return new ActionResponse<MovimientoBancario>
                {
                    WasSuccess = false,
                    Message = "No se puede conciliar un movimiento anulado"
                };
            }

            movimiento.Conciliado = true;
            movimiento.FechaConciliacion = FechaCostaRicaHelper.Ahora;
            movimiento.ConciliacionId = conciliacionId;
            movimiento.Estado = EstadosMovimientoBancario.Conciliado;
            movimiento.FechaModificacion = FechaCostaRicaHelper.Ahora;

            await _context.SaveChangesAsync();

            return new ActionResponse<MovimientoBancario>
            {
                WasSuccess = true,
                Result = movimiento,
                Message = "Movimiento marcado como conciliado"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar movimiento como conciliado");
            return new ActionResponse<MovimientoBancario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
