using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Facturacion.Backend.Repositories.Implementations;

public class GastoRepository : IGastoRepository
{
    private readonly DataContext _context;

    public GastoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Gasto?> GetAsync(Guid id)
    {
        return await _context.Gastos
            .Include(g => g.Empresa)
            .Include(g => g.Proveedor)
            .Include(g => g.CategoriaGasto)
            .Include(g => g.UsuarioCreacion)
            .Include(g => g.UsuarioModificacion)
            .Include(g => g.UsuarioAprobacion)
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
    }

    public async Task<IEnumerable<Gasto>> GetByEmpresaAsync(Guid empresaId, bool includeDeleted = false)
    {
        var query = _context.Gastos
            .Include(g => g.Proveedor)
            .Include(g => g.CategoriaGasto)
            .Include(g => g.UsuarioCreacion)
            .Include(g => g.UsuarioAprobacion)
            .Where(g => g.EmpresaId == empresaId);

        if (!includeDeleted)
        {
            query = query.Where(g => !g.IsDeleted);
        }

        return await query
            .OrderByDescending(g => g.FechaGasto)
            .ToListAsync();
    }

    public async Task<IEnumerable<GastoPendienteDTO>> GetPendientesPagoAsync(Guid empresaId)
    {
        var gastos = await _context.Gastos
            .Include(g => g.Proveedor)
            .Include(g => g.CategoriaGasto)
            .Where(g => g.EmpresaId == empresaId &&
                       !g.IsDeleted &&
                       g.EstadoPago != EstadoPago.Pagado &&
                       g.SaldoPendiente > 0)
            .OrderBy(g => g.FechaGasto)
            .ToListAsync();

        return gastos.Select(g => new GastoPendienteDTO
        {
            Id = g.Id,
            NumeroDocumento = g.NumeroDocumento,
            FechaGasto = g.FechaGasto,
            Descripcion = g.Descripcion,
            MontoTotal = g.MontoTotal,
            MontoPagado = g.MontoPagado,
            SaldoPendiente = g.SaldoPendiente,
            Moneda = g.Moneda,
            EstadoPago = g.EstadoPago,
            ProveedorNombre = g.Proveedor?.Nombre ?? "Sin proveedor",
            CategoriaGastoNombre = g.CategoriaGasto!.Nombre,
            DiasVencido = (FechaCostaRicaHelper.Ahora.Date - g.FechaGasto.Date).Days
        }).ToList();
    }

    public async Task<IEnumerable<Gasto>> GetByProveedorAsync(Guid proveedorId)
    {
        return await _context.Gastos
            .Include(g => g.CategoriaGasto)
            .Include(g => g.UsuarioCreacion)
            .Where(g => g.ProveedorId == proveedorId && !g.IsDeleted)
            .OrderByDescending(g => g.FechaGasto)
            .ToListAsync();
    }

    public async Task<IEnumerable<Gasto>> GetByCategoriaAsync(int categoriaId)
    {
        return await _context.Gastos
            .Include(g => g.Proveedor)
            .Include(g => g.UsuarioCreacion)
            .Where(g => g.CategoriaGastoId == categoriaId && !g.IsDeleted)
            .OrderByDescending(g => g.FechaGasto)
            .ToListAsync();
    }

