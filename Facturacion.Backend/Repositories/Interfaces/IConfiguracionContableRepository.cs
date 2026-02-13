using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para acceder a la configuración contable de una empresa.
/// Proporciona acceso a los parámetros de contabilidad y generación automática de asientos.
/// </summary>
public interface IConfiguracionContableRepository
{
    /// <summary>
    /// Obtiene la configuración contable de una empresa.
    /// </summary>
    Task<ConfiguracionContable?> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene una configuración contable por su ID.
    /// </summary>
    Task<ConfiguracionContable?> GetAsync(Guid id);

    /// <summary>
    /// Crea una configuración contable para una empresa.
    /// </summary>
    Task<ConfiguracionContable> AddAsync(ConfiguracionContable configuracion);

    /// <summary>
    /// Actualiza la configuración contable de una empresa.
    /// </summary>
    Task UpdateAsync(ConfiguracionContable configuracion);

    /// <summary>
    /// Verifica si la generación automática de asientos está habilitada para una empresa.
    /// </summary>
    Task<bool> EstaHabilitadaGeneracionAutomaticaAsync(Guid empresaId);

    /// <summary>
    /// Verifica si la aprobación automática de asientos está habilitada para una empresa.
    /// </summary>
    Task<bool> EstaHabilitadaAprobacionAutomaticaAsync(Guid empresaId);
}
