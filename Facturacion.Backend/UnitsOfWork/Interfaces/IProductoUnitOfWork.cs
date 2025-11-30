using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IProductoUnitOfWork
{
    IProductoRepository ProductoRepository { get; }
    ICategoriaRepository CategoriaRepository { get; }
    Task<int> SaveAsync();
}
