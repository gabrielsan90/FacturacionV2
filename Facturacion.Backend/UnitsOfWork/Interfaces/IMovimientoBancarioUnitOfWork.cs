using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

/// <summary>
/// Unit of Work para el módulo de movimientos bancarios
/// Gestiona las transacciones entre múltiples repositorios del módulo bancario
/// </summary>
public interface IMovimientoBancarioUnitOfWork
{
    /// <summary>
    /// Repositorio de movimientos bancarios
    /// </summary>
    IMovimientoBancarioRepository MovimientoBancarioRepository { get; }

    /// <summary>
    /// Repositorio de cuentas bancarias
    /// </summary>
    ICuentaBancariaRepository CuentaBancariaRepository { get; }

    /// <summary>
    /// Repositorio de conciliaciones bancarias
    /// </summary>
    IConciliacionBancariaRepository ConciliacionBancariaRepository { get; }

    /// <summary>
    /// Repositorio de extractos bancarios
    /// </summary>
    IExtractoBancarioRepository ExtractoBancarioRepository { get; }

    /// <summary>
    /// Guarda todos los cambios pendientes en una sola transacción
    /// </summary>
    Task<int> SaveAsync();
}
