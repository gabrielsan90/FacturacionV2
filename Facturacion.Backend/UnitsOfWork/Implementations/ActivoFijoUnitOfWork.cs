using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class ActivoFijoUnitOfWork : IActivoFijoUnitOfWork
{
    private readonly DataContext _context;
    private IActivoFijoRepository? _activoFijoRepository;
    private IDepreciacionActivoRepository? _depreciacionActivoRepository;
    private ICategoriaActivoRepository? _categoriaActivoRepository;
    private ITrasladoActivoRepository? _trasladoActivoRepository;

    public ActivoFijoUnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IActivoFijoRepository ActivoFijoRepository
    {
        get
        {
            _activoFijoRepository ??= new ActivoFijoRepository(_context);
            return _activoFijoRepository;
        }
    }

    public IDepreciacionActivoRepository DepreciacionActivoRepository
    {
        get
        {
            _depreciacionActivoRepository ??= new DepreciacionActivoRepository(_context);
            return _depreciacionActivoRepository;
        }
    }

    public ICategoriaActivoRepository CategoriaActivoRepository
    {
        get
        {
            _categoriaActivoRepository ??= new CategoriaActivoRepository(_context);
            return _categoriaActivoRepository;
        }
    }

    public ITrasladoActivoRepository TrasladoActivoRepository
    {
        get
        {
            _trasladoActivoRepository ??= new TrasladoActivoRepository(_context);
            return _trasladoActivoRepository;
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
