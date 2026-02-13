using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para operaciones CRUD de CuentaBancaria
/// </summary>
public interface ICuentaBancariaRepository
{
    /// <summary>
    /// Obtiene una cuenta bancaria por su ID
    /// </summary>
    Task<ActionResponse<CuentaBancaria>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todas las cuentas bancarias de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaBancaria>>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene solo las cuentas bancarias activas de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<CuentaBancaria>>> GetActivasAsync(Guid empresaId);

    /// <summary>
    /// Obtiene una cuenta bancaria por su número de cuenta
    /// </summary>
    Task<ActionResponse<CuentaBancaria>> GetByNumeroCuentaAsync(Guid empresaId, string numeroCuenta);

    /// <summary>
    /// Crea una nueva cuenta bancaria
    /// </summary>
    Task<ActionResponse<CuentaBancaria>> AddAsync(CuentaBancaria cuenta);

    /// <summary>
    /// Actualiza una cuenta bancaria existente
    /// </summary>
    Task<ActionResponse<CuentaBancaria>> UpdateAsync(CuentaBancaria cuenta);

    /// <summary>
    /// Actualiza el saldo actual de una cuenta bancaria
    /// </summary>
    Task<ActionResponse<CuentaBancaria>> UpdateSaldoAsync(Guid cuentaBancariaId, decimal nuevoSaldo);

    /// <summary>
    /// Elimina una cuenta bancaria (soft delete)
    /// </summary>
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);
}
