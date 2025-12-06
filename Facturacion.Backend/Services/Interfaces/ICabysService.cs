using Facturacion.Shared.DTOs;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para consultar códigos CABYS (Códigos de Actividades, Bienes y Servicios)
/// API oficial de Hacienda: https://api.hacienda.go.cr/fe/cabys
/// Implementa caching para mejorar rendimiento
/// </summary>
public interface ICabysService
{
    /// <summary>
    /// Valida que un código CABYS existe y es válido
    /// </summary>
    /// <param name="codigo">Código CABYS a validar (13 dígitos)</param>
    /// <returns>True si el código es válido, False en caso contrario</returns>
    Task<bool> ValidarCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene información detallada de un código CABYS específico
    /// </summary>
    /// <param name="codigo">Código CABYS (13 dígitos)</param>
    /// <returns>Información del código CABYS o null si no existe</returns>
    Task<CabysDTO?> ObtenerPorCodigoAsync(string codigo);

    /// <summary>
    /// Busca códigos CABYS por descripción
    /// </summary>
    /// <param name="descripcion">Texto a buscar en la descripción</param>
    /// <param name="top">Número máximo de resultados (default: 10)</param>
    /// <returns>Lista de códigos CABYS que coinciden con la búsqueda</returns>
    Task<CabysBusquedaRespuestaDTO> BuscarPorDescripcionAsync(string descripcion, int top = 10);

    /// <summary>
    /// Realiza una búsqueda general de códigos CABYS
    /// </summary>
    /// <param name="parametros">Parámetros de búsqueda</param>
    /// <returns>Lista de códigos CABYS que coinciden con la búsqueda</returns>
    Task<CabysBusquedaRespuestaDTO> BuscarAsync(CabysBusquedaDTO parametros);

    /// <summary>
    /// Obtiene el porcentaje de impuesto aplicable para un código CABYS
    /// </summary>
    /// <param name="codigo">Código CABYS (13 dígitos)</param>
    /// <returns>Porcentaje de impuesto o null si el código no existe</returns>
    Task<decimal?> ObtenerPorcentajeImpuestoAsync(string codigo);

    /// <summary>
    /// Limpia la caché de códigos CABYS
    /// Útil cuando se necesita forzar la actualización de datos
    /// </summary>
    void LimpiarCache();
}
