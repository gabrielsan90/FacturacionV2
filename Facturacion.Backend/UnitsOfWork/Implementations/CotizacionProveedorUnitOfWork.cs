using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class CotizacionProveedorUnitOfWork : ICotizacionProveedorUnitOfWork
{
    private readonly ICotizacionProveedorRepository _cotizacionProveedorRepository;

    public CotizacionProveedorUnitOfWork(ICotizacionProveedorRepository cotizacionProveedorRepository)
    {
        _cotizacionProveedorRepository = cotizacionProveedorRepository;
    }

    public async Task<ActionResponse<CotizacionProveedor>> GetAsync(Guid id)
        => await _cotizacionProveedorRepository.GetAsync(id);

    public async Task<ActionResponse<CotizacionProveedor>> GetWithDetallesAsync(Guid id)
        => await _cotizacionProveedorRepository.GetWithDetallesAsync(id);

    public async Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetByRequisicionAsync(Guid requisicionId)
        => await _cotizacionProveedorRepository.GetByRequisicionAsync(requisicionId);

    public async Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetByProveedorAsync(Guid proveedorId)
        => await _cotizacionProveedorRepository.GetByProveedorAsync(proveedorId);

    public async Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetByEstadoAsync(Guid empresaId, string estado)
        => await _cotizacionProveedorRepository.GetByEstadoAsync(empresaId, estado);

    public async Task<ActionResponse<IEnumerable<CotizacionProveedor>>> GetVencidasAsync(Guid empresaId)
        => await _cotizacionProveedorRepository.GetVencidasAsync(empresaId);

    public async Task<ActionResponse<CotizacionProveedor>> AddAsync(CotizacionProveedor cotizacion)
        => await _cotizacionProveedorRepository.AddAsync(cotizacion);

    public async Task<ActionResponse<CotizacionProveedor>> UpdateAsync(CotizacionProveedor cotizacion)
        => await _cotizacionProveedorRepository.UpdateAsync(cotizacion);

    public async Task<ActionResponse<CotizacionProveedor>> SeleccionarAsync(Guid id, string usuarioId)
        => await _cotizacionProveedorRepository.SeleccionarAsync(id, usuarioId);

    public async Task<ActionResponse<CotizacionProveedor>> RechazarAsync(Guid id, string usuarioId, string motivo)
        => await _cotizacionProveedorRepository.RechazarAsync(id, usuarioId, motivo);

    public async Task<ActionResponse<string>> GenerarNumeroAsync(Guid empresaId)
        => await _cotizacionProveedorRepository.GenerarNumeroAsync(empresaId);
}
