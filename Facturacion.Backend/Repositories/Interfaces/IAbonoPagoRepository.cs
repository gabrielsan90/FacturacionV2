using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para gestión de Abonos y Pagos a Cuentas por Pagar
/// </summary>
public interface IAbonoPagoRepository
{
    /// <summary>
    /// Obtiene un abono por su ID
    /// </summary>
    Task<ActionResponse<AbonoPago>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todos los abonos de una cuenta por pagar
    /// </summary>
    Task<ActionResponse<IEnumerable<AbonoPago>>> GetByCuentaPorPagarAsync(Guid cuentaPorPagarId);

    /// <summary>
    /// Obtiene abonos por rango de fechas
    /// </summary>
    Task<ActionResponse<IEnumerable<AbonoPago>>> GetByFechaAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Obtiene abonos por método de pago
    /// </summary>
    Task<ActionResponse<IEnumerable<AbonoPago>>> GetByMetodoPagoAsync(Guid empresaId, string metodoPago);

    /// <summary>
    /// Crea un nuevo abono
    /// </summary>
    Task<ActionResponse<AbonoPago>> AddAsync(AbonoPago abono);

    /// <summary>
    /// Actualiza un abono
    /// </summary>
    Task<ActionResponse<AbonoPago>> UpdateAsync(AbonoPago abono);

    /// <summary>
    /// Elimina un abono (soft delete)
    /// </summary>
    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
