using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IOrdenCompraRepository
{
    Task<ActionResponse<OrdenCompra>> GetAsync(Guid id);
    Task<ActionResponse<OrdenCompra>> GetWithDetallesAsync(Guid id);
    Task<ActionResponse<IEnumerable<OrdenCompra>>> GetByEmpresaAsync(Guid empresaId);
    Task<ActionResponse<IEnumerable<OrdenCompra>>> GetByProveedorAsync(Guid proveedorId);
    Task<ActionResponse<IEnumerable<OrdenCompra>>> GetByEstadoAsync(Guid empresaId, string estado);
    Task<ActionResponse<IEnumerable<OrdenCompra>>> GetPendientesRecepcionAsync(Guid empresaId);
    Task<ActionResponse<OrdenCompra>> AddAsync(OrdenCompra orden);
    Task<ActionResponse<OrdenCompra>> UpdateAsync(OrdenCompra orden);
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);
    Task<ActionResponse<OrdenCompra>> AprobarAsync(Guid id, string usuarioId);
    Task<ActionResponse<OrdenCompra>> AnularAsync(Guid id, string usuarioId, string motivo);
    Task<ActionResponse<string>> GenerarNumeroAsync(Guid empresaId);
}
