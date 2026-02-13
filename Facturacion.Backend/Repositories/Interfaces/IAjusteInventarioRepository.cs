using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para operaciones CRUD de AjusteInventario
/// </summary>
public interface IAjusteInventarioRepository
{
    /// <summary>
    /// Obtiene un ajuste de inventario por su ID
    /// </summary>
    Task<ActionResponse<AjusteInventario>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todos los ajustes de inventario de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<AjusteInventario>>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene todos los ajustes de inventario de un producto específico
    /// </summary>
    Task<ActionResponse<IEnumerable<AjusteInventario>>> GetByProductoAsync(Guid productoId);

    /// <summary>
    /// Obtiene todos los ajustes de inventario de una bodega específica
    /// </summary>
    Task<ActionResponse<IEnumerable<AjusteInventario>>> GetByBodegaAsync(Guid bodegaId);

    /// <summary>
    /// Crea un nuevo ajuste de inventario
    /// </summary>
    Task<ActionResponse<AjusteInventario>> AddAsync(AjusteInventario ajuste);

    /// <summary>
    /// Aprueba un ajuste de inventario pendiente
    /// </summary>
    Task<ActionResponse<AjusteInventario>> AprobarAsync(Guid id, string aprobadoPorId);

    /// <summary>
    /// Anula un ajuste de inventario
    /// </summary>
    Task<ActionResponse<AjusteInventario>> AnularAsync(Guid id, string usuarioId);
}
