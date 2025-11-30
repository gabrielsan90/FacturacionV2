using Facturacion.Shared.DTOs;
using Facturacion.Shared.Enums;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para gestionar Mensajes Receptor (MR)
/// Orquesta el proceso completo de generación, firma y envío a Hacienda
/// </summary>
public interface IMensajeReceptorService
{
    /// <summary>
    /// Genera y envía un Mensaje Receptor a Hacienda
    /// </summary>
    /// <param name="documentoOriginalId">ID del documento recibido</param>
    /// <param name="tipoMensaje">Tipo de mensaje (1=Aceptación, 2=Parcial, 3=Rechazo)</param>
    /// <param name="codigoMensaje">Código del mensaje según catálogo</param>
    /// <param name="detalle">Detalle opcional (obligatorio para rechazo y parcial)</param>
    /// <param name="montoAceptado">Monto total aceptado (solo para parcial)</param>
    /// <param name="montoImpuestoAceptado">Monto de impuesto aceptado (solo para parcial)</param>
    /// <returns>Resultado del proceso</returns>
    Task<ResultadoMR> GenerarYEnviarMensajeAsync(
        Guid documentoOriginalId,
        TipoMensajeReceptor tipoMensaje,
        CodigoMensajeReceptor codigoMensaje,
        string? detalle = null,
        decimal? montoAceptado = null,
        decimal? montoImpuestoAceptado = null);

    /// <summary>
    /// Valida si se puede enviar un MR para un documento
    /// </summary>
    /// <param name="documentoOriginalId">ID del documento</param>
    /// <returns>True si se puede enviar MR</returns>
    Task<(bool puedeEnviar, string? razon)> ValidarMensajeReceptorAsync(Guid documentoOriginalId);

    /// <summary>
    /// Obtiene documentos pendientes de enviar MR
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de documentos pendientes con información para MR</returns>
    Task<List<DocumentosPendientesMR>> ObtenerDocumentosPendientesMRAsync(Guid empresaId);

    /// <summary>
    /// Reenvía un MR a Hacienda (si falló el primer envío)
    /// </summary>
    /// <param name="mensajeId">ID del mensaje a reenviar</param>
    /// <returns>Resultado del reenvío</returns>
    Task<ResultadoMR> ReenviarMensajeAsync(Guid mensajeId);

    /// <summary>
    /// Obtiene los mensajes receptor asociados a un documento
    /// </summary>
    /// <param name="documentoOriginalId">ID del documento original</param>
    /// <returns>Lista de mensajes asociados</returns>
    Task<IEnumerable<ResultadoMR>> ObtenerMensajesPorDocumentoAsync(Guid documentoOriginalId);
}
