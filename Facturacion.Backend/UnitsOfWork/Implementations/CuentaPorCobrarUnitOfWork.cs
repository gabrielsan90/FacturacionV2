using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

/// <summary>
/// Unit of Work para gestión de Cuentas por Cobrar, Abonos y Cotizaciones
/// </summary>
public class CuentaPorCobrarUnitOfWork : ICuentaPorCobrarUnitOfWork
{
    private readonly DataContext _context;

    public CuentaPorCobrarUnitOfWork(
        DataContext context,
        ICuentaPorCobrarRepository cuentaPorCobrarRepository,
        IAbonoCobroRepository abonoCobroRepository,
        ICotizacionRepository cotizacionRepository)
    {
        _context = context;
        CuentaPorCobrarRepository = cuentaPorCobrarRepository;
        AbonoCobroRepository = abonoCobroRepository;
        CotizacionRepository = cotizacionRepository;
    }

    public ICuentaPorCobrarRepository CuentaPorCobrarRepository { get; }
    public IAbonoCobroRepository AbonoCobroRepository { get; }
    public ICotizacionRepository CotizacionRepository { get; }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
