using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

/// <summary>
/// Implementación del repositorio de auditoría
/// </summary>
public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly DataContext _context;

    public AuditoriaRepository(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene registros de auditoría por empresa
    /// </summary>
    public async Task<IEnumerable<Auditoria>> GetByEmpresaAsync(
        Guid empresaId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? tabla = null,
        string? accion = null,
        int limite = 100)
    {
        var query = _context.Auditorias
            .Where(a => a.EmpresaId == empresaId);

        if (fechaDesde.HasValue)
            query = query.Where(a => a.Fecha >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(a => a.Fecha <= fechaHasta.Value);

        if (!string.IsNullOrEmpty(tabla))
            query = query.Where(a => a.Tabla == tabla);

        if (!string.IsNullOrEmpty(accion))
            query = query.Where(a => a.Accion == accion);

        return await query
            .OrderByDescending(a => a.Fecha)
            .Take(limite)
            .Include(a => a.Usuario)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene registros de auditoría por usuario
    /// </summary>
    public async Task<IEnumerable<Auditoria>> GetByUsuarioAsync(
        string usuarioId,
        Guid? empresaId = null,
        int limite = 100)
    {
        var query = _context.Auditorias
            .Where(a => a.UsuarioId == usuarioId);

        if (empresaId.HasValue)
            query = query.Where(a => a.EmpresaId == empresaId.Value);

        return await query
            .OrderByDescending(a => a.Fecha)
            .Take(limite)
            .Include(a => a.Empresa)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene registros de auditoría de un registro específico
    /// </summary>
    public async Task<IEnumerable<Auditoria>> GetByRegistroAsync(
        string tabla,
        string registroId,
        Guid? empresaId = null)
    {
        var query = _context.Auditorias
            .Where(a => a.Tabla == tabla && a.RegistroId == registroId);

        if (empresaId.HasValue)
            query = query.Where(a => a.EmpresaId == empresaId.Value);

        return await query
            .OrderByDescending(a => a.Fecha)
            .Include(a => a.Usuario)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene una auditoría por ID
    /// </summary>
    public async Task<Auditoria?> GetAsync(Guid id)
    {
        return await _context.Auditorias
            .Include(a => a.Empresa)
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// Crea un nuevo registro de auditoría
    /// </summary>
    public async Task<Auditoria> AddAsync(Auditoria auditoria)
    {
        if (auditoria.Id == Guid.Empty)
            auditoria.Id = Guid.NewGuid();

        if (auditoria.Fecha == default)
            auditoria.Fecha = DateTime.Now;

        _context.Auditorias.Add(auditoria);
        await _context.SaveChangesAsync();

        return auditoria;
    }

    /// <summary>
    /// Elimina registros antiguos
    /// </summary>
    public async Task EliminarAntiguosAsync(int diasRetener = 365)
    {
        var fechaLimite = DateTime.Now.AddDays(-diasRetener);

        await _context.Auditorias
            .Where(a => a.Fecha < fechaLimite)
            .ExecuteDeleteAsync();
    }
}
