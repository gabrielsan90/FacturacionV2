using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

/// <summary>
/// Repositorio para gestión de Abonos y Pagos a Cuentas por Pagar
/// </summary>
public class AbonoPagoRepository : IAbonoPagoRepository
{
    private readonly DataContext _context;
    private readonly ILogger<AbonoPagoRepository> _logger;

    public AbonoPagoRepository(DataContext context, ILogger<AbonoPagoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<AbonoPago>> GetAsync(Guid id)
    {
        try
        {
            var abono = await _context.AbonosPago
                .Include(a => a.CuentaPorPagar)
                    .ThenInclude(c => c!.Proveedor)
                .Include(a => a.CuentaBancaria)
                .Include(a => a.RegistradoPor)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (abono == null)
            {
                return new ActionResponse<AbonoPago>
                {
                    WasSuccess = false,
                    Message = "Abono no encontrado"
                };
            }

            return new ActionResponse<AbonoPago>
            {
                WasSuccess = true,
                Result = abono
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener abono con ID {Id}", id);
            return new ActionResponse<AbonoPago>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<AbonoPago>>> GetByCuentaPorPagarAsync(Guid cuentaPorPagarId)
    {
        try
        {
            var abonos = await _context.AbonosPago
                .Include(a => a.CuentaBancaria)
                .Include(a => a.RegistradoPor)
                .Where(a => a.CuentaPorPagarId == cuentaPorPagarId)
                .OrderByDescending(a => a.FechaPago)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<AbonoPago>>
            {
                WasSuccess = true,
                Result = abonos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener abonos de la cuenta por pagar {CuentaPorPagarId}", cuentaPorPagarId);
            return new ActionResponse<IEnumerable<AbonoPago>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<AbonoPago>>> GetByFechaAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var abonos = await _context.AbonosPago
                .Include(a => a.CuentaPorPagar)
                    .ThenInclude(c => c!.Proveedor)
                .Include(a => a.CuentaBancaria)
                .Include(a => a.RegistradoPor)
                .Where(a => a.CuentaPorPagar!.EmpresaId == empresaId &&
                           a.FechaPago >= fechaInicio.Date &&
                           a.FechaPago <= fechaFin.Date)
                .OrderByDescending(a => a.FechaPago)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<AbonoPago>>
            {
                WasSuccess = true,
                Result = abonos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener abonos por fecha de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<AbonoPago>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<AbonoPago>>> GetByMetodoPagoAsync(Guid empresaId, string metodoPago)
    {
        try
        {
            var abonos = await _context.AbonosPago
                .Include(a => a.CuentaPorPagar)
                    .ThenInclude(c => c!.Proveedor)
                .Include(a => a.CuentaBancaria)
                .Include(a => a.RegistradoPor)
                .Where(a => a.CuentaPorPagar!.EmpresaId == empresaId &&
                           a.MetodoPago == metodoPago)
                .OrderByDescending(a => a.FechaPago)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<AbonoPago>>
            {
                WasSuccess = true,
                Result = abonos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener abonos por método de pago de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<AbonoPago>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<AbonoPago>> AddAsync(AbonoPago abono)
    {
        try
        {
            // Verificar que la cuenta por pagar existe
            var cuenta = await _context.CuentasPorPagar
                .FirstOrDefaultAsync(c => c.Id == abono.CuentaPorPagarId && !c.IsDeleted);

            if (cuenta == null)
            {
                return new ActionResponse<AbonoPago>
                {
                    WasSuccess = false,
                    Message = "Cuenta por pagar no encontrada"
                };
            }

            // Validar que el monto no exceda el saldo
            if (abono.Monto > cuenta.MontoSaldo)
            {
                return new ActionResponse<AbonoPago>
                {
                    WasSuccess = false,
                    Message = $"El monto del abono ({abono.Monto:N2}) no puede ser mayor al saldo pendiente ({cuenta.MontoSaldo:N2})"
                };
            }

            // Validar que el monto sea positivo
            if (abono.Monto <= 0)
            {
                return new ActionResponse<AbonoPago>
                {
                    WasSuccess = false,
                    Message = "El monto del abono debe ser mayor a cero"
                };
            }

            // Establecer valores de auditoría
            abono.FechaCreacion = FechaCostaRicaHelper.Ahora;

            _context.AbonosPago.Add(abono);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Abono de {Monto} registrado para cuenta por pagar {CuentaPorPagarId}",
                abono.Monto, abono.CuentaPorPagarId);

            return new ActionResponse<AbonoPago>
            {
                WasSuccess = true,
                Result = abono,
                Message = "Abono registrado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear abono para cuenta por pagar {CuentaPorPagarId}", abono.CuentaPorPagarId);
            return new ActionResponse<AbonoPago>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<AbonoPago>> UpdateAsync(AbonoPago abono)
    {
        try
        {
            var existente = await _context.AbonosPago
                .FirstOrDefaultAsync(a => a.Id == abono.Id);

            if (existente == null)
            {
                return new ActionResponse<AbonoPago>
                {
                    WasSuccess = false,
                    Message = "Abono no encontrado"
                };
            }

            // Preservar valores de auditoría
            abono.FechaCreacion = existente.FechaCreacion;
            abono.RegistradoPorId = existente.RegistradoPorId;

            _context.Entry(existente).CurrentValues.SetValues(abono);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Abono {Id} actualizado exitosamente", abono.Id);

            return new ActionResponse<AbonoPago>
            {
                WasSuccess = true,
                Result = abono
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar abono {Id}", abono.Id);
            return new ActionResponse<AbonoPago>
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
            var abono = await _context.AbonosPago
                .Include(a => a.CuentaPorPagar)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (abono == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Abono no encontrado"
                };
            }

            // Validar que la cuenta por pagar aún existe
            if (abono.CuentaPorPagar == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar el abono porque la cuenta por pagar no existe"
                };
            }

            // Revertir el abono en la cuenta por pagar
            abono.CuentaPorPagar.MontoSaldo += abono.Monto;

            // Actualizar estado de la cuenta por pagar
            if (abono.CuentaPorPagar.MontoSaldo >= abono.CuentaPorPagar.MontoOriginal)
            {
                abono.CuentaPorPagar.Estado = EstadoCuentaPorPagar.Pendiente;
            }
            else if (abono.CuentaPorPagar.MontoSaldo > 0)
            {
                abono.CuentaPorPagar.Estado = EstadoCuentaPorPagar.Parcial;
            }

            // Eliminar el abono (hard delete ya que no tiene IsDeleted)
            _context.AbonosPago.Remove(abono);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Abono {Id} eliminado. Saldo revertido a {NuevoSaldo}",
                id, abono.CuentaPorPagar.MontoSaldo);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Abono eliminado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar abono {Id}", id);
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
