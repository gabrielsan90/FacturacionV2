using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class RecepcionCompraUnitOfWork : IRecepcionCompraUnitOfWork
{
    private readonly IRecepcionCompraRepository _recepcionCompraRepository;

    public RecepcionCompraUnitOfWork(IRecepcionCompraRepository recepcionCompraRepository)
    {
        _recepcionCompraRepository = recepcionCompraRepository;
    }

    public async Task<ActionResponse<RecepcionCompra>> GetAsync(Guid id)
        => await _recepcionCompraRepository.GetAsync(id);

    public async Task<ActionResponse<RecepcionCompra>> GetWithDetallesAsync(Guid id)
        => await _recepcionCompraRepository.GetWithDetallesAsync(id);

    public async Task<ActionResponse<IEnumerable<RecepcionCompra>>> GetByEmpresaAsync(Guid empresaId)
        => await _recepcionCompraRepository.GetByEmpresaAsync(empresaId);

    public async Task<ActionResponse<IEnumerable<RecepcionCompra>>> GetByOrdenCompraAsync(Guid ordenCompraId)
        => await _recepcionCompraRepository.GetByOrdenCompraAsync(ordenCompraId);

    public async Task<ActionResponse<IEnumerable<RecepcionCompra>>> GetPendientesAsync(Guid empresaId)
        => await _recepcionCompraRepository.GetPendientesAsync(empresaId);

    public async Task<ActionResponse<bool>> ValidarCantidadesAsync(Guid ordenCompraId, List<RecepcionCompraDetalle> detalles)
        => await _recepcionCompraRepository.ValidarCantidadesAsync(ordenCompraId, detalles);

    public async Task<ActionResponse<RecepcionCompra>> AddAsync(RecepcionCompra recepcion)
        => await _recepcionCompraRepository.AddAsync(recepcion);

    public async Task<ActionResponse<RecepcionCompra>> UpdateAsync(RecepcionCompra recepcion)
        => await _recepcionCompraRepository.UpdateAsync(recepcion);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId)
        => await _recepcionCompraRepository.DeleteAsync(id, usuarioId);

    public async Task<ActionResponse<RecepcionCompra>> AnularAsync(Guid id, string usuarioId, string motivo)
        => await _recepcionCompraRepository.AnularAsync(id, usuarioId, motivo);

    public async Task<ActionResponse<string>> GenerarNumeroAsync(Guid empresaId)
        => await _recepcionCompraRepository.GenerarNumeroAsync(empresaId);
}
