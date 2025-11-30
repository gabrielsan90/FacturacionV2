using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio para gestionar tokens OAuth2 de autenticación con Hacienda
/// Implementa la lógica de obtención, refresco y almacenamiento de tokens
/// </summary>
public class HaciendaTokenService : IHaciendaTokenService
{
    private readonly DataContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HaciendaTokenService> _logger;
    private readonly IConfiguration _configuration;

    // URLs de IDP de Hacienda
    private const string UrlIdpStag = "https://idp.comprobanteselectronicos.go.cr/auth/realms/rut-stag/protocol/openid-connect/token";
    private const string UrlIdpProd = "https://idp.comprobanteselectronicos.go.cr/auth/realms/rut/protocol/openid-connect/token";

    public HaciendaTokenService(
        DataContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<HaciendaTokenService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Obtiene un token válido de Hacienda para la empresa especificada
    /// Implementa la lógica de verificación de expiración y refresco automático
    /// </summary>
    public async Task<string> ObtenerTokenValidoAsync(Guid empresaId, string ambiente)
    {
        try
        {
            _logger.LogInformation("Obteniendo token válido para empresa {EmpresaId}, ambiente {Ambiente}",
                empresaId, ambiente);

            // Buscar token existente en BD
            var tokenExistente = await _context.Set<HaciendaToken>()
                .Where(t => t.EmpresaId == empresaId && t.Ambiente == ambiente && t.Activo)
                .OrderByDescending(t => t.FechaCreacion)
                .FirstOrDefaultAsync();

            var ahora = DateTime.Now;

            // CASO 1: No hay token -> Obtener nuevo token
            if (tokenExistente == null)
            {
                _logger.LogInformation("No existe token almacenado. Obteniendo nuevo token...");
                var nuevoToken = await ObtenerNuevoTokenAsync(empresaId, ambiente);
                return nuevoToken.AccessToken;
            }

            // CASO 2: Refresh token expiró -> Obtener nuevo token
            if (ahora >= tokenExistente.FechaExpiracionRefreshToken)
            {
                _logger.LogInformation("Refresh token expirado. Obteniendo nuevo token...");

                // Invalidar token anterior
                tokenExistente.Activo = false;
                await _context.SaveChangesAsync();

                var nuevoToken = await ObtenerNuevoTokenAsync(empresaId, ambiente);
                return nuevoToken.AccessToken;
            }

            // CASO 3: Access token expiró pero refresh token válido -> Refrescar token
            if (ahora >= tokenExistente.FechaExpiracionToken)
            {
                _logger.LogInformation("Access token expirado. Refrescando token...");
                var tokenRefrescado = await RefrescarTokenAsync(empresaId, ambiente);
                return tokenRefrescado.AccessToken;
            }

            // CASO 4: Token válido -> Retornar token existente
            _logger.LogInformation("Token válido encontrado. Expira en {Minutos} minutos",
                (tokenExistente.FechaExpiracionToken - ahora).TotalMinutes);

            return tokenExistente.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener token válido para empresa {EmpresaId}", empresaId);
            throw new InvalidOperationException($"No se pudo obtener un token válido: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Obtiene un nuevo token usando credenciales de ATV (grant_type=password)
    /// </summary>
    public async Task<HaciendaTokenResponse> ObtenerNuevoTokenAsync(Guid empresaId, string ambiente)
    {
        try
        {
            _logger.LogInformation("Solicitando nuevo token para empresa {EmpresaId}, ambiente {Ambiente}",
                empresaId, ambiente);

            // Obtener empresa con credenciales
            var empresa = await _context.Set<Empresa>()
                .FirstOrDefaultAsync(e => e.Id == empresaId);

            if (empresa == null)
            {
                throw new InvalidOperationException($"No se encontró la empresa {empresaId}");
            }

            if (string.IsNullOrWhiteSpace(empresa.UsuarioHacienda) ||
                string.IsNullOrWhiteSpace(empresa.ClaveHacienda))
            {
                throw new InvalidOperationException("La empresa no tiene configuradas las credenciales de Hacienda (Usuario y Clave ATV)");
            }

            // Determinar URL y client_id según ambiente
            var url = ambiente.ToLower() == "prod" ? UrlIdpProd : UrlIdpStag;
            var clientId = ambiente.ToLower() == "prod" ? "api-prod" : "api-stag";

            // Preparar parámetros para grant_type=password
            var parametros = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "password" },
                { "username", empresa.UsuarioHacienda },
                { "password", empresa.ClaveHacienda }
            };

            // Realizar petición
            var tokenResponse = await SolicitarTokenAsync(url, parametros);

            // Guardar token en BD
            await GuardarTokenEnBDAsync(empresaId, ambiente, tokenResponse);

            _logger.LogInformation("Nuevo token obtenido exitosamente para empresa {EmpresaId}", empresaId);

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener nuevo token para empresa {EmpresaId}", empresaId);
            throw new InvalidOperationException($"No se pudo obtener el token de Hacienda: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Refresca un token existente usando refresh token (grant_type=refresh_token)
    /// </summary>
    public async Task<HaciendaTokenResponse> RefrescarTokenAsync(Guid empresaId, string ambiente)
    {
        try
        {
            _logger.LogInformation("Refrescando token para empresa {EmpresaId}, ambiente {Ambiente}",
                empresaId, ambiente);

            // Obtener token existente
            var tokenExistente = await _context.Set<HaciendaToken>()
                .Where(t => t.EmpresaId == empresaId && t.Ambiente == ambiente && t.Activo)
                .OrderByDescending(t => t.FechaCreacion)
                .FirstOrDefaultAsync();

            if (tokenExistente == null)
            {
                throw new InvalidOperationException("No existe un token para refrescar. Obtenga un nuevo token.");
            }

            if (DateTime.Now >= tokenExistente.FechaExpiracionRefreshToken)
            {
                throw new InvalidOperationException("El refresh token ha expirado. Obtenga un nuevo token.");
            }

            // Determinar URL y client_id según ambiente
            var url = ambiente.ToLower() == "prod" ? UrlIdpProd : UrlIdpStag;
            var clientId = ambiente.ToLower() == "prod" ? "api-prod" : "api-stag";

            // Preparar parámetros para grant_type=refresh_token
            var parametros = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "refresh_token" },
                { "refresh_token", tokenExistente.RefreshToken }
            };

            // Realizar petición
            var tokenResponse = await SolicitarTokenAsync(url, parametros);

            // Invalidar token anterior
            tokenExistente.Activo = false;

            // Guardar nuevo token en BD
            await GuardarTokenEnBDAsync(empresaId, ambiente, tokenResponse);

            _logger.LogInformation("Token refrescado exitosamente para empresa {EmpresaId}", empresaId);

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al refrescar token para empresa {EmpresaId}", empresaId);
            throw new InvalidOperationException($"No se pudo refrescar el token: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Invalida el token actual forzando la obtención de uno nuevo
    /// </summary>
    public async Task InvalidarTokenAsync(Guid empresaId, string ambiente)
    {
        try
        {
            _logger.LogInformation("Invalidando token para empresa {EmpresaId}, ambiente {Ambiente}",
                empresaId, ambiente);

            var tokens = await _context.Set<HaciendaToken>()
                .Where(t => t.EmpresaId == empresaId && t.Ambiente == ambiente && t.Activo)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Activo = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Token invalidado para empresa {EmpresaId}", empresaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al invalidar token para empresa {EmpresaId}", empresaId);
            throw;
        }
    }

    #region Métodos Privados

    /// <summary>
    /// Realiza la petición HTTP al IDP de Hacienda para obtener el token
    /// </summary>
    private async Task<HaciendaTokenResponse> SolicitarTokenAsync(string url, Dictionary<string, string> parametros)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Crear contenido con application/x-www-form-urlencoded
            var content = new FormUrlEncodedContent(parametros);

            _logger.LogDebug("Solicitando token a URL: {Url}", url);

            // Realizar petición POST
            var response = await httpClient.PostAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error al solicitar token. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"Error al solicitar token de Hacienda: {response.StatusCode} - {responseContent}");
            }

            // Deserializar respuesta
            var tokenResponse = JsonSerializer.Deserialize<HaciendaTokenResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException("La respuesta de Hacienda no contiene un access_token válido");
            }

            _logger.LogDebug("Token recibido exitosamente. Expira en {ExpiresIn} segundos", tokenResponse.ExpiresIn);

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en la petición HTTP al IDP de Hacienda");
            throw;
        }
    }

    /// <summary>
    /// Guarda el token en la base de datos
    /// </summary>
    private async Task GuardarTokenEnBDAsync(Guid empresaId, string ambiente, HaciendaTokenResponse tokenResponse)
    {
        try
        {
            var ahora = DateTime.Now;

            var haciendaToken = new HaciendaToken
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                FechaExpiracionToken = ahora.AddSeconds(tokenResponse.ExpiresIn),
                FechaExpiracionRefreshToken = ahora.AddSeconds(tokenResponse.RefreshExpiresIn),
                Ambiente = ambiente,
                FechaCreacion = ahora,
                Activo = true
            };

            _context.Set<HaciendaToken>().Add(haciendaToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Token guardado en BD. Empresa: {EmpresaId}, Expira: {Expiracion}",
                empresaId, haciendaToken.FechaExpiracionToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar token en BD");
            throw;
        }
    }

    #endregion
}
