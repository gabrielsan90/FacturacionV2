using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Implementations;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

public class DocumentoUnitOfWork : IDocumentoUnitOfWork
{
    private readonly DataContext _context;
    private IDocumentoRepository? _documentoRepository;

    public DocumentoUnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IDocumentoRepository DocumentoRepository
    {
        get
        {
            _documentoRepository ??= new DocumentoRepository(_context);
            return _documentoRepository;
        }
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
