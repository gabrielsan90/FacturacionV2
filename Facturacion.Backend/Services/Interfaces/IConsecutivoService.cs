using Facturacion.Shared.Enums;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para gestión de consecutivos de documentos electrónicos
/// según formato de Hacienda de Costa Rica
/// </summary>
public interface IConsecutivoService
{
    /// <summary>
    /// Genera y asigna el siguiente número consecutivo de forma atómica.
    /// Formato: XXX-YYYYY-ZZ-AAAAAAAAAA (con guiones para DB/visualización)
    /// XXX = Código de sucursal (3 dígitos)
    /// YYYYY = Código de terminal (5 dígitos)
    /// ZZ = Tipo de documento (2 dígitos: 01=FE, 02=ND, 03=NC, 04=TE)
    /// AAAAAAAAAA = Consecutivo (10 dígitos)
    /// Los consecutivos son independientes por ambiente (Pruebas vs Producción)
    /// </summary>
    /// <param name="terminalId">ID del terminal</param>
    /// <param name="tipoDocumento">Tipo de documento (enum)</param>
    /// <param name="ambiente">Ambiente de Hacienda (Pruebas o Producción)</param>
    /// <returns>Número consecutivo formateado con guiones</returns>
    Task<string> GenerarYAsignarConsecutivoAsync(Guid terminalId, DocumentoTipo tipoDocumento, Ambiente ambiente);

    /// <summary>
    /// Obtiene el siguiente número consecutivo sin incrementarlo (solo consulta)
    /// </summary>
    /// <param name="terminalId">ID del terminal</param>
    /// <param name="tipoDocumento">Tipo de documento (enum)</param>
    /// <param name="ambiente">Ambiente de Hacienda (Pruebas o Producción)</param>
    /// <returns>Siguiente número consecutivo que se asignará</returns>
    Task<string> ObtenerSiguienteConsecutivoAsync(Guid terminalId, DocumentoTipo tipoDocumento, Ambiente ambiente);

    /// <summary>
    /// Convierte el número consecutivo con guiones al formato numérico de 20 dígitos
    /// para envío a Hacienda (sin guiones)
    /// </summary>
    /// <param name="consecutivoConGuiones">Consecutivo formato: XXX-YYYYY-ZZ-AAAAAAAAAA</param>
    /// <returns>Consecutivo formato numérico: XXXYYYYYZZAAAAAAAAAA (20 dígitos)</returns>
    string ConvertirAFormatoHacienda(string consecutivoConGuiones);

    /// <summary>
    /// Convierte el tipo de documento enum al código de 2 dígitos usado por Hacienda
    /// </summary>
    /// <param name="tipoDocumento">Tipo de documento (enum)</param>
    /// <returns>Código de 2 dígitos (ej: "01", "04", "03")</returns>
    string ObtenerCodigoTipoDocumento(DocumentoTipo tipoDocumento);

    /// <summary>
    /// Verifica si un terminal tiene consecutivos disponibles para un tipo de documento y ambiente
    /// </summary>
    /// <param name="terminalId">ID del terminal</param>
    /// <param name="tipoDocumento">Tipo de documento</param>
    /// <param name="ambiente">Ambiente de Hacienda (Pruebas o Producción)</param>
    /// <returns>True si hay consecutivos disponibles, false si alcanzó el límite</returns>
    Task<bool> TieneConsecutivosDisponiblesAsync(Guid terminalId, DocumentoTipo tipoDocumento, Ambiente ambiente);
}
