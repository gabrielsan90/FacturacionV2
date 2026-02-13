using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class ContabilidadUnitOfWork : IContabilidadUnitOfWork
{
    private readonly DataContext _context;
    private IAsientoContableRepository? _asientoContableRepository;
    private IPeriodoContableRepository? _periodoContableRepository;
    private IPlantillaAsientoRepository? _plantillaAsientoRepository;
    private ICuentaContableRepository? _cuentaContableRepository;
    private ICentroCostoRepository? _centroCostoRepository;
    private ICuentaIntegracionRepository? _cuentaIntegracionRepository;
    private IConfiguracionContableRepository? _configuracionContableRepository;

    public ContabilidadUnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IAsientoContableRepository AsientoContableRepository
    {
        get
        {
            _asientoContableRepository ??= new AsientoContableRepository(_context);
            return _asientoContableRepository;
        }
    }

    public IPeriodoContableRepository PeriodoContableRepository
    {
        get
        {
            _periodoContableRepository ??= new PeriodoContableRepository(_context);
            return _periodoContableRepository;
        }
    }

    public IPlantillaAsientoRepository PlantillaAsientoRepository
    {
        get
        {
            _plantillaAsientoRepository ??= new PlantillaAsientoRepository(_context);
            return _plantillaAsientoRepository;
        }
    }

    public ICuentaContableRepository CuentaContableRepository
    {
        get
        {
            _cuentaContableRepository ??= new CuentaContableRepository(_context);
            return _cuentaContableRepository;
        }
    }

    public ICentroCostoRepository CentroCostoRepository
    {
        get
        {
            _centroCostoRepository ??= new CentroCostoRepository(_context);
            return _centroCostoRepository;
        }
    }

    public ICuentaIntegracionRepository CuentaIntegracionRepository
    {
        get
        {
            _cuentaIntegracionRepository ??= new CuentaIntegracionRepository(_context);
            return _cuentaIntegracionRepository;
        }
    }

    public IConfiguracionContableRepository ConfiguracionContableRepository
    {
        get
        {
            _configuracionContableRepository ??= new ConfiguracionContableRepository(_context);
            return _configuracionContableRepository;
        }
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
