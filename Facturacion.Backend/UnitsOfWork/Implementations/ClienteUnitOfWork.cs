using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class ClienteUnitOfWork : IClienteUnitOfWork
{
    private readonly DataContext _context;
    private IClienteRepository? _clienteRepository;

    public ClienteUnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IClienteRepository ClienteRepository
    {
        get
        {
            _clienteRepository ??= new ClienteRepository(_context);
            return _clienteRepository;
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
