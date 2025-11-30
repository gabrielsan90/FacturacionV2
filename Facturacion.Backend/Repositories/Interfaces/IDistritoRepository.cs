using Facturacion.Shared.Entities.Catalogos;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IDistritoRepository
{
    Task<Distrito?> GetAsync(int id);
    Task<IEnumerable<Distrito>> GetAllAsync();
    Task<IEnumerable<Distrito>> GetByCantonAsync(int cantonId);
    Task<Distrito?> GetByCodigoAsync(string codigo);
}
