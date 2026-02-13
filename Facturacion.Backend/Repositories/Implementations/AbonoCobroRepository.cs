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
/// Repositorio para gestión de Abonos de Cobranza
/// </summary>
public class AbonoCobroRepository : IAbonoCobroRepository
{
    private readonly DataContext _context;
    private readonly ILogger<AbonoCobroRepository> _logger;

    public AbonoCobroRepository(DataContext context, ILogger<AbonoCobroRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<AbonoCobranza>> GetAsync(Guid id)
    {
        try
        {
            var abono = await _context.AbonosCobranza
                .Include(a => a.CuentaPorCobrar)
                    .ThenInclude(c => c!.Cliente)
                .Include(a => a.ReciboPago)
                .Include(a => a.MovimientoBancario)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (abono == null)
            {
                return new ActionResponse<AbonoCobranza>
                {
                    WasSuccess = false,
                    Message = "Abono de cobranza no encontrado"
                };
            }

            return new ActionResponse<AbonoCobranza>
            {
                WasSuccess = true,
                Result = abono
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener abono de cobranza con ID {Id}", id);
            return new ActionResponse<AbonoCobranza>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<AbonoCobranza>>> GetByCuentaPorCobrarAsync(Guid cuentaPorCobrarId)
    {
        try
        {
            var abonos = await _context.AbonosCobranza
                .Include(a => a.ReciboPago)
                .Include(a => a.MovimientoBancario)
                .Where(a => a.CuentaPorCobrarId == cuentaPorCobrarId && !a.IsDeleted)
                .OrderByDescending(a => a.FechaPago)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<AbonoCobranza>>
            {
                WasSuccess = true,
                Result = abonos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener abonos de la cuenta por cobrar {CuentaPorCobrarId}", cuentaPorCobrarId);
            return new ActionResponse<IEnumerable<AbonoCobranza>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<AbonoCobranza>>> GetByFechaAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var abonos = await _context.AbonosCobranza
                .Include(a => a.CuentaPorCobrar)
                    .ThenInclude(c => c!.Cliente)
                .Include(a => a.ReciboPago)
                .Include(a => a.MovimientoBancario)
                .Where(a => a.CuentaPorCobrar!.EmpresaId == empresaId &&
                           !a.IsDeleted &&
                           a.FechaPago >= fechaInicio.Date &&
                           a.FechaPago <= fechaFin.Date)
                .OrderByDescending(a => a.FechaPago)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<AbonoCobranza>>
            {
                WasSuccess = true,
                Result = abonos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener abonos por rango de fechas de la empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<AbonoCobranza>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<AbonoCobranza>> AddAsync(AbonoCobranza abono)
    {
        try
        {
            // Validar que la cuenta por cobrar exista y no esté eliminada
            var cuentaPorCobrar = await _context.CuentasPorCobrar
                .FirstOrDefaultAsync(c => c.Id == abono.CuentaPorCobrarId && !c.IsDeleted);

            if (cuentaPorCobrar == null)
            {
                return new ActionResponse<AbonoCobranza>
                {
                    WasSuccess = false,
                    Message = "La cuenta por cobrar no existe o está eliminada"
                };
            }

            // Validar que el monto del abono no exceda el saldo
            if (abono.Monto > cuentaPorCobrar.MontoSaldo)
            {
                return new ActionResponse<AbonoCobranza>
                {
                    WasSuccess = false,
                    Message = $"El monto del abono ({abono.Monto:N2}) no puede ser mayor al saldo pendiente ({cuentaPorCobrar.MontoSaldo:N2})"
                };
            }

            // Validar que el monto sea positivo
            if (abono.Monto <= 0)
            {
                return new ActionResponse<AbonoCobranza>
                {
                    WasSuccess = false,
                    Message = "El monto del abono debe ser mayor a cero"
                };
            }

            // Establecer valores iniciales
            abono.FechaCreacion = FechaCostaRicaHelper.Ahora;

            _context.AbonosCobranza.Add(abono);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Abono de cobranza {Id} creado exitosamente para cuenta {CuentaPorCobrarId}",
                abono.Id, abono.CuentaPorCobrarId);

            return new ActionResponse<AbonoCobranza>
            {
                WasSuccess = true,
                Result = abono
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear abono de cobranza para cuenta {CuentaPorCobrarId}", abono.CuentaPorCobrarId);
            return new ActionResponse<AbonoCobranza>
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
            var abono = await _context.AbonosCobranza
                .Include(a => a.CuentaPorCobrar)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (abono == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Abono de cobranza no encontrado"
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
            abono.IsDeleted = true;
            abono.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            abono.UsuarioEliminacionId = usuarioId;
            abono.Notas = string.IsNullOrEmpty(abono.Notas)
                ? $"ANULADO: {motivo}"
                : $"{abono.Notas}\nANULADO: {motivo}";

            // Revertir el saldo en la cuenta por cobrar
            if (abono.CuentaPorCobrar != null)
            {
                abono.CuentaPorCobrar.MontoSaldo += abono.Monto;
                abono.CuentaPorCobrar.FechaModificacion = FechaCostaRicaHelper.Ahora;
                abono.CuentaPorCobrar.UsuarioModificacionId = usuarioId;

                // Actualizar el estado de la cuenta
                if (abono.CuentaPorCobrar.MontoSaldo >= abono.CuentaPorCobrar.MontoOriginal)
                {
                    abono.CuentaPorCobrar.Estado = EstadoCuentaPorCobrar.Pendiente;
                }
                else if (abono.CuentaPorCobrar.MontoSaldo > 0)
                {
                    abono.CuentaPorCobrar.Estado = EstadoCuentaPorCobrar.Parcial;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogWarning("Abono de cobranza {Id} anulado por usuario {UsuarioId}. Motivo: {Motivo}",
                id, usuarioId, motivo);

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Abono de cobranza anulado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular abono de cobranza {Id}", id);
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
