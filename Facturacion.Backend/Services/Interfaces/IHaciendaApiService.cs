using Facturacion.Shared.DTOs;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para comunicación con la API de Hacienda (ATV)
/// </summary>
public interface IHaciendaApiService
{
    /// <summary>
    /// Envía un documento firmado a Hacienda usando autenticación Basic (método legacy)
    /// </summary>
    /// <param name="clave">Clave del documento (50 dígitos)</param>
    /// <param name="xmlFirmado">XML firmado digitalmente</param>
    /// <param name="atvUsername">Usuario ATV</param>
    /// <param name="atvPassword">Contraseña ATV</param>
    /// <param name="ambiente">Ambiente (stag o prod)</param>
    /// <returns>Respuesta de Hacienda</returns>
    Task<HaciendaRespuesta> EnviarDocumentoAsync(
        string clave,
        string xmlFirmado,
        string atvUsername,
        string atvPassword,
        string ambiente = "stag");

    /// <summary>
    /// Envía un documento firmado a Hacienda usando OAuth2 Bearer token (método recomendado)
    /// </summary>
    /// <param name="clave">Clave del documento (50 dígitos)</param>
    /// <param name="xmlFirmado">XML firmado digitalmente</param>
    /// <param name="empresaId">ID de la empresa (para obtener el token OAuth2)</param>
    /// <param name="ambiente">Ambiente (stag o prod)</param>
    /// <returns>Respuesta de Hacienda</returns>
    Task<HaciendaRespuesta> EnviarDocumentoConTokenAsync(
        string clave,
        string xmlFirmado,
        Guid empresaId,
        string ambiente = "stag");

    /// <summary>
    /// Consulta el estado de un documento en Hacienda usando autenticación Basic (método legacy)
    /// </summary>
    /// <param name="clave">Clave del documento (50 dígitos)</param>
    /// <param name="atvUsername">Usuario ATV</param>
    /// <param name="atvPassword">Contraseña ATV</param>
    /// <param name="ambiente">Ambiente (stag o prod)</param>
    /// <returns>Respuesta de Hacienda con el estado</returns>
    Task<HaciendaRespuesta> ConsultarEstadoAsync(
        string clave,
        string atvUsername,
        string atvPassword,
        string ambiente = "stag");

    /// <summary>
    /// Consulta el estado de un documento en Hacienda usando OAuth2 Bearer token (método recomendado)
    /// </summary>
    /// <param name="clave">Clave del documento (50 dígitos)</param>
    /// <param name="empresaId">ID de la empresa (para obtener el token OAuth2)</param>
    /// <param name="ambiente">Ambiente (stag o prod)</param>
    /// <returns>Respuesta de Hacienda con el estado</returns>
    Task<HaciendaRespuesta> ConsultarEstadoConTokenAsync(
        string clave,
        Guid empresaId,
        string ambiente = "stag");

    /// <summary>
    /// Verifica la conectividad con la API de Hacienda
    /// </summary>
    /// <param name="ambiente">Ambiente (stag o prod)</param>
    /// <returns>True si hay conexión</returns>
    Task<bool> VerificarConexionAsync(string ambiente = "stag");
}
