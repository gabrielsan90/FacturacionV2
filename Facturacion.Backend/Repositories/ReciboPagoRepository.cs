using Facturacion.Backend.Data;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories;

/// <summary>
/// Implementación del repositorio para Recibos Electrónicos de Pago (REP)
/// </summary>
public class ReciboPagoRepository : IReciboPagoRepository
{
    private readonly DataContext _context;

    public ReciboPagoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<ReciboPago?> GetByIdAsync(Guid id)
    {
        return await _context.RecibosPago
            .Include(r => r.Documento)
            .Include(r => r.DocumentoOriginal)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<ReciboPago>> GetByDocumentoOriginalAsync(Guid documentoOriginalId)
    {
        return await _context.RecibosPago
            .Include(r => r.Documento)
            .Where(r => r.DocumentoOriginalId == documentoOriginalId)
            .OrderBy(r => r.FechaPago)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReciboPago>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.RecibosPago
            .Include(r => r.Documento)
            .Include(r => r.DocumentoOriginal)
            .Where(r => r.Documento!.EmpresaId == empresaId)
            .OrderByDescending(r => r.FechaPago)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalPagadoAsync(Guid documentoOriginalId)
    {
        var total = await _context.RecibosPago
            .Where(r => r.DocumentoOriginalId == documentoOriginalId
                     && r.Documento!.Estado != EstadoDocumento.Rechazado)
            .SumAsync(r => (decimal?)r.MontoPagado);

        return total ?? 0m;
    }

    public async Task<decimal> CalcularSaldoPendienteAsync(Guid documentoOriginalId)
    {
        // Obtener el documento original
        var documento = await _context.Documentos
            .FirstOrDefaultAsync(d => d.Id == documentoOriginalId);

        if (documento == null)
            return 0m;

        // Calcular total pagado
        var totalPagado = await GetTotalPagadoAsync(documentoOriginalId);

        // Retornar saldo pendiente
        return documento.TotalVenta - totalPagado;
    }

    public async Task<ReciboPago> AddAsync(ReciboPago recibo)
    {
        recibo.FechaCreacion = DateTime.Now;
        await _context.RecibosPago.AddAsync(recibo);
        await _context.SaveChangesAsync();
        return recibo;
    }

    public async Task<ReciboPago> UpdateAsync(ReciboPago recibo)
    {
        recibo.FechaModificacion = DateTime.Now;
        _context.RecibosPago.Update(recibo);
        await _context.SaveChangesAsync();
        return recibo;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var recibo = await GetByIdAsync(id);
        if (recibo == null)
            return false;

        recibo.IsDeleted = true;
        recibo.FechaEliminacion = DateTime.Now;
        recibo.UsuarioEliminacionId = userId;

        await UpdateAsync(recibo);
        return true;
    }

    public async Task<IEnumerable<ReciboPago>> GetByFechaRangoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.RecibosPago
            .Include(r => r.Documento)
            .Include(r => r.DocumentoOriginal)
            .Where(r => r.Documento!.EmpresaId == empresaId
                     && r.FechaPago >= fechaInicio
                     && r.FechaPago <= fechaFin)
            .OrderBy(r => r.FechaPago)
            .ToListAsync();
    }

    public async Task<bool> ExisteReciboPorDocumentoAsync(Guid documentoId)
    {
        return await _context.RecibosPago
            .AnyAsync(r => r.DocumentoId == documentoId);
    }

    public async Task<ReciboPago?> GetByDocumentoREPAsync(Guid documentoREPId)
    {
        return await _context.RecibosPago
            .Include(r => r.Documento)
            .Include(r => r.DocumentoOriginal)
            .FirstOrDefaultAsync(r => r.DocumentoId == documentoREPId);
    }
}
