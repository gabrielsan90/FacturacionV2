using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IRecepcionCompraUnitOfWork
{
    Task<ActionResponse<RecepcionCompra>> GetAsync(Guid id);
    Task<ActionResponse<RecepcionCompra>> GetWithDetallesAsync(Guid id);
    Task<ActionResponse<IEnumerable<RecepcionCompra>>> GetByEmpresaAsync(Guid empresaId);
    Task<ActionResponse<IEnumerable<RecepcionCompra>>> GetByOrdenCompraAsync(Guid ordenCompraId);
    Task<ActionResponse<IEnumerable<RecepcionCompra>>> GetPendientesAsync(Guid empresaId);
    Task<ActionResponse<bool>> ValidarCantidadesAsync(Guid ordenCompraId, List<RecepcionCompraDetalle> detalles);
    Task<ActionResponse<RecepcionCompra>> AddAsync(RecepcionCompra recepcion);
    Task<ActionResponse<RecepcionCompra>> UpdateAsync(RecepcionCompra recepcion);
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);
    Task<ActionResponse<RecepcionCompra>> AnularAsync(Guid id, string usuarioId, string motivo);
    Task<ActionResponse<string>> GenerarNumeroAsync(Guid empresaId);
}
