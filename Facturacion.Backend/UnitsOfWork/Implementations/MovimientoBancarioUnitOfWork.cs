using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class MovimientoBancarioUnitOfWork : IMovimientoBancarioUnitOfWork
{
    private readonly DataContext _context;

    public MovimientoBancarioUnitOfWork(
        DataContext context,
        IMovimientoBancarioRepository movimientoBancarioRepository,
        ICuentaBancariaRepository cuentaBancariaRepository,
        IConciliacionBancariaRepository conciliacionBancariaRepository,
        IExtractoBancarioRepository extractoBancarioRepository)
    {
        _context = context;
        MovimientoBancarioRepository = movimientoBancarioRepository;
        CuentaBancariaRepository = cuentaBancariaRepository;
        ConciliacionBancariaRepository = conciliacionBancariaRepository;
        ExtractoBancarioRepository = extractoBancarioRepository;
    }

    public IMovimientoBancarioRepository MovimientoBancarioRepository { get; }
    public ICuentaBancariaRepository CuentaBancariaRepository { get; }
    public IConciliacionBancariaRepository ConciliacionBancariaRepository { get; }
    public IExtractoBancarioRepository ExtractoBancarioRepository { get; }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
