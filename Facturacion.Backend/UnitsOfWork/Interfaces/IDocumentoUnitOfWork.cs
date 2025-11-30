using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IDocumentoUnitOfWork
{
    IDocumentoRepository DocumentoRepository { get; }
    Task<int> SaveAsync();
}
