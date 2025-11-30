using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface ISucursalUnitOfWork
{
    ISucursalRepository SucursalRepository { get; }
    ITerminalRepository TerminalRepository { get; }
    Task<int> SaveAsync();
}
