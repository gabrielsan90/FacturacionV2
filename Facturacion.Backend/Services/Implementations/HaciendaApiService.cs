using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de comunicación con la API de Hacienda
/// Utiliza OAuth2 Bearer token para autenticación
/// </summary>
public class HaciendaApiService : IHaciendaApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHaciendaTokenService _tokenService;
    private readonly ILogger<HaciendaApiService> _logger;
    private readonly IConfiguration _configuration;

    // URLs de la API de Hacienda - v1 es la versión correcta para recepción
    private const string UrlRecepcionStag = "https://api-sandbox.comprobanteselectronicos.go.cr/recepcion/v1/recepcion";
    private const string UrlRecepcionProd = "https://api.comprobanteselectronicos.go.cr/recepcion/v1/recepcion";
    private const string UrlConsultaStag = "https://api-sandbox.comprobanteselectronicos.go.cr/recepcion/v1/recepcion";
    private const string UrlConsultaProd = "https://api.comprobanteselectronicos.go.cr/recepcion/v1/recepcion";

    public HaciendaApiService(
        IHttpClientFactory httpClientFactory,
        IHaciendaTokenService tokenService,
        ILogger<HaciendaApiService> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Envía un documento a Hacienda usando OAuth2 Bearer token
    /// Maneja correctamente los códigos HTTP según la especificación de Hacienda
    /// </summary>
    public async Task<HaciendaRespuesta> EnviarDocumentoAsync(
        string clave,
        string xmlFirmado,
        string atvUsername,
        string atvPassword,
        string ambiente = "stag")
    {
        try
        {
            _logger.LogInformation("Enviando documento a Hacienda. Clave: {Clave}, Ambiente: {Ambiente}",
                clave, ambiente);

            // Obtener URL según ambiente
            var url = ambiente.ToLower() == "prod" ? UrlRecepcionProd : UrlRecepcionStag;

            // Convertir XML a Base64
            var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlFirmado));

            // Extraer información del emisor y receptor de la clave (50 dígitos)
            // Formato clave: PAIS(3) DIA(2) MES(2) AÑO(2) TIPOCED_EMISOR(2) CEDULA_EMISOR(12)
            //               SITUACION(1) CODSUCURSAL(3) TERMINAL(5) TIPODOC(2) CONSECUTIVO(10) CODSEG(8)
            var tipoIdEmisor = clave.Substring(9, 2);
            var numeroIdEmisor = clave.Substring(11, 12).TrimStart('0');

            // Preparar payload JSON según especificación de Hacienda
            // IMPORTANTE: Incluir receptor si existe en el documento
            var payload = new
            {
                clave = clave,
                fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                emisor = new
                {
                    tipoIdentificacion = tipoIdEmisor,
                    numeroIdentificacion = numeroIdEmisor
                },
                comprobanteXml = xmlBase64
                // NOTA: El receptor se debe incluir si está disponible, pero no es obligatorio para todos los tipos de documento
            };

            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogDebug("Payload enviado a Hacienda: {Payload}", jsonPayload);

            // Crear HttpClient
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("HaciendaApi:Timeout", 30));

            // Crear request
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            // NOTA IMPORTANTE: Según el código de ejemplo, primero se debe obtener el token de Hacienda
            // Sin embargo, en este método actual recibimos usuario/password de ATV
            // La nueva versión debería recibir el empresaId y obtener el token automáticamente
            // Por ahora mantenemos compatibilidad hacia atrás usando Basic auth
            // TODO: Migrar a usar solo el token service
            var authBytes = Encoding.ASCII.GetBytes($"{atvUsername}:{atvPassword}");
            var authHeader = Convert.ToBase64String(authBytes);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            // Enviar request
            var response = await httpClient.SendAsync(request);

            // Leer respuesta
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Respuesta de Hacienda. Status: {StatusCode}, Content: {Content}",
                response.StatusCode, responseContent);

            // Procesar respuesta según código HTTP
            return await ProcesarRespuestaHaciendaAsync(clave, response, responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión con Hacienda");
            return new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "error",
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "HTTP_ERROR",
                        Mensaje = "Error de conexión con Hacienda",
                        Detalle = ex.Message,
                        Tipo = "error"
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al enviar documento a Hacienda");
            return new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "error",
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "INTERNAL_ERROR",
                        Mensaje = "Error interno al procesar la solicitud",
                        Detalle = ex.Message,
                        Tipo = "error"
                    }
                }
            };
        }
    }

    /// <summary>
    /// Envía un documento a Hacienda usando OAuth2 Bearer token (método recomendado)
    /// Este es el método correcto según la especificación actual de Hacienda
    /// </summary>
    public async Task<HaciendaRespuesta> EnviarDocumentoConTokenAsync(
        string clave,
        string xmlFirmado,
        Guid empresaId,
        string ambiente = "stag")
    {
        try
        {
            _logger.LogInformation("Enviando documento a Hacienda con OAuth2. Clave: {Clave}, Ambiente: {Ambiente}, EmpresaId: {EmpresaId}",
                clave, ambiente, empresaId);

            // 1. Obtener token válido de Hacienda usando el servicio de tokens
            var token = await _tokenService.ObtenerTokenValidoAsync(empresaId, ambiente);

            _logger.LogDebug("Token OAuth2 obtenido exitosamente para empresa {EmpresaId}", empresaId);

            // 2. Obtener URL según ambiente
            var url = ambiente.ToLower() == "prod" ? UrlRecepcionProd : UrlRecepcionStag;

            // 3. Convertir XML a Base64
            var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlFirmado));

            // 4. Extraer información del emisor de la clave (50 dígitos)
            var tipoIdEmisor = clave.Substring(9, 2);
            var numeroIdEmisor = clave.Substring(11, 12).TrimStart('0');

            // 5. Preparar payload JSON según especificación de Hacienda
            var payload = new
            {
                clave = clave,
                fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                emisor = new
                {
                    tipoIdentificacion = tipoIdEmisor,
                    numeroIdentificacion = numeroIdEmisor
                },
                comprobanteXml = xmlBase64
            };

            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogDebug("Payload JSON preparado para envío a Hacienda");

            // 6. Crear HttpClient
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("HaciendaApi:Timeout", 30));

            // 7. Crear request con OAuth2 Bearer token
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            // IMPORTANTE: Usar Bearer token (OAuth2) en lugar de Basic auth
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _logger.LogDebug("Request configurado con Bearer token. Enviando a {Url}", url);

            // 8. Enviar request
            var response = await httpClient.SendAsync(request);

            // 9. Leer respuesta
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Respuesta de Hacienda recibida. Status: {StatusCode}",
                response.StatusCode);

            // 10. Procesar respuesta según código HTTP
            return await ProcesarRespuestaHaciendaAsync(clave, response, responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión con Hacienda al enviar documento");
            return new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "error",
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "HTTP_ERROR",
                        Mensaje = "Error de conexión con Hacienda",
                        Detalle = ex.Message,
                        Tipo = "error"
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al enviar documento a Hacienda con token");
            return new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "error",
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "INTERNAL_ERROR",
                        Mensaje = "Error interno al procesar la solicitud",
                        Detalle = ex.Message,
                        Tipo = "error"
                    }
                }
            };
        }
    }

    /// <summary>
    /// Consulta el estado de un documento en Hacienda usando OAuth2 Bearer token (método recomendado)
    /// </summary>
    public async Task<HaciendaRespuesta> ConsultarEstadoConTokenAsync(
        string clave,
        Guid empresaId,
        string ambiente = "stag")
    {
        try
        {
            _logger.LogInformation("Consultando estado en Hacienda con OAuth2. Clave: {Clave}, Ambiente: {Ambiente}",
                clave, ambiente);

            // 1. Obtener token válido
            var token = await _tokenService.ObtenerTokenValidoAsync(empresaId, ambiente);

            // 2. Obtener URL según ambiente
            var baseUrl = ambiente.ToLower() == "prod" ? UrlConsultaProd : UrlConsultaStag;
            var url = $"{baseUrl}/{clave}";

            // 3. Crear HttpClient
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("HaciendaApi:Timeout", 30));

            // 4. Crear request con Bearer token
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 5. Enviar request
            var response = await httpClient.SendAsync(request);

            // 6. Leer respuesta
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Respuesta de consulta recibida. Status: {StatusCode}",
                response.StatusCode);

            // 7. Procesar respuesta
            return await ProcesarRespuestaHaciendaAsync(clave, response, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar estado en Hacienda con token");
            return new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "error",
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "CONSULTA_ERROR",
                        Mensaje = "Error al consultar el estado",
                        Detalle = ex.Message,
                        Tipo = "error"
                    }
                }
            };
        }
    }

    /// <summary>
    /// Consulta el estado de un documento en Hacienda (método legacy con Basic auth)
    /// </summary>
    public async Task<HaciendaRespuesta> ConsultarEstadoAsync(
        string clave,
        string atvUsername,
        string atvPassword,
        string ambiente = "stag")
    {
        try
        {
            _logger.LogInformation("Consultando estado en Hacienda (Basic auth). Clave: {Clave}, Ambiente: {Ambiente}",
                clave, ambiente);

            // Obtener URL según ambiente
            var baseUrl = ambiente.ToLower() == "prod" ? UrlConsultaProd : UrlConsultaStag;
            var url = $"{baseUrl}/{clave}";

            // Crear HttpClient
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("HaciendaApi:Timeout", 30));

            // Crear request
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Agregar autenticación Basic
            var authBytes = Encoding.ASCII.GetBytes($"{atvUsername}:{atvPassword}");
            var authHeader = Convert.ToBase64String(authBytes);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            // Enviar request
            var response = await httpClient.SendAsync(request);

            // Leer respuesta
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Respuesta de Hacienda. Status: {StatusCode}",
                response.StatusCode);

            // Procesar respuesta usando el nuevo método
            return await ProcesarRespuestaHaciendaAsync(clave, response, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar estado en Hacienda");
            return new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "error",
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "CONSULTA_ERROR",
                        Mensaje = "Error al consultar el estado",
                        Detalle = ex.Message,
                        Tipo = "error"
                    }
                }
            };
        }
    }

    /// <summary>
    /// Verifica conectividad con Hacienda
    /// </summary>
    public async Task<bool> VerificarConexionAsync(string ambiente = "stag")
    {
        try
        {
            var url = ambiente.ToLower() == "prod" ? UrlRecepcionProd : UrlRecepcionStag;

            // Crear HttpClient
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5); // Timeout corto para verificación

            // Intentar hacer un HEAD request
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await httpClient.SendAsync(request);

            // Si no da error de red, hay conexión (aunque devuelva 401 por falta de auth)
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar conexión con Hacienda");
            return false;
        }
    }

    #region Helpers

    /// <summary>
    /// Procesa la respuesta HTTP de Hacienda según los códigos de estado
    /// Maneja correctamente: 200, 201, 202, 400, 401, 403, 404, 429, 50x
    /// Extrae el header X-Error-Cause en caso de error
    /// Para códigos 200/202, parsea el XML de respuesta de Hacienda
    /// </summary>
    private Task<HaciendaRespuesta> ProcesarRespuestaHaciendaAsync(
        string clave,
        HttpResponseMessage response,
        string responseContent)
    {
        var statusCode = (int)response.StatusCode;

        // CASO EXITOSO: 200 OK - Documento consultado con respuesta de Hacienda
        // Este código se recibe al consultar el estado de un documento que ya fue procesado
        if (statusCode == 200)
        {
            _logger.LogInformation("Consulta exitosa. Documento procesado por Hacienda. Clave: {Clave}", clave);

            try
            {
                // Deserializar respuesta JSON de Hacienda
                var consultaRespuesta = JsonSerializer.Deserialize<HaciendaConsultaRespuesta>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (consultaRespuesta == null)
                {
                    _logger.LogError("No se pudo deserializar la respuesta JSON de Hacienda");
                    return Task.FromResult(CrearRespuestaError(clave, "JSON_ERROR", "Error al procesar la respuesta de Hacienda", responseContent));
                }

                // Si hay respuesta XML, decodificarla y parsearla
                if (!string.IsNullOrWhiteSpace(consultaRespuesta.RespuestaXml))
                {
                    var mensajeXml = ParsearXmlRespuestaHacienda(consultaRespuesta.RespuestaXml);

                    if (mensajeXml != null)
                    {
                        // Determinar el estado basado en el código del mensaje
                        string indEstado = mensajeXml.Mensaje switch
                        {
                            "1" => "aceptado",
                            "2" => "aceptado_parcial",
                            "3" => "rechazado",
                            _ => consultaRespuesta.IndEstado
                        };

                        return Task.FromResult(new HaciendaRespuesta
                        {
                            Clave = clave,
                            Fecha = DateTime.Now,
                            IndEstado = indEstado,
                            RespuestaXml = consultaRespuesta.RespuestaXml, // XML en Base64
                            Mensajes = new List<HaciendaMensaje>
                            {
                                new HaciendaMensaje
                                {
                                    Codigo = mensajeXml.Mensaje,
                                    Mensaje = mensajeXml.EstadoTexto,
                                    Detalle = mensajeXml.DetalleMensaje,
                                    Tipo = mensajeXml.EsRechazado ? "error" : "info"
                                }
                            }
                        });
                    }
                }

                // Si no hay XML o no se pudo parsear, retornar información básica
                return Task.FromResult(new HaciendaRespuesta
                {
                    Clave = clave,
                    Fecha = DateTime.Now,
                    IndEstado = consultaRespuesta.IndEstado,
                    RespuestaXml = consultaRespuesta.RespuestaXml,
                    Mensajes = new List<HaciendaMensaje>
                    {
                        new HaciendaMensaje
                        {
                            Codigo = "200",
                            Mensaje = $"Documento en estado: {consultaRespuesta.IndEstado}",
                            Tipo = "info"
                        }
                    }
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar respuesta JSON de Hacienda");
                return Task.FromResult(CrearRespuestaError(clave, "JSON_ERROR", "Error al procesar JSON de Hacienda", ex.Message));
            }
        }

        // CASO EXITOSO: 201 Created - Documento enviado y creado en Hacienda
        if (statusCode == 201)
        {
            _logger.LogInformation("Documento enviado exitosamente. Status: 201 Created. Clave: {Clave}", clave);

            return Task.FromResult(new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "enviado",
                RespuestaXml = responseContent,
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "201",
                        Mensaje = "Documento creado exitosamente en Hacienda",
                        Tipo = "info"
                    }
                }
            });
        }

        // CASO EXITOSO: 202 Accepted - Documento aceptado para procesamiento
        if (statusCode == 202)
        {
            _logger.LogInformation("Documento aceptado para procesamiento. Status: 202 Accepted. Clave: {Clave}", clave);

            // Intentar parsear respuesta JSON si existe
            try
            {
                var consultaRespuesta = JsonSerializer.Deserialize<HaciendaConsultaRespuesta>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (consultaRespuesta != null && !string.IsNullOrWhiteSpace(consultaRespuesta.RespuestaXml))
                {
                    var mensajeXml = ParsearXmlRespuestaHacienda(consultaRespuesta.RespuestaXml);

                    if (mensajeXml != null)
                    {
                        string indEstado = mensajeXml.Mensaje switch
                        {
                            "1" => "aceptado",
                            "2" => "aceptado_parcial",
                            "3" => "rechazado",
                            _ => "procesando"
                        };

                        return Task.FromResult(new HaciendaRespuesta
                        {
                            Clave = clave,
                            Fecha = DateTime.Now,
                            IndEstado = indEstado,
                            RespuestaXml = consultaRespuesta.RespuestaXml,
                            Mensajes = new List<HaciendaMensaje>
                            {
                                new HaciendaMensaje
                                {
                                    Codigo = mensajeXml.Mensaje,
                                    Mensaje = mensajeXml.EstadoTexto,
                                    Detalle = mensajeXml.DetalleMensaje,
                                    Tipo = mensajeXml.EsRechazado ? "error" : "info"
                                }
                            }
                        });
                    }
                }
            }
            catch (JsonException)
            {
                // Si no se puede parsear JSON, continuar con respuesta básica
            }

            return Task.FromResult(new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "procesando",
                RespuestaXml = responseContent,
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "202",
                        Mensaje = "Documento aceptado para procesamiento",
                        Tipo = "info"
                    }
                }
            });
        }

        // CASO ERROR: 400 Bad Request
        // Error en el formato o validación del documento
        if (statusCode == 400)
        {
            var errorCause = ObtenerHeaderXErrorCause(response);
            _logger.LogWarning("Error 400 Bad Request. Clave: {Clave}, X-Error-Cause: {ErrorCause}",
                clave, errorCause);

            return Task.FromResult(new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "rechazado",
                RespuestaXml = responseContent,
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "400",
                        Mensaje = "Error de validación en el documento",
                        Detalle = errorCause ?? responseContent,
                        Tipo = "error"
                    }
                }
            });
        }

        // CASO ERROR: 401 Unauthorized
        // Token inválido o expirado
        if (statusCode == 401)
        {
            var errorCause = ObtenerHeaderXErrorCause(response);
            _logger.LogWarning("Error 401 Unauthorized. Clave: {Clave}, X-Error-Cause: {ErrorCause}",
                clave, errorCause);

            return Task.FromResult(new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "error",
                RespuestaXml = responseContent,
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "401",
                        Mensaje = "Token de autenticación inválido o expirado",
                        Detalle = errorCause ?? "Debe obtener un nuevo token",
                        Tipo = "error"
                    }
                }
            });
        }

        // CASO ERROR: 403 Forbidden
        // Acceso bloqueado (usuario bloqueado por demasiados intentos fallidos)
        if (statusCode == 403)
        {
            var errorCause = ObtenerHeaderXErrorCause(response);
            _logger.LogWarning("Error 403 Forbidden. Clave: {Clave}, X-Error-Cause: {ErrorCause}",
                clave, errorCause);

            return Task.FromResult(new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "error",
                RespuestaXml = responseContent,
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "403",
                        Mensaje = "Acceso bloqueado por Hacienda",
                        Detalle = errorCause ?? "Usuario bloqueado temporalmente por intentos fallidos",
                        Tipo = "error"
                    }
                }
            });
        }

        // CASO ESPECIAL: 404 Not Found
        // El documento NO existe en Hacienda
        // Esto puede significar que nunca se envió o que aún no está registrado
        // Se puede considerar para reenvío automático
        if (statusCode == 404)
        {
            var errorCause = ObtenerHeaderXErrorCause(response);
            _logger.LogWarning("Error 404 Not Found. Documento no existe en Hacienda. Clave: {Clave}, X-Error-Cause: {ErrorCause}",
                clave, errorCause);

            return Task.FromResult(new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "no_encontrado",
                RespuestaXml = responseContent,
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "404",
                        Mensaje = "Documento no encontrado en Hacienda",
                        Detalle = errorCause ?? "El documento no existe en Hacienda. Puede ser necesario reenviarlo.",
                        Tipo = "advertencia"
                    }
                }
            });
        }

        // CASO ERROR: 429 Too Many Requests
        // Rate limit excedido
        if (statusCode == 429)
        {
            var errorCause = ObtenerHeaderXErrorCause(response);
            _logger.LogWarning("Error 429 Too Many Requests. X-Error-Cause: {ErrorCause}", errorCause);

            return Task.FromResult(new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "error",
                RespuestaXml = responseContent,
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = "429",
                        Mensaje = "Límite de peticiones excedido",
                        Detalle = errorCause ?? "Demasiadas peticiones en poco tiempo. Espere antes de reintentar.",
                        Tipo = "error"
                    }
                }
            });
        }

        // CASO ERROR: 50x Server Error
        // Error del servidor de Hacienda
        if (statusCode >= 500 && statusCode < 600)
        {
            var errorCause = ObtenerHeaderXErrorCause(response);
            _logger.LogError("Error {StatusCode} del servidor de Hacienda. X-Error-Cause: {ErrorCause}",
                statusCode, errorCause);

            return Task.FromResult(new HaciendaRespuesta
            {
                Clave = clave,
                Fecha = DateTime.Now,
                IndEstado = "error",
                RespuestaXml = responseContent,
                Mensajes = new List<HaciendaMensaje>
                {
                    new HaciendaMensaje
                    {
                        Codigo = statusCode.ToString(),
                        Mensaje = "Error del servidor de Hacienda",
                        Detalle = errorCause ?? $"Error {statusCode}: {responseContent}",
                        Tipo = "error"
                    }
                }
            });
        }

        // CASO OTROS ERRORES
        var errorCauseOtros = ObtenerHeaderXErrorCause(response);
        _logger.LogWarning("Código HTTP no manejado: {StatusCode}. X-Error-Cause: {ErrorCause}",
            statusCode, errorCauseOtros);

        return Task.FromResult(new HaciendaRespuesta
        {
            Clave = clave,
            Fecha = DateTime.Now,
            IndEstado = "error",
            RespuestaXml = responseContent,
            Mensajes = new List<HaciendaMensaje>
            {
                new HaciendaMensaje
                {
                    Codigo = statusCode.ToString(),
                    Mensaje = $"Código HTTP no esperado: {statusCode}",
                    Detalle = errorCauseOtros ?? responseContent,
                    Tipo = "error"
                }
            }
        });
    }

    /// <summary>
    /// Extrae el valor del header X-Error-Cause de la respuesta HTTP
    /// Este header contiene el mensaje de error detallado de Hacienda
    /// </summary>
    private string? ObtenerHeaderXErrorCause(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("X-Error-Cause", out var values))
        {
            return string.Join("; ", values);
        }

        // Algunos servidores pueden usar minúsculas
        if (response.Headers.TryGetValues("x-error-cause", out var valuesLower))
        {
            return string.Join("; ", valuesLower);
        }

        return null;
    }

    /// <summary>
    /// Parsea el XML de respuesta de Hacienda que viene codificado en Base64
    /// Estructura esperada: MensajeHacienda con campos Mensaje y DetalleMensaje
    /// </summary>
    /// <param name="xmlBase64">XML codificado en Base64</param>
    /// <returns>DTO con los datos parseados o null si hay error</returns>
    private HaciendaMensajeRespuestaXml? ParsearXmlRespuestaHacienda(string xmlBase64)
    {
        try
        {
            // Decodificar de Base64
            var xmlBytes = Convert.FromBase64String(xmlBase64);
            var xmlString = Encoding.UTF8.GetString(xmlBytes);

            _logger.LogDebug("XML decodificado de Hacienda: {Xml}", xmlString);

            // Parsear XML
            var xdoc = XDocument.Parse(xmlString);
            var root = xdoc.Root;

            if (root == null || root.Name.LocalName != "MensajeHacienda")
            {
                _logger.LogWarning("El XML no tiene la estructura esperada (MensajeHacienda)");
                return null;
            }

            // Extraer campos del XML
            var mensajeXml = new HaciendaMensajeRespuestaXml
            {
                Clave = root.Element("Clave")?.Value ?? string.Empty,
                NombreEmisor = root.Element("NombreEmisor")?.Value ?? string.Empty,
                TipoIdentificacionEmisor = root.Element("TipoIdentificacionEmisor")?.Value ?? string.Empty,
                NumeroCedulaEmisor = root.Element("NumeroCedulaEmisor")?.Value ?? string.Empty,
                Mensaje = root.Element("Mensaje")?.Value ?? string.Empty,
                DetalleMensaje = root.Element("DetalleMensaje")?.Value ?? string.Empty
            };

            // Extraer montos (opcionales)
            if (decimal.TryParse(root.Element("MontoTotalImpuesto")?.Value, out var montoImpuesto))
            {
                mensajeXml.MontoTotalImpuesto = montoImpuesto;
            }

            if (decimal.TryParse(root.Element("TotalFactura")?.Value, out var totalFactura))
            {
                mensajeXml.TotalFactura = totalFactura;
            }

            _logger.LogInformation("XML parseado exitosamente. Mensaje: {Mensaje} ({EstadoTexto}), Detalle: {Detalle}",
                mensajeXml.Mensaje, mensajeXml.EstadoTexto, mensajeXml.DetalleMensaje);

            return mensajeXml;
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Error al decodificar Base64 del XML de respuesta");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al parsear XML de respuesta de Hacienda");
            return null;
        }
    }

    /// <summary>
    /// Crea una respuesta de error genérica
    /// </summary>
    private HaciendaRespuesta CrearRespuestaError(string clave, string codigo, string mensaje, string detalle)
    {
        return new HaciendaRespuesta
        {
            Clave = clave,
            Fecha = DateTime.Now,
            IndEstado = "error",
            Mensajes = new List<HaciendaMensaje>
            {
                new HaciendaMensaje
                {
                    Codigo = codigo,
                    Mensaje = mensaje,
                    Detalle = detalle,
                    Tipo = "error"
                }
            }
        };
    }

    #endregion
}
