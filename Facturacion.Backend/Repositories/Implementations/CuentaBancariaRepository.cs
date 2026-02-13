using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class CuentaBancariaRepository : ICuentaBancariaRepository
{
    private readonly DataContext _context;
    private readonly ILogger<CuentaBancariaRepository> _logger;

    public CuentaBancariaRepository(DataContext context, ILogger<CuentaBancariaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<CuentaBancaria>> GetAsync(Guid id)
    {
        try
        {
            var cuenta = await _context.CuentasBancarias
                .Include(c => c.Empresa)
                .Include(c => c.Banco)
                .Include(c => c.Sucursal)
                .Include(c => c.CuentaContable)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (cuenta == null)
            {
                return new ActionResponse<CuentaBancaria>
                {
                    WasSuccess = false,
                    Message = "Cuenta bancaria no encontrada"
                };
            }

            return new ActionResponse<CuentaBancaria>
            {
                WasSuccess = true,
                Result = cuenta
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuenta bancaria {Id}", id);
            return new ActionResponse<CuentaBancaria>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaBancaria>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var cuentas = await _context.CuentasBancarias
                .Include(c => c.Banco)
                .Include(c => c.Sucursal)
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
                .OrderBy(c => c.Nombre)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaBancaria>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas bancarias de empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<CuentaBancaria>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<CuentaBancaria>>> GetActivasAsync(Guid empresaId)
    {
        try
        {
            var cuentas = await _context.CuentasBancarias
                .Include(c => c.Banco)
                .Include(c => c.Sucursal)
                .Where(c => c.EmpresaId == empresaId && c.Activo && !c.IsDeleted)
                .OrderBy(c => c.Nombre)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<CuentaBancaria>>
            {
                WasSuccess = true,
                Result = cuentas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cuentas bancarias activas");
            return new ActionResponse<IEnumerable<CuentaBancaria>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CuentaBancaria>> GetByNumeroCuentaAsync(Guid empresaId, string numeroCuenta)
    {
        try
        {
            var cuenta = await _context.CuentasBancarias
                .Include(c => c.Banco)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.EmpresaId == empresaId &&
                                         c.NumeroCuenta == numeroCuenta &&
                                         !c.IsDeleted);

            if (cuenta == null)
            {
                return new ActionResponse<CuentaBancaria>
                {
                    WasSuccess = false,
                    Message = "Cuenta bancaria no encontrada"
                };
            }

            return new ActionResponse<CuentaBancaria>
            {
                WasSuccess = true,
                Result = cuenta
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar cuenta bancaria por número");
            return new ActionResponse<CuentaBancaria>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CuentaBancaria>> AddAsync(CuentaBancaria cuenta)
    {
        try
        {
            // Verificar que no exista otra cuenta con el mismo número en la misma empresa
            var existente = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.EmpresaId == cuenta.EmpresaId &&
                                         c.NumeroCuenta == cuenta.NumeroCuenta &&
                                         !c.IsDeleted);

            if (existente != null)
            {
                return new ActionResponse<CuentaBancaria>
                {
                    WasSuccess = false,
                    Message = $"Ya existe una cuenta bancaria con el número {cuenta.NumeroCuenta}"
                };
            }

            cuenta.FechaCreacion = FechaCostaRicaHelper.Ahora;
            cuenta.SaldoActual = cuenta.SaldoInicial;

            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            return new ActionResponse<CuentaBancaria>
            {
                WasSuccess = true,
                Result = cuenta,
                Message = "Cuenta bancaria creada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cuenta bancaria");
            return new ActionResponse<CuentaBancaria>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CuentaBancaria>> UpdateAsync(CuentaBancaria cuenta)
    {
        try
        {
            var existente = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.Id == cuenta.Id);

            if (existente == null)
            {
                return new ActionResponse<CuentaBancaria>
                {
                    WasSuccess = false,
                    Message = "Cuenta bancaria no encontrada"
                };
            }

            // Verificar que no exista otra cuenta con el mismo número
            var duplicado = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.Id != cuenta.Id &&
                                         c.EmpresaId == cuenta.EmpresaId &&
                                         c.NumeroCuenta == cuenta.NumeroCuenta &&
                                         !c.IsDeleted);

            if (duplicado != null)
            {
                return new ActionResponse<CuentaBancaria>
                {
                    WasSuccess = false,
                    Message = $"Ya existe otra cuenta bancaria con el número {cuenta.NumeroCuenta}"
                };
            }

            cuenta.FechaModificacion = FechaCostaRicaHelper.Ahora;
            cuenta.FechaCreacion = existente.FechaCreacion;
            cuenta.CreadoPorId = existente.CreadoPorId;
            cuenta.SaldoActual = existente.SaldoActual; // El saldo no se modifica manualmente
            cuenta.SaldoInicial = existente.SaldoInicial; // El saldo inicial tampoco se modifica

            _context.Entry(existente).CurrentValues.SetValues(cuenta);
            await _context.SaveChangesAsync();

            return new ActionResponse<CuentaBancaria>
            {
                WasSuccess = true,
                Result = cuenta,
                Message = "Cuenta bancaria actualizada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cuenta bancaria");
            return new ActionResponse<CuentaBancaria>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<CuentaBancaria>> UpdateSaldoAsync(Guid cuentaBancariaId, decimal nuevoSaldo)
    {
        try
        {
            var cuenta = await _context.CuentasBancarias.FindAsync(cuentaBancariaId);

            if (cuenta == null || cuenta.IsDeleted)
            {
                return new ActionResponse<CuentaBancaria>
                {
                    WasSuccess = false,
                    Message = "Cuenta bancaria no encontrada"
                };
            }

            cuenta.SaldoActual = nuevoSaldo;
            cuenta.FechaModificacion = FechaCostaRicaHelper.Ahora;

            await _context.SaveChangesAsync();

            return new ActionResponse<CuentaBancaria>
            {
                WasSuccess = true,
                Result = cuenta,
                Message = "Saldo actualizado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar saldo de cuenta bancaria");
            return new ActionResponse<CuentaBancaria>
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
            var cuenta = await _context.CuentasBancarias.FindAsync(id);

            if (cuenta == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Cuenta bancaria no encontrada"
                };
            }

            // Verificar si tiene movimientos asociados
            var tieneMovimientos = await _context.MovimientosBancarios
                .AnyAsync(m => m.CuentaBancariaId == id && m.Estado != EstadosMovimientoBancario.Anulado);

            if (tieneMovimientos)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar la cuenta porque tiene movimientos bancarios asociados. Considere desactivarla en su lugar."
                };
            }

            // Soft delete
            cuenta.IsDeleted = true;
            cuenta.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            cuenta.UsuarioEliminacionId = usuarioId;

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Cuenta bancaria eliminada exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cuenta bancaria");
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
