using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IMovimientoInventarioRepository
{
    Task<MovimientoInventario?> GetAsync(Guid id);
    Task<IEnumerable<MovimientoInventario>> GetByInventarioAsync(Guid inventarioId);
    Task<IEnumerable<MovimientoInventario>> GetByFechaAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<MovimientoInventario>> GetByTipoAsync(Guid empresaId, TipoMovimientoInventario tipo);
    Task<MovimientoInventario> AddAsync(MovimientoInventario movimiento);
}
