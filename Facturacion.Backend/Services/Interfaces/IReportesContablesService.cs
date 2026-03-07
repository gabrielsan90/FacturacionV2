using Facturacion.Shared.DTOs;

namespace Facturacion.Backend.Services.Interfaces;

public interface IReportesContablesService
{
    Task<BalanzaComprobacionDTO> GetBalanzaComprobacionAsync(Guid empresaId, DateTime fechaDesde, DateTime fechaHasta, int? nivel, bool incluirSinMovimiento);
    Task<BalanceGeneralDTO> GetBalanceGeneralAsync(Guid empresaId, DateTime fechaCorte);
    Task<EstadoResultadosDTO> GetEstadoResultadosAsync(Guid empresaId, DateTime fechaDesde, DateTime fechaHasta);
    Task<LibroDiarioDTO> GetLibroDiarioAsync(Guid empresaId, DateTime fechaDesde, DateTime fechaHasta);
    Task<FlujoEfectivoDTO> GetFlujoEfectivoAsync(Guid empresaId, DateTime fechaDesde, DateTime fechaHasta);
    Task<BalanceSumasySaldosDTO> GetBalanceSumasySaldosAsync(Guid empresaId, DateTime fechaDesde, DateTime fechaHasta, int? nivel, bool incluirSinMovimiento);
}
