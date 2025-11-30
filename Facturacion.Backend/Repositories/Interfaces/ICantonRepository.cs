using Facturacion.Shared.Entities.Catalogos;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface ICantonRepository
{
    Task<Canton?> GetAsync(int id);
    Task<IEnumerable<Canton>> GetAllAsync();
    Task<IEnumerable<Canton>> GetByProvinciaAsync(int provinciaId);
    Task<Canton?> GetByCodigoAsync(string codigo);
}
