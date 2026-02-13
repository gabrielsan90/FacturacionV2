using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IPlantillaAsientoRepository
{
    Task<ActionResponse<PlantillaAsiento>> GetAsync(Guid id);
    Task<ActionResponse<IEnumerable<PlantillaAsiento>>> GetByEmpresaAsync(Guid empresaId);
    Task<ActionResponse<PlantillaAsiento>> GetByIdWithLinesAsync(Guid id);
    Task<ActionResponse<PlantillaAsiento>> GetByIdForUseAsync(Guid id);
    Task<ActionResponse<PlantillaAsiento>> AddAsync(PlantillaAsiento plantilla, IEnumerable<PlantillaAsientoLinea> lineas);
    Task<ActionResponse<PlantillaAsiento>> UpdateAsync(PlantillaAsiento plantilla, IEnumerable<PlantillaAsientoLinea> lineas);
    Task<ActionResponse<bool>> DeleteAsync(Guid id);
    Task<ActionResponse<PlantillaAsiento>> ToggleActivoAsync(Guid id, string? userId);
    Task<ActionResponse<bool>> CodigoExistsAsync(Guid empresaId, string codigo, Guid? excludeId = null);
}
