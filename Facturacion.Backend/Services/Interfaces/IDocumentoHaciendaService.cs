using Facturacion.Shared.DTOs;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio orquestador para el proceso completo de Hacienda
/// Genera clave, XML, firma y envía a Hacienda
/// </summary>
public interface IDocumentoHaciendaService
{
    /// <summary>
    /// Procesa un documento completo: genera clave, XML, firma y envía a Hacienda
    /// </summary>
    /// <param name="documentoId">ID del documento a procesar</param>
    /// <returns>Resultado del proceso</returns>
    Task<ResultadoEnvio> ProcesarYEnviarAsync(Guid documentoId);

    /// <summary>
    /// Consulta el estado de un documento en Hacienda
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <returns>Resultado de la consulta</returns>
    Task<ResultadoConsulta> ConsultarEstadoAsync(Guid documentoId);

    /// <summary>
    /// Reenvía un documento rechazado a Hacienda
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <returns>True si se reenvió exitosamente</returns>
    Task<bool> ReenviarAsync(Guid documentoId);

    /// <summary>
    /// Genera solo el XML de un documento sin enviarlo
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <returns>XML generado</returns>
    Task<string> GenerarXmlAsync(Guid documentoId);

    /// <summary>
    /// Valida un documento antes de enviarlo a Hacienda
    /// </summary>
    /// <param name="documentoId">ID del documento</param>
    /// <returns>Lista de errores de validación (vacía si es válido)</returns>
    Task<List<string>> ValidarDocumentoAsync(Guid documentoId);
}
