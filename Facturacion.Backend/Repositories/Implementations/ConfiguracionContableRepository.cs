using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

/// <summary>
/// Implementación del repositorio de configuración contable.
/// </summary>
public class ConfiguracionContableRepository : IConfiguracionContableRepository
{
    private readonly DataContext _context;

    public ConfiguracionContableRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracionContable?> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.ConfiguracionesContables
            .Include(c => c.Empresa)
            .Include(c => c.CuentaVentasGravadas)
            .Include(c => c.CuentaVentasExentas)
            .Include(c => c.CuentaIvaDebito)
            .Include(c => c.CuentaIvaCredito)
            .Include(c => c.CuentaClientes)
            .Include(c => c.CuentaProveedores)
            .Include(c => c.CuentaCajaGeneral)
            .Include(c => c.CuentaBancosColones)
            .Include(c => c.CuentaBancosDolares)
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId);
    }

    public async Task<ConfiguracionContable?> GetAsync(Guid id)
    {
        return await _context.ConfiguracionesContables
            .Include(c => c.Empresa)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ConfiguracionContable> AddAsync(ConfiguracionContable configuracion)
    {
        configuracion.Id = Guid.NewGuid();
        configuracion.FechaCreacion = FechaCostaRicaHelper.Ahora;

        _context.ConfiguracionesContables.Add(configuracion);
        await _context.SaveChangesAsync();

        return configuracion;
    }

    public async Task UpdateAsync(ConfiguracionContable configuracion)
    {
        configuracion.FechaModificacion = FechaCostaRicaHelper.Ahora;
        _context.ConfiguracionesContables.Update(configuracion);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EstaHabilitadaGeneracionAutomaticaAsync(Guid empresaId)
    {
        var config = await _context.ConfiguracionesContables
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId);

        return config?.GenerarAsientosAutomaticos ?? false;
    }

    public async Task<bool> EstaHabilitadaAprobacionAutomaticaAsync(Guid empresaId)
    {
        var config = await _context.ConfiguracionesContables
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId);

        return config?.AprobarAsientosAutomaticos ?? false;
    }
}
