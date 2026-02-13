using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class OrdenCompraUnitOfWork : IOrdenCompraUnitOfWork
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;

    public OrdenCompraUnitOfWork(IOrdenCompraRepository ordenCompraRepository)
    {
        _ordenCompraRepository = ordenCompraRepository;
    }

    public async Task<ActionResponse<OrdenCompra>> GetAsync(Guid id)
        => await _ordenCompraRepository.GetAsync(id);

    public async Task<ActionResponse<OrdenCompra>> GetWithDetallesAsync(Guid id)
        => await _ordenCompraRepository.GetWithDetallesAsync(id);

    public async Task<ActionResponse<IEnumerable<OrdenCompra>>> GetByEmpresaAsync(Guid empresaId)
        => await _ordenCompraRepository.GetByEmpresaAsync(empresaId);

    public async Task<ActionResponse<IEnumerable<OrdenCompra>>> GetByProveedorAsync(Guid proveedorId)
        => await _ordenCompraRepository.GetByProveedorAsync(proveedorId);

    public async Task<ActionResponse<IEnumerable<OrdenCompra>>> GetByEstadoAsync(Guid empresaId, string estado)
        => await _ordenCompraRepository.GetByEstadoAsync(empresaId, estado);

    public async Task<ActionResponse<IEnumerable<OrdenCompra>>> GetPendientesRecepcionAsync(Guid empresaId)
        => await _ordenCompraRepository.GetPendientesRecepcionAsync(empresaId);

    public async Task<ActionResponse<OrdenCompra>> AddAsync(OrdenCompra orden)
        => await _ordenCompraRepository.AddAsync(orden);

    public async Task<ActionResponse<OrdenCompra>> UpdateAsync(OrdenCompra orden)
        => await _ordenCompraRepository.UpdateAsync(orden);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId)
        => await _ordenCompraRepository.DeleteAsync(id, usuarioId);

    public async Task<ActionResponse<OrdenCompra>> AprobarAsync(Guid id, string usuarioId)
        => await _ordenCompraRepository.AprobarAsync(id, usuarioId);

    public async Task<ActionResponse<OrdenCompra>> AnularAsync(Guid id, string usuarioId, string motivo)
        => await _ordenCompraRepository.AnularAsync(id, usuarioId, motivo);

    public async Task<ActionResponse<string>> GenerarNumeroAsync(Guid empresaId)
        => await _ordenCompraRepository.GenerarNumeroAsync(empresaId);
}
