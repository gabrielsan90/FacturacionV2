using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IInventarioRepository
{
    Task<Inventario?> GetAsync(Guid id);
    Task<Inventario?> GetByProductoSucursalAsync(Guid productoId, Guid sucursalId);
    Task<IEnumerable<Inventario>> GetBySucursalAsync(Guid sucursalId);
    Task<IEnumerable<Inventario>> GetByProductoAsync(Guid productoId);
    Task<IEnumerable<Inventario>> GetByEmpresaAsync(Guid empresaId);
    Task<IEnumerable<Inventario>> GetBajoStockAsync(Guid empresaId);
    Task<Inventario> AddAsync(Inventario inventario);
    Task UpdateAsync(Inventario inventario);
    Task DeleteAsync(Guid id, string userId);
    Task<bool> AjustarInventarioAsync(Guid id, decimal cantidad, string? referencia, string? observaciones, string userId);
    Task<bool> TrasladarInventarioAsync(Guid inventarioOrigenId, Guid sucursalDestinoId, decimal cantidad, string? referencia, string? observaciones, string userId);
}
