using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para gestionar la recepción de documentos electrónicos de proveedores
/// </summary>
public interface IDocumentoRecepcionService
{
    /// <summary>
    /// Recibe y procesa un documento electrónico XML
    /// </summary>
    /// <param name="xmlContent">Contenido XML del documento recibido</param>
    /// <param name="empresaId">ID de la empresa que recibe el documento</param>
    /// <returns>Resultado del proceso de recepción</returns>
    Task<ResultadoRecepcion> RecibirDocumentoAsync(string xmlContent, Guid empresaId);

    /// <summary>
    /// Busca un documento por su clave
    /// </summary>
    /// <param name="clave">Clave de 50 dígitos del documento</param>
    /// <returns>Documento encontrado o null</returns>
    Task<Documento?> BuscarDocumentoPorClaveAsync(string clave);

    /// <summary>
    /// Verifica si un documento ya fue recibido previamente (evita duplicados)
    /// </summary>
    /// <param name="clave">Clave del documento</param>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>True si el documento ya existe</returns>
    Task<bool> VerificarDuplicadoAsync(string clave, Guid empresaId);

    /// <summary>
    /// Valida un XML antes de recibirlo (pre-validación sin guardar)
    /// </summary>
    /// <param name="xmlContent">Contenido XML a validar</param>
    /// <returns>Resultado de la validación con información parseada</returns>
    Task<ResultadoRecepcion> ValidarXmlAsync(string xmlContent);

    /// <summary>
    /// Obtiene todos los documentos recibidos de una empresa
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <returns>Lista de documentos recibidos</returns>
    Task<IEnumerable<Documento>> ObtenerDocumentosRecibidosAsync(Guid empresaId);
}
