using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class EmpresaUnitOfWork : IEmpresaUnitOfWork
{
    private readonly DataContext _context;
    private IEmpresaRepository? _empresaRepository;

    public EmpresaUnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IEmpresaRepository EmpresaRepository
    {
        get
        {
            _empresaRepository ??= new EmpresaRepository(_context);
            return _empresaRepository;
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
