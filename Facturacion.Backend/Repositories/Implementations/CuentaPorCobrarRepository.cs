using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

/// <summary>
/// Repositorio para gestión de Cuentas por Cobrar
/// </summary>
public class CuentaPorCobrarRepository : ICuentaPorCobrarRepository
{
    private readonly DataContext _context;
    private readonly ILogger<CuentaPorCobrarRepository> _logger;

    public CuentaPorCobrarRepository(DataContext context, ILogger<CuentaPorCobrarRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<CuentaPorCobrar>> GetAsync(Guid id)
    {
        try
        {
            var cuentaPorCobrar = await _context.CuentasPorCobrar
                .Include(c => c.Empresa)
                .Include(c => c.Cliente)
                .Include(c => c.Documento)
                .Include(c => c.Abonos.Where(a => !a.IsDeleted))
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (cuentaPorCobrar == null)
            {
                return new ActionResponse<CuentaPorCobrar>
                {
                    WasSuccess = false,
                    Message = "Cuenta por cobrar no encontrada"
                };
            }

            return new ActionResponse<CuentaPorCobrar>
            {
                WasSuccess = true,
                Result = cuentaPorCobrar
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuenta por cobrar con ID {Id}", id);
            return new ActionResponse<CuentaPorCobrar>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaPorCobrar>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var cuentas = await _context.CuentasPorCobrar
                .Include(c => c.Cliente)
                .Include(c => c.Abonos.Where(a => !a.IsDeleted))
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
                .OrderByDescending(c => c.FechaEmision)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaPorCobrar>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas por cobrar de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<CuentaPorCobrar>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaPorCobrar>>> GetPendientesAsync(Guid empresaId)
    {
        try
        {
            var cuentas = await _context.CuentasPorCobrar
                .Include(c => c.Cliente)
                .Include(c => c.Abonos.Where(a => !a.IsDeleted))
                .Where(c => c.EmpresaId == empresaId &&
                           !c.IsDeleted &&
                           (c.Estado == EstadoCuentaPorCobrar.Pendiente ||
                            c.Estado == EstadoCuentaPorCobrar.Parcial))
                .OrderBy(c => c.FechaVencimiento)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaPorCobrar>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<CuentaPorCobrar>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaPorCobrar>>> GetVencidasAsync(Guid empresaId)
    {
        try
        {
            var fechaActual = FechaCostaRicaHelper.Ahora.Date;

            var cuentas = await _context.CuentasPorCobrar
                .Include(c => c.Cliente)
                .Include(c => c.Abonos.Where(a => !a.IsDeleted))
                .Where(c => c.EmpresaId == empresaId &&
                           !c.IsDeleted &&
                           c.FechaVencimiento < fechaActual &&
                           (c.Estado == EstadoCuentaPorCobrar.Pendiente ||
                            c.Estado == EstadoCuentaPorCobrar.Parcial ||
                            c.Estado == EstadoCuentaPorCobrar.Vencida))
                .OrderBy(c => c.FechaVencimiento)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaPorCobrar>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<CuentaPorCobrar>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaPorCobrar>>> GetByClienteAsync(Guid clienteId)
    {
        try
        {
            var cuentas = await _context.CuentasPorCobrar
                .Include(c => c.Cliente)
                .Include(c => c.Abonos.Where(a => !a.IsDeleted))
                .Where(c => c.ClienteId == clienteId && !c.IsDeleted)
                .OrderByDescending(c => c.FechaEmision)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaPorCobrar>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<CuentaPorCobrar>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CuentaPorCobrar>> GetByDocumentoIdAsync(Guid documentoId)
    {
        try
        {
            var cuentaPorCobrar = await _context.CuentasPorCobrar
                .Include(c => c.Cliente)
                .Include(c => c.Abonos.Where(a => !a.IsDeleted))
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.DocumentoId == documentoId && !c.IsDeleted);

            if (cuentaPorCobrar == null)
            {
                return new ActionResponse<CuentaPorCobrar>
                {
                    WasSuccess = false,
                    Message = "No se encontró cuenta por cobrar para este documento"
                };
            }

            return new ActionResponse<CuentaPorCobrar>
            {
                WasSuccess = true,
                Result = cuentaPorCobrar
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<CuentaPorCobrar>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<object>> GetAntiguedadSaldosAsync(Guid empresaId)
    {
        try
        {
            var fechaActual = FechaCostaRicaHelper.Ahora.Date;

            var cuentas = await _context.CuentasPorCobrar
                .Include(c => c.Cliente)
                .Where(c => c.EmpresaId == empresaId &&
                           !c.IsDeleted &&
                           c.MontoSaldo > 0 &&
                           (c.Estado == EstadoCuentaPorCobrar.Pendiente ||
                            c.Estado == EstadoCuentaPorCobrar.Parcial ||
                            c.Estado == EstadoCuentaPorCobrar.Vencida))
                .AsNoTracking()
                .ToListAsync();

            // Agrupar por rangos de antigüedad
            var reporte = cuentas
                .GroupBy(c => c.ClienteId)
                .Select(g => new
                {
                    ClienteId = g.Key,
                    NombreCliente = g.First().NombreCliente ?? g.First().Cliente?.Nombre,
                    Corriente = g.Where(c => (fechaActual - c.FechaVencimiento).Days <= 0)
                                 .Sum(c => c.MontoSaldo),
                    Dias1a30 = g.Where(c => (fechaActual - c.FechaVencimiento).Days >= 1 &&
                                           (fechaActual - c.FechaVencimiento).Days <= 30)
                                .Sum(c => c.MontoSaldo),
                    Dias31a60 = g.Where(c => (fechaActual - c.FechaVencimiento).Days >= 31 &&
                                            (fechaActual - c.FechaVencimiento).Days <= 60)
                                 .Sum(c => c.MontoSaldo),
                    Dias61a90 = g.Where(c => (fechaActual - c.FechaVencimiento).Days >= 61 &&
                                            (fechaActual - c.FechaVencimiento).Days <= 90)
                                 .Sum(c => c.MontoSaldo),
                    MasDe90 = g.Where(c => (fechaActual - c.FechaVencimiento).Days > 90)
                               .Sum(c => c.MontoSaldo),
                    Total = g.Sum(c => c.MontoSaldo)
                })
                .OrderByDescending(r => r.Total)
                .ToList();

            return new ActionResponse<object>
            {
                WasSuccess = true,
                Result = reporte
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<object>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CuentaPorCobrar>> AddAsync(CuentaPorCobrar cuentaPorCobrar)
    {
        try
        {
            // Verificar que no exista ya una cuenta para este documento
            var existente = await _context.CuentasPorCobrar
                .FirstOrDefaultAsync(c => c.DocumentoId == cuentaPorCobrar.DocumentoId && !c.IsDeleted);

            if (existente != null)
            {
                return new ActionResponse<CuentaPorCobrar>
                {
                    WasSuccess = false,
                    Message = "Ya existe una cuenta por cobrar para este documento"
                };
            }

            cuentaPorCobrar.FechaCreacion = FechaCostaRicaHelper.Ahora;
            cuentaPorCobrar.MontoSaldo = cuentaPorCobrar.MontoOriginal; // Inicialmente el saldo es igual al monto original

            // Establecer estado inicial
            if (cuentaPorCobrar.FechaVencimiento < FechaCostaRicaHelper.Ahora.Date)
            {
                cuentaPorCobrar.Estado = EstadoCuentaPorCobrar.Vencida;
            }
            else
            {
                cuentaPorCobrar.Estado = EstadoCuentaPorCobrar.Pendiente;
            }

            _context.CuentasPorCobrar.Add(cuentaPorCobrar);
            await _context.SaveChangesAsync();

            return new ActionResponse<CuentaPorCobrar>
            {
                WasSuccess = true,
                Result = cuentaPorCobrar
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<CuentaPorCobrar>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CuentaPorCobrar>> UpdateAsync(CuentaPorCobrar cuentaPorCobrar)
    {
        try
        {
            var existente = await _context.CuentasPorCobrar
                .FirstOrDefaultAsync(c => c.Id == cuentaPorCobrar.Id);

            if (existente == null)
            {
                return new ActionResponse<CuentaPorCobrar>
                {
                    WasSuccess = false,
                    Message = "Cuenta por cobrar no encontrada"
                };
            }

            cuentaPorCobrar.FechaModificacion = FechaCostaRicaHelper.Ahora;
            cuentaPorCobrar.FechaCreacion = existente.FechaCreacion;
            cuentaPorCobrar.UsuarioCreacionId = existente.UsuarioCreacionId;

            _context.Entry(existente).CurrentValues.SetValues(cuentaPorCobrar);
            await _context.SaveChangesAsync();

            return new ActionResponse<CuentaPorCobrar>
            {
                WasSuccess = true,
                Result = cuentaPorCobrar
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<CuentaPorCobrar>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CuentaPorCobrar>> AplicarAbonoAsync(Guid cuentaPorCobrarId, AbonoCobranza abono)
    {
        try
        {
            var cuentaPorCobrar = await _context.CuentasPorCobrar
                .Include(c => c.Abonos)
                .FirstOrDefaultAsync(c => c.Id == cuentaPorCobrarId && !c.IsDeleted);

            if (cuentaPorCobrar == null)
            {
                return new ActionResponse<CuentaPorCobrar>
                {
                    WasSuccess = false,
                    Message = "Cuenta por cobrar no encontrada"
                };
            }

            // Validar que el monto del abono no exceda el saldo
            if (abono.Monto > cuentaPorCobrar.MontoSaldo)
            {
                return new ActionResponse<CuentaPorCobrar>
                {
                    WasSuccess = false,
                    Message = $"El monto del abono ({abono.Monto:N2}) no puede ser mayor al saldo pendiente ({cuentaPorCobrar.MontoSaldo:N2})"
                };
            }

            // Validar que el monto sea positivo
            if (abono.Monto <= 0)
            {
                return new ActionResponse<CuentaPorCobrar>
                {
                    WasSuccess = false,
                    Message = "El monto del abono debe ser mayor a cero"
                };
            }

            // Crear el abono
            abono.CuentaPorCobrarId = cuentaPorCobrarId;
            abono.FechaCreacion = FechaCostaRicaHelper.Ahora;
            _context.AbonosCobranza.Add(abono);

            // Actualizar el saldo de la cuenta
            cuentaPorCobrar.MontoSaldo -= abono.Monto;
            cuentaPorCobrar.FechaUltimoPago = abono.FechaPago;
            cuentaPorCobrar.FechaModificacion = FechaCostaRicaHelper.Ahora;

            // Actualizar el estado según el saldo restante
            if (cuentaPorCobrar.MontoSaldo <= 0)
            {
                cuentaPorCobrar.Estado = EstadoCuentaPorCobrar.Pagada;
                cuentaPorCobrar.MontoSaldo = 0; // Asegurar que no quede saldo negativo
            }
            else
            {
                cuentaPorCobrar.Estado = EstadoCuentaPorCobrar.Parcial;
            }

            await _context.SaveChangesAsync();

            // Recargar para incluir todos los abonos
            await _context.Entry(cuentaPorCobrar).Collection(c => c.Abonos).LoadAsync();

            return new ActionResponse<CuentaPorCobrar>
            {
                WasSuccess = true,
                Result = cuentaPorCobrar,
                Message = "Abono aplicado exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<CuentaPorCobrar>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaPorCobrar>>> GetByFechaVencimientoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var cuentas = await _context.CuentasPorCobrar
                .Include(c => c.Cliente)
                .Include(c => c.Abonos.Where(a => !a.IsDeleted))
                .Where(c => c.EmpresaId == empresaId &&
                           !c.IsDeleted &&
                           c.FechaVencimiento >= fechaInicio.Date &&
                           c.FechaVencimiento <= fechaFin.Date)
                .OrderBy(c => c.FechaVencimiento)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaPorCobrar>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas por cobrar por rango de fechas de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<CuentaPorCobrar>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<decimal>> GetSaldoPendienteClienteAsync(Guid clienteId)
    {
        try
        {
            var saldoTotal = await _context.CuentasPorCobrar
                .Where(c => c.ClienteId == clienteId &&
                           !c.IsDeleted &&
                           c.Estado != EstadoCuentaPorCobrar.Pagada)
                .SumAsync(c => c.MontoSaldo);

            return new ActionResponse<decimal>
            {
                WasSuccess = true,
                Result = saldoTotal
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener saldo pendiente del cliente {ClienteId}", clienteId);
            return new ActionResponse<decimal>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> AplicarCobroAsync(Guid cuentaId, decimal monto, string usuarioId)
    {
        try
        {
            var cuenta = await _context.CuentasPorCobrar
                .Include(c => c.Abonos)
                .FirstOrDefaultAsync(c => c.Id == cuentaId && !c.IsDeleted);

            if (cuenta == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Cuenta por cobrar no encontrada"
                };
            }

            // Validar que el monto no exceda el saldo
            if (monto > cuenta.MontoSaldo)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = $"El monto del cobro ({monto:N2}) no puede ser mayor al saldo pendiente ({cuenta.MontoSaldo:N2})"
                };
            }

            // Validar que el monto sea positivo
            if (monto <= 0)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "El monto del cobro debe ser mayor a cero"
                };
            }

            // Crear el abono
            var abono = new AbonoCobranza
            {
                Id = Guid.NewGuid(),
                CuentaPorCobrarId = cuentaId,
                FechaPago = FechaCostaRicaHelper.Ahora,
                Monto = monto,
                MetodoPago = "04", // Transferencia por defecto
                Moneda = cuenta.Moneda,
                TipoCambio = cuenta.TipoCambio,
                FechaCreacion = FechaCostaRicaHelper.Ahora,
                UsuarioCreacionId = usuarioId
            };

            _context.AbonosCobranza.Add(abono);

            // Actualizar el saldo de la cuenta
            cuenta.MontoSaldo -= monto;
            cuenta.FechaUltimoPago = FechaCostaRicaHelper.Ahora;
            cuenta.FechaModificacion = FechaCostaRicaHelper.Ahora;
            cuenta.UsuarioModificacionId = usuarioId;

            // Actualizar el estado según el saldo restante
            if (cuenta.MontoSaldo <= 0)
            {
                cuenta.Estado = EstadoCuentaPorCobrar.Pagada;
                cuenta.MontoSaldo = 0; // Asegurar que no quede saldo negativo
            }
            else
            {
                cuenta.Estado = EstadoCuentaPorCobrar.Parcial;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cobro de {Monto} aplicado a cuenta por cobrar {CuentaId}. Nuevo saldo: {NuevoSaldo}",
                monto, cuentaId, cuenta.MontoSaldo);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cobro aplicado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aplicar cobro a cuenta por cobrar {CuentaId}", cuentaId);
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        try
        {
            var cuenta = await _context.CuentasPorCobrar.FindAsync(id);

            if (cuenta == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Cuenta por cobrar no encontrada"
                };
            }

            // Soft delete
            cuenta.IsDeleted = true;
            cuenta.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            cuenta.UsuarioEliminacionId = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cuenta por cobrar {Id} eliminada por usuario {UsuarioId}", id, userId);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cuenta por cobrar eliminada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cuenta por cobrar {Id}", id);
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
            var cuenta = await _context.CuentasPorCobrar.FindAsync(id);

            if (cuenta == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Cuenta por cobrar no encontrada"
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

            _logger.LogWarning("Cuenta por cobrar {Id} anulada por usuario {UsuarioId}. Motivo: {Motivo}",
                id, usuarioId, motivo);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cuenta por cobrar anulada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular cuenta por cobrar {Id}", id);
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
