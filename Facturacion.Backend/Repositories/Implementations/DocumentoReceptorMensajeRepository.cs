using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

/// <summary>
/// Repositorio para gestionar Mensajes Receptor (MR)
/// </summary>
public class DocumentoReceptorMensajeRepository : IDocumentoReceptorMensajeRepository
{
    private readonly DataContext _context;

    public DocumentoReceptorMensajeRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<DocumentoReceptorMensaje?> GetAsync(Guid id)
    {
        return await _context.Set<DocumentoReceptorMensaje>()
            .Include(m => m.DocumentoOriginal)
                .ThenInclude(d => d!.Proveedor)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
    }

    public async Task<IEnumerable<DocumentoReceptorMensaje>> GetByDocumentoOriginalAsync(Guid documentoOriginalId)
    {
        return await _context.Set<DocumentoReceptorMensaje>()
            .Include(m => m.DocumentoOriginal)
            .Where(m => m.DocumentoOriginalId == documentoOriginalId && !m.IsDeleted)
            .OrderByDescending(m => m.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<DocumentoReceptorMensaje?> GetByClaveMensajeAsync(string claveMensaje)
    {
        return await _context.Set<DocumentoReceptorMensaje>()
            .Include(m => m.DocumentoOriginal)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ClaveMensaje == claveMensaje && !m.IsDeleted);
    }

    public async Task<bool> ExisteMensajeParaDocumentoAsync(Guid documentoOriginalId)
    {
        return await _context.Set<DocumentoReceptorMensaje>()
            .AnyAsync(m => m.DocumentoOriginalId == documentoOriginalId && !m.IsDeleted);
    }

    public async Task<IEnumerable<DocumentoReceptorMensaje>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.Set<DocumentoReceptorMensaje>()
            .Include(m => m.DocumentoOriginal)
                .ThenInclude(d => d!.Proveedor)
            .Where(m => m.DocumentoOriginal!.EmpresaId == empresaId && !m.IsDeleted)
            .OrderByDescending(m => m.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<DocumentoReceptorMensaje>> GetPendientesEnvioAsync(Guid empresaId)
    {
        return await _context.Set<DocumentoReceptorMensaje>()
            .Include(m => m.DocumentoOriginal)
            .Where(m => m.DocumentoOriginal!.EmpresaId == empresaId &&
                       m.Estado == "Pendiente" &&
                       !m.IsDeleted)
            .OrderBy(m => m.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<DocumentoReceptorMensaje> AddAsync(DocumentoReceptorMensaje mensaje)
    {
        mensaje.FechaCreacion = FechaCostaRicaHelper.Ahora;
        _context.Set<DocumentoReceptorMensaje>().Add(mensaje);
        await _context.SaveChangesAsync();
        return mensaje;
    }

    public async Task UpdateAsync(DocumentoReceptorMensaje mensaje)
    {
        mensaje.FechaModificacion = FechaCostaRicaHelper.Ahora;
        _context.Set<DocumentoReceptorMensaje>().Update(mensaje);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var mensaje = await _context.Set<DocumentoReceptorMensaje>().FindAsync(id);
        if (mensaje != null)
        {
            mensaje.IsDeleted = true;
            mensaje.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            mensaje.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
