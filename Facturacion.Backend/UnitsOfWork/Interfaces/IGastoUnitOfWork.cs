using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.Services.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IGastoUnitOfWork
{
    IGastoRepository GastoRepository { get; }
    ICategoriaGastoRepository CategoriaGastoRepository { get; }
    IGastoService GastoService { get; }
    Task<int> SaveAsync();
}
