using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para gestión de Abonos de Cobranza
/// </summary>
public interface IAbonoCobroRepository
{
    /// <summary>
    /// Obtiene un abono de cobranza por su ID
    /// </summary>
    Task<ActionResponse<AbonoCobranza>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todos los abonos aplicados a una cuenta por cobrar
    /// </summary>
    Task<ActionResponse<IEnumerable<AbonoCobranza>>> GetByCuentaPorCobrarAsync(Guid cuentaPorCobrarId);

    /// <summary>
    /// Obtiene abonos de cobranza por rango de fechas
    /// </summary>
    Task<ActionResponse<IEnumerable<AbonoCobranza>>> GetByFechaAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Crea un nuevo abono de cobranza
    /// </summary>
    Task<ActionResponse<AbonoCobranza>> AddAsync(AbonoCobranza abono);

    /// <summary>
    /// Anula un abono de cobranza (soft delete)
    /// </summary>
    Task<ActionResponse<bool>> AnularAsync(Guid id, string usuarioId, string motivo);
}
