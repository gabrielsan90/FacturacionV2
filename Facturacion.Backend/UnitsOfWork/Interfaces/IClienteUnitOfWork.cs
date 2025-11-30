using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IClienteUnitOfWork
{
    IClienteRepository ClienteRepository { get; }
    Task<int> SaveAsync();
}
