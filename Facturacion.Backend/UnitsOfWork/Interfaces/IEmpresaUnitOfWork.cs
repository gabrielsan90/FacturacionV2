using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IEmpresaUnitOfWork : IDisposable
{
    IEmpresaRepository EmpresaRepository { get; }
    Task<int> SaveChangesAsync();
}
