using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IDepreciacionActivoRepository
{
    Task<ActionResponse<DepreciacionActivo>> GetAsync(Guid id);
    Task<ActionResponse<IEnumerable<DepreciacionActivo>>> GetByActivoAsync(Guid activoFijoId);
    Task<ActionResponse<IEnumerable<DepreciacionActivo>>> GetByPeriodoAsync(Guid empresaId, int anio, int mes);
    Task<ActionResponse<decimal>> CalcularDepreciacionMensualAsync(Guid activoFijoId);
    Task<ActionResponse<IEnumerable<DepreciacionActivo>>> GenerarDepreciacionesMasivasAsync(Guid empresaId, int anio, int mes);
    Task<ActionResponse<DepreciacionActivo>> AddAsync(DepreciacionActivo depreciacion);
    Task<ActionResponse<bool>> ReversarDepreciacionAsync(Guid id, string usuarioId);
}
