using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IContabilidadUnitOfWork : IDisposable
{
    IAsientoContableRepository AsientoContableRepository { get; }
    IPeriodoContableRepository PeriodoContableRepository { get; }
    IPlantillaAsientoRepository PlantillaAsientoRepository { get; }
    ICuentaContableRepository CuentaContableRepository { get; }
    ICentroCostoRepository CentroCostoRepository { get; }
    ICuentaIntegracionRepository CuentaIntegracionRepository { get; }
    IConfiguracionContableRepository ConfiguracionContableRepository { get; }
    Task<int> SaveAsync();
}
