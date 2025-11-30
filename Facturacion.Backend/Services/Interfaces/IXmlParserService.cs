using Facturacion.Shared.DTOs;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para parsear y validar XML de documentos electrónicos recibidos
/// Soporta todos los tipos de documentos de Hacienda v4.4
/// </summary>
public interface IXmlParserService
{
    /// <summary>
    /// Parsea un XML recibido y extrae toda la información en un DTO estructurado
    /// </summary>
    /// <param name="xmlContent">Contenido XML completo</param>
    /// <returns>DTO con la información parseada del documento</returns>
    Task<DocumentoRecibido> ParseXmlAsync(string xmlContent);

    /// <summary>
    /// Valida la estructura del XML contra los esquemas XSD de Hacienda
    /// </summary>
    /// <param name="xmlContent">Contenido XML a validar</param>
    /// <returns>True si el XML es válido estructuralmente</returns>
    Task<bool> ValidarEstructuraXmlAsync(string xmlContent);

    /// <summary>
    /// Extrae la clave numérica de 50 dígitos del XML
    /// </summary>
    /// <param name="xmlContent">Contenido XML</param>
    /// <returns>Clave de 50 dígitos</returns>
    string ExtraerClave(string xmlContent);

    /// <summary>
    /// Extrae el tipo de documento del XML
    /// </summary>
    /// <param name="xmlContent">Contenido XML</param>
    /// <returns>Código de tipo de documento (01-10)</returns>
    string ExtraerTipoDocumento(string xmlContent);

    /// <summary>
    /// Extrae el número consecutivo del XML
    /// </summary>
    /// <param name="xmlContent">Contenido XML</param>
    /// <returns>Número consecutivo del documento</returns>
    string ExtraerConsecutivo(string xmlContent);

    /// <summary>
    /// Verifica si el XML corresponde a un tipo de documento válido para recepción
    /// </summary>
    /// <param name="xmlContent">Contenido XML</param>
    /// <returns>True si es FE, ND, NC, TE o FEE</returns>
    bool EsDocumentoRecibible(string xmlContent);
}
