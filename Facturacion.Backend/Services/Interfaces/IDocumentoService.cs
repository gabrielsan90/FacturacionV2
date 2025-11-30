using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio de lógica de negocio para documentos electrónicos
/// Maneja validaciones, cálculos y generación de consecutivos
/// </summary>
public interface IDocumentoService
{
    /// <summary>
    /// Genera el número consecutivo para un documento
    /// Formato: XXX-YYYYY-ZZ-AAAAAAAAAA
    /// </summary>
    /// <param name="terminalId">ID del terminal que emite el documento</param>
    /// <param name="tipoDocumento">Código del tipo de documento (01-10)</param>
    /// <returns>Número consecutivo formateado</returns>
    Task<string> GenerarNumeroConsecutivoAsync(Guid terminalId, string tipoDocumento);

    /// <summary>
    /// Obtiene el siguiente número consecutivo disponible sin incrementarlo
    /// </summary>
    /// <param name="terminalId">ID del terminal</param>
    /// <param name="tipoDocumento">Código del tipo de documento</param>
    /// <returns>Próximo número consecutivo</returns>
    Task<string> ObtenerSiguienteConsecutivoAsync(Guid terminalId, string tipoDocumento);

    /// <summary>
    /// Calcula todos los totales de un documento basándose en sus detalles
    /// </summary>
    /// <param name="documento">Documento a calcular</param>
    void CalcularTotales(Documento documento);

    /// <summary>
    /// Valida que un documento cumpla con todas las reglas de negocio
    /// </summary>
    /// <param name="documento">Documento a validar</param>
    /// <returns>Lista de errores (vacía si es válido)</returns>
    Task<List<string>> ValidarDocumentoAsync(Documento documento);

    /// <summary>
    /// Convierte un DTO de creación en una entidad Documento
    /// </summary>
    /// <param name="dto">DTO con los datos del documento</param>
    /// <param name="userId">ID del usuario que crea el documento</param>
    /// <returns>Entidad Documento lista para persistir</returns>
    Task<Documento> CrearDocumentoDesdeDTO(CreateDocumentoDTO dto, string userId);

    /// <summary>
    /// Actualiza un documento existente con datos del DTO
    /// </summary>
    /// <param name="documento">Documento existente</param>
    /// <param name="dto">DTO con los datos actualizados</param>
    /// <param name="userId">ID del usuario que modifica</param>
    Task ActualizarDocumentoDesdeDTO(Documento documento, UpdateDocumentoDTO dto, string userId);

    /// <summary>
    /// Incrementa el contador de consecutivos del terminal para un tipo de documento específico
    /// </summary>
    /// <param name="terminalId">ID del terminal</param>
    /// <param name="tipoDocumento">Tipo de documento (ej: "01" para factura electrónica)</param>
    /// <returns>True si se incrementó exitosamente</returns>
    Task<bool> IncrementarConsecutivoTerminalAsync(Guid terminalId, string tipoDocumento = "01");

    /// <summary>
    /// Verifica si un terminal ha alcanzado su límite de consecutivos para un tipo de documento
    /// </summary>
    /// <param name="terminalId">ID del terminal</param>
    /// <param name="tipoDocumento">Tipo de documento (ej: "01" para factura electrónica)</param>
    /// <returns>True si alcanzó el límite</returns>
    Task<bool> TerminalAlcanzoLimiteAsync(Guid terminalId, string tipoDocumento = "01");

    /// <summary>
    /// Obtiene el código de tipo de documento en formato de 2 dígitos
    /// </summary>
    /// <param name="tipoDocumento">Enum de tipo de documento</param>
    /// <returns>Código de 2 dígitos (01-10)</returns>
    string ObtenerCodigoTipoDocumento(Shared.Enums.DocumentoTipo tipoDocumento);
}
