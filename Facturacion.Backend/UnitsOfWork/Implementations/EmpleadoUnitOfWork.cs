using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class EmpleadoUnitOfWork : IEmpleadoUnitOfWork
{
    private readonly DataContext _context;
    private readonly ILoggerFactory _loggerFactory;
    private IEmpleadoRepository? _empleadoRepository;
    private IDepartamentoRepository? _departamentoRepository;
    private IPuestoRepository? _puestoRepository;
    private IVacacionRepository? _vacacionRepository;
    private IIncapacidadRepository? _incapacidadRepository;

    public EmpleadoUnitOfWork(DataContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _loggerFactory = loggerFactory;
    }

    public IEmpleadoRepository EmpleadoRepository
    {
        get
        {
            _empleadoRepository ??= new EmpleadoRepository(_context, _loggerFactory.CreateLogger<EmpleadoRepository>());
            return _empleadoRepository;
        }
    }

    public IDepartamentoRepository DepartamentoRepository
    {
        get
        {
            _departamentoRepository ??= new DepartamentoRepository(_context, _loggerFactory.CreateLogger<DepartamentoRepository>());
            return _departamentoRepository;
        }
    }

    public IPuestoRepository PuestoRepository
    {
        get
        {
            _puestoRepository ??= new PuestoRepository(_context, _loggerFactory.CreateLogger<PuestoRepository>());
            return _puestoRepository;
        }
    }

    public IVacacionRepository VacacionRepository
    {
        get
        {
            _vacacionRepository ??= new VacacionRepository(_context, _loggerFactory.CreateLogger<VacacionRepository>());
            return _vacacionRepository;
        }
    }

    public IIncapacidadRepository IncapacidadRepository
    {
        get
        {
            _incapacidadRepository ??= new IncapacidadRepository(_context, _loggerFactory.CreateLogger<IncapacidadRepository>());
            return _incapacidadRepository;
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
