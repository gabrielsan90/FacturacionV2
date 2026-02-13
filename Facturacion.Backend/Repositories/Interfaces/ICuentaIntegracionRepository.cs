using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para acceder a los mapeos de cuentas de integración contable.
/// Permite obtener las cuentas contables asociadas a cada módulo/tipo de operación.
/// </summary>
public interface ICuentaIntegracionRepository
{
    /// <summary>
    /// Obtiene una cuenta de integración por su ID.
    /// </summary>
    Task<CuentaIntegracion?> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todas las cuentas de integración de una empresa.
    /// </summary>
    Task<IEnumerable<CuentaIntegracion>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene las cuentas de integración para un módulo específico.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="modulo">Código del módulo (VEN, COM, CXC, CXP, BAN, NOM, etc.)</param>
    Task<IEnumerable<CuentaIntegracion>> GetByModuloAsync(Guid empresaId, string modulo);

    /// <summary>
    /// Obtiene las cuentas de integración para un módulo y tipo de operación específicos.
    /// </summary>
    /// <param name="empresaId">ID de la empresa</param>
    /// <param name="modulo">Código del módulo (VEN, COM, CXC, CXP, BAN, NOM, etc.)</param>
    /// <param name="tipoOperacion">Tipo de operación (VENTA_CONTADO, VENTA_CREDITO, etc.)</param>
    Task<IEnumerable<CuentaIntegracion>> GetByModuloYTipoOperacionAsync(
        Guid empresaId,
        string modulo,
        string tipoOperacion);

    /// <summary>
    /// Obtiene una cuenta de integración específica por módulo, tipo de operación y concepto.
    /// </summary>
    Task<CuentaIntegracion?> GetByModuloTipoOperacionYConceptoAsync(
        Guid empresaId,
        string modulo,
        string tipoOperacion,
        string conceptoContable);

    /// <summary>
    /// Agrega una nueva cuenta de integración.
    /// </summary>
    Task<CuentaIntegracion> AddAsync(CuentaIntegracion cuentaIntegracion);

    /// <summary>
    /// Actualiza una cuenta de integración existente.
    /// </summary>
    Task UpdateAsync(CuentaIntegracion cuentaIntegracion);

    /// <summary>
    /// Elimina (desactiva) una cuenta de integración.
    /// </summary>
    Task DeleteAsync(Guid id, string userId);
}
