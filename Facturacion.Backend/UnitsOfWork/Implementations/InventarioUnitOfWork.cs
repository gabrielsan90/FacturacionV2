using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class InventarioUnitOfWork : IInventarioUnitOfWork
{
    private readonly DataContext _context;
    private IInventarioRepository? _inventarioRepository;
    private IMovimientoInventarioRepository? _movimientoInventarioRepository;

    public InventarioUnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IInventarioRepository InventarioRepository
    {
        get
        {
            _inventarioRepository ??= new InventarioRepository(_context);
            return _inventarioRepository;
        }
    }

    public IMovimientoInventarioRepository MovimientoInventarioRepository
    {
        get
        {
            _movimientoInventarioRepository ??= new MovimientoInventarioRepository(_context);
            return _movimientoInventarioRepository;
        }
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
