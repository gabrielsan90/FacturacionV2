using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class CatalogoUnitOfWork : ICatalogoUnitOfWork
{
    private readonly DataContext _context;
    private IProvinciaRepository? _provinciaRepository;
    private ICantonRepository? _cantonRepository;
    private IDistritoRepository? _distritoRepository;

    public CatalogoUnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IProvinciaRepository Provincias
    {
        get
        {
            _provinciaRepository ??= new ProvinciaRepository(_context);
            return _provinciaRepository;
        }
    }

    public ICantonRepository Cantones
    {
        get
        {
            _cantonRepository ??= new CantonRepository(_context);
            return _cantonRepository;
        }
    }

    public IDistritoRepository Distritos
    {
        get
        {
            _distritoRepository ??= new DistritoRepository(_context);
            return _distritoRepository;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
