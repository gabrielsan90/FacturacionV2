using Facturacion.Shared.Entities.Catalogos;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IProvinciaRepository
{
    Task<Provincia?> GetAsync(int id);
    Task<IEnumerable<Provincia>> GetAllAsync();
    Task<Provincia?> GetByCodigoAsync(string codigo);
}
