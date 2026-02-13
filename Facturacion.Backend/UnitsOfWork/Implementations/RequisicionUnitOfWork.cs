using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class RequisicionUnitOfWork : IRequisicionUnitOfWork
{
    private readonly IRequisicionRepository _requisicionRepository;

    public RequisicionUnitOfWork(IRequisicionRepository requisicionRepository)
    {
        _requisicionRepository = requisicionRepository;
    }

    public async Task<ActionResponse<Requisicion>> GetAsync(Guid id)
        => await _requisicionRepository.GetAsync(id);

    public async Task<ActionResponse<Requisicion>> GetWithDetallesAsync(Guid id)
        => await _requisicionRepository.GetWithDetallesAsync(id);

    public async Task<ActionResponse<IEnumerable<Requisicion>>> GetByEmpresaAsync(Guid empresaId)
        => await _requisicionRepository.GetByEmpresaAsync(empresaId);

    public async Task<ActionResponse<IEnumerable<Requisicion>>> GetBySolicitanteAsync(string solicitanteId)
        => await _requisicionRepository.GetBySolicitanteAsync(solicitanteId);

    public async Task<ActionResponse<IEnumerable<Requisicion>>> GetByEstadoAsync(Guid empresaId, string estado)
        => await _requisicionRepository.GetByEstadoAsync(empresaId, estado);

    public async Task<ActionResponse<IEnumerable<Requisicion>>> GetPendientesAprobacionAsync(Guid empresaId)
        => await _requisicionRepository.GetPendientesAprobacionAsync(empresaId);

    public async Task<ActionResponse<IEnumerable<Requisicion>>> GetAprobadasSinOCAsync(Guid empresaId)
        => await _requisicionRepository.GetAprobadasSinOCAsync(empresaId);

    public async Task<ActionResponse<Requisicion>> AddAsync(Requisicion requisicion)
        => await _requisicionRepository.AddAsync(requisicion);

    public async Task<ActionResponse<Requisicion>> UpdateAsync(Requisicion requisicion)
        => await _requisicionRepository.UpdateAsync(requisicion);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId)
        => await _requisicionRepository.DeleteAsync(id, usuarioId);

    public async Task<ActionResponse<Requisicion>> AprobarAsync(Guid id, string usuarioId)
        => await _requisicionRepository.AprobarAsync(id, usuarioId);

    public async Task<ActionResponse<Requisicion>> RechazarAsync(Guid id, string usuarioId, string motivo)
        => await _requisicionRepository.RechazarAsync(id, usuarioId, motivo);

    public async Task<ActionResponse<Requisicion>> AnularAsync(Guid id, string usuarioId, string motivo)
        => await _requisicionRepository.AnularAsync(id, usuarioId, motivo);

    public async Task<ActionResponse<string>> GenerarNumeroAsync(Guid empresaId)
        => await _requisicionRepository.GenerarNumeroAsync(empresaId);
}
