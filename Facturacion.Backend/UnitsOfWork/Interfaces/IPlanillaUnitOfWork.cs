using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IPlanillaUnitOfWork
{
    IPlanillaRepository PlanillaRepository { get; }
    IEmpleadoRepository EmpleadoRepository { get; }
    Task<int> SaveAsync();
}
