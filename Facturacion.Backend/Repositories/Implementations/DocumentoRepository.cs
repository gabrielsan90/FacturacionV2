using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class DocumentoRepository : IDocumentoRepository
{
    private readonly DataContext _context;

    public DocumentoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Documento?> GetAsync(Guid id)
    {
        return await _context.Documentos
            .Include(d => d.Empresa)
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }

    public async Task<Documento?> GetWithDetallesAsync(Guid id)
    {
        return await _context.Documentos
            .Include(d => d.Empresa)
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .Include(d => d.Detalles.Where(det => !det.IsDeleted))
                .ThenInclude(det => det.Producto)
            .Include(d => d.Detalles.Where(det => !det.IsDeleted))
                .ThenInclude(det => det.UnidadMedida)
            .Include(d => d.Detalles.Where(det => !det.IsDeleted))
                .ThenInclude(det => det.Impuestos.Where(imp => !imp.IsDeleted))
            .Include(d => d.Detalles.Where(det => !det.IsDeleted))
                .ThenInclude(det => det.Descuentos.Where(desc => !desc.IsDeleted))
            .Include(d => d.Descuentos.Where(desc => !desc.IsDeleted))
            .Include(d => d.Referencias.Where(r => !r.IsDeleted))
            .Include(d => d.MediosPago.Where(mp => !mp.IsDeleted))
            .Include(d => d.OtraInformacion.Where(oi => !oi.IsDeleted))
            .Include(d => d.Exportacion)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }

    public async Task<IEnumerable<Documento>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.Documentos
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .Where(d => d.EmpresaId == empresaId && !d.IsDeleted)
            .OrderByDescending(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(IEnumerable<object> Data, int TotalCount)> GetByEmpresaPagedAsync(
        Guid empresaId,
        int skip,
        int take,
        Guid? sucursalId = null,
        Guid? terminalId = null,
        EstadoDocumento? estado = null,
        DocumentoTipo? tipoDocumento = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        Ambiente? ambiente = null,
        bool? esDocumentoRecibido = null)
    {
        var query = _context.Documentos
            .Where(d => d.EmpresaId == empresaId && !d.IsDeleted);

        if (esDocumentoRecibido.HasValue)
            query = query.Where(d => d.EsDocumentoRecibido == esDocumentoRecibido.Value);

        if (sucursalId.HasValue)
            query = query.Where(d => d.SucursalId == sucursalId.Value);

        if (terminalId.HasValue)
            query = query.Where(d => d.TerminalId == terminalId.Value);

        if (estado.HasValue)
            query = query.Where(d => d.Estado == estado.Value);

        if (tipoDocumento.HasValue)
            query = query.Where(d => d.TipoDocumento == tipoDocumento.Value);

        if (fechaInicio.HasValue)
            query = query.Where(d => d.FechaEmision >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(d => d.FechaEmision < fechaFin.Value.AddDays(1));

        if (ambiente.HasValue)
            query = query.Where(d => d.Ambiente == ambiente.Value);

        var totalCount = await query.CountAsync();

        // Use Select projection to avoid loading heavy XML fields
        var data = await query
            .OrderByDescending(d => d.FechaCreacion)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .Select(d => new
            {
                d.Id,
                d.Ambiente,
                d.TipoDocumento,
                d.NumeroConsecutivo,
                d.Clave,
                d.FechaEmision,
                d.FechaCreacion,
                d.Estado,
                d.MensajeHacienda,
                d.FechaEnvioHacienda,
                d.Moneda,
                d.TotalVenta,
                d.TotalImpuestos,
                d.TotalOtrosCargos,
                d.ClienteId,
                ReceptorNombre = d.Cliente != null ? d.Cliente.Nombre : d.ReceptorNombre,
                XmlRespuestaHacienda = d.XmlRespuestaHacienda != null ? "1" : null // Only presence flag, not full XML
            })
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<IEnumerable<Documento>> GetBySucursalAsync(Guid sucursalId)
    {
        return await _context.Documentos
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .Where(d => d.SucursalId == sucursalId && !d.IsDeleted)
            .OrderByDescending(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Documento>> GetByTerminalAsync(Guid terminalId)
    {
        return await _context.Documentos
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .Where(d => d.TerminalId == terminalId && !d.IsDeleted)
            .OrderByDescending(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Documento>> GetByClienteAsync(Guid clienteId)
    {
        return await _context.Documentos
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Where(d => d.ClienteId == clienteId && !d.IsDeleted)
            .OrderByDescending(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Documento?> GetByClaveAsync(string clave)
    {
        return await _context.Documentos
            .Include(d => d.Empresa)
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Clave == clave && !d.IsDeleted);
    }

    public async Task<Documento?> GetByConsecutivoAsync(Guid empresaId, string numeroConsecutivo)
    {
        return await _context.Documentos
            .Include(d => d.Empresa)
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.EmpresaId == empresaId &&
                                     d.NumeroConsecutivo == numeroConsecutivo &&
                                     !d.IsDeleted);
    }

    public async Task<IEnumerable<Documento>> GetByFechaRangoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.Documentos
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .Where(d => d.EmpresaId == empresaId &&
                       d.FechaEmision >= fechaInicio &&
                       d.FechaEmision <= fechaFin &&
                       !d.IsDeleted)
            .OrderByDescending(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Documento>> GetByEstadoAsync(Guid empresaId, EstadoDocumento estado)
    {
        return await _context.Documentos
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .Where(d => d.EmpresaId == empresaId &&
                       d.Estado == estado &&
                       !d.IsDeleted)
            .OrderByDescending(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Documento>> GetPendientesEnvioAsync(Guid empresaId)
    {
        return await _context.Documentos
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .Where(d => d.EmpresaId == empresaId &&
                       (d.Estado == EstadoDocumento.Pendiente || d.Estado == EstadoDocumento.Contingencia) &&
                       !d.IsDeleted)
            .OrderBy(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Documento> AddAsync(Documento documento)
    {
        documento.FechaCreacion = FechaCostaRicaHelper.Ahora;
        _context.Documentos.Add(documento);
        await _context.SaveChangesAsync();
        return documento;
    }

    public async Task UpdateAsync(Documento documento)
    {
        documento.FechaModificacion = FechaCostaRicaHelper.Ahora;
        _context.Documentos.Update(documento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var documento = await _context.Documentos.FindAsync(id);
        if (documento != null)
        {
            documento.IsDeleted = true;
            documento.FechaEliminacion = FechaCostaRicaHelper.Ahora;
            documento.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }

    // ========================================
    // MÉTODOS PARA DOCUMENTOS RECIBIDOS
    // ========================================

    public async Task<IEnumerable<Documento>> GetDocumentosRecibidosAsync(Guid empresaId)
    {
        return await _context.Documentos
            .Include(d => d.Proveedor)
            .Include(d => d.Sucursal)
            .Where(d => d.EmpresaId == empresaId &&
                       d.EsDocumentoRecibido == true &&
                       !d.IsDeleted)
            .OrderByDescending(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Documento>> GetDocumentosPendientesMRAsync(Guid empresaId)
    {
        // Obtener documentos recibidos que NO tienen mensaje receptor asociado
        // y que están dentro del plazo de 8 días calendario desde la emisión
        var fechaLimite = FechaCostaRicaHelper.Ahora.AddDays(-8);

        return await _context.Documentos
            .Include(d => d.Proveedor)
            .Where(d => d.EmpresaId == empresaId &&
                       d.EsDocumentoRecibido == true &&
                       d.FechaEmision >= fechaLimite && // Aún dentro del plazo
                       !d.IsDeleted &&
                       !_context.Set<DocumentoReceptorMensaje>()
                           .Any(m => m.DocumentoOriginalId == d.Id && !m.IsDeleted))
            .OrderBy(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExisteDocumentoPorClaveAsync(string clave, Guid empresaId)
    {
        return await _context.Documentos
            .AnyAsync(d => d.Clave == clave &&
                          d.EmpresaId == empresaId &&
                          !d.IsDeleted);
    }

    // ========================================
    // DIAGNÓSTICO
    // ========================================

    public async Task<IEnumerable<Documento>> GetAllByEmpresaIncludingDeletedAsync(Guid empresaId)
    {
        return await _context.Documentos
            .Where(d => d.EmpresaId == empresaId)
            .OrderByDescending(d => d.FechaEmision)
            .AsNoTracking()
            .ToListAsync();
    }
}
