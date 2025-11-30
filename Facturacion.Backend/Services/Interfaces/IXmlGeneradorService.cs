using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para generar XML de documentos electrónicos según esquemas Hacienda v4.4
/// </summary>
public interface IXmlGeneradorService
{
    /// <summary>
    /// Genera el XML completo del documento según su tipo
    /// </summary>
    /// <param name="documento">Documento a convertir en XML</param>
    /// <returns>XML del documento sin firmar</returns>
    Task<string> GenerarXmlAsync(Documento documento);

    /// <summary>
    /// Genera el XML de un Mensaje Receptor (MR) - tipos 05, 06, 07
    /// </summary>
    /// <param name="mensaje">Mensaje receptor a convertir en XML</param>
    /// <param name="documentoOriginal">Documento original que se está aceptando/rechazando</param>
    /// <returns>XML del mensaje receptor sin firmar</returns>
    Task<string> GenerarMensajeReceptorAsync(DocumentoReceptorMensaje mensaje, Documento documentoOriginal);

    /// <summary>
    /// Genera el XML de un Recibo Electrónico de Pago (REP) - tipo 10
    /// NUEVO en v4.4
    /// </summary>
    /// <param name="documentoREP">Documento REP a convertir en XML</param>
    /// <param name="reciboPago">Información del recibo de pago</param>
    /// <returns>XML del REP sin firmar</returns>
    Task<string> GenerarREPAsync(Documento documentoREP, ReciboPago reciboPago);

    /// <summary>
    /// Valida que un XML cumpla con la estructura básica requerida
    /// </summary>
    /// <param name="xml">XML a validar</param>
    /// <returns>True si el XML es válido</returns>
    bool ValidarXml(string xml);
}
