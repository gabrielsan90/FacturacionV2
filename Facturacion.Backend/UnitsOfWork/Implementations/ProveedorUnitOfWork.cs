using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class ProveedorUnitOfWork : IProveedorUnitOfWork
{
    private readonly DataContext _context;
    private IProveedorRepository? _proveedorRepository;

    public ProveedorUnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IProveedorRepository ProveedorRepository
    {
        get
        {
            _proveedorRepository ??= new ProveedorRepository(_context);
            return _proveedorRepository;
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
