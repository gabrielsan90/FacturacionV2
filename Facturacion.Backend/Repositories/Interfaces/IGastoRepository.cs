using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IGastoRepository
{
    Task<Gasto?> GetAsync(Guid id);
    Task<IEnumerable<Gasto>> GetByEmpresaAsync(Guid empresaId, bool includeDeleted = false);
    Task<IEnumerable<GastoPendienteDTO>> GetPendientesPagoAsync(Guid empresaId);
    Task<IEnumerable<Gasto>> GetByProveedorAsync(Guid proveedorId);
    Task<IEnumerable<Gasto>> GetByCategoriaAsync(int categoriaId);
    Task<IEnumerable<Gasto>> GetByFechaRangoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<GastoPorCategoriaDTO>> GetTotalPorCategoriaAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<GastoPorProveedorDTO>> GetTotalPorProveedorAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<GastoPorMesDTO>> GetTotalPorMesAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);
    Task<Gasto> AddAsync(Gasto gasto);
    Task UpdateAsync(Gasto gasto);
    Task DeleteAsync(Guid id, string userId);
}
