using Facturacion.Shared.DTOs;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para leer correos electrónicos vía IMAP y extraer XMLs de facturas
/// </summary>
public interface IEmailReaderService
{
    /// <summary>
    /// Lee correos no leídos vía IMAP y procesa los XMLs adjuntos
    /// </summary>
    Task<ResultadoLecturaCorreo> LeerCorreosAsync(Guid empresaId);

    /// <summary>
    /// Prueba la conexión IMAP de una empresa
    /// </summary>
    Task<ActionResponse<string>> ProbarConexionImapAsync(Guid empresaId);
}
