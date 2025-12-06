using Facturacion.Shared.DTOs;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para consultar actividades económicas (CIIU)
/// API oficial de Hacienda: https://api.hacienda.go.cr/fe/ae
/// Implementa caching para mejorar rendimiento
/// </summary>
public interface IActividadEconomicaService
{
    /// <summary>
    /// Valida que un código de actividad económica existe y está activo
    /// </summary>
    /// <param name="codigo">Código de actividad económica</param>
    /// <returns>True si el código es válido y está activo, False en caso contrario</returns>
    Task<bool> ValidarCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene información detallada de una actividad económica específica
    /// </summary>
    /// <param name="codigo">Código de actividad económica</param>
    /// <returns>Información de la actividad económica o null si no existe</returns>
    Task<ActividadEconomicaDTO?> ObtenerPorCodigoAsync(string codigo);

    /// <summary>
    /// Busca actividades económicas por descripción
    /// </summary>
    /// <param name="descripcion">Texto a buscar en la descripción</param>
    /// <param name="top">Número máximo de resultados (default: 10)</param>
    /// <returns>Lista de actividades económicas que coinciden con la búsqueda</returns>
    Task<ActividadEconomicaBusquedaRespuestaDTO> BuscarPorDescripcionAsync(string descripcion, int top = 10);

    /// <summary>
    /// Realiza una búsqueda general de actividades económicas
    /// </summary>
    /// <param name="parametros">Parámetros de búsqueda</param>
    /// <returns>Lista de actividades económicas que coinciden con la búsqueda</returns>
    Task<ActividadEconomicaBusquedaRespuestaDTO> BuscarAsync(ActividadEconomicaBusquedaDTO parametros);

    /// <summary>
    /// Obtiene todas las actividades económicas activas
    /// Útil para poblar catálogos o combos
    /// </summary>
    /// <returns>Lista de todas las actividades económicas activas</returns>
    Task<List<ActividadEconomicaDTO>> ObtenerTodasActivasAsync();

    /// <summary>
    /// Limpia la caché de actividades económicas
    /// Útil cuando se necesita forzar la actualización de datos
    /// </summary>
    void LimpiarCache();
}
