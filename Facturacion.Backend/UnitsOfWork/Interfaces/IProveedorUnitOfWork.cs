using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IProveedorUnitOfWork
{
    IProveedorRepository ProveedorRepository { get; }
    Task<int> SaveAsync();
}
