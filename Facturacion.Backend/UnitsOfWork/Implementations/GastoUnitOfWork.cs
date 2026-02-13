using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.Services.Implementations;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class GastoUnitOfWork : IGastoUnitOfWork
{
    private readonly DataContext _context;
    private readonly IContabilidadIntegracionService _contabilidadIntegracion;
    private readonly ILogger<GastoService> _logger;
    private IGastoRepository? _gastoRepository;
    private ICategoriaGastoRepository? _categoriaGastoRepository;
    private IGastoService? _gastoService;
    private IProveedorRepository? _proveedorRepository;

    public GastoUnitOfWork(
        DataContext context,
        IContabilidadIntegracionService contabilidadIntegracion,
        ILogger<GastoService> logger)
    {
        _context = context;
        _contabilidadIntegracion = contabilidadIntegracion;
        _logger = logger;
    }

    public IGastoRepository GastoRepository
    {
        get
        {
            _gastoRepository ??= new GastoRepository(_context);
            return _gastoRepository;
        }
    }

    public ICategoriaGastoRepository CategoriaGastoRepository
    {
        get
        {
            _categoriaGastoRepository ??= new CategoriaGastoRepository(_context);
            return _categoriaGastoRepository;
        }
    }

    private IProveedorRepository ProveedorRepository
    {
        get
        {
            _proveedorRepository ??= new ProveedorRepository(_context);
            return _proveedorRepository;
        }
    }

    public IGastoService GastoService
    {
        get
        {
            _gastoService ??= new GastoService(
                GastoRepository,
                CategoriaGastoRepository,
                ProveedorRepository,
                _contabilidadIntegracion,
                _logger);
            return _gastoService;
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
