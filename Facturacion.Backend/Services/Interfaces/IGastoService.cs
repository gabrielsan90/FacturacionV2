using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Services.Interfaces;

public interface IGastoService
{
    Task<Gasto> CrearGastoAsync(GastoDTO dto, Guid empresaId, string userId);
    Task<Gasto> ActualizarGastoAsync(Guid id, GastoDTO dto, string userId);
    Task<Gasto> RegistrarPagoAsync(RegistrarPagoDTO dto, string userId);
    Task<Gasto> AprobarGastoAsync(AprobarGastoDTO dto, string userId);
    Task<EstadisticasGastosDTO> GetEstadisticasAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<GastoResumenDTO>> GetResumenGastosAsync(Guid empresaId, DateTime? fechaInicio = null, DateTime? fechaFin = null);
}
