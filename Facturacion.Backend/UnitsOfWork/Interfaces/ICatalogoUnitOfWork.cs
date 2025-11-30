using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface ICatalogoUnitOfWork : IDisposable
{
    IProvinciaRepository Provincias { get; }
    ICantonRepository Cantones { get; }
    IDistritoRepository Distritos { get; }

    Task<int> SaveChangesAsync();
}
