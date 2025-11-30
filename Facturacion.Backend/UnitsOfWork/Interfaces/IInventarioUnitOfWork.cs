using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IInventarioUnitOfWork
{
    IInventarioRepository InventarioRepository { get; }
    IMovimientoInventarioRepository MovimientoInventarioRepository { get; }
    Task<int> SaveAsync();
}
