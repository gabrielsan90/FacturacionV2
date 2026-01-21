using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para comunicacion con la API de Hacienda (ATV)
/// </summary>
public interface IHaciendaApiService
{
    /// <summary>
    /// Envia un documento firmado a Hacienda usando autenticacion Basic (metodo legacy)
    /// </summary>
    /// <param name="clave">Clave del documento (50 digitos)</param>
    /// <param name="xmlFirmado">XML firmado digitalmente</param>
    /// <param name="atvUsername">Usuario ATV</param>
    /// <param name="atvPassword">Contrasena ATV</param>
    /// <param name="ambiente">Ambiente (stag o prod)</param>
    /// <returns>Respuesta de Hacienda</returns>
    Task<HaciendaRespuesta> EnviarDocumentoAsync(
        string clave,
        string xmlFirmado,
        string atvUsername,
        string atvPassword,
        string ambiente = "stag");

    /// <summary>
    /// Envia un documento firmado a Hacienda usando OAuth2 Bearer token (metodo recomendado)
    /// </summary>
    /// <param name="clave">Clave del documento (50 digitos)</param>
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
    /// Envia un documento firmado a Hacienda con informacion completa del emisor y receptor
    /// Este es el metodo preferido ya que obtiene el tipo de identificacion de las entidades
    /// en lugar de extraerlo incorrectamente de la clave
    /// </summary>
    /// <param name="clave">Clave del documento (50 digitos)</param>
    /// <param name="xmlFirmado">XML firmado digitalmente</param>
    /// <param name="empresa">Entidad Empresa con datos del emisor</param>
    /// <param name="receptor">Cliente receptor (puede ser null)</param>
    /// <param name="ambiente">Ambiente (stag o prod)</param>
    /// <returns>Respuesta de Hacienda</returns>
    Task<HaciendaRespuesta> EnviarDocumentoConEmisorReceptorAsync(
        string clave,
        string xmlFirmado,
        Empresa empresa,
        Cliente? receptor,
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
