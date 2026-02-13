using Facturacion.Backend.Repositories.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Interfaces;

public interface IEmpleadoUnitOfWork
{
    IEmpleadoRepository EmpleadoRepository { get; }
    IDepartamentoRepository DepartamentoRepository { get; }
    IPuestoRepository PuestoRepository { get; }
    IVacacionRepository VacacionRepository { get; }
    IIncapacidadRepository IncapacidadRepository { get; }
    Task<int> SaveAsync();
}
