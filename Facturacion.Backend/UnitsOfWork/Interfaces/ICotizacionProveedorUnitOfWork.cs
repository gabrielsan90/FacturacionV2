using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface ICotizacionProveedorUnitOfWork
{
    Task<ActionResponse<CotizacionProveedor>> GetAsync(Guid id);
    Task<ActionResponse<CotizacionProveedor>> GetWithDetallesAsync(Guid id);
    Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetByRequisicionAsync(Guid requisicionId);
    Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetByProveedorAsync(Guid proveedorId);
    Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetByEstadoAsync(Guid empresaId, string estado);
    Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetVencidasAsync(Guid empresaId);
    Task<ActionResponse<CotizacionProveedor>> AddAsync(CotizacionProveedor cotizacion);
    Task<ActionResponse<CotizacionProveedor>> UpdateAsync(CotizacionProveedor cotizacion);
    Task<ActionResponse<CotizacionProveedor>> SeleccionarAsync(Guid id, string usuarioId);
    Task<ActionResponse<CotizacionProveedor>> RechazarAsync(Guid id, string usuarioId, string motivo);
    Task<ActionResponse<string>> GenerarNumeroAsync(Guid empresaId);
}
