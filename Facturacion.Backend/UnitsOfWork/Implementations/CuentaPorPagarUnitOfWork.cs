using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

/// <summary>
/// Unit of Work para gestión de Cuentas por Pagar y Abonos
/// </summary>
public class CuentaPorPagarUnitOfWork : ICuentaPorPagarUnitOfWork
{
    private readonly DataContext _context;
    private readonly ILoggerFactory _loggerFactory;
    private ICuentaPorPagarRepository? _cuentaPorPagarRepository;
    private IAbonoPagoRepository? _abonoPagoRepository;

    public CuentaPorPagarUnitOfWork(DataContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _loggerFactory = loggerFactory;
    }

    public ICuentaPorPagarRepository CuentaPorPagarRepository
    {
        get
        {
            _cuentaPorPagarRepository ??= new CuentaPorPagarRepository(
                _context,
                _loggerFactory.CreateLogger<CuentaPorPagarRepository>());
            return _cuentaPorPagarRepository;
        }
    }

    public IAbonoPagoRepository AbonoPagoRepository
    {
        get
        {
            _abonoPagoRepository ??= new AbonoPagoRepository(
                _context,
                _loggerFactory.CreateLogger<AbonoPagoRepository>());
            return _abonoPagoRepository;
        }
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
