using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para gestión de Recibos Electrónicos de Pago (REP)
/// Nuevo en Hacienda v4.4 para registrar pagos sobre ventas a crédito
/// </summary>
public interface IReciboPagoService
{
    /// <summary>
    /// Genera un nuevo REP (Recibo Electrónico de Pago)
    /// 1. Valida documento original a crédito
    /// 2. Calcula saldo pendiente
    /// 3. Crea documento REP
    /// 4. Genera XML, firma y envía a Hacienda
    /// </summary>
    /// <param name="dto">Datos del recibo de pago</param>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="terminalId">ID del terminal</param>
    /// <param name="userId">ID del usuario que genera el REP</param>
    /// <returns>Resultado del proceso</returns>
    Task<ResultadoREP> GenerarREPAsync(ReciboPagoDTO dto, Guid empresaId, Guid terminalId, string userId);

    /// <summary>
    /// Genera un REP que aplica pago a múltiples facturas del mismo cliente
    /// </summary>
    Task<ResultadoREP> GenerarREPMultiAsync(ReciboPagoMultiDTO dto, Guid empresaId, Guid terminalId, string userId);

    /// <summary>
    /// Obtiene documentos pendientes de pago (cuentas por cobrar)
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="clienteId">Filtro opcional por cliente</param>
    /// <param name="soloVencidos">Si true, solo retorna documentos vencidos</param>
    /// <returns>Lista de documentos con saldo pendiente</returns>
    Task<List<DocumentoPendientePagoDTO>> GetDocumentosPendientesPagoAsync(
        Guid empresaId,
        Guid? clienteId = null,
        bool soloVencidos = false);

    /// <summary>
    /// Obtiene el detalle de pagos de un documento específico
    /// </summary>
    /// <param name="documentoId">ID del documento original</param>
    /// <returns>Detalle con todos los pagos aplicados</returns>
    Task<DetallePagosDocumentoDTO> GetDetallePagosDocumentoAsync(Guid documentoId);

    /// <summary>
    /// Calcula el saldo pendiente de un documento
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <returns>Saldo pendiente</returns>
    Task<decimal> CalcularSaldoPendienteAsync(Guid documentoId);

    /// <summary>
    /// Valida si un pago puede ser aplicado a un documento
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <param name="montoPago">Monto que se desea pagar</param>
    /// <returns>True si el pago es válido</returns>
    Task<(bool esValido, string mensaje)> ValidarPagoAsync(Guid documentoId, decimal montoPago);

    /// <summary>
    /// Obtiene todos los REPs de un documento original
    /// </summary>
    /// <param name="documentoId">ID del documento original</param>
    /// <returns>Lista de recibos de pago</returns>
    Task<List<ReciboPago>> GetRecibosPorDocumentoAsync(Guid documentoId);

    /// <summary>
    /// Obtiene un REP específico con todos sus datos
    /// </summary>
    /// <param name="reciboId">ID del recibo</param>
    /// <returns>Recibo de pago completo</returns>
    Task<ReciboPago?> GetReciboAsync(Guid reciboId);

    /// <summary>
    /// Anula un REP (genera nota de crédito o marca como anulado)
    /// </summary>
    /// <param name="reciboId">ID del recibo a anular</param>
    /// <param name="userId">ID del usuario que anula</param>
    /// <param name="razon">Razón de la anulación</param>
    /// <returns>True si se anuló exitosamente</returns>
    Task<(bool exitoso, string mensaje)> AnularREPAsync(Guid reciboId, string userId, string razon);

    /// <summary>
    /// Obtiene estadísticas de cuentas por cobrar de una empresa
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Estadísticas de cobranza</returns>
    Task<EstadisticasCobranzaDTO> GetEstadisticasCobranzaAsync(Guid empresaId);
}
