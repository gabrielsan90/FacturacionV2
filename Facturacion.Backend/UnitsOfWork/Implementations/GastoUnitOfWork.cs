using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.Services.Implementations;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class GastoUnitOfWork : IGastoUnitOfWork
{
    private readonly DataContext _context;
    private IGastoRepository? _gastoRepository;
    private ICategoriaGastoRepository? _categoriaGastoRepository;
    private IGastoService? _gastoService;
    private IProveedorRepository? _proveedorRepository;

    public GastoUnitOfWork(DataContext context)
    {
        _context = context;
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
            _gastoService ??= new GastoService(GastoRepository, CategoriaGastoRepository, ProveedorRepository);
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
