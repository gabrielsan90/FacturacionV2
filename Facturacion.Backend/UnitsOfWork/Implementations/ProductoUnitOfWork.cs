using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class ProductoUnitOfWork : IProductoUnitOfWork
{
    private readonly DataContext _context;
    private IProductoRepository? _productoRepository;
    private ICategoriaRepository? _categoriaRepository;

    public ProductoUnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IProductoRepository ProductoRepository
    {
        get
        {
            _productoRepository ??= new ProductoRepository(_context);
            return _productoRepository;
        }
    }

    public ICategoriaRepository CategoriaRepository
    {
        get
        {
            _categoriaRepository ??= new CategoriaRepository(_context);
            return _categoriaRepository;
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
