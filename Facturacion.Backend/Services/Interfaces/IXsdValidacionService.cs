using Facturacion.Shared.DTOs;
using Facturacion.Shared.Enums;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para validar documentos XML contra esquemas XSD v4.4 de Hacienda
/// </summary>
public interface IXsdValidacionService
{
    /// <summary>
    /// Valida un XML generado contra el esquema XSD correspondiente según el tipo de documento
    /// </summary>
    /// <param name="xml">XML a validar</param>
    /// <param name="tipoDocumento">Tipo de documento electrónico</param>
    /// <returns>Resultado de validación con lista de errores (vacía si es válido)</returns>
    Task<ResultadoValidacionXsd> ValidarXmlContraXsdAsync(string xml, DocumentoTipo tipoDocumento);

    /// <summary>
    /// Obtiene la ruta del archivo XSD correspondiente al tipo de documento
    /// </summary>
    /// <param name="tipoDocumento">Tipo de documento</param>
    /// <returns>Ruta completa al archivo XSD</returns>
    string ObtenerRutaXsd(DocumentoTipo tipoDocumento);

    /// <summary>
    /// Valida que todos los archivos XSD necesarios existan
    /// </summary>
    /// <returns>True si todos los archivos XSD existen</returns>
    bool ValidarExistenciaEsquemasXsd();
}
