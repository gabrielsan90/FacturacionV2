using Facturacion.Shared.DTOs;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para validar documentos de exoneración
/// API oficial de Hacienda: https://api.hacienda.go.cr/fe/ex
/// Implementa caching para mejorar rendimiento
/// </summary>
public interface IExoneracionService
{
    /// <summary>
    /// Valida que un documento de exoneración existe y está vigente
    /// </summary>
    /// <param name="numeroDocumento">Número del documento de autorización</param>
    /// <param name="nombreInstitucion">Nombre de la institución emisora</param>
    /// <param name="fechaValidacion">Fecha en la que se desea validar (default: fecha actual)</param>
    /// <returns>Resultado de la validación con información detallada</returns>
    Task<ExoneracionValidacionRespuestaDTO> ValidarExoneracionAsync(
        string numeroDocumento,
        string nombreInstitucion,
        DateTime? fechaValidacion = null);

    /// <summary>
    /// Verifica si una exoneración está vigente en una fecha específica
    /// </summary>
    /// <param name="numeroDocumento">Número del documento de autorización</param>
    /// <param name="nombreInstitucion">Nombre de la institución emisora</param>
    /// <param name="fecha">Fecha a validar (default: fecha actual)</param>
    /// <returns>True si está vigente, False en caso contrario</returns>
    Task<bool> EstaVigenteAsync(string numeroDocumento, string nombreInstitucion, DateTime? fecha = null);

    /// <summary>
    /// Obtiene información detallada de una exoneración
    /// </summary>
    /// <param name="numeroDocumento">Número del documento de autorización</param>
    /// <param name="nombreInstitucion">Nombre de la institución emisora</param>
    /// <returns>Información de la exoneración o null si no existe</returns>
    Task<ExoneracionDTO?> ObtenerExoneracionAsync(string numeroDocumento, string nombreInstitucion);

    /// <summary>
    /// Verifica si un beneficiario tiene exoneraciones vigentes
    /// </summary>
    /// <param name="identificacion">Identificación del beneficiario</param>
    /// <returns>Lista de exoneraciones vigentes del beneficiario</returns>
    Task<List<ExoneracionDTO>> ObtenerExoneracionesPorBeneficiarioAsync(string identificacion);

    /// <summary>
    /// Limpia la caché de exoneraciones
    /// Útil cuando se necesita forzar la actualización de datos
    /// </summary>
    void LimpiarCache();
}
