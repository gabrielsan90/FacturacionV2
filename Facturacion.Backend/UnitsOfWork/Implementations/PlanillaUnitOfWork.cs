using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class PlanillaUnitOfWork : IPlanillaUnitOfWork
{
    private readonly DataContext _context;
    private readonly ILoggerFactory _loggerFactory;
    private IPlanillaRepository? _planillaRepository;
    private IEmpleadoRepository? _empleadoRepository;

    public PlanillaUnitOfWork(DataContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _loggerFactory = loggerFactory;
    }

    public IPlanillaRepository PlanillaRepository
    {
        get
        {
            _planillaRepository ??= new PlanillaRepository(_context, _loggerFactory.CreateLogger<PlanillaRepository>());
            return _planillaRepository;
        }
    }

    public IEmpleadoRepository EmpleadoRepository
    {
        get
        {
            _empleadoRepository ??= new EmpleadoRepository(_context, _loggerFactory.CreateLogger<EmpleadoRepository>());
            return _empleadoRepository;
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
