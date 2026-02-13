using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IActivoFijoUnitOfWork
{
    IActivoFijoRepository ActivoFijoRepository { get; }
    IDepreciacionActivoRepository DepreciacionActivoRepository { get; }
    ICategoriaActivoRepository CategoriaActivoRepository { get; }
    ITrasladoActivoRepository TrasladoActivoRepository { get; }
    Task<int> SaveAsync();
}
