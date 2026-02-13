using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

/// <summary>
/// Unit of Work para gestión de Cuentas por Pagar y Abonos
/// </summary>
public interface ICuentaPorPagarUnitOfWork
{
    ICuentaPorPagarRepository CuentaPorPagarRepository { get; }
    IAbonoPagoRepository AbonoPagoRepository { get; }
    Task<int> SaveAsync();
}
