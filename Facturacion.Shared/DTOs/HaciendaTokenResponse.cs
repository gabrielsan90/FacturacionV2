using System.Text.Json.Serialization;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// Respuesta del endpoint de token OAuth2 de Hacienda
/// </summary>
public class HaciendaTokenResponse
{
    /// <summary>
    /// Token de acceso (JWT)
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Tiempo de expiración del access token en segundos (típicamente 300 segundos = 5 minutos)
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Tiempo de expiración del refresh token en segundos (típicamente 1800 segundos = 30 minutos)
    /// </summary>
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    /// <summary>
    /// Token para refrescar el access token
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// Tipo de token (siempre "bearer")
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = null!;

    /// <summary>
    /// Scope del token (opcional)
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// Session state (opcional)
    /// </summary>
    [JsonPropertyName("session_state")]
    public string? SessionState { get; set; }
}
