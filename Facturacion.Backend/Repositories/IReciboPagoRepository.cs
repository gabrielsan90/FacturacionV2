using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories;

/// <summary>
/// Repositorio para manejo de Recibos Electrónicos de Pago (REP)
/// </summary>
public interface IReciboPagoRepository
{
    /// <summary>
    /// Obtiene un recibo de pago por su ID
    /// </summary>
    Task<ReciboPago?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtiene todos los recibos de pago de un documento original
    /// </summary>
    Task<IEnumerable<ReciboPago>> GetByDocumentoOriginalAsync(Guid documentoOriginalId);

    /// <summary>
    /// Obtiene todos los recibos de pago de una empresa
    /// </summary>
    Task<IEnumerable<ReciboPago>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene el total pagado sobre un documento original (suma de todos los REPs)
    /// </summary>
    Task<decimal> GetTotalPagadoAsync(Guid documentoOriginalId);

    /// <summary>
    /// Calcula el saldo pendiente de un documento original
    /// </summary>
    Task<decimal> CalcularSaldoPendienteAsync(Guid documentoOriginalId);

    /// <summary>
    /// Agrega un nuevo recibo de pago
    /// </summary>
    Task<ReciboPago> AddAsync(ReciboPago recibo);

    /// <summary>
    /// Actualiza un recibo de pago existente
    /// </summary>
    Task<ReciboPago> UpdateAsync(ReciboPago recibo);

    /// <summary>
    /// Elimina (soft delete) un recibo de pago
    /// </summary>
    Task<bool> DeleteAsync(Guid id, string userId);

    /// <summary>
    /// Obtiene recibos de pago por rango de fechas
    /// </summary>
    Task<IEnumerable<ReciboPago>> GetByFechaRangoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);

    /// <summary>
    /// Verifica si existe un recibo de pago para un documento REP específico
    /// </summary>
    Task<bool> ExisteReciboPorDocumentoAsync(Guid documentoId);

    /// <summary>
    /// Obtiene el recibo de pago asociado a un documento REP
    /// </summary>
    Task<ReciboPago?> GetByDocumentoREPAsync(Guid documentoREPId);
}
