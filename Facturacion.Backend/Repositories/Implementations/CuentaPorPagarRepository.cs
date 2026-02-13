using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

/// <summary>
/// Repositorio para gestión de Cuentas por Pagar
/// </summary>
public class CuentaPorPagarRepository : ICuentaPorPagarRepository
{
    private readonly DataContext _context;
    private readonly ILogger<CuentaPorPagarRepository> _logger;

    public CuentaPorPagarRepository(DataContext context, ILogger<CuentaPorPagarRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<CuentaPorPagar>> GetAsync(Guid id)
    {
        try
        {
            var cuenta = await _context.CuentasPorPagar
                .Include(c => c.Empresa)
                .Include(c => c.Proveedor)
                .Include(c => c.OrdenCompra)
                .Include(c => c.Abonos)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (cuenta == null)
            {
                return new ActionResponse<CuentaPorPagar>
                {
                    WasSuccess = false,
                    Message = "Cuenta por pagar no encontrada"
                };
            }

            return new ActionResponse<CuentaPorPagar>
            {
                WasSuccess = true,
                Result = cuenta
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuenta por pagar con ID {Id}", id);
            return new ActionResponse<CuentaPorPagar>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaPorPagar>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var cuentas = await _context.CuentasPorPagar
                .Include(c => c.Proveedor)
                .Include(c => c.Abonos)
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
                .OrderByDescending(c => c.FechaFactura)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaPorPagar>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas por pagar de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<CuentaPorPagar>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaPorPagar>>> GetByProveedorAsync(Guid proveedorId)
    {
        try
        {
            var cuentas = await _context.CuentasPorPagar
                .Include(c => c.Proveedor)
                .Include(c => c.Abonos)
                .Where(c => c.ProveedorId == proveedorId && !c.IsDeleted)
                .OrderByDescending(c => c.FechaFactura)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaPorPagar>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas por pagar del proveedor {ProveedorId}", proveedorId);
            return new ActionResponse<IEnumerable<CuentaPorPagar>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaPorPagar>>> GetPendientesAsync(Guid empresaId)
    {
        try
        {
            var cuentas = await _context.CuentasPorPagar
                .Include(c => c.Proveedor)
                .Include(c => c.Abonos)
                .Where(c => c.EmpresaId == empresaId &&
                           !c.IsDeleted &&
                           (c.Estado == EstadoCuentaPorPagar.Pendiente ||
                            c.Estado == EstadoCuentaPorPagar.Parcial))
                .OrderBy(c => c.FechaVencimiento)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaPorPagar>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas por pagar pendientes de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<CuentaPorPagar>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaPorPagar>>> GetVencidasAsync(Guid empresaId)
    {
        try
        {
            var fechaActual = FechaCostaRicaHelper.Ahora.Date;

            var cuentas = await _context.CuentasPorPagar
                .Include(c => c.Proveedor)
                .Include(c => c.Abonos)
                .Where(c => c.EmpresaId == empresaId &&
                           !c.IsDeleted &&
                           c.FechaVencimiento < fechaActual &&
                           c.Estado != EstadoCuentaPorPagar.Pagada &&
                           c.MontoSaldo > 0)
                .OrderBy(c => c.FechaVencimiento)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaPorPagar>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas por pagar vencidas de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<CuentaPorPagar>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaPorPagar>>> GetByFechaVencimientoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var cuentas = await _context.CuentasPorPagar
                .Include(c => c.Proveedor)
                .Include(c => c.Abonos)
                .Where(c => c.EmpresaId == empresaId &&
                           !c.IsDeleted &&
                           c.FechaVencimiento >= fechaInicio.Date &&
                           c.FechaVencimiento <= fechaFin.Date)
                .OrderBy(c => c.FechaVencimiento)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaPorPagar>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas por pagar por rango de fechas de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<CuentaPorPagar>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<decimal>> GetSaldoPendienteProveedorAsync(Guid proveedorId)
    {
        try
        {
            var saldoTotal = await _context.CuentasPorPagar
                .Where(c => c.ProveedorId == proveedorId &&
                           !c.IsDeleted &&
                           c.Estado != EstadoCuentaPorPagar.Pagada)
                .SumAsync(c => c.MontoSaldo);

            return new ActionResponse<decimal>
            {
                WasSuccess = true,
                Result = saldoTotal
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener saldo pendiente del proveedor {ProveedorId}", proveedorId);
            return new ActionResponse<decimal>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CuentaPorPagar>> AddAsync(CuentaPorPagar cuenta)
    {
        try
        {
            // Verificar que no exista ya una cuenta para esta factura del mismo proveedor
            var existente = await _context.CuentasPorPagar
                .FirstOrDefaultAsync(c => c.ProveedorId == cuenta.ProveedorId &&
                                         c.NumeroFactura == cuenta.NumeroFactura &&
                                         !c.IsDeleted);

            if (existente != null)
            {
                return new ActionResponse<CuentaPorPagar>
                {
                    WasSuccess = false,
                    Message = $"Ya existe una cuenta por pagar para la factura {cuenta.NumeroFactura} de este proveedor"
                };
            }

            // Establecer valores iniciales
            cuenta.FechaCreacion = FechaCostaRicaHelper.Ahora;
            cuenta.MontoSaldo = cuenta.MontoOriginal;

            // Establecer estado inicial
            var fechaActual = FechaCostaRicaHelper.Ahora.Date;
            if (cuenta.FechaVencimiento < fechaActual)
            {
                cuenta.Estado = EstadoCuentaPorPagar.Vencida;
            }
            else
            {
                cuenta.Estado = EstadoCuentaPorPagar.Pendiente;
            }

            _context.CuentasPorPagar.Add(cuenta);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cuenta por pagar {NumeroFactura} creada exitosamente para proveedor {ProveedorId}",
                cuenta.NumeroFactura, cuenta.ProveedorId);

            return new ActionResponse<CuentaPorPagar>
            {
                WasSuccess = true,
                Result = cuenta
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cuenta por pagar {NumeroFactura}", cuenta.NumeroFactura);
            return new ActionResponse<CuentaPorPagar>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CuentaPorPagar>> UpdateAsync(CuentaPorPagar cuenta)
    {
        try
        {
            var existente = await _context.CuentasPorPagar
                .FirstOrDefaultAsync(c => c.Id == cuenta.Id);

            if (existente == null)
            {
                return new ActionResponse<CuentaPorPagar>
                {
                    WasSuccess = false,
                    Message = "Cuenta por pagar no encontrada"
                };
            }

            // Preservar valores de auditoría
            cuenta.FechaModificacion = FechaCostaRicaHelper.Ahora;
            cuenta.FechaCreacion = existente.FechaCreacion;
            cuenta.CreadoPorId = existente.CreadoPorId;

            _context.Entry(existente).CurrentValues.SetValues(cuenta);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cuenta por pagar {Id} actualizada exitosamente", cuenta.Id);

            return new ActionResponse<CuentaPorPagar>
            {
                WasSuccess = true,
                Result = cuenta
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cuenta por pagar {Id}", cuenta.Id);
            return new ActionResponse<CuentaPorPagar>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> AplicarPagoAsync(Guid cuentaId, decimal monto, string usuarioId)
    {
        try
        {
            var cuenta = await _context.CuentasPorPagar
                .Include(c => c.Abonos)
                .FirstOrDefaultAsync(c => c.Id == cuentaId && !c.IsDeleted);

            if (cuenta == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Cuenta por pagar no encontrada"
                };
            }

            // Validar que el monto no exceda el saldo
            if (monto > cuenta.MontoSaldo)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = $"El monto del pago ({monto:N2}) no puede ser mayor al saldo pendiente ({cuenta.MontoSaldo:N2})"
                };
            }

            // Validar que el monto sea positivo
            if (monto <= 0)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "El monto del pago debe ser mayor a cero"
                };
            }

            // Crear el abono
            var abono = new AbonoPago
            {
                Id = Guid.NewGuid(),
                CuentaPorPagarId = cuentaId,
                FechaPago = FechaCostaRicaHelper.Ahora,
                Monto = monto,
                MetodoPago = MetodoPago.Transferencia, // Por defecto, se puede modificar según la necesidad
                FechaCreacion = FechaCostaRicaHelper.Ahora,
                RegistradoPorId = usuarioId
            };

            _context.AbonosPago.Add(abono);

            // Actualizar el saldo de la cuenta
            cuenta.MontoSaldo -= monto;
            cuenta.FechaModificacion = FechaCostaRicaHelper.Ahora;
            cuenta.ModificadoPorId = usuarioId;

            // Actualizar el estado según el saldo restante
            if (cuenta.MontoSaldo <= 0)
            {
                cuenta.Estado = EstadoCuentaPorPagar.Pagada;
                cuenta.MontoSaldo = 0; // Asegurar que no quede saldo negativo
            }
            else
            {
                cuenta.Estado = EstadoCuentaPorPagar.Parcial;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Pago de {Monto} aplicado a cuenta por pagar {CuentaId}. Nuevo saldo: {NuevoSaldo}",
                monto, cuentaId, cuenta.MontoSaldo);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Pago aplicado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aplicar pago a cuenta por pagar {CuentaId}", cuentaId);
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> AnularAsync(Guid id, string usuarioId, string motivo)
    {
        try
        {
            var cuenta = await _context.CuentasPorPagar.FindAsync(id);

            if (cuenta == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Cuenta por pagar no encontrada"
                };
            }

            // Validar que tenga motivo
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Debe proporcionar un motivo de anulación"
                };
            }

            // Soft delete
            cuenta.IsDeleted = true;
            cuenta.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            cuenta.UsuarioEliminacionId = usuarioId;
            cuenta.Observaciones = string.IsNullOrEmpty(cuenta.Observaciones)
                ? $"ANULADA: {motivo}"
                : $"{cuenta.Observaciones}\nANULADA: {motivo}";

            await _context.SaveChangesAsync();

            _logger.LogWarning("Cuenta por pagar {Id} anulada por usuario {UsuarioId}. Motivo: {Motivo}",
                id, usuarioId, motivo);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cuenta por pagar anulada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular cuenta por pagar {Id}", id);
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
