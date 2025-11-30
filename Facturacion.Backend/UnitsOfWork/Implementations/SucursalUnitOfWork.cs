using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class SucursalUnitOfWork : ISucursalUnitOfWork
{
    private readonly DataContext _context;
    private ISucursalRepository? _sucursalRepository;
    private ITerminalRepository? _terminalRepository;

    public SucursalUnitOfWork(DataContext context)
    {
        _context = context;
    }

    public ISucursalRepository SucursalRepository
    {
        get
        {
            _sucursalRepository ??= new SucursalRepository(_context);
            return _sucursalRepository;
        }
    }

    public ITerminalRepository TerminalRepository
    {
        get
        {
            _terminalRepository ??= new TerminalRepository(_context);
            return _terminalRepository;
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
