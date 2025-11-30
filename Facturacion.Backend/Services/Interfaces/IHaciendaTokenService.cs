using Facturacion.Shared.DTOs;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para gestionar tokens OAuth2 de autenticación con Hacienda
/// </summary>
public interface IHaciendaTokenService
{
    /// <summary>
    /// Obtiene un token válido de Hacienda para la empresa especificada.
    /// Si existe un token guardado y no ha expirado, lo retorna.
    /// Si el access token expiró pero el refresh token no, refresca el token.
    /// Si el refresh token también expiró, obtiene un nuevo token usando credenciales.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="ambiente">Ambiente (stag/prod)</param>
    /// <returns>Access token válido</returns>
    Task<string> ObtenerTokenValidoAsync(Guid empresaId, string ambiente);

    /// <summary>
    /// Obtiene un nuevo token de Hacienda usando las credenciales de ATV
    /// (grant_type=password)
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="ambiente">Ambiente (stag/prod)</param>
    /// <returns>Respuesta del IDP de Hacienda</returns>
    Task<HaciendaTokenResponse> ObtenerNuevoTokenAsync(Guid empresaId, string ambiente);

    /// <summary>
    /// Refresca un token existente usando el refresh token
    /// (grant_type=refresh_token)
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="ambiente">Ambiente (stag/prod)</param>
    /// <returns>Respuesta del IDP de Hacienda</returns>
    Task<HaciendaTokenResponse> RefrescarTokenAsync(Guid empresaId, string ambiente);

    /// <summary>
    /// Invalida el token actual de una empresa forzando la obtención de uno nuevo
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="ambiente">Ambiente (stag/prod)</param>
    Task InvalidarTokenAsync(Guid empresaId, string ambiente);
}
