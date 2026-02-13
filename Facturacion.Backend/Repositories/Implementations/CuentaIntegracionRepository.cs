using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

/// <summary>
/// Implementación del repositorio de cuentas de integración contable.
/// </summary>
public class CuentaIntegracionRepository : ICuentaIntegracionRepository
{
    private readonly DataContext _context;

    public CuentaIntegracionRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<CuentaIntegracion?> GetAsync(Guid id)
    {
        return await _context.CuentasIntegracion
            .Include(c => c.CuentaContable)
            .Include(c => c.Empresa)
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
    }

    public async Task<IEnumerable<CuentaIntegracion>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.CuentasIntegracion
            .Include(c => c.CuentaContable)
            .Where(c => c.EmpresaId == empresaId && c.Activo)
            .OrderBy(c => c.Modulo)
            .ThenBy(c => c.TipoOperacion)
            .ThenBy(c => c.Orden)
            .ToListAsync();
    }

    public async Task<IEnumerable<CuentaIntegracion>> GetByModuloAsync(Guid empresaId, string modulo)
    {
        return await _context.CuentasIntegracion
            .Include(c => c.CuentaContable)
            .Where(c => c.EmpresaId == empresaId &&
                       c.Modulo == modulo &&
                       c.Activo)
            .OrderBy(c => c.TipoOperacion)
            .ThenBy(c => c.Orden)
            .ToListAsync();
    }

    public async Task<IEnumerable<CuentaIntegracion>> GetByModuloYTipoOperacionAsync(
        Guid empresaId,
        string modulo,
        string tipoOperacion)
    {
        return await _context.CuentasIntegracion
            .Include(c => c.CuentaContable)
            .Where(c => c.EmpresaId == empresaId &&
                       c.Modulo == modulo &&
                       c.TipoOperacion == tipoOperacion &&
                       c.Activo)
            .OrderBy(c => c.Orden)
            .ToListAsync();
    }

    public async Task<CuentaIntegracion?> GetByModuloTipoOperacionYConceptoAsync(
        Guid empresaId,
        string modulo,
        string tipoOperacion,
        string conceptoContable)
    {
        return await _context.CuentasIntegracion
            .Include(c => c.CuentaContable)
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId &&
                                     c.Modulo == modulo &&
                                     c.TipoOperacion == tipoOperacion &&
                                     c.ConceptoContable == conceptoContable &&
                                     c.Activo);
    }

    public async Task<CuentaIntegracion> AddAsync(CuentaIntegracion cuentaIntegracion)
    {
        cuentaIntegracion.Id = Guid.NewGuid();
        cuentaIntegracion.FechaCreacion = FechaCostaRicaHelper.Ahora;
        cuentaIntegracion.Activo = true;

        _context.CuentasIntegracion.Add(cuentaIntegracion);
        await _context.SaveChangesAsync();

        return cuentaIntegracion;
    }

    public async Task UpdateAsync(CuentaIntegracion cuentaIntegracion)
    {
        cuentaIntegracion.FechaModificacion = FechaCostaRicaHelper.Ahora;
        _context.CuentasIntegracion.Update(cuentaIntegracion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var cuenta = await _context.CuentasIntegracion.FindAsync(id);
        if (cuenta != null)
        {
            cuenta.Activo = false;
            cuenta.FechaModificacion = FechaCostaRicaHelper.Ahora;
            cuenta.ModificadoPorId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