    public async Task<IEnumerable<Gasto>> GetByFechaRangoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.Gastos
            .Include(g => g.Proveedor)
            .Include(g => g.CategoriaGasto)
            .Include(g => g.UsuarioCreacion)
            .Where(g => g.EmpresaId == empresaId &&
                       !g.IsDeleted &&
                       g.FechaGasto >= fechaInicio &&
                       g.FechaGasto <= fechaFin)
            .OrderByDescending(g => g.FechaGasto)
            .ToListAsync();
    }

    public async Task<IEnumerable<GastoPorCategoriaDTO>> GetTotalPorCategoriaAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        var gastos = await _context.Gastos
            .Include(g => g.CategoriaGasto)
            .Where(g => g.EmpresaId == empresaId &&
                       !g.IsDeleted &&
                       g.FechaGasto >= fechaInicio &&
                       g.FechaGasto <= fechaFin)
            .ToListAsync();

        var totalGeneral = gastos.Sum(g => g.MontoTotal);

        var resultado = gastos
            .GroupBy(g => new { g.CategoriaGastoId, g.CategoriaGasto!.Nombre })
            .Select(group => new GastoPorCategoriaDTO
            {
                CategoriaId = group.Key.CategoriaGastoId,
                CategoriaNombre = group.Key.Nombre,
                Total = group.Sum(g => g.MontoTotal),
                Cantidad = group.Count(),
                Porcentaje = totalGeneral > 0 ? (group.Sum(g => g.MontoTotal) / totalGeneral) * 100 : 0
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        return resultado;
    }

    public async Task<IEnumerable<GastoPorProveedorDTO>> GetTotalPorProveedorAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        var gastos = await _context.Gastos
            .Include(g => g.Proveedor)
            .Where(g => g.EmpresaId == empresaId &&
                       !g.IsDeleted &&
                       g.FechaGasto >= fechaInicio &&
                       g.FechaGasto <= fechaFin)
            .ToListAsync();

        var resultado = gastos
            .GroupBy(g => new
            {
                g.ProveedorId,
                ProveedorNombre = g.Proveedor != null ? g.Proveedor.Nombre : "Sin proveedor"
            })
            .Select(group => new GastoPorProveedorDTO
            {
                ProveedorId = group.Key.ProveedorId,
                ProveedorNombre = group.Key.ProveedorNombre,
                Total = group.Sum(g => g.MontoTotal),
                Cantidad = group.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        return resultado;
    }

    public async Task<IEnumerable<GastoPorMesDTO>> GetTotalPorMesAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        var gastos = await _context.Gastos
            .Where(g => g.EmpresaId == empresaId &&
                       !g.IsDeleted &&
                       g.FechaGasto >= fechaInicio &&
                       g.FechaGasto <= fechaFin)
            .ToListAsync();

        var resultado = gastos
            .GroupBy(g => new { g.FechaGasto.Year, g.FechaGasto.Month })
            .Select(group => new GastoPorMesDTO
            {
                Anio = group.Key.Year,
                Mes = group.Key.Month,
                MesNombre = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(group.Key.Month),
                Total = group.Sum(g => g.MontoTotal),
                Cantidad = group.Count()
            })
            .OrderBy(x => x.Anio)
            .ThenBy(x => x.Mes)
            .ToList();

        return resultado;
    }

    public async Task<Gasto> AddAsync(Gasto gasto)
    {
        gasto.FechaCreacion = FechaCostaRicaHelper.Ahora;
        gasto.SaldoPendiente = gasto.MontoTotal - gasto.MontoPagado;

        // Actualizar estado de pago automáticamente
        if (gasto.MontoPagado == 0)
        {
            gasto.EstadoPago = EstadoPago.Pendiente;
        }
        else if (gasto.MontoPagado >= gasto.MontoTotal)
        {
            gasto.EstadoPago = EstadoPago.Pagado;
            gasto.SaldoPendiente = 0;
        }
        else
        {
            gasto.EstadoPago = EstadoPago.Parcial;
        }

        _context.Gastos.Add(gasto);
        await _context.SaveChangesAsync();
        return gasto;
    }

    public async Task UpdateAsync(Gasto gasto)
    {
        gasto.FechaModificacion = FechaCostaRicaHelper.Ahora;
        gasto.SaldoPendiente = gasto.MontoTotal - gasto.MontoPagado;

        // Actualizar estado de pago automáticamente
        if (gasto.MontoPagado == 0)
        {
            gasto.EstadoPago = EstadoPago.Pendiente;
        }
        else if (gasto.MontoPagado >= gasto.MontoTotal)
        {
            gasto.EstadoPago = EstadoPago.Pagado;
            gasto.SaldoPendiente = 0;
        }
        else
        {
            gasto.EstadoPago = EstadoPago.Parcial;
        }

        _context.Gastos.Update(gasto);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var gasto = await _context.Gastos.FindAsync(id);
        if (gasto != null)
        {
            gasto.IsDeleted = true;
            gasto.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            gasto.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
