using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

/// <summary>
/// Unit of Work para gestión de Cuentas por Cobrar, Abonos y Cotizaciones
/// </summary>
public interface ICuentaPorCobrarUnitOfWork
{
    ICuentaPorCobrarRepository CuentaPorCobrarRepository { get; }
    IAbonoCobroRepository AbonoCobroRepository { get; }
    ICotizacionRepository CotizacionRepository { get; }
    Task<int> SaveAsync();
}
